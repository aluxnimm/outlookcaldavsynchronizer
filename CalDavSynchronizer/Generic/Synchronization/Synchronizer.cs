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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.Synchronization.States;
using log4net;

namespace CalDavSynchronizer.Generic.Synchronization
{
  public class Synchronizer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : ISynchronizer
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private static readonly IEqualityComparer<TAtypeEntityVersion> _atypeComparer = EqualityComparer<TAtypeEntityVersion>.Default;
    private static readonly IEqualityComparer<TBtypeEntityVersion> _btypeComparer = EqualityComparer<TBtypeEntityVersion>.Default;
    private readonly ISynchronizerContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _synchronizerContext;
    private IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _initialSyncStateCreationStrategy;

    public Synchronizer (ISynchronizerContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> synchronizerContext, IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> initialSyncStateCreationStrategy)
    {
      _synchronizerContext = synchronizerContext;
      _initialSyncStateCreationStrategy = initialSyncStateCreationStrategy;
    }


    public void Synchronize ()
    {
      s_logger.InfoFormat ("Entered. Atype='{0}', Btype='{1}'", typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);

      var atypeEntityRepository = _synchronizerContext.AtypeRepository;
      var btypeEntityRepository = _synchronizerContext.BtypeRepository;

      var cachedData = _synchronizerContext.Load();
      var atypeRepositoryVersions = atypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);
      var btypeRepositoryVersions = btypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);

      IDictionary<TAtypeEntityId, TAtypeEntity> allAtypeEntities = null;
      IDictionary<TBtypeEntityId, TBtypeEntity> allBtypeEntities = null;
     


      if (cachedData == null)
      {
        s_logger.Info ("Did not find entity caches. Performing initial population");

        allAtypeEntities = atypeEntityRepository.GetEntities (atypeRepositoryVersions.Keys);
        allBtypeEntities = btypeEntityRepository.GetEntities (btypeRepositoryVersions.Keys);

        cachedData = _synchronizerContext.InitialEntityMatcher.PopulateEntityRelationStorage (
            _synchronizerContext.EntityRelationDataFactory,
            allAtypeEntities,
            allBtypeEntities,
            atypeRepositoryVersions,
            btypeRepositoryVersions);
      }

      var entitySyncStates = new List<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>>();

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

        var entitySyncState = CreateInitialSyncState (cachedEntityData, repositoryAVersionAvailable, repositoryAVersion, repositoryBVersionAvailable, repositoryBVersion);

        entitySyncStates.Add (entitySyncState);
      }

      foreach (var newA in atypeRepositoryVersions)
        entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_Added_NotExisting (newA.Key, newA.Value));

      foreach (var newB in btypeRepositoryVersions)
        entitySyncStates.Add (_initialSyncStateCreationStrategy.CreateFor_NotExisting_Added (newB.Key, newB.Value));

      HashSet<TAtypeEntityId> aEntitesToLoad = new HashSet<TAtypeEntityId>();
      HashSet<TBtypeEntityId> bEntitesToLoad = new HashSet<TBtypeEntityId>();

      foreach (var syncAction in entitySyncStates)
        syncAction.AddRequiredEntitiesToLoad (aEntitesToLoad.Add, bEntitesToLoad.Add);


      var aEntities = allAtypeEntities ?? atypeEntityRepository.GetEntities (aEntitesToLoad);
     
      var bEntities = allBtypeEntities ?? btypeEntityRepository.GetEntities (bEntitesToLoad);


      entitySyncStates = entitySyncStates.Select (a => a.FetchRequiredEntities (aEntities, bEntities)).ToList();;
      entitySyncStates = entitySyncStates.Select (a => a.Resolve ()).ToList ();

      // since resolve may change to an new state, required entities have to be fetched again.
      // an state is allowed only to resolve to another state, if the following states requires equal or less entities!
      entitySyncStates = entitySyncStates.Select (a => a.FetchRequiredEntities (aEntities, bEntities)).ToList ();
      ;
      entitySyncStates = entitySyncStates.Select (a => a.PerformSyncAction ()).ToList ();

      var newData = new List<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>>();

      foreach (var syncAction in entitySyncStates)
        syncAction.AddNewData (newData.Add);

      _synchronizerContext.Save (newData);


      s_logger.DebugFormat ("Exiting.");
    }

    private IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateInitialSyncState (
        IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> cachedData,
        bool repositoryAVersionAvailable,
        TAtypeEntityVersion repositoryAVersion,
        bool repositoryBVersionAvailable,
        TBtypeEntityVersion repositoryBVersion)
    {
      IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> state;
      if (repositoryAVersionAvailable)
      {
        var aChanged = !_atypeComparer.Equals (repositoryAVersion, cachedData.AtypeVersion);
        if (aChanged)
        {
          if (repositoryBVersionAvailable)
          {
            var bChanged = !_btypeComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
            if (bChanged)
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Changed (cachedData, repositoryAVersion, repositoryBVersion);
            else
              state = _initialSyncStateCreationStrategy.CreateFor_Changed_Unchanged (cachedData, repositoryAVersion);
          }
          else
          {
            state = _initialSyncStateCreationStrategy.CreateFor_Changed_Deleted (cachedData, repositoryAVersion);
          }
        }
        else
        {
          if (repositoryBVersionAvailable)
          {
            var bChanged = !_btypeComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
            if (bChanged)
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Changed (cachedData, repositoryBVersion);
            else
              state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Unchanged (cachedData);
          }
          else
          {
            state = _initialSyncStateCreationStrategy.CreateFor_Unchanged_Deleted (cachedData);
          }
        }
      }
      else
      {
        if (repositoryBVersionAvailable)
        {
          var bChanged = !_btypeComparer.Equals (repositoryBVersion, cachedData.BtypeVersion);
          if (bChanged)
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Changed (cachedData, repositoryBVersion);
          else
            state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Unchanged (cachedData);
        }
        else
        {
          state = _initialSyncStateCreationStrategy.CreateFor_Deleted_Deleted (cachedData);
        }
      }
      return state;
    }

 
  }
}