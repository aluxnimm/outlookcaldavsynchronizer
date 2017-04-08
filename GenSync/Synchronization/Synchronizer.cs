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
using log4net;

namespace GenSync.Synchronization
{
  /// <summary>
  /// Synchronizes tow repositories
  /// </summary>
  public class Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
     :  IPartialSynchronizer<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion, TContext>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IEqualityComparer<TAtypeEntityVersion> _atypeVersionComparer;
    private readonly IEqualityComparer<TBtypeEntityVersion> _btypeVersionComparer;
    private readonly ISynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _synchronizationInterceptorFactory;

    private readonly IEqualityComparer<TAtypeEntityId> _atypeIdComparer;
    private readonly IEqualityComparer<TBtypeEntityId> _btypeIdComparer;

    private readonly IInitialEntityMatcher<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _initialEntityMatcher;
    private readonly IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataFactory;
    private readonly IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataAccess;
    private readonly IReadOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> _atypeRepository;
    private readonly IReadOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _btypeRepository;
    private readonly IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _initialSyncStateCreationStrategy;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly IBatchWriteOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> _atypeWriteRepository;
    private readonly IBatchWriteOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _btypeWriteRepository;
    private readonly IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _syncStateFactory;

    private readonly IExceptionHandlingStrategy _exceptionHandlingStrategy;


    public Synchronizer (
        IReadOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> atypeRepository,
        IReadOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> btypeRepository,
        IBatchWriteOnlyEntityRepository<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TContext> atypeWriteRepository,
        IBatchWriteOnlyEntityRepository<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> btypeWriteRepository,
        IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> initialSyncStateCreationStrategy,
        IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataAccess,
        IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataFactory,
        IInitialEntityMatcher<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> initialEntityMatcher,
        IEqualityComparer<TAtypeEntityId> atypeIdComparer, IEqualityComparer<TBtypeEntityId> btypeIdComparer,
        ITotalProgressFactory totalProgressFactory,
        IEqualityComparer<TAtypeEntityVersion> atypeVersionComparer, 
        IEqualityComparer<TBtypeEntityVersion> btypeVersionComparer, IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> syncStateFactory,
        IExceptionHandlingStrategy exceptionHandlingStrategy,
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

      _initialSyncStateCreationStrategy = initialSyncStateCreationStrategy;
      _totalProgressFactory = totalProgressFactory;
      _atypeIdComparer = atypeIdComparer;
      _btypeIdComparer = btypeIdComparer;
      _atypeVersionComparer = atypeVersionComparer;
      _btypeVersionComparer = btypeVersionComparer;
      _syncStateFactory = syncStateFactory;
      _exceptionHandlingStrategy = exceptionHandlingStrategy;
      _synchronizationInterceptorFactory = synchronizationInterceptorFactoryOrNull ?? NullSynchronizationInterceptorFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>.Instance;
      _atypeWriteRepository = atypeWriteRepository;
      _btypeWriteRepository = btypeWriteRepository;
      _atypeRepository = atypeRepository;
      _btypeRepository = btypeRepository;
      _entityRelationDataAccess = entityRelationDataAccess;
      _entityRelationDataFactory = entityRelationDataFactory;
      _initialEntityMatcher = initialEntityMatcher;
    }
    

    public async Task Synchronize(ISynchronizationLogger logger, TContext synchronizationContext)
    {
      s_logger.InfoFormat("Entered. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'", _initialSyncStateCreationStrategy.GetType().Name, typeof(TAtypeEntity).Name, typeof(TBtypeEntity).Name);

      using (var totalProgress = _totalProgressFactory.Create())
      {
        var knownEntityRelations = _entityRelationDataAccess.LoadEntityRelationData()
                                   ?? new IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>[] {};

        using (var interceptor = _synchronizationInterceptorFactory.Create())
        {

          var newAVersionsTask = _atypeRepository.GetAllVersions(knownEntityRelations.Select(r => r.AtypeId), synchronizationContext);
          var newBVersionsTask = _btypeRepository.GetAllVersions(knownEntityRelations.Select(r => r.BtypeId), synchronizationContext);

          var newAVersions = CreateDictionary(
            await newAVersionsTask,
            _atypeIdComparer);

          var newBVersions = CreateDictionary(
            await newBVersionsTask,
            _btypeIdComparer);

          using (var entityContainer = new EntityContainer(this, totalProgress, logger.ALoadEntityLogger, logger.BLoadEntityLogger))
          {
            await Synchronize(
              totalProgress,
              knownEntityRelations,
              newAVersions,
              newBVersions,
              entityContainer,
              logger,
              synchronizationContext,
              interceptor,
              newEntityRelations => _entityRelationDataAccess.SaveEntityRelationData(newEntityRelations));
          }
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

          Task<IReadOnlyList<EntityVersion<TAtypeEntityId, TAtypeEntityVersion>>> newAVersionsTask;
          if (aIdsWithAwarenessLevel.Count > 0)
            newAVersionsTask = _atypeRepository.GetVersions(aIdsWithAwarenessLevel, synchronizationContext);
          else
            newAVersionsTask = Task.FromResult<IReadOnlyList<EntityVersion<TAtypeEntityId, TAtypeEntityVersion>>>(new EntityVersion<TAtypeEntityId, TAtypeEntityVersion>[] {});

          Task<IReadOnlyList<EntityVersion<TBtypeEntityId, TBtypeEntityVersion>>> newBVersionsTask;
          if (bIdsWithAwarenessLevel.Count > 0)
            newBVersionsTask = _btypeRepository.GetVersions(bIdsWithAwarenessLevel, synchronizationContext);
          else
            newBVersionsTask = Task.FromResult<IReadOnlyList<EntityVersion<TBtypeEntityId, TBtypeEntityVersion>>>(new EntityVersion<TBtypeEntityId, TBtypeEntityVersion>[] {});

          var newAVersions = CreateDictionary(
            await newAVersionsTask,
            _atypeIdComparer);

          var newBVersions = CreateDictionary(
            await newBVersionsTask,
            _btypeIdComparer);

          using (var entityContainer = new EntityContainer(this, totalProgress, logger.ALoadEntityLogger, logger.BLoadEntityLogger))
          {
            await Synchronize(
              totalProgress,
              entityRelationsToUse,
              newAVersions,
              newBVersions,
              entityContainer,
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
      }

      s_logger.DebugFormat("Exiting.");
      return true;
    }

    private bool RemoveAFromRequestedAndCheckIfCausesSync (
        Dictionary<TAtypeEntityId, IIdWithHints<TAtypeEntityId, TAtypeEntityVersion>> requestedAIdsById,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelation)
    {
      IIdWithHints<TAtypeEntityId, TAtypeEntityVersion> aIdWithHints;
      bool isACausingSync;
      if (requestedAIdsById.TryGetValue (entityRelation.AtypeId, out aIdWithHints))
      {
        requestedAIdsById.Remove (entityRelation.AtypeId);
        isACausingSync =
            (aIdWithHints.WasDeletedHint ?? false) ||
            !aIdWithHints.IsVersionHintSpecified ||
            !_atypeVersionComparer.Equals (entityRelation.AtypeVersion, aIdWithHints.VersionHint);
      }
      else
      {
        isACausingSync = false;
      }
      return isACausingSync;
    }

    private bool RemoveBFromRequestedAndCheckIfCausesSync (
        Dictionary<TBtypeEntityId, IIdWithHints<TBtypeEntityId, TBtypeEntityVersion>> requestedBIdsById,
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelation)
    {
      IIdWithHints<TBtypeEntityId, TBtypeEntityVersion> bIdWithHints;
      bool isBCausingSync;
      if (requestedBIdsById.TryGetValue (entityRelation.BtypeId, out bIdWithHints))
      {
        requestedBIdsById.Remove (entityRelation.BtypeId);
        isBCausingSync =
            (bIdWithHints.WasDeletedHint ?? false) ||
            !bIdWithHints.IsVersionHintSpecified ||
            !_btypeVersionComparer.Equals (entityRelation.BtypeVersion, bIdWithHints.VersionHint);
      }
      else
      {
        isBCausingSync = false;
      }
      return isBCausingSync;
    }

    private async Task Synchronize (
        ITotalProgressLogger totalProgress,
        IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> knownEntityRelations,
        Dictionary<TAtypeEntityId, TAtypeEntityVersion> newAVersions,
        Dictionary<TBtypeEntityId, TBtypeEntityVersion> newBVersions,
        EntityContainer entityContainer,
        ISynchronizationLogger logger,
        TContext synchronizationContext,
        ISynchronizationInterceptor<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> interceptor,
        Action<List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>> saveNewRelations)
    {
      var entitySynchronizationContexts = new List<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>>();

      var aDeltaLogInfo = new VersionDeltaLoginInformation();
      var bDeltaLogInfo = new VersionDeltaLoginInformation();

      foreach (var knownEntityRelationData in knownEntityRelations)
      {
        TAtypeEntityVersion newAVersion;
        TBtypeEntityVersion newBVersion;

        var newAVersionAvailable = newAVersions.TryGetValue (knownEntityRelationData.AtypeId, out newAVersion);
        var newBVersionAvailable = newBVersions.TryGetValue (knownEntityRelationData.BtypeId, out newBVersion);

        if (newAVersionAvailable)
          newAVersions.Remove (knownEntityRelationData.AtypeId);

        if (newBVersionAvailable)
          newBVersions.Remove (knownEntityRelationData.BtypeId);

        entitySynchronizationContexts.Add(new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>(
          CreateInitialSyncState(knownEntityRelationData, newAVersionAvailable, newAVersion, newBVersionAvailable, newBVersion, aDeltaLogInfo, bDeltaLogInfo)));
      }

      HashSet<TAtypeEntityId> aEntitesToLoad = new HashSet<TAtypeEntityId>();
      HashSet<TBtypeEntityId> bEntitesToLoad = new HashSet<TBtypeEntityId>();
      entitySynchronizationContexts.ForEach (s => s.AddRequiredEntitiesToLoad (aEntitesToLoad.Add, bEntitesToLoad.Add));

      await _atypeRepository.VerifyUnknownEntities (newAVersions, synchronizationContext);
      await _btypeRepository.VerifyUnknownEntities (newBVersions, synchronizationContext);

      if (newAVersions.Count > 0 && newBVersions.Count > 0)
      {
        foreach (var newA in newAVersions)
          aEntitesToLoad.Add (newA.Key);

        foreach (var newB in newBVersions)
          bEntitesToLoad.Add (newB.Key);

        await entityContainer.EnsureEntitiesLoaded(aEntitesToLoad, bEntitesToLoad, synchronizationContext);

        var newAtypeEntities = GetSubSet(entityContainer.AEntities, newAVersions.Keys, _atypeIdComparer);
        var newBtypeEntities = GetSubSet(entityContainer.BEntities, newBVersions.Keys, _btypeIdComparer);

        var matchingEntites = _initialEntityMatcher.FindMatchingEntities(
          _entityRelationDataFactory,
          newAtypeEntities,
          newBtypeEntities,
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
      }

      foreach (var newA in newAVersions)
      {
        var syncState = _initialSyncStateCreationStrategy.CreateFor_Added_NotExisting(newA.Key, newA.Value);
        entitySynchronizationContexts.Add (new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> (syncState));
      }

      foreach (var newB in newBVersions)
      {
        var syncState = _initialSyncStateCreationStrategy.CreateFor_NotExisting_Added(newB.Key, newB.Value);
        entitySynchronizationContexts.Add (new EntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> (syncState));
      }

      interceptor.TransformInitialCreatedStates(entitySynchronizationContexts, _syncStateFactory);

      entitySynchronizationContexts.ForEach (s => s.AddRequiredEntitiesToLoad (aEntitesToLoad.Add, bEntitesToLoad.Add));

      await entityContainer.EnsureEntitiesLoaded (aEntitesToLoad, bEntitesToLoad, synchronizationContext);


      // all the leftovers in newAVersions and newBVersions must be the added ones 
      aDeltaLogInfo.IncAdded(newAVersions.Count);
      bDeltaLogInfo.IncAdded(newBVersions.Count);
      s_logger.InfoFormat("Atype delta: {0}", aDeltaLogInfo);
      s_logger.InfoFormat("Btype delta: {0}", bDeltaLogInfo);
      logger.LogDeltas(aDeltaLogInfo, bDeltaLogInfo);

      entitySynchronizationContexts.ForEach (s => s.FetchRequiredEntities(entityContainer.AEntities, entityContainer.BEntities));
      entitySynchronizationContexts.ForEach (s => s.Resolve());

      // since resolve may change to an new state, required entities have to be fetched again.
      // an state is allowed only to resolve to another state, if the following states requires equal or less entities!
      entitySynchronizationContexts.ForEach (s => s.FetchRequiredEntities (entityContainer.AEntities, entityContainer.BEntities));

      var aJobs = new JobList<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity> ();
      var bJobs = new JobList<TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> ();

      entitySynchronizationContexts.ForEach (s => s.AddSyncronizationJob (aJobs, bJobs, logger.CreateEntitySynchronizationLogger(), synchronizationContext));

      s_logger.InfoFormat($"A repository jobs: {aJobs}");
      s_logger.InfoFormat($"B repository jobs: {bJobs}");
      logger.LogJobs(aJobs.ToString(), bJobs.ToString());

      try
      {
        using (var progress = totalProgress.StartProcessing(aJobs.TotalJobCount + bJobs.TotalJobCount))
        {
          await _atypeWriteRepository.PerformOperations(aJobs.CreateJobs, aJobs.UpdateJobs, aJobs.DeleteJobs, progress, synchronizationContext);
          await _btypeWriteRepository.PerformOperations(bJobs.CreateJobs, bJobs.UpdateJobs, bJobs.DeleteJobs, progress, synchronizationContext);
        }

        entitySynchronizationContexts.ForEach (s => s.NotifyJobExecuted());
      }
      catch (Exception x)
      {
        if (_exceptionHandlingStrategy.DoesAbortSynchronization(x))
        {
          entitySynchronizationContexts.ForEach(s => s.Abort());
          SaveNewRelations(entitySynchronizationContexts, saveNewRelations);
        }
        throw;
      }

      SaveNewRelations(entitySynchronizationContexts, saveNewRelations);
    }

    private void SaveNewRelations(
      List<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>> syncStateContexts,
      Action<List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>> saveNewRelations)
    {
      var newEntityRelations = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> ();
      syncStateContexts.ForEach (s => s.AddNewRelationNoThrow (newEntityRelations.Add));
      syncStateContexts.ForEach (s => s.Dispose ());
      saveNewRelations(newEntityRelations);
    }

    private static Dictionary<TId, TEntity> GetSubSet<TId, TEntity> (IReadOnlyDictionary<TId, TEntity> set, IEnumerable<TId> subSetIds, IEqualityComparer<TId> idComparer)
    {
      var subSet = new Dictionary<TId, TEntity> (idComparer);
      foreach (var id in subSetIds)
      {
        TEntity entity;
        if (set.TryGetValue (id, out entity))
          subSet.Add (id, entity);
      }
      return subSet;
    }

    private static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue> (IReadOnlyList<EntityVersion<TKey, TValue>> tuples, IEqualityComparer<TKey> equalityComparer)
    {
      var dictionary = new Dictionary<TKey, TValue> (equalityComparer);

      foreach (var tuple in tuples)
      {
        if (!dictionary.ContainsKey (tuple.Id))
          dictionary.Add (tuple.Id, tuple.Version);
        else
          s_logger.WarnFormat ("EntitiyVersion '{0}' was contained multiple times in server response. Ignoring redundant entity", tuple.Id);
      }

      return dictionary;
    }

    private static void AddToDictionary<TKey, TValue> (Dictionary<TKey, TValue> dictionary, IReadOnlyList<EntityWithId<TKey, TValue>> tuples)
    {
      foreach (var tuple in tuples)
      {
        if (!dictionary.ContainsKey (tuple.Id))
          dictionary.Add (tuple.Id, tuple.Entity);
        else
          s_logger.WarnFormat ("Entitiy '{0}' was contained multiple times in server response. Ignoring redundant entity", tuple.Id);
      }
    }

    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> CreateInitialSyncState (
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownEntityRelation,
        bool newAVersionAvailable,
        TAtypeEntityVersion newAVersion,
        bool newBVersionAvailable,
        TBtypeEntityVersion newBVersion,
        VersionDeltaLoginInformation aLogInfo,
        VersionDeltaLoginInformation bLogInfo)
    {
      IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> state;
      if (newAVersionAvailable)
      {
        var aChanged = !_atypeVersionComparer.Equals (newAVersion, knownEntityRelation.AtypeVersion);
        if (aChanged)
        {
          aLogInfo.IncChanged();
          if (newBVersionAvailable)
          {
            var bChanged = !_btypeVersionComparer.Equals (newBVersion, knownEntityRelation.BtypeVersion);
            if (bChanged)
            {
              bLogInfo.IncChanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Changed (knownEntityRelation, newAVersion, newBVersion);
            }
            else
            {
              bLogInfo.IncUnchanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Unchanged (knownEntityRelation, newAVersion);
            }
          }
          else
          {
            bLogInfo.IncDeleted();
            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Deleted (knownEntityRelation, newAVersion);
          }
        }
        else
        {
          aLogInfo.IncUnchanged();
          if (newBVersionAvailable)
          {
            var bChanged = !_btypeVersionComparer.Equals (newBVersion, knownEntityRelation.BtypeVersion);
            if (bChanged)
            {
              bLogInfo.IncChanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Changed (knownEntityRelation, newBVersion);
            }
            else
            {
              bLogInfo.IncUnchanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Unchanged (knownEntityRelation);
            }
          }
          else
          {
            bLogInfo.IncDeleted();
            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Deleted (knownEntityRelation);
          }
        }
      }
      else
      {
        aLogInfo.IncDeleted();
        if (newBVersionAvailable)
        {
          var bChanged = !_btypeVersionComparer.Equals (newBVersion, knownEntityRelation.BtypeVersion);
          if (bChanged)
          {
            bLogInfo.IncChanged();
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Changed (knownEntityRelation, newBVersion);
          }
          else
          {
            bLogInfo.IncUnchanged();
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Unchanged (knownEntityRelation);
          }
        }
        else
        {
          bLogInfo.IncDeleted();
          state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Deleted (knownEntityRelation);
        }
      }
      return state;
    }

    private class EntityContainer : IDisposable
    {
      private Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> _context;

      private readonly ITotalProgressLogger _totalProgress;
      private readonly Dictionary<TAtypeEntityId, TAtypeEntity> _loadedAEntities;
      private readonly Dictionary<TBtypeEntityId, TBtypeEntity> _loadedBEntities;
      private readonly ILoadEntityLogger _aLogger;
      private readonly ILoadEntityLogger _bLogger;

      public EntityContainer (
          Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext> context,
          ITotalProgressLogger totalProgress,
          ILoadEntityLogger aLogger,
          ILoadEntityLogger bLogger)
      {
        _context = context;
        _totalProgress = totalProgress;
        _aLogger = aLogger;
        _bLogger = bLogger;

        _loadedAEntities = new Dictionary<TAtypeEntityId, TAtypeEntity>(context._atypeIdComparer);
        _loadedBEntities = new Dictionary<TBtypeEntityId, TBtypeEntity>(context._btypeIdComparer);
      }

      public IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> AEntities
      {
        get { return _loadedAEntities; }
      }

      public IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> BEntities
      {
        get { return _loadedBEntities; }
      }

      public async Task EnsureEntitiesLoaded (IEnumerable<TAtypeEntityId> aEntities, IEnumerable<TBtypeEntityId> bEntities, TContext context)
      {
        var aEntitiesToLoad = aEntities.Except(_loadedAEntities.Keys, _context._atypeIdComparer).ToArray();
        var bEntitiesToLoad = bEntities.Except(_loadedBEntities.Keys, _context._btypeIdComparer).ToArray();

        if (aEntitiesToLoad.Any() || bEntitiesToLoad.Any())
        {
          _totalProgress.NotifyLoadCount(aEntitiesToLoad.Length, bEntitiesToLoad.Length);
          using (_totalProgress.StartARepositoryLoad())
          {
            AddToDictionary(_loadedAEntities, await _context._atypeRepository.Get(aEntitiesToLoad, _aLogger, context));
          }

          using (_totalProgress.StartBRepositoryLoad())
          {
            AddToDictionary(_loadedBEntities, await _context._btypeRepository.Get(bEntitiesToLoad, _bLogger, context));
          }
        }
      }

      public void Dispose ()
      {
        if (_context != null)
        {
          if (_loadedAEntities.Any())
          {
            _context._atypeRepository.Cleanup (_loadedAEntities);
            _loadedAEntities.Clear();
          }
          if (_loadedBEntities.Any())
          {
            _context._btypeRepository.Cleanup (_loadedBEntities);
            _loadedBEntities.Clear();
          }
          _context = null;
        }
      }
    }
  }
}