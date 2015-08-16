// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
      : ISynchronizer
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


    public async Task<bool> Synchronize ()
    {
      s_logger.InfoFormat ("Entered. Syncstrategy '{0}' with Atype='{1}' and Btype='{2}'", _initialSyncStateCreationStrategy.GetType().Name, typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);


      try
      {
        using (var totalProgress = _totalProgressFactory.Create())
        {
          var cachedData = _entityRelationDataAccess.LoadEntityRelationData ();

          var atypeRepositoryVersions = CreateDictionary (
              _atypeRepository.GetVersions(),
              _atypeIdComparer);

          var btypeRepositoryVersions = CreateDictionary (
              _btypeRepository.GetVersions(),
              _btypeIdComparer);

          IReadOnlyDictionary<TAtypeEntityId, TAtypeEntity> aEntities = null;
          IReadOnlyDictionary<TBtypeEntityId, TBtypeEntity> bEntities = null;

          try
          {
            if (cachedData == null)
            {
              s_logger.Info ("Did not find entity caches. Performing initial population");

              totalProgress.NotifyLoadCount (atypeRepositoryVersions.Count, btypeRepositoryVersions.Count);

              using (totalProgress.StartARepositoryLoad())
              {
                aEntities = CreateDictionary (
                    await _atypeRepository.Get (atypeRepositoryVersions.Keys),
                    _atypeIdComparer);
              }

              using (totalProgress.StartBRepositoryLoad())
              {
                bEntities = CreateDictionary (
                    await _btypeRepository.Get (btypeRepositoryVersions.Keys),
                    _btypeIdComparer);
              }

              cachedData = _initialEntityMatcher.FindMatchingEntities (
                  _entityRelationDataFactory,
                  aEntities,
                  bEntities,
                  atypeRepositoryVersions,
                  btypeRepositoryVersions);
            }

            var entitySyncStates = new EntitySyncStateContainer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>();

            var aDeltaLogInfo = new VersionDeltaLoginInformation();
            var bDeltaLogInfo = new VersionDeltaLoginInformation();

            foreach (var cachedEntityData in cachedData)
            {
              TAtypeEntityVersion repositoryAVersion;
              TBtypeEntityVersion repositoryBVersion;

              var repositoryAVersionAvailable = atypeRepositoryVersions.TryGetValue (cachedEntityData.AtypeId, out repositoryAVersion);
              var repositoryBVersionAvailable = btypeRepositoryVersions.TryGetValue (cachedEntityData.BtypeId, out repositoryBVersion);

              if (repositoryAVersionAvailable)
                atypeRepositoryVersions.Remove (cachedEntityData.AtypeId);

              if (repositoryBVersionAvailable)
                btypeRepositoryVersions.Remove (cachedEntityData.BtypeId);

              var entitySyncState = CreateInitialSyncState (cachedEntityData, repositoryAVersionAvailable, repositoryAVersion, repositoryBVersionAvailable, repositoryBVersion, aDeltaLogInfo, bDeltaLogInfo);

              entitySyncStates.Add (entitySyncState);
            }

            aDeltaLogInfo.IncAdded (atypeRepositoryVersions.Count);
            bDeltaLogInfo.IncAdded (btypeRepositoryVersions.Count);

            s_logger.InfoFormat ("Atype delta: {0}", aDeltaLogInfo);
            s_logger.InfoFormat ("Btype delta: {0}", bDeltaLogInfo);

            foreach (var newA in atypeRepositoryVersions)
              entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_Added_NotExisting (newA.Key, newA.Value));

            foreach (var newB in btypeRepositoryVersions)
              entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_NotExisting_Added (newB.Key, newB.Value));

            HashSet<TAtypeEntityId> aEntitesToLoad = new HashSet<TAtypeEntityId>();
            HashSet<TBtypeEntityId> bEntitesToLoad = new HashSet<TBtypeEntityId>();

            entitySyncStates.Execute (s => s.AddRequiredEntitiesToLoad (aEntitesToLoad.Add, bEntitesToLoad.Add));

            if (aEntities == null && bEntities == null)
            {
              totalProgress.NotifyLoadCount (aEntitesToLoad.Count, bEntitesToLoad.Count);
              using (totalProgress.StartARepositoryLoad())
              {
                aEntities = CreateDictionary (
                    await _atypeRepository.Get (aEntitesToLoad),
                    _atypeIdComparer);
              }

              using (totalProgress.StartBRepositoryLoad())
              {
                bEntities = CreateDictionary (
                    await _btypeRepository.Get (bEntitesToLoad),
                    _btypeIdComparer);
              }
            }

            entitySyncStates.DoTransition (s => s.FetchRequiredEntities (aEntities, bEntities));
            entitySyncStates.DoTransition (s => s.Resolve());

            // since resolve may change to an new state, required entities have to be fetched again.
            // an state is allowed only to resolve to another state, if the following states requires equal or less entities!
            entitySyncStates.DoTransition (s => s.FetchRequiredEntities (aEntities, bEntities));

            using (var progress = totalProgress.StartProcessing (entitySyncStates.Count))
            {
              entitySyncStates.DoTransition (
                  s =>
                  {
                    var nextState = s.PerformSyncActionNoThrow();
                    progress.Increase();
                    return nextState;
                  });
            }

            var newData = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

            entitySyncStates.Execute (s => s.AddNewRelationNoThrow (newData.Add));

            entitySyncStates.Dispose();

            _entityRelationDataAccess.SaveEntityRelationData (newData);
          }
          finally
          {
            _atypeRepository.Cleanup (aEntities);
            _btypeRepository.Cleanup (bEntities);
          }
        }
      }
      catch (Exception x)
      {
        _exceptionLogger.LogException (x, s_logger);
        return false;
      }

      s_logger.DebugFormat ("Exiting.");
      return true;
    }

    private Dictionary<TKey, TValue> CreateDictionary<TKey, TValue> (IReadOnlyList<EntityIdWithVersion<TKey, TValue>> tuples, IEqualityComparer<TKey> equalityComparer)
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

    private Dictionary<TKey, TValue> CreateDictionary<TKey, TValue> (IReadOnlyList<EntityWithVersion<TKey, TValue>> tuples, IEqualityComparer<TKey> equalityComparer)
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
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> cachedData,
        bool repositoryAVersionAvailable,
        TAtypeEntityVersion repositoryAVersion,
        bool repositoryBVersionAvailable,
        TBtypeEntityVersion repositoryBVersion,
        VersionDeltaLoginInformation aLogInfo,
        VersionDeltaLoginInformation bLogInfo)
    {
      IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> state;
      if (repositoryAVersionAvailable)
      {
        var aChanged = !_atypeVersionComparer.Equals (repositoryAVersion, cachedData.AtypeVersion);
        if (aChanged)
        {
          aLogInfo.IncChanged();
          if (repositoryBVersionAvailable)
          {
            var bChanged = !_btypeVersionComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
            if (bChanged)
            {
              bLogInfo.IncChanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Changed (cachedData, repositoryAVersion, repositoryBVersion);
            }
            else
            {
              bLogInfo.IncUnchanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Unchanged (cachedData, repositoryAVersion);
            }
          }
          else
          {
            bLogInfo.IncDeleted();
            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Deleted (cachedData, repositoryAVersion);
          }
        }
        else
        {
          aLogInfo.IncUnchanged();
          if (repositoryBVersionAvailable)
          {
            var bChanged = !_btypeVersionComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
            if (bChanged)
            {
              bLogInfo.IncChanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Changed (cachedData, repositoryBVersion);
            }
            else
            {
              bLogInfo.IncUnchanged();
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Unchanged (cachedData);
            }
          }
          else
          {
            bLogInfo.IncDeleted();
            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Deleted (cachedData);
          }
        }
      }
      else
      {
        aLogInfo.IncDeleted();
        if (repositoryBVersionAvailable)
        {
          var bChanged = !_btypeVersionComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
          if (bChanged)
          {
            bLogInfo.IncChanged();
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Changed (cachedData, repositoryBVersion);
          }
          else
          {
            bLogInfo.IncUnchanged();
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Unchanged (cachedData);
          }
        }
        else
        {
          bLogInfo.IncDeleted();
          state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Deleted (cachedData);
        }
      }
      return state;
    }
  }
}