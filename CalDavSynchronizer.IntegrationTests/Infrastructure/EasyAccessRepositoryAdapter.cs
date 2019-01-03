// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
    public static class EasyAccessRepositoryAdapter
    {
        public static EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> Create<TEntityId, TEntityVersion, TEntity, TContext>(
            IVersionAwareReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> readRepository,
            IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> writeRepository,
            ISynchronizationContextFactory<TContext> contextFactory)
        {
            return new EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext>(readRepository, writeRepository, contextFactory);
        }

        public static EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> Create<TEntityId, TEntityVersion, TEntity, TContext>(
            IEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository,
            ISynchronizationContextFactory<TContext> contextFactory)
        {
            return new EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext>(repository, contextFactory);
        }
    }

    public class EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext>
    {
        private readonly IVersionAwareReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _readRepository;
        private readonly IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _writeRepository;
        private readonly ISynchronizationContextFactory<TContext> _contextFactory;
        private readonly IEqualityComparer<TEntityId> _idComparer;

        public EasyAccessRepositoryAdapter(IVersionAwareReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> readRepository, IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> writeRepository, ISynchronizationContextFactory<TContext> contextFactory)
        {
            if (readRepository == null) throw new ArgumentNullException(nameof(readRepository));
            if (writeRepository == null) throw new ArgumentNullException(nameof(writeRepository));
            if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

            _readRepository = readRepository;
            _writeRepository = writeRepository;
            _contextFactory = contextFactory;
        }

        public EasyAccessRepositoryAdapter(IEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository, ISynchronizationContextFactory<TContext> contextFactory)
        {
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));
            if (contextFactory == null)
                throw new ArgumentNullException(nameof(contextFactory));

            _readRepository = repository;
            _writeRepository = BatchEntityRepositoryAdapter.Create(repository, new TestExceptionHandlingStrategy());
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<EntityWithId<TEntityId, TEntity>>> GetAllEntities()
        {
            var context = await _contextFactory.Create();

            try
            {
                return await _readRepository.Get(
                    (await _readRepository.GetAllVersions(new TEntityId[0], context, NullGetVersionsLogger.Instance)).Select(v => v.Id).ToArray(),
                    NullLoadEntityLogger.Instance,
                    context);
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }
        }

        public async Task<IReadOnlyList<(TEntityId Id, TEntityProjection Entity)>> GetEntities<TEntityProjection>(ICollection<TEntityId> ids, Func<TEntity,TEntityProjection> selector, Action<TEntity> cleanupOrNull = null)
        {
            var context = await _contextFactory.Create();

            try
            {
                return (await _readRepository.Get(
                    ids,
                    NullLoadEntityLogger.Instance,
                    context))
                    .Select(e =>
                    {
                        try
                        {
                            return (e.Id, selector(e.Entity));
                        }
                        finally
                        {
                            cleanupOrNull?.Invoke(e.Entity);
                        }
                    })
                    .ToArray();
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }
        }

        public async Task<TEntityId> CreateEntity(Action<TEntity> initializeAction)
        {
            return (await CreateEntities(new[] {initializeAction})).Single();
        }

        public async Task<IReadOnlyList<TEntityId>> CreateEntities(IEnumerable<Action<TEntity>> initializeActions)
        {
            var createJobs = initializeActions.Select(a => new CreateJob(a)).ToArray();

            var context = await _contextFactory.Create();
            try
            {
                await _writeRepository.PerformOperations(
                    createJobs,
                    new IUpdateJob<TEntityId, TEntityVersion, TEntity>[0],
                    new IDeleteJob<TEntityId, TEntityVersion>[0],
                    NullProgressLogger.Instance,
                    context);
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }

            return createJobs.Select(j => j.Id).ToArray();
        }

        public Task UpdateEntity(TEntityId id, Action<TEntity> modifyAction)
        {
            return UpdateEntities(new[] {(id, modifyAction)});
        }

        public Task UpdateEntities<TContent>(IEnumerable<(TEntityId Id, TContent NewContent)> updates, Func<TContent, Action<TEntity>> updateActionFactory)
        {
            return UpdateEntities(updates.Select(u => (u.Id, updateActionFactory(u.NewContent))).ToArray());
        }

        public async Task UpdateEntities(ICollection<(TEntityId Id, Action<TEntity> ModifyAction)> updates)
        {
            var context = await _contextFactory.Create();
            try
            {
                var versions = (await _readRepository.GetVersions(updates.Select(u => new IdWithAwarenessLevel<TEntityId>(u.Id, false)), context, NullGetVersionsLogger.Instance)).ToArray();
                Assert.That(versions.Length, Is.EqualTo(updates.Count));
                var entities = (await _readRepository.Get(updates.Select(u => u.Id).ToArray(), NullLoadEntityLogger.Instance, context)).ToArray();
                Assert.That(entities.Length, Is.EqualTo(updates.Count));

                var updateJobs = versions
                    .Join(entities, v => v.Id, e => e.Id, (v, e) => (Version: v, Entity: e), _idComparer)
                    .Join(updates, o => o.Version.Id, u => u.Id, (o, u) => new UpdateJob(o.Version.Id, o.Version.Version, o.Entity.Entity, u.ModifyAction), _idComparer)
                    .ToArray();

                Assert.That(updateJobs.Length, Is.EqualTo(updates.Count));

                await _writeRepository.PerformOperations(
                    new ICreateJob<TEntityId, TEntityVersion, TEntity>[0],
                    updateJobs,
                    new IDeleteJob<TEntityId, TEntityVersion>[0],
                    NullProgressLogger.Instance,
                    context);

                _readRepository.Cleanup(entities.Select(e => e.Entity));
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }
        }

        public async Task DeleteAllEntities()
        {
            var context = await _contextFactory.Create();
            try
            {
                var versions = await _readRepository.GetAllVersions(new TEntityId[0], context, NullGetVersionsLogger.Instance);

                await _writeRepository.PerformOperations(
                    new ICreateJob<TEntityId, TEntityVersion, TEntity>[0],
                    new IUpdateJob<TEntityId, TEntityVersion, TEntity>[0],
                    versions.Select(v => new DeleteJob(v.Id, v.Version)).ToArray(),
                    NullProgressLogger.Instance,
                    context);
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }
        }

        public Task DeleteEntity(TEntityId id)
        {
            return DeleteEntities(new[] {id});
        }

        public async Task DeleteEntities(IEnumerable<TEntityId> ids)
        {
            var context = await _contextFactory.Create();
            try
            {
                var versions = (await _readRepository.GetVersions(ids.Select(id => new IdWithAwarenessLevel<TEntityId>(id, false)) , context, NullGetVersionsLogger.Instance));

                await _writeRepository.PerformOperations(
                    new ICreateJob<TEntityId, TEntityVersion, TEntity>[0],
                    new IUpdateJob<TEntityId, TEntityVersion, TEntity>[0],
                    versions.Select(v => new DeleteJob(v.Id, v.Version)).ToArray(),
                    NullProgressLogger.Instance,
                    context);
            }
            finally
            {
                await _contextFactory.SynchronizationFinished(context);
            }
        }

        class CreateJob : ICreateJob<TEntityId, TEntityVersion, TEntity>
        {
            private readonly Action<TEntity> _initializeEntity;

            public CreateJob(Action<TEntity> initializeEntity)
            {
                if (initializeEntity == null) throw new ArgumentNullException(nameof(initializeEntity));
                _initializeEntity = initializeEntity;
            }

            public Task<TEntity> InitializeEntity(TEntity entity)
            {
                _initializeEntity(entity);
                return Task.FromResult(entity);
            }

            public TEntityId Id { get; private set; }

            public void NotifyOperationSuceeded(EntityVersion<TEntityId, TEntityVersion> result)
            {
                Id = result.Id;
            }

            public void NotifyOperationFailed(Exception exception)
            {
                Assert.Fail($"Creation of Entity failed: {exception}");
            }

            public void NotifyOperationFailed(string errorMessage)
            {
                Assert.Fail($"Creation of Entity failed: {errorMessage}");
            }
        }

        class UpdateJob : IUpdateJob<TEntityId, TEntityVersion, TEntity>
        {
            private readonly Action<TEntity> _modifyEntity;

            public UpdateJob(TEntityId entityId, TEntityVersion version, TEntity entityToUpdate, Action<TEntity> modifyEntity)
            {
                if (entityId == null) throw new ArgumentNullException(nameof(entityId));
                if (version == null) throw new ArgumentNullException(nameof(version));
                if (entityToUpdate == null) throw new ArgumentNullException(nameof(entityToUpdate));
                if (modifyEntity == null) throw new ArgumentNullException(nameof(modifyEntity));
                EntityId = entityId;
                Version = version;
                EntityToUpdate = entityToUpdate;
                _modifyEntity = modifyEntity;
            }

            public IEntitySynchronizationLogger Logger => NullEntitySynchronizationLogger.Instance;
            public TEntityId EntityId { get; }
            public TEntityVersion Version { get; }
            public TEntity EntityToUpdate { get; }

            public Task<TEntity> UpdateEntity(TEntity entity)
            {
                _modifyEntity(entity);
                return Task.FromResult(entity);
            }

            public void NotifyOperationSuceeded(EntityVersion<TEntityId, TEntityVersion> result)
            {

            }

            public void NotifyEntityNotFound()
            {
                throw new Exception($"Entity '{EntityId}' not found.");
            }

            public void NotifyOperationFailed(Exception exception)
            {
                Assert.Fail($"Update of Entity '{EntityId}' failed: {exception}");
            }

            public void NotifyOperationFailed(string errorMessage)
            {
                Assert.Fail($"Update of Entity '{EntityId}' failed: {errorMessage}");
            }
        }

        class DeleteJob : IDeleteJob<TEntityId, TEntityVersion>
        {
            public DeleteJob(TEntityId entityId, TEntityVersion version)
            {
                if (entityId == null) throw new ArgumentNullException(nameof(entityId));
                if (version == null) throw new ArgumentNullException(nameof(version));
                EntityId = entityId;
                Version = version;
            }

            public IEntitySynchronizationLogger Logger => NullEntitySynchronizationLogger.Instance;
            public TEntityId EntityId { get; }
            public TEntityVersion Version { get; }

            public void NotifyOperationSuceeded()
            {

            }

            public void NotifyEntityNotFound()
            {
                throw new Exception($"Entity '{EntityId}' not found.");
            }

            public void NotifyOperationFailed(Exception exception)
            {
                Assert.Fail($"Deletion of Entity '{EntityId}' failed: {exception}");
            }

            public void NotifyOperationFailed(string errorMessage)
            {
                Assert.Fail($"Deletion of Entity '{EntityId}' failed: {errorMessage}");

            }
        }
    }
}