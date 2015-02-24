// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using log4net;

namespace CalDavSynchronizer.ConflictManagement
{
  public class TwoWayConflictSolver<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion, TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    public TwoWayConflictSolver ()
    {
    }


    public SolvedConflicts<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> SolveConflicts (
      EntityDelta<TAtypeEntity, TAtypeEntityId> atypeDelta, 
      EntityDelta<TBtypeEntity, TBtypeEntityId> btypeDelta, 
      IConflictResolutionStrategy<TAtypeEntity, TBtypeEntity> conflictResolutionStrategy, 
      IEntityRelationStorage<TAtypeEntityId, TBtypeEntityId> atypeToBtypeEntityRelationStorage, 
      IEntityRelationStorage<TBtypeEntityId, TAtypeEntityId> btypeToAtypeEntityRelationStorage)
    {
      var newATypeDelta = new EntityDelta<TAtypeEntity, TAtypeEntityId>();
      var newBTypeDelta = new EntityDelta<TBtypeEntity, TBtypeEntityId>();

      HashSet<TAtypeEntityId> atypeResolvedConflictParticipants = new HashSet<TAtypeEntityId>();
      HashSet<TBtypeEntityId> btypeResolvedConflictParticipants = new HashSet<TBtypeEntityId>();

      var aToBConflictResolution = new TwoWayConflictSolutionTasks<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> (atypeToBtypeEntityRelationStorage, atypeDelta, btypeDelta, atypeResolvedConflictParticipants, btypeResolvedConflictParticipants, newATypeDelta, newBTypeDelta);

      var bToAConflictResolution = new TwoWayConflictSolutionTasks<TBtypeEntity, TBtypeEntityId, TAtypeEntity, TAtypeEntityId> (btypeToAtypeEntityRelationStorage, btypeDelta, atypeDelta, btypeResolvedConflictParticipants, atypeResolvedConflictParticipants, newBTypeDelta, newATypeDelta);

      aToBConflictResolution.SolveDeletionConflicts (
          target => conflictResolutionStrategy.ResolveDeletionInAConflict (target) == GenericConflictResolution.AWins ? OneWayConflictResolution.SourceWins : OneWayConflictResolution.TargetWins);
      bToAConflictResolution.SolveDeletionConflicts (
          target => conflictResolutionStrategy.ResolveDeletionInBConflict (target) == GenericConflictResolution.AWins ? OneWayConflictResolution.TargetWins : OneWayConflictResolution.SourceWins);

      aToBConflictResolution.SolveModificationConflicts (
          (source, target) => conflictResolutionStrategy.ResolveModifiedConflict (source, target) == GenericConflictResolution.AWins ? OneWayConflictResolution.SourceWins : OneWayConflictResolution.TargetWins);
      bToAConflictResolution.SolveModificationConflicts (
          (source, target) => conflictResolutionStrategy.ResolveModifiedConflict (target, source) == GenericConflictResolution.AWins ? OneWayConflictResolution.TargetWins : OneWayConflictResolution.SourceWins);

      return new SolvedConflicts<TAtypeEntity, TAtypeEntityId, TBtypeEntity, TBtypeEntityId> (newATypeDelta, newBTypeDelta);
    }
  }
}