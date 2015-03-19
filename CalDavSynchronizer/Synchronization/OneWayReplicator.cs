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
using CalDavSynchronizer.ConflictManagement;
using CalDavSynchronizer.EntityRepositories;
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  public class OneWayReplicator<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
      : SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, OneWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId>>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    public OneWayReplicator (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext)
        : base (synchronizerContext)
    {
    }

    protected override OneWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> CalculateDataToSynchronize (EntityRepositories.IReadOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeEntityRepository, EntityRepositories.IReadOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeEntityRepository, EntityVersionManagement.VersionDelta<TAtypeEntityId, TAtypeEntityVersion> atypeDelta, EntityVersionManagement.VersionDelta<TBtypeEntityId, TBtypeEntityVersion> btypeDelta, EntityRelationManagement.IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> atypeToBtypeEntityRelationStorage, EntityRelationManagement.IEntityRelationStorage<TBtypeEntityId, TAtypeEntityId> btypeToAtypeEntityRelationStorage)
    {

      var conflictSolver = new OneWayConflictSolutionTasks<TAtypeEntity, TAtypeEntityId, TBtypeEntityId, TBtypeEntityVersion> (atypeToBtypeEntityRelationStorage);

      var atypeEntityDelta = atypeEntityRepository.LoadDelta (atypeDelta);


      var btypeChangesWithOutConflicts = conflictSolver.GetTargetChangesWithoutConflict (btypeDelta.Changed, atypeEntityDelta.Deleted, atypeEntityDelta.Changed);
      var btypeDeletionsWithoutConflicts = conflictSolver.GetTargetDeletionsWithoutConflict (btypeDelta.Deleted, atypeEntityDelta.Deleted, atypeEntityDelta.Changed);

      var atypeIdsOfbtypeChangesWithOutConflicts = btypeChangesWithOutConflicts.Select (e => e.Item1).ToArray();

      var btypeChangesRestoreInformation = atypeEntityRepository.GetEntities (atypeIdsOfbtypeChangesWithOutConflicts);
      var btypeDeletionRestoreInformation = atypeEntityRepository.GetEntities (btypeDeletionsWithoutConflicts.Select (e => e.Item1).ToArray());

      conflictSolver.ClearTargetDeletionConflicts (btypeDelta.Deleted, atypeEntityDelta.Changed);


      var allAtypeIdsThatWillCauseUpdatesOnB = atypeEntityDelta.Changed.Keys.Union (atypeIdsOfbtypeChangesWithOutConflicts);
      List<TBtypeEntityId> allBtypeIdsThatWillBeUpdated = new List<TBtypeEntityId>();
      foreach (var atypeId in allAtypeIdsThatWillCauseUpdatesOnB)
      {
        TBtypeEntityId btypeId;
        if (atypeToBtypeEntityRelationStorage.TryGetEntity2ByEntity1 (atypeId, out btypeId))
          allBtypeIdsThatWillBeUpdated.Add (btypeId);
      }
      var currentTargetEntityCache = btypeEntityRepository.GetEntities (allBtypeIdsThatWillBeUpdated);

      return new OneWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> (
          atypeEntityDelta,
          btypeDelta.Added.Select (v => v.Id),
          btypeChangesWithOutConflicts,
          btypeDeletionsWithoutConflicts,
          currentTargetEntityCache,
          btypeChangesRestoreInformation,
          btypeDeletionRestoreInformation
       );
    }


    protected override void DoSynchronization (SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> aToBtasks, SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> bToAtasks, OneWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> syncData)
    {
      aToBtasks.SnychronizeDeleted (syncData.AtypeEntityDelta.Deleted);
      aToBtasks.SynchronizeChanged (syncData.AtypeEntityDelta.Changed,syncData.CurrentTargetEntityCache);
      aToBtasks.SynchronizeAdded (syncData.AtypeEntityDelta.Added);

      aToBtasks.DeleteAdded (syncData.BTypeAdded);
      aToBtasks.RestoreDeletedInTarget (syncData.BtypeDeletionsWithoutConflicts, syncData.BtypeDeletionRestoreInformation);
      aToBtasks.RestoreChangedInTarget (syncData.BtypeChangesWithOutConflicts, syncData.BtypeChangesRestoreInformation, syncData.CurrentTargetEntityCache);
    }

  }

  public struct OneWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId>
  {
    public readonly EntityDelta<TAtypeEntity, TAtypeEntityId> AtypeEntityDelta;
    public readonly IEnumerable<TBtypeEntityId> BTypeAdded;
    public readonly IEnumerable<Tuple<TAtypeEntityId, TBtypeEntityId>> BtypeChangesWithOutConflicts;
    public readonly IEnumerable<Tuple<TAtypeEntityId, TBtypeEntityId>> BtypeDeletionsWithoutConflicts;
    public readonly IDictionary<TBtypeEntityId, TBtypeEntity> CurrentTargetEntityCache;
    public readonly IDictionary<TAtypeEntityId, TAtypeEntity> BtypeChangesRestoreInformation;
    public readonly IDictionary<TAtypeEntityId, TAtypeEntity> BtypeDeletionRestoreInformation;

    public OneWaySyncData (EntityDelta<TAtypeEntity, TAtypeEntityId> atypeEntityDelta, IEnumerable<TBtypeEntityId> bTypeAdded, IEnumerable<Tuple<TAtypeEntityId, TBtypeEntityId>> btypeChangesWithOutConflicts, IEnumerable<Tuple<TAtypeEntityId, TBtypeEntityId>> btypeDeletionsWithoutConflicts, IDictionary<TBtypeEntityId, TBtypeEntity> currentTargetEntityCache, IDictionary<TAtypeEntityId, TAtypeEntity> btypeChangesRestoreInformation, IDictionary<TAtypeEntityId, TAtypeEntity> btypeDeletionRestoreInformation)
        : this()
    {
      AtypeEntityDelta = atypeEntityDelta;
      BTypeAdded = bTypeAdded;
      BtypeChangesWithOutConflicts = btypeChangesWithOutConflicts;
      BtypeDeletionsWithoutConflicts = btypeDeletionsWithoutConflicts;
      CurrentTargetEntityCache = currentTargetEntityCache;
      BtypeChangesRestoreInformation = btypeChangesRestoreInformation;
      BtypeDeletionRestoreInformation = btypeDeletionRestoreInformation;
    }
  }
}