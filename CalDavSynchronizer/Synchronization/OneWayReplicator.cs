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
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  public class OneWayReplicator<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
      : SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    public OneWayReplicator (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext)
        : base (synchronizerContext)
    {
    }


    protected override void SynchronizeOverride ()
    {
      var aToBtasks = new SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (
          _atypeToBtypeEntityRelationStorage,
          _btypeEntityRepository,
          _entityMapper.Map1To2,
          _btypeVersions);

      var conflictSolver = new OneWayConflictSolutionTasks<TAtypeEntity, TAtypeEntityId, TBtypeEntityId, TBtypeEntityVersion> (_atypeToBtypeEntityRelationStorage);

      var atypeEntityDelta = _atypeEntityRepository.LoadDelta (_atypeDelta);


      var btypeChangesWithOutConflicts = conflictSolver.GetTargetChangesWithoutConflict (_btypeDelta.Changed, atypeEntityDelta.Deleted, atypeEntityDelta.Changed);
      var btypeDeletionsWithoutConflicts = conflictSolver.GetTargetDeletionsWithoutConflict (_btypeDelta.Deleted, atypeEntityDelta.Deleted, atypeEntityDelta.Changed);

      var atypeIdsOfbtypeChangesWithOutConflicts = btypeChangesWithOutConflicts.Select (e => e.Item1).ToArray();

      var btypeChangesRestoreInformation = _atypeEntityRepository.GetEntities (atypeIdsOfbtypeChangesWithOutConflicts);
      var btypeDeletionRestoreInformation = _atypeEntityRepository.GetEntities (btypeDeletionsWithoutConflicts.Select (e => e.Item1));

      conflictSolver.ClearTargetDeletionConflicts (_btypeDelta.Deleted, atypeEntityDelta.Changed);


      var allAtypeIdsThatWillCauseUpdatesOnB = atypeEntityDelta.Changed.Keys.Union (atypeIdsOfbtypeChangesWithOutConflicts);
      List<TBtypeEntityId> allBtypeIdsThatWillBeUpdated = new List<TBtypeEntityId>();
      foreach (var atypeId in allAtypeIdsThatWillCauseUpdatesOnB)
      {
        TBtypeEntityId btypeId;
        if (_atypeToBtypeEntityRelationStorage.TryGetEntity2ByEntity1 (atypeId, out btypeId))
          allBtypeIdsThatWillBeUpdated.Add (btypeId);
      }
      var currentTargetEntityCache = _btypeEntityRepository.GetEntities (allBtypeIdsThatWillBeUpdated);

      aToBtasks.SnychronizeDeleted (atypeEntityDelta.Deleted);
      aToBtasks.SynchronizeChanged (atypeEntityDelta.Changed, currentTargetEntityCache);
      aToBtasks.SynchronizeAdded (atypeEntityDelta.Added);

      aToBtasks.DeleteAdded (_btypeDelta.Added.Select (v => v.Id));
      aToBtasks.RestoreDeletedInTarget (btypeDeletionsWithoutConflicts, btypeDeletionRestoreInformation);
      aToBtasks.RestoreChangedInTarget (btypeChangesWithOutConflicts, btypeChangesRestoreInformation, currentTargetEntityCache);
    }
  }
}