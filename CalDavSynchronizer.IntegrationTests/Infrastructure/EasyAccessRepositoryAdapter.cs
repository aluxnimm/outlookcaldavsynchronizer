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

namespace CalDavSynchronizer.IntegrationTests.Infrastructure
{
  public static class EasyAccessRepositoryAdapter
  {
    public static EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> Create<TEntityId, TEntityVersion, TEntity, TContext>(
      IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> readRepository,
      IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> writeRepository,
      ISynchronizationContextFactory<TContext> contextFactory)
    {
      return new EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext>(readRepository, writeRepository, contextFactory);
    }

    public static EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> Create<TEntityId, TEntityVersion, TEntity, TContext> (
      IEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository,
      ISynchronizationContextFactory<TContext> contextFactory)
    {
      return new EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext> (repository, contextFactory);
    }
  }

  public class EasyAccessRepositoryAdapter<TEntityId, TEntityVersion, TEntity, TContext>
  {
    private readonly IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _readRepository;
    private readonly IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _writeRepository;
    private readonly ISynchronizationContextFactory<TContext> _contextFactory;
    
    public EasyAccessRepositoryAdapter(IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> readRepository, IBatchWriteOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> writeRepository, ISynchronizationContextFactory<TContext> contextFactory)
    {
      if (readRepository == null) throw new ArgumentNullException(nameof(readRepository));
      if (writeRepository == null) throw new ArgumentNullException(nameof(writeRepository));
      if (contextFactory == null) throw new ArgumentNullException(nameof(contextFactory));

      _readRepository = readRepository;
      _writeRepository = writeRepository;
      _contextFactory = contextFactory;
    }

    public EasyAccessRepositoryAdapter (IEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository, ISynchronizationContextFactory<TContext> contextFactory)
    {
      if (repository == null)
        throw new ArgumentNullException (nameof (repository));
      if (contextFactory == null)
        throw new ArgumentNullException (nameof (contextFactory));

      _readRepository = repository;
      _writeRepository = BatchEntityRepositoryAdapter.Create(repository, new ExceptionHandlingStrategy());
      _contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<EntityWithId<TEntityId, TEntity>>> GetAllEntities()
    {
      var context = await _contextFactory.Create();

      IReadOnlyList<EntityWithId<TEntityId, TEntity>> result;
      try
      {
        return await _readRepository.Get(
          (await _readRepository.GetAllVersions(new TEntityId[0], context)).Select(v => v.Id).ToArray(),
          NullLoadEntityLogger.Instance,
          context);
      }
      finally
      {
        await _contextFactory.SynchronizationFinished(context);
      }
    }

    public async Task<TEntityId> CreateEntity(Action<TEntity> initializeAction)
    {
      var createJob = new CreateJob(initializeAction);

      var context = await _contextFactory.Create ();
      try
      {
        await _writeRepository.PerformOperations(
        new[] {createJob},
        new IUpdateJob<TEntityId, TEntityVersion, TEntity>[0],
        new IDeleteJob<TEntityId, TEntityVersion>[0],
        NullProgressLogger.Instance,
        context);
      }
      finally
      {
        await _contextFactory.SynchronizationFinished (context);
      }

      return createJob.Id;
    }

    public async Task UpdateEntity(TEntityId id, Action<TEntity> modifyAction)
    {
      var context = await _contextFactory.Create();
      try
      {
        var version = (await _readRepository.GetVersions(new[] {new IdWithAwarenessLevel<TEntityId>(id, false)}, context)).Single();
        var entity = (await _readRepository.Get(new[] {id}, NullLoadEntityLogger.Instance, context)).Single();

        await _writeRepository.PerformOperations(
          new ICreateJob<TEntityId, TEntityVersion, TEntity>[0],
          new[] {new UpdateJob(id, version.Version, entity.Entity, modifyAction)},
          new IDeleteJob<TEntityId, TEntityVersion>[0],
          NullProgressLogger.Instance,
          context);
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
        var versions = await _readRepository.GetAllVersions(new TEntityId[0], context);

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

    public async Task DeleteEntity(TEntityId id)
    {
      var context = await _contextFactory.Create();
      try
      {
        var version = (await _readRepository.GetVersions(new[] {new IdWithAwarenessLevel<TEntityId>(id, false)}, context)).Single();

        await _writeRepository.PerformOperations(
          new ICreateJob<TEntityId, TEntityVersion, TEntity>[0],
          new IUpdateJob<TEntityId, TEntityVersion, TEntity>[0],
          new[] {new DeleteJob(id, version.Version)},
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
        _initializeEntity (entity);
        return Task.FromResult (entity);
      }

      public TEntityId Id { get; private set; }

      public void NotifyOperationSuceeded(EntityVersion<TEntityId, TEntityVersion> result)
      {
        Id = result.Id;
      }

      public void NotifyOperationFailed(Exception exception)
      {
        
      }

      public void NotifyOperationFailed(string errorMessage)
      {
       
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
        throw new Exception ($"Entity '{EntityId}' not found.");
      }

      public void NotifyOperationFailed(Exception exception)
      {
      
      }

      public void NotifyOperationFailed(string errorMessage)
      {
       
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

      public TEntityId EntityId { get; }
      public TEntityVersion Version { get; }
      
      public void NotifyOperationSuceeded()
      {
       
      }

      public void NotifyEntityNotFound()
      {
        throw new Exception ($"Entity '{EntityId}' not found.");
      }

      public void NotifyOperationFailed(Exception exception)
      {
        
      }

      public void NotifyOperationFailed(string errorMessage)
      {
       
      }
    }

    class ExceptionHandlingStrategy : IExceptionHandlingStrategy
    {
      public bool DoesAbortSynchronization(Exception x)
      {
        return true;
      }
    }

  }
}