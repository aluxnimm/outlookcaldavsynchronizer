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
using CalDavSynchronizer.Generic.EntityRelationManagement;
using CalDavSynchronizer.Generic.Synchronization.StateFactories;
using CalDavSynchronizer.Generic.Synchronization.States;

namespace CalDavSynchronizer.Generic.Synchronization.StateCreationStrategies
{
  /// <summary>
  /// Creates initial states in that way, so that it results in a merge in both directions
  /// </summary>
  /// <remarks>
  /// since two way-merging can result in conflicts, this class requires a strategy to resolve conflicts
  /// </remarks>
  public class TwoWayInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> : IInitialSyncStateCreationStrategy<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    private readonly IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _stateFactory;
    private readonly IEntityConflictSyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> _conflictStateFactory;


    public TwoWayInitialSyncStateCreationStrategy (IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> factory, IEntityConflictSyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> conflictStateFactory)
    {
      _stateFactory = factory;
      _conflictStateFactory = conflictStateFactory;
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Changed_Changed (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TAtypeEntityVersion newA, TBtypeEntityVersion newB)
    {
      return _conflictStateFactory.Create_DetermineByConflictSolver_Changed_Changed (knownData, newA, newB);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Changed_Unchanged (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TAtypeEntityVersion newA)
    {
      return _stateFactory.Create_UpdateAtoB (knownData,newA);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Changed_Deleted (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TAtypeEntityVersion newA)
    {
      return _conflictStateFactory.Create_DetermineByConflictSolver_Changed_Deleted (knownData, newA);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Unchanged_Changed (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TBtypeEntityVersion newB)
    {
      return _stateFactory.Create_UpdateBtoA (knownData, newB);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Deleted_Changed (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TBtypeEntityVersion newB)
    {
      return _conflictStateFactory.Create_DetermineByConflictSolver_Deleted_Changed (knownData, newB);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Deleted_Deleted (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
    {
      return _stateFactory.Create_Discard();
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Deleted_Unchanged (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
    {
      return _stateFactory.Create_DeleteInB (knownData);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Unchanged_Deleted (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
    {
      return _stateFactory.Create_DeleteInA (knownData);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Unchanged_Unchanged (IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
    {
      return _stateFactory.Create_DoNothing (knownData);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_Added_NotExisting (TAtypeEntityId aId, TAtypeEntityVersion a)
    {
      return _stateFactory.Create_CreateInB (aId, a);
    }

    public IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> CreateFor_NotExisting_Added (TBtypeEntityId bId, TBtypeEntityVersion b)
    {
      return _stateFactory.Create_CreateInA (bId, b);
    }
  }
}