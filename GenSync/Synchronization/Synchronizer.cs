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
using GenSync.ProgressReport;
using GenSync.Synchronization.StateCreationStrategies;
using GenSync.Synchronization.States;
using log4net;

namespace GenSync.Synchronization
{
  /// <summary>
  /// Synchronizes tow repositories
  /// </summary>
  public class Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : IPartialSynchronizer<TAtypeEntityId, TBtypeEntityId>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private static readonly IEqualityComparer<TAtypeEntityVersion> _atypeVersionComparer = EqualityComparer<TAtypeEntityVersion>.Default;
    private static readonly IEqualityComparer<TBtypeEntityVersion> _btypeVersionComparer = EqualityComparer<TBtypeEntityVersion>.Default;

    private readonly IEqualityComparer<TAtypeEntityId> _atypeIdComparer;
    private readonly IEqualityComparer<TBtypeEntityId> _btypeIdComparer;

    private readonly IInitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _initialEntityMatcher;
    private readonly IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataFactory;
    private readonly IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> _entityRelationDataAccess;
    private readonly IEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> _atypeRepository;
    private readonly IEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _btypeRepository;
    private readonly IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _initialSyncStateCreationStrategy;
    private readonly ITotalProgressFactory _totalProgressFactory;
    private readonly IExceptionLogger _exceptionLogger;


    public Synchronizer (
        IEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeRepository,
        IEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeRepository,
        IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> initialSyncStateCreationStrategy,
        IEntityRelationDataAccess<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataAccess,
        IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> entityRelationDataFactory,
        IInitialEntityMatcher<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> initialEntityMatcher,
        IEqualityComparer<TAtypeEntityId> atypeIdComparer, IEqualityComparer<TBtypeEntityId> btypeIdComparer,
        ITotalProgressFactory totalProgressFactory,
        IExceptionLogger exceptionLogger)
    {
      _initialSyncStateCreationStrategy = initialSyncStateCreationStrategy;
      _totalProgressFactory = totalProgressFactory;
      _atypeIdComparer = atypeIdComparer;
      _btypeIdComparer = btypeIdComparer;
      _exceptionLogger = exceptionLogger;
      _atypeRepository = atypeRepository;
      _btypeRepository = btypeRepository;
      _entityRelationDataAccess = entityRelationDataAccess;
      _entityRelationDataFactory = entityRelationDataFactory;
      _initialEntityMatcher = initialEntityMatcher;
    }


    public async Task Synchronize ()
    {
      s_logger.InfoFormat ("Entered. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'", _initialSyncStateCreationStrategy.GetType().Name, typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);

      try
      {
        using (var totalProgress = _totalProgressFactory.Create())
        {
          var knownEntityRelationsOrNull = _entityRelationDataAccess.LoadEntityRelationData();

          var knownEntityRelations = knownEntityRelationsOrNull ?? new IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>[] { };

          var newAVersionsTask = _atypeRepository.GetAllVersions(knownEntityRelations.Select(r => r.AtypeId));
          var newBVersionsTask = _btypeRepository.GetAllVersions (knownEntityRelations.Select (r => r.BtypeId));

          var newAVersions = CreateDictionary (
              await newAVersionsTask,
              _atypeIdComparer);

          var newBVersions = CreateDictionary (
              await newBVersionsTask,
              _btypeIdComparer);

          using (var entityContainer = new EntityContainer (this, totalProgress))
          {
            if (knownEntityRelationsOrNull == null)
            {
              s_logger.Info ("Did not find entity caches. Performing initial population");

              await entityContainer.FillIfEmpty (newAVersions.Keys, newBVersions.Keys);

              knownEntityRelationsOrNull = _initialEntityMatcher.FindMatchingEntities (
                  _entityRelationDataFactory,
                  entityContainer.AEntities,
                  entityContainer.BEntities,
                  newAVersions,
                  newBVersions);
            }

            var newEntityRelations = await Synchronize (
                totalProgress,
                knownEntityRelationsOrNull,
                newAVersions,
                newBVersions,
                entityContainer);

            _entityRelationDataAccess.SaveEntityRelationData (newEntityRelations);
          }
        }
      }
      catch (Exception x)
      {
        _exceptionLogger.LogException (x, s_logger);
      }

      s_logger.DebugFormat ("Exiting.");
    }

    public async Task SynchronizePartial (IEnumerable<TAtypeEntityId> aEntityIds, IEnumerable<TBtypeEntityId> bEntityIds)
    {
      s_logger.InfoFormat ("Entered. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'", _initialSyncStateCreationStrategy.GetType().Name, typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);

      try
      {
        var knownEntityRelations = _entityRelationDataAccess.LoadEntityRelationData();

        if (knownEntityRelations == null)
        {
          await Synchronize();
          return;
        }

        var aEntitesToSynchronize = new HashSet<TAtypeEntityId> (aEntityIds, _atypeIdComparer);
        var bEntitesToSynchronize = new HashSet<TBtypeEntityId> (bEntityIds, _btypeIdComparer);

        using (var totalProgress = _totalProgressFactory.Create())
        {
          var entityRelationsToUse = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();
          var entityRelationsNotToUse = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

          foreach (var entityRelation in knownEntityRelations)
          {
            if (aEntitesToSynchronize.Contains (entityRelation.AtypeId))
            {
              bEntitesToSynchronize.Add (entityRelation.BtypeId);
              entityRelationsToUse.Add (entityRelation);
            }
            else if (bEntitesToSynchronize.Contains (entityRelation.BtypeId))
            {
              aEntitesToSynchronize.Add (entityRelation.AtypeId);
              entityRelationsToUse.Add (entityRelation);
            }
            else
            {
              entityRelationsNotToUse.Add (entityRelation);
            }
          }

          var newAVersionsTask = _atypeRepository.GetVersions (aEntitesToSynchronize);
          var newBVersionsTask = _btypeRepository.GetVersions (bEntitesToSynchronize);

          var newAVersions = CreateDictionary (
              await newAVersionsTask,
              _atypeIdComparer);

          var newBVersions = CreateDictionary (
              await newBVersionsTask,
              _btypeIdComparer);

          using (var entityContainer = new EntityContainer (this, totalProgress))
          {
            var newEntityRelations = await Synchronize (
                totalProgress,
                entityRelationsToUse,
                newAVersions,
                newBVersions,
                entityContainer);

            entityRelationsNotToUse.AddRange (newEntityRelations);

            _entityRelationDataAccess.SaveEntityRelationData (entityRelationsNotToUse);
          }
        }
      }
      catch (Exception x)
      {
        _exceptionLogger.LogException (x, s_logger);
      }

      s_logger.DebugFormat ("Exiting.");
    }

    private async Task<List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>> Synchronize (
        ITotalProgressLogger totalProgress,
        IEnumerable<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> knownEntityRelations,
        Dictionary<TAtypeEntityId, TAtypeEntityVersion> newAVersions,
        Dictionary<TBtypeEntityId, TBtypeEntityVersion> newBVersions,
        EntityContainer entityContainer)
    {
      var entitySyncStates = new EntitySyncStateContainer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>();

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

        var entitySyncState = CreateInitialSyncState (knownEntityRelationData, newAVersionAvailable, newAVersion, newBVersionAvailable, newBVersion, aDeltaLogInfo, bDeltaLogInfo);

        entitySyncStates.Add (entitySyncState);
      }

      aDeltaLogInfo.IncAdded (newAVersions.Count);
      bDeltaLogInfo.IncAdded (newBVersions.Count);

      s_logger.InfoFormat ("Atype delta: {0}", aDeltaLogInfo);
      s_logger.InfoFormat ("Btype delta: {0}", bDeltaLogInfo);

      foreach (var newA in newAVersions)
        entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_Added_NotExisting (newA.Key, newA.Value));

      foreach (var newB in newBVersions)
        entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_NotExisting_Added (newB.Key, newB.Value));

      HashSet<TAtypeEntityId> aEntitesToLoad = new HashSet<TAtypeEntityId>();
      HashSet<TBtypeEntityId> bEntitesToLoad = new HashSet<TBtypeEntityId>();

      entitySyncStates.Execute (s => s.AddRequiredEntitiesToLoad (aEntitesToLoad.Add, bEntitesToLoad.Add));

      await entityContainer.FillIfEmpty (aEntitesToLoad, bEntitesToLoad);

      entitySyncStates.DoTransition (s => s.FetchRequiredEntities (entityContainer.AEntities, entityContainer.BEntities));
      entitySyncStates.DoTransition (s => s.Resolve());

      // since resolve may change to an new state, required entities have to be fetched again.
      // an state is allowed only to resolve to another state, if the following states requires equal or less entities!
      entitySyncStates.DoTransition (s => s.FetchRequiredEntities (entityContainer.AEntities, entityContainer.BEntities));

      using (var progress = totalProgress.StartProcessing (entitySyncStates.Count))
      {
        await entitySyncStates.DoTransition (
            async s =>
            {
              var nextState = await s.PerformSyncActionNoThrow();
              progress.Increase();
              return nextState;
            });
      }

      var newEntityRelations = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

      entitySyncStates.Execute (s => s.AddNewRelationNoThrow (newEntityRelations.Add));

      entitySyncStates.Dispose();


      return newEntityRelations;
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

    private static Dictionary<TKey, TValue> CreateDictionary<TKey, TValue> (IReadOnlyList<EntityWithId<TKey, TValue>> tuples, IEqualityComparer<TKey> equalityComparer)
    {
      var dictionary = new Dictionary<TKey, TValue> (equalityComparer);

      foreach (var tuple in tuples)
      {
        if (!dictionary.ContainsKey (tuple.Id))
          dictionary.Add (tuple.Id, tuple.Entity);
        else
          s_logger.WarnFormat ("Entitiy '{0}' was contained multiple times in server response. Ignoring redundant entity", tuple.Id);
      }

      return dictionary;
    }

    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateInitialSyncState (
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownEntityRelation,
        bool newAVersionAvailable,
        TAtypeEntityVersion newAVersion,
        bool newBVersionAvailable,
        TBtypeEntityVersion newBVersion,
        VersionDeltaLoginInformation aLogInfo,
        VersionDeltaLoginInformation bLogInfo)
    {
      IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> state;
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
      private Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _context;

      private readonly ITotalProgressLogger _totalProgress;
      private Dictionary<TAtypeEntityId, TAtypeEntity> _aEntities;
      private Dictionary<TBtypeEntityId, TBtypeEntity> _bEntities;

      public EntityContainer (
          Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> context,
          ITotalProgressLogger totalProgress)
      {
        _context = context;
        _totalProgress = totalProgress;
      }

      public IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> AEntities
      {
        get { return _aEntities; }
      }

      public IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> BEntities
      {
        get { return _bEntities; }
      }

      public async Task FillIfEmpty (ICollection<TAtypeEntityId> aEntitiesToLoad, ICollection<TBtypeEntityId> bEntitiesToLoad)
      {
        if (_aEntities == null)
        {
          _totalProgress.NotifyLoadCount (aEntitiesToLoad.Count, bEntitiesToLoad.Count);
          using (_totalProgress.StartARepositoryLoad())
          {
            _aEntities = CreateDictionary (
                await _context._atypeRepository.Get (aEntitiesToLoad),
                _context._atypeIdComparer);
          }

          using (_totalProgress.StartBRepositoryLoad())
          {
            _bEntities = CreateDictionary (
                await _context._btypeRepository.Get (bEntitiesToLoad),
                _context._btypeIdComparer);
          }
        }
      }

      public void Dispose ()
      {
        if (_context != null)
        {
          if (_aEntities != null)
          {
            _context._atypeRepository.Cleanup (_aEntities);
            _aEntities = null;
          }
          if (_bEntities != null)
          {
            _context._btypeRepository.Cleanup (_bEntities);
            _bEntities = null;
          }
          _context = null;
        }
      }
    }
  }
}