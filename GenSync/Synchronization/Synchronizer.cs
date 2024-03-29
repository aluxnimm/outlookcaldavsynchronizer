﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using System.Reflection;
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.InitialEntityMatching;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.StateFactories;
using GenSync.Synchronization.States;
using GenSync.Utilities;
using log4net;

namespace GenSync.Synchronization
{
    /// <summary>
    /// Synchronizes tow repositories
    /// </summary>
    public class Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext, TAMatchData, TBMatchData, TAtypeStateToken, TBtypeStateToken>
        : IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, TContext>
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private readonly IEqualityComparer<TAtypeEntityVersion> _atypeVersionComparer;
        private readonly IEqualityComparer<TBtypeEntityVersion> _btypeVersionComparer;
        private readonly ISynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _synchronizationInterceptorFactory;

        private readonly IEqualityComparer<TAtypeEntityId> _atypeIdComparer;
        private readonly IEqualityComparer<TBtypeEntityId> _btypeIdComparer;

        private readonly IInitialEntityMatcher<TAtypeEntityId, TAtypeEntityVersion, TAMatchData, TBtypeEntityId, TBtypeEntityVersion, TBMatchData> _initialEntityMatcher;
        private readonly IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataFactory;
        private readonly IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataAccess;
        private readonly IStateTokenDataAccess<TAtypeStateToken, TBtypeStateToken> _stateTokenDataAccess;
        private readonly IReadOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> _atypeRepository;
        private readonly IReadOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _btypeRepository;
        private readonly IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _initialSyncStateCreationStrategy;
        private readonly ITotalProgressFactory _totalProgressFactory;
        private readonly IBatchWriteOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> _atypeWriteRepository;
        private readonly IBatchWriteOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _btypeWriteRepository;
        private readonly IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _syncStateFactory;

        private readonly IExceptionHandlingStrategy _exceptionHandlingStrategy;
        private readonly IMatchDataFactory<TAtypeEntity, TAMatchData> _aMatchDataFactory;
        private readonly IMatchDataFactory<TBtypeEntity, TBMatchData> _bMatchDataFactory;
        private readonly EntitySyncStateChunkCreator<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _entitySyncStateChunkCreator;
        private readonly int? _chunkSize;
        private readonly IChunkedExecutor _chunkedExecutor;
        private readonly IFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> _fullEntitySynchronizationLoggerFactory;
        private readonly IStateAwareEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TContext, TAtypeStateToken> _atypeStateAwareEntityRepository;
        private readonly IStateAwareEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TContext, TBtypeStateToken> _btypeStateAwareEntityRepository;

        public Synchronizer(
            IReadOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> atypeRepository,
            IReadOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> btypeRepository,
            IBatchWriteOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> atypeWriteRepository,
            IBatchWriteOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> btypeWriteRepository,
            IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> initialSyncStateCreationStrategy,
            IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataAccess,
            IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataFactory,
            IInitialEntityMatcher<TAtypeEntityId, TAtypeEntityVersion, TAMatchData, TBtypeEntityId, TBtypeEntityVersion, TBMatchData> initialEntityMatcher,
            IEqualityComparer<TAtypeEntityId> atypeIdComparer,
            IEqualityComparer<TBtypeEntityId> btypeIdComparer,
            ITotalProgressFactory totalProgressFactory,
            IEqualityComparer<TAtypeEntityVersion> atypeVersionComparer,
            IEqualityComparer<TBtypeEntityVersion> btypeVersionComparer,
            IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateFactory,
            IExceptionHandlingStrategy exceptionHandlingStrategy,
            IMatchDataFactory<TAtypeEntity, TAMatchData> aMatchDataFactory,
            IMatchDataFactory<TBtypeEntity, TBMatchData> bMatchDataFactory,
            int? chunkSize,
            IChunkedExecutor chunkedExecutor,
            IFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> fullEntitySynchronizationLoggerFactory,
            IStateAwareEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TContext, TAtypeStateToken> atypeStateAwareEntityRepository,
            IStateAwareEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TContext, TBtypeStateToken> btypeStateAwareEntityRepository,
            IStateTokenDataAccess<TAtypeStateToken, TBtypeStateToken> stateTokenDataAccess,
            ISynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> synchronizationInterceptorFactoryOrNull = null)
        {
            if (atypeRepository == null) throw new ArgumentNullException(nameof(atypeRepository));
            if (btypeRepository == null) throw new ArgumentNullException(nameof(btypeRepository));
            if (atypeWriteRepository == null) throw new ArgumentNullException(nameof(atypeWriteRepository));
            if (btypeWriteRepository == null) throw new ArgumentNullException(nameof(btypeWriteRepository));
            if (initialSyncStateCreationStrategy == null) throw new ArgumentNullException(nameof(initialSyncStateCreationStrategy));
            if (entityRelationDataAccess == null) throw new ArgumentNullException(nameof(entityRelationDataAccess));
            if (entityRelationDataFactory == null) throw new ArgumentNullException(nameof(entityRelationDataFactory));
            if (initialEntityMatcher == null) throw new ArgumentNullException(nameof(initialEntityMatcher));
            if (atypeIdComparer == null) throw new ArgumentNullException(nameof(atypeIdComparer));
            if (btypeIdComparer == null) throw new ArgumentNullException(nameof(btypeIdComparer));
            if (totalProgressFactory == null) throw new ArgumentNullException(nameof(totalProgressFactory));
            if (atypeVersionComparer == null) throw new ArgumentNullException(nameof(atypeVersionComparer));
            if (btypeVersionComparer == null) throw new ArgumentNullException(nameof(btypeVersionComparer));
            if (syncStateFactory == null) throw new ArgumentNullException(nameof(syncStateFactory));
            if (exceptionHandlingStrategy == null) throw new ArgumentNullException(nameof(exceptionHandlingStrategy));
            if (aMatchDataFactory == null) throw new ArgumentNullException(nameof(aMatchDataFactory));
            if (bMatchDataFactory == null) throw new ArgumentNullException(nameof(bMatchDataFactory));
            if (fullEntitySynchronizationLoggerFactory == null) throw new ArgumentNullException(nameof(fullEntitySynchronizationLoggerFactory));
            if (atypeStateAwareEntityRepository == null) throw new ArgumentNullException(nameof(atypeStateAwareEntityRepository));
            if (btypeStateAwareEntityRepository == null) throw new ArgumentNullException(nameof(btypeStateAwareEntityRepository));
            if (stateTokenDataAccess == null) throw new ArgumentNullException(nameof(stateTokenDataAccess));

            _chunkSize = chunkSize;
            _chunkedExecutor = chunkedExecutor;
            _fullEntitySynchronizationLoggerFactory = fullEntitySynchronizationLoggerFactory;
            _atypeStateAwareEntityRepository = atypeStateAwareEntityRepository;
            _btypeStateAwareEntityRepository = btypeStateAwareEntityRepository;
            _stateTokenDataAccess = stateTokenDataAccess;

            _initialSyncStateCreationStrategy = initialSyncStateCreationStrategy;
            _totalProgressFactory = totalProgressFactory;
            _atypeIdComparer = atypeIdComparer;
            _btypeIdComparer = btypeIdComparer;
            _atypeVersionComparer = atypeVersionComparer;
            _btypeVersionComparer = btypeVersionComparer;
            _syncStateFactory = syncStateFactory;
            _exceptionHandlingStrategy = exceptionHandlingStrategy;
            _aMatchDataFactory = aMatchDataFactory;
            _bMatchDataFactory = bMatchDataFactory;
            _synchronizationInterceptorFactory = synchronizationInterceptorFactoryOrNull ?? NullSynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>.Instance;
            _atypeWriteRepository = atypeWriteRepository;
            _btypeWriteRepository = btypeWriteRepository;
            _atypeRepository = atypeRepository;
            _btypeRepository = btypeRepository;
            _entityRelationDataAccess = entityRelationDataAccess;
            _entityRelationDataFactory = entityRelationDataFactory;
            _initialEntityMatcher = initialEntityMatcher;
            _entitySyncStateChunkCreator = new EntitySyncStateChunkCreator<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(chunkSize);
        }


        public async Task Synchronize(ISynchronizationLogger logger, TContext synchronizationContext)
        {
            s_logger.InfoFormat("Entered. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'", _initialSyncStateCreationStrategy.GetType().Name, typeof(TAtypeEntity).Name, typeof(TBtypeEntity).Name);

            using (var totalProgress = _totalProgressFactory.Create())
            {
                var knownEntityRelations = _entityRelationDataAccess.LoadEntityRelationData()
                                           ?? new IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>[] { };

                var stateTokens = _stateTokenDataAccess.LoadKnownStateTokens();

                using (var interceptor = _synchronizationInterceptorFactory.Create())
                {
                    var aStatesTask = _atypeStateAwareEntityRepository.GetFullRepositoryState(knownEntityRelations.Select(r => r.AtypeId), stateTokens.AToken, synchronizationContext, logger.AGetVersionsEntityLogger);
                    var bStatesTask = _btypeStateAwareEntityRepository.GetFullRepositoryState(knownEntityRelations.Select(r => r.BtypeId), stateTokens.BToken, synchronizationContext, logger.BGetVersionsEntityLogger);

                    (var aStates, var newAToken) = await aStatesTask;
                    (var bStates, var newBToken) = await bStatesTask;

                    await Synchronize(
                        totalProgress,
                        knownEntityRelations,
                        aStates,
                        bStates,
                        logger,
                        synchronizationContext,
                        interceptor,
                        newEntityRelations => _entityRelationDataAccess.SaveEntityRelationData(newEntityRelations));

                    _stateTokenDataAccess.SaveKnownStateTokens(newAToken, newBToken);
                }
            }

            s_logger.DebugFormat("Exiting.");
        }

        public async Task<bool> SynchronizePartial(
            IEnumerable<IIdWithHints<TAtypeEntityId, TAtypeEntityVersion>> aIds,
            IEnumerable<IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>> bIds,
            ISynchronizationLogger logger,
            Func<Task<TContext>> contextFactoryAsync,
            Func<TContext, Task> syncronizationFinishedAsync)
        {
            s_logger.InfoFormat(
                "Entered partial. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'",
                _initialSyncStateCreationStrategy.GetType().Name,
                typeof(TAtypeEntity).Name,
                typeof(TBtypeEntity).Name);

            var knownEntityRelations = _entityRelationDataAccess.LoadEntityRelationData();

            if (knownEntityRelations == null)
            {
                var synchronizationContext = await contextFactoryAsync();
                await Synchronize(logger, synchronizationContext);
                await syncronizationFinishedAsync(synchronizationContext);
                return true;
            }

            var requestedAIdsById = aIds.ToDictionary(e => e.Id, _atypeIdComparer);
            var requestedBIdsById = bIds.ToDictionary(e => e.Id, _btypeIdComparer);

            var aIdsWithAwarenessLevel = new List<IdWithAwarenessLevel<TAtypeEntityId>>();
            var bIdsWithAwarenessLevel = new List<IdWithAwarenessLevel<TBtypeEntityId>>();

            using (var totalProgress = _totalProgressFactory.Create())
            {
                var entityRelationsToUse = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();
                var entityRelationsNotToUse = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

                foreach (var entityRelation in knownEntityRelations)
                {
                    var isACausingSync = RemoveAFromRequestedAndCheckIfCausesSync(requestedAIdsById, entityRelation);
                    var isBCausingSync = RemoveBFromRequestedAndCheckIfCausesSync(requestedBIdsById, entityRelation);

                    if (isACausingSync || isBCausingSync)
                    {
                        aIdsWithAwarenessLevel.Add(new IdWithAwarenessLevel<TAtypeEntityId>(entityRelation.AtypeId, true));
                        bIdsWithAwarenessLevel.Add(new IdWithAwarenessLevel<TBtypeEntityId>(entityRelation.BtypeId, true));

                        entityRelationsToUse.Add(entityRelation);
                    }
                    else
                    {
                        entityRelationsNotToUse.Add(entityRelation);
                    }
                }

                aIdsWithAwarenessLevel.AddRange(requestedAIdsById.Where(kv => !(kv.Value.WasDeletedHint ?? false)).Select(kv => new IdWithAwarenessLevel<TAtypeEntityId>(kv.Key, false)));
                bIdsWithAwarenessLevel.AddRange(requestedBIdsById.Where(kv => !(kv.Value.WasDeletedHint ?? false)).Select(kv => new IdWithAwarenessLevel<TBtypeEntityId>(kv.Key, false)));

                if (aIdsWithAwarenessLevel.Count == 0 && bIdsWithAwarenessLevel.Count == 0)
                {
                    s_logger.InfoFormat("Exiting partial since there is nothing to synchronize.");
                    return false;
                }

                using (var interceptor = _synchronizationInterceptorFactory.Create())
                {
                    var synchronizationContext = await contextFactoryAsync();

                    Task<IEnumerable<EntityVersion<TAtypeEntityId, TAtypeEntityVersion>>> aVersionsTask;
                    if (aIdsWithAwarenessLevel.Count > 0)
                        aVersionsTask = _atypeRepository.GetVersions(aIdsWithAwarenessLevel, synchronizationContext, logger.AGetVersionsEntityLogger);
                    else
                        aVersionsTask = Task.FromResult<IEnumerable<EntityVersion<TAtypeEntityId, TAtypeEntityVersion>>>(new EntityVersion<TAtypeEntityId, TAtypeEntityVersion>[] { });

                    Task<IEnumerable<EntityVersion<TBtypeEntityId, TBtypeEntityVersion>>> bVersionsTask;
                    if (bIdsWithAwarenessLevel.Count > 0)
                        bVersionsTask = _btypeRepository.GetVersions(bIdsWithAwarenessLevel, synchronizationContext, logger.BGetVersionsEntityLogger);
                    else
                        bVersionsTask = Task.FromResult<IEnumerable<EntityVersion<TBtypeEntityId, TBtypeEntityVersion>>>(new EntityVersion<TBtypeEntityId, TBtypeEntityVersion>[] { });

                    var aStates = VersionAwareEntityStateCollection<TAtypeEntityId, TAtypeEntityVersion>.Create(await aVersionsTask, _atypeIdComparer, _atypeVersionComparer);
                    var bStates = VersionAwareEntityStateCollection<TBtypeEntityId, TBtypeEntityVersion>.Create(await bVersionsTask, _btypeIdComparer, _btypeVersionComparer);

                    await Synchronize(
                        totalProgress,
                        entityRelationsToUse,
                        aStates,
                        bStates,
                        logger,
                        synchronizationContext,
                        interceptor,
                        newEntityRelations =>
                        {
                            entityRelationsNotToUse.AddRange(newEntityRelations);
                            _entityRelationDataAccess.SaveEntityRelationData(entityRelationsNotToUse);
                        });
                }
            }

            s_logger.DebugFormat("Exiting.");
            return true;
        }

        private bool RemoveAFromRequestedAndCheckIfCausesSync(
            Dictionary<TAtypeEntityId, IIdWithHints<TAtypeEntityId, TAtypeEntityVersion>> requestedAIdsById,
            IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelation)
        {
            IIdWithHints<TAtypeEntityId, TAtypeEntityVersion> aIdWithHints;
            bool isACausingSync;
            if (requestedAIdsById.TryGetValue(entityRelation.AtypeId, out aIdWithHints))
            {
                requestedAIdsById.Remove(entityRelation.AtypeId);
                isACausingSync =
                    (aIdWithHints.WasDeletedHint ?? false) ||
                    !aIdWithHints.IsVersionHintSpecified ||
                    !_atypeVersionComparer.Equals(entityRelation.AtypeVersion, aIdWithHints.VersionHint);
            }
            else
            {
                isACausingSync = false;
            }

            return isACausingSync;
        }

        private bool RemoveBFromRequestedAndCheckIfCausesSync(
            Dictionary<TBtypeEntityId, IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>> requestedBIdsById,
            IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelation)
        {
            IIdWithHints<TBtypeEntityId, TBtypeEntityVersion> bIdWithHints;
            bool isBCausingSync;
            if (requestedBIdsById.TryGetValue(entityRelation.BtypeId, out bIdWithHints))
            {
                requestedBIdsById.Remove(entityRelation.BtypeId);
                isBCausingSync =
                    (bIdWithHints.WasDeletedHint ?? false) ||
                    !bIdWithHints.IsVersionHintSpecified ||
                    !_btypeVersionComparer.Equals(entityRelation.BtypeVersion, bIdWithHints.VersionHint);
            }
            else
            {
                isBCausingSync = false;
            }

            return isBCausingSync;
        }

        private async Task Synchronize(
            ITotalProgressLogger totalProgress,
            IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> knownEntityRelations,
            IEntityStateCollection<TAtypeEntityId, TAtypeEntityVersion> aStates,
            IEntityStateCollection<TBtypeEntityId, TBtypeEntityVersion> bStates,
            ISynchronizationLogger logger,
            TContext synchronizationContext,
            ISynchronizationInterceptor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> interceptor,
            Action<List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>> saveNewRelations)
        {
            var entitySynchronizationLoggerFactory = new SynchronizationLoggerBoundEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>(logger, _fullEntitySynchronizationLoggerFactory);

            using (IEntityContainer<TAtypeEntityId, TAtypeEntity, TContext> aEntities = new EntityContainer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext>(_atypeRepository, _atypeIdComparer, _chunkSize, _chunkedExecutor))
            using (IEntityContainer<TBtypeEntityId, TBtypeEntity, TContext> bEntities = new EntityContainer<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(_btypeRepository, _btypeIdComparer, _chunkSize, _chunkedExecutor))
            {
                var entitySynchronizationContexts = await CreateEntitySyncStateContexts(aEntities, bEntities, knownEntityRelations, aStates, bStates, logger, synchronizationContext, interceptor);

                var totalAJobs = new JobCount();
                var totalBJobs = new JobCount();

                try
                {
                    var chunks = _entitySyncStateChunkCreator.CreateChunks(entitySynchronizationContexts, _atypeIdComparer, _btypeIdComparer).ToArray();


                    totalProgress.NotifyWork(chunks.Aggregate(0, (acc, c) => acc + c.AEntitesToLoad.Count + c.BEntitesToLoad.Count), chunks.Length);

                    foreach ((var aEntitesToLoad, var bEntitesToLoad, var currentBatch) in chunks)
                    {
                        var chunkLogger = totalProgress.StartChunk();

                        IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntitiesById;
                        using (chunkLogger.StartARepositoryLoad(aEntitesToLoad.Count))
                        {
                            aEntitiesById = await aEntities.GetEntities(aEntitesToLoad, logger.ALoadEntityLogger, synchronizationContext);
                        }

                        IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntitiesById;
                        using (chunkLogger.StartBRepositoryLoad(bEntitesToLoad.Count))
                        {
                            bEntitiesById = await bEntities.GetEntities(bEntitesToLoad, logger.BLoadEntityLogger, synchronizationContext);
                        }

                        currentBatch.ForEach(s => s.FetchRequiredEntities(aEntitiesById, bEntitiesById));
                        currentBatch.ForEach(s => s.Resolve());

                        // since resolve may change to an new state, required entities have to be fetched again.
                        // an state is allowed only to resolve to another state, if the following states requires equal or less entities!
                        currentBatch.ForEach(s => s.FetchRequiredEntities(aEntitiesById, bEntitiesById));

                        var aJobs = new JobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity>();
                        var bJobs = new JobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>();

                        currentBatch.ForEach(s => s.AddSyncronizationJob(aJobs, bJobs, entitySynchronizationLoggerFactory, synchronizationContext));

                        totalAJobs = totalAJobs.Add(aJobs.Count);
                        totalBJobs = totalBJobs.Add(bJobs.Count);

                        try
                        {
                            using (var progress = chunkLogger.StartProcessing(aJobs.TotalJobCount + bJobs.TotalJobCount))
                            {
                                await _atypeWriteRepository.PerformOperations(aJobs.CreateJobs, aJobs.UpdateJobs, aJobs.DeleteJobs, progress, synchronizationContext);
                                await _btypeWriteRepository.PerformOperations(bJobs.CreateJobs, bJobs.UpdateJobs, bJobs.DeleteJobs, progress, synchronizationContext);
                            }

                            currentBatch.ForEach(s => s.NotifyJobExecuted());
                        }
                        catch (Exception x)
                        {
                            if (_exceptionHandlingStrategy.DoesGracefullyAbortSynchronization(x))
                            {
                                entitySynchronizationContexts.ForEach(s => s.Abort());
                                SaveNewRelations(entitySynchronizationContexts, saveNewRelations);
                            }

                            throw;
                        }
                    }
                }
                finally
                {
                    s_logger.InfoFormat($"A repository jobs: {totalAJobs}");
                    s_logger.InfoFormat($"B repository jobs: {totalBJobs}");
                    logger.LogJobs(totalAJobs.ToString(), totalBJobs.ToString());
                }

                SaveNewRelations(entitySynchronizationContexts, saveNewRelations);
            }
        }

        private async Task<IReadOnlyList<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>>> CreateEntitySyncStateContexts(
            IEntityContainer<TAtypeEntityId, TAtypeEntity, TContext> aEntities,
            IEntityContainer<TBtypeEntityId, TBtypeEntity, TContext> bEntities,
            IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> knownEntityRelations,
            IEntityStateCollection<TAtypeEntityId, TAtypeEntityVersion> aStates,
            IEntityStateCollection<TBtypeEntityId, TBtypeEntityVersion> bStates,
            ISynchronizationLogger logger,
            TContext synchronizationContext,
            ISynchronizationInterceptor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> interceptor)
        {
            var entitySynchronizationContexts = new List<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>>();

            var aDeltaLogInfo = new VersionDeltaLoginInformation();
            var bDeltaLogInfo = new VersionDeltaLoginInformation();

            foreach (var knownEntityRelationData in knownEntityRelations)
            {
                (var aState, var aRepositoryVersion) = aStates.RemoveState(knownEntityRelationData.AtypeId, knownEntityRelationData.AtypeVersion);
                (var bState, var bRepositoryVersion) = bStates.RemoveState(knownEntityRelationData.BtypeId, knownEntityRelationData.BtypeVersion);

                entitySynchronizationContexts.Add(new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(
                    CreateInitialSyncState(knownEntityRelationData, aState, bState, aRepositoryVersion, bRepositoryVersion, aDeltaLogInfo, bDeltaLogInfo)));
            }

            var newAVersions = aStates.DisposeAndGetLeftovers();
            var newBVersions = bStates.DisposeAndGetLeftovers();

            await _atypeRepository.VerifyUnknownEntities(newAVersions, synchronizationContext);
            await _btypeRepository.VerifyUnknownEntities(newBVersions, synchronizationContext);

            if (newAVersions.Count > 0 && newBVersions.Count > 0)
            {
                s_logger.Info($"Performing entity matching with {newAVersions.Count} Atype and {newBVersions.Count} Btype entities.");

                var matchingEntites = _initialEntityMatcher.FindMatchingEntities(
                    _entityRelationDataFactory,
                    await aEntities.GetTransformedEntities(newAVersions.Keys, logger.ALoadEntityLogger, synchronizationContext, e => _aMatchDataFactory.CreateMatchData(e.Entity)),
                    await bEntities.GetTransformedEntities(newBVersions.Keys, logger.BLoadEntityLogger, synchronizationContext, e => _bMatchDataFactory.CreateMatchData(e.Entity)),
                    newAVersions,
                    newBVersions);

                foreach (var knownEntityRelationData in matchingEntites)
                {
                    newAVersions.Remove(knownEntityRelationData.AtypeId);
                    newBVersions.Remove(knownEntityRelationData.BtypeId);
                    var entitySyncState = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Unchanged(knownEntityRelationData);
                    entitySynchronizationContexts.Add(new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(entitySyncState));
                    aDeltaLogInfo.IncUnchanged();
                    bDeltaLogInfo.IncUnchanged();
                }

                s_logger.Info("Entity matching finished.");
            }

            foreach (var newA in newAVersions)
            {
                var syncState = _initialSyncStateCreationStrategy.CreateFor_Added_NotExisting(newA.Key, newA.Value);
                entitySynchronizationContexts.Add(new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(syncState));
            }

            foreach (var newB in newBVersions)
            {
                var syncState = _initialSyncStateCreationStrategy.CreateFor_NotExisting_Added(newB.Key, newB.Value);
                entitySynchronizationContexts.Add(new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(syncState));
            }

            interceptor.TransformInitialCreatedStates(entitySynchronizationContexts, _syncStateFactory);

            // all the leftovers in newAVersions and newBVersions must be the added ones 
            aDeltaLogInfo.IncAdded(newAVersions.Count);
            bDeltaLogInfo.IncAdded(newBVersions.Count);
            s_logger.InfoFormat("Atype delta: {0}", aDeltaLogInfo);
            s_logger.InfoFormat("Btype delta: {0}", bDeltaLogInfo);
            logger.LogDeltas(aDeltaLogInfo, bDeltaLogInfo);

            return entitySynchronizationContexts;
        }

        private void SaveNewRelations(
            IReadOnlyCollection<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>> syncStateContexts,
            Action<List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>> saveNewRelations)
        {
            var newEntityRelations = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();
            syncStateContexts.ForEach(s => s.AddNewRelationNoThrow(newEntityRelations.Add));
            syncStateContexts.ForEach(s => s.Dispose());
            saveNewRelations(newEntityRelations);
        }

        private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateInitialSyncState(
            IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownEntityRelation,
            EntityState aState,
            EntityState bState,
            TAtypeEntityVersion aRepositoryVersion,
            TBtypeEntityVersion bRepositoryVersion,
            VersionDeltaLoginInformation aLogInfo,
            VersionDeltaLoginInformation bLogInfo)
        {
            IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state;

            switch (aState)
            {
                case EntityState.ChangedOrAdded:
                    aLogInfo.IncChanged();
                    switch (bState)
                    {
                        case EntityState.ChangedOrAdded:
                            bLogInfo.IncChanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Changed(knownEntityRelation, aRepositoryVersion, bRepositoryVersion);
                            break;
                        case EntityState.Unchanged:
                            bLogInfo.IncUnchanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Unchanged(knownEntityRelation, aRepositoryVersion);
                            break;
                        case EntityState.Deleted:
                            bLogInfo.IncDeleted();
                            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Deleted(knownEntityRelation, aRepositoryVersion);
                            break;
                        default:
                            throw new NotImplementedException($"'{bState}' not implemented");
                    }

                    break;
                case EntityState.Unchanged:
                    aLogInfo.IncUnchanged();
                    switch (bState)
                    {
                        case EntityState.ChangedOrAdded:
                            bLogInfo.IncChanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Changed(knownEntityRelation, bRepositoryVersion);
                            break;
                        case EntityState.Unchanged:
                            bLogInfo.IncUnchanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Unchanged(knownEntityRelation);
                            break;
                        case EntityState.Deleted:
                            bLogInfo.IncDeleted();
                            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Deleted(knownEntityRelation);
                            break;
                        default:
                            throw new NotImplementedException($"'{bState}' not implemented");
                    }

                    break;
                case EntityState.Deleted:
                    aLogInfo.IncDeleted();
                    switch (bState)
                    {
                        case EntityState.ChangedOrAdded:
                            bLogInfo.IncChanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Changed(knownEntityRelation, bRepositoryVersion);
                            break;
                        case EntityState.Unchanged:
                            bLogInfo.IncUnchanged();
                            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Unchanged(knownEntityRelation);
                            break;
                        case EntityState.Deleted:
                            bLogInfo.IncDeleted();
                            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Deleted(knownEntityRelation);
                            break;
                        default:
                            throw new NotImplementedException($"'{bState}' not implemented");
                    }

                    break;
                default:
                    throw new NotImplementedException($"'{aState}' not implemented");
            }

            return state;
        }
    }
}