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
using System.Reflection;
using CalDavSynchronizer.ConflictManagement;
using CalDavSynchronizer.EntityMapping;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  public class TwoWaySynchronizer<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
    : SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod ().DeclaringType);

    private readonly IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> _conflictResolutionStrategy;
    private TwoWayConflictSolver<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _conflictSolver;

    public TwoWaySynchronizer (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext, IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> conflictResolutionStrategy)
        : base(synchronizerContext)
    {
      _conflictResolutionStrategy = conflictResolutionStrategy;
      _conflictSolver = new TwoWayConflictSolver<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> ();
    }

    protected override void SynchronizeOverride ()
    {
      var aToBtasks = new SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> (
        _atypeToBtypeEntityRelationStorage,
        _btypeEntityRepository,
        _entityMapper.Map1To2,
        _btypeVersions);

      var bToAtasks = new SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> (
        _btypeToAtypeEntityRelationStorage,
        _atypeEntityRepository,
        _entityMapper.Map2To1,
        _atypeVersions);

      var atypeEntityDelta = _atypeEntityRepository.LoadDelta (_atypeDelta);
      var btypeEntityDelta = _btypeEntityRepository.LoadDelta (_btypeDelta);

      var solvedConflicts = _conflictSolver.SolveConflicts (atypeEntityDelta, btypeEntityDelta, _conflictResolutionStrategy, _atypeToBtypeEntityRelationStorage, _btypeToAtypeEntityRelationStorage);

      aToBtasks.SnychronizeDeleted (solvedConflicts.ADelta.Deleted);
      aToBtasks.SynchronizeChanged (solvedConflicts.ADelta.Changed);
      aToBtasks.SynchronizeAdded (atypeEntityDelta.Added);

      bToAtasks.SnychronizeDeleted (solvedConflicts.BDelta.Deleted);
      bToAtasks.SynchronizeChanged (solvedConflicts.BDelta.Changed);
      bToAtasks.SynchronizeAdded (btypeEntityDelta.Added);
    }
  
   

  }
}