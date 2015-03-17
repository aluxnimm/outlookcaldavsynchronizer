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
      : SynchronizerBase<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TwoWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId>>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> _conflictResolutionStrategy;
    private readonly TwoWayConflictSolver<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> _conflictSolver;


    public TwoWaySynchronizer (ISynchronizerContext<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> synchronizerContext, IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> conflictResolutionStrategy)
        : base (synchronizerContext)
    {
      _conflictResolutionStrategy = conflictResolutionStrategy;
      _conflictSolver = new TwoWayConflictSolver<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>();
    }

    protected override TwoWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> CalculateDataToSynchronize (IReadOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> atypeEntityRepository, IReadOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> btypeEntityRepository, VersionDelta<TAtypeEntityId, TAtypeEntityVersion> atypeDelta, VersionDelta<TBtypeEntityId, TBtypeEntityVersion> btypeDelta, IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> atypeToBtypeEntityRelationStorage, IEntityRelationStorage<TBtypeEntityId, TAtypeEntityId> btypeToAtypeEntityRelationStorage)
    {
      var atypeEntityDelta = atypeEntityRepository.LoadDelta (atypeDelta);
      var btypeEntityDelta = btypeEntityRepository.LoadDelta (btypeDelta);
      var solvedConflicts = _conflictSolver.SolveConflicts (atypeEntityDelta, btypeEntityDelta, _conflictResolutionStrategy, atypeToBtypeEntityRelationStorage, btypeToAtypeEntityRelationStorage);
      return new TwoWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> (atypeEntityDelta, btypeEntityDelta, solvedConflicts);
    }

    protected override void SynchronizeOverride (SynchronizationTasks<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> aToBtasks, SynchronizationTasks<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> bToAtasks, TwoWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> syncData)
    {
      var bCurrentTargetEntityCache = aToBtasks.LoadTargetEntityCache (syncData.SolvedConflicts.ADelta.Changed, syncData.BtypeDelta.Changed);
      var aCurrentTargetEntityCache = bToAtasks.LoadTargetEntityCache (syncData.SolvedConflicts.BDelta.Changed, syncData.AtypeDelta.Changed);


      aToBtasks.SnychronizeDeleted (syncData.SolvedConflicts.ADelta.Deleted);
      aToBtasks.SynchronizeChanged (syncData.SolvedConflicts.ADelta.Changed, bCurrentTargetEntityCache);
      aToBtasks.SynchronizeAdded (syncData.AtypeDelta.Added);

      bToAtasks.SnychronizeDeleted (syncData.SolvedConflicts.BDelta.Deleted);
      bToAtasks.SynchronizeChanged (syncData.SolvedConflicts.BDelta.Changed, aCurrentTargetEntityCache);
      bToAtasks.SynchronizeAdded (syncData.BtypeDelta.Added);
    }
   
  }

  public struct TwoWaySyncData<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId>
  {
    public readonly EntityDelta<TAtypeEntity, TAtypeEntityId> AtypeDelta;
    public readonly EntityDelta<TBtypeEntity, TBtypeEntityId> BtypeDelta;
    public readonly SolvedConflicts<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> SolvedConflicts;

    public TwoWaySyncData (EntityDelta<TAtypeEntity, TAtypeEntityId> atypeDelta, EntityDelta<TBtypeEntity, TBtypeEntityId> btypeDelta, SolvedConflicts<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> solvedConflicts)
        : this()
    {
      AtypeDelta = atypeDelta;
      BtypeDelta = btypeDelta;
      SolvedConflicts = solvedConflicts;
    }
  }
}