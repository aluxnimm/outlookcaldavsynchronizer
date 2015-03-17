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
using System.Linq;
using System.Reflection;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using CalDavSynchronizer.InitialEntityMatching;
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  public abstract class SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TSyncData>
      : ISynchronizer
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _synchronizerContext;

    protected SynchronizerBase (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext)
    {
      _synchronizerContext = synchronizerContext;
    }

    public void Synchronize ()
    {
      s_logger.InfoFormat ("Entered. Atype='{0}', Btype='{1}'", typeof (TAtypeEntity).Name, typeof (TBtypeEntity).Name);

      TSyncData syncData;
      SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> aToBtasks;
      SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> bToAtasks;
      EntityCaches<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> caches;

      try
      {
        var atypeEntityRepository = _synchronizerContext.AtypeRepository;
        var btypeEntityRepository = _synchronizerContext.BtypeRepository;
        var entityMapper = _synchronizerContext.EntityMapper;

        bool cachesWereCreatedNew;
        caches = _synchronizerContext.LoadOrCreateCaches (out cachesWereCreatedNew);

        var atypeVersionStorage = caches.AtypeStorage;
        var btypeVersionStorage = caches.BtypeStorage;

        var atypeVersions = atypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);
        var btypeVersions = btypeEntityRepository.GetEntityVersions (_synchronizerContext.From, _synchronizerContext.To);
        var atypeToBtypeEntityRelationStorage = caches.EntityRelationStorage;

        VersionDelta<TAtypeEntityId, TAtypeEntityVersion> atypeDelta;
        VersionDelta<TBtypeEntityId, TBtypeEntityVersion> btypeDelta;

        if (cachesWereCreatedNew)
        {
          s_logger.Info ("Did not find entity caches. Performing initial population");

          var result = _synchronizerContext.InitialEntityMatcher.PopulateEntityRelationStorage (
              atypeToBtypeEntityRelationStorage,
              atypeEntityRepository,
              btypeEntityRepository,
              atypeVersions,
              btypeVersions,
              atypeVersionStorage,
              btypeVersionStorage);

          atypeDelta = new VersionDelta<TAtypeEntityId, TAtypeEntityVersion> (result.Item1.ToList(), new EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>[] { }, new EntityIdWithVersion<TAtypeEntityId, TAtypeEntityVersion>[] { });
          btypeDelta = new VersionDelta<TBtypeEntityId, TBtypeEntityVersion> (result.Item2.ToList(), new EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>[] { }, new EntityIdWithVersion<TBtypeEntityId, TBtypeEntityVersion>[] { });
        }
        else
        {
          atypeDelta = atypeVersionStorage.SetNewVersions (atypeVersions);
          btypeDelta = btypeVersionStorage.SetNewVersions (btypeVersions);
        }

        var btypeToAtypeEntityRelationStorage = new EntityRelationStorageSwitchRolesWrapper<TBtypeEntityId, TAtypeEntityId> (atypeToBtypeEntityRelationStorage);

        s_logger.InfoFormat ("Atype delta: {0}", atypeDelta);
        s_logger.InfoFormat ("Btype delta: {0}", btypeDelta);

      
        syncData = CalculateDataToSynchronize (
            atypeEntityRepository,
            btypeEntityRepository,
            atypeDelta,
            btypeDelta,
            atypeToBtypeEntityRelationStorage,
            btypeToAtypeEntityRelationStorage);

        aToBtasks = new SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (
            atypeToBtypeEntityRelationStorage,
            btypeEntityRepository,
            entityMapper.Map1To2,
            btypeVersionStorage);

        bToAtasks = new SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> (
            btypeToAtypeEntityRelationStorage,
            atypeEntityRepository,
            entityMapper.Map2To1,
            atypeVersionStorage);
      }
      catch (Exception x)
      {
        s_logger.Error ("Error during synchronization preparation. Aborting synchronization.", x);
        return;
      }

      try
      {
        DoSynchronization (
            aToBtasks,
            bToAtasks,
            syncData);
      }
      catch (Exception x)
      {
        // DoSynchronization should skip and log exceptions but not rethrow them!
        s_logger.Error ("Error during synchronization. Deleting caches to prevent cache inconsistencies.", x);
        try
        {
          _synchronizerContext.DeleteCaches();
        }
        catch (Exception x2)
        {
          s_logger.Error ("Error during cache deletion.", x2);
        }
        return;
      }


      _synchronizerContext.SaveChaches (caches);

      s_logger.DebugFormat ("Exiting.");
    }

    protected abstract TSyncData CalculateDataToSynchronize (
        IReadOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeEntityRepository,
        IReadOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeEntityRepository,
        VersionDelta<TAtypeEntityId, TAtypeEntityVersion> atypeDelta,
        VersionDelta<TBtypeEntityId, TBtypeEntityVersion> btypeDelta,
        IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> atypeToBtypeEntityRelationStorage,
        IEntityRelationStorage<TBtypeEntityId, TAtypeEntityId> btypeToAtypeEntityRelationStorage
        );


    protected abstract void DoSynchronization (
        SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> aToBtasks,
        SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> bToAtasks,
        TSyncData syncData
        );
  }
}