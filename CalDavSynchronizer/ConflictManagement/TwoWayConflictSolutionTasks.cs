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
  internal class TwoWayConflictSolutionTasks<TSourceEntity, TSourceEntityId, TTargetEntity, TTargetEntityId>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IEntityRelationStorage<TSourceEntityId, TTargetEntityId> _sourceToTargetEntityRelationStorage;
    private readonly EntityDelta<TSourceEntity, TSourceEntityId> _loadedSourceDelta;
    private readonly EntityDelta<TTargetEntity, TTargetEntityId> _loadedTargetDelta;

    private readonly EntityDelta<TSourceEntity, TSourceEntityId> _effectiveSourceDelta;
    private readonly EntityDelta<TTargetEntity, TTargetEntityId> _effectiveTargetDelta;

    private readonly HashSet<TSourceEntityId> _solvedSourceParticipantIds;
    private readonly HashSet<TTargetEntityId> _solvedTargetParticipantIds;

    public TwoWayConflictSolutionTasks (
        IEntityRelationStorage<TSourceEntityId, TTargetEntityId> sourceToTargetEntityRelationStorage,
        EntityDelta<TSourceEntity, TSourceEntityId> loadedSourceDelta,
        EntityDelta<TTargetEntity, TTargetEntityId> loadedTargetDelta,
        HashSet<TSourceEntityId> solvedSourceParticipantIds,
        HashSet<TTargetEntityId> solvedTargetParticipantIds, 
        EntityDelta<TSourceEntity, TSourceEntityId> effectiveSourceDelta, 
        EntityDelta<TTargetEntity, TTargetEntityId> effectiveTargetDelta)
    {
      _sourceToTargetEntityRelationStorage = sourceToTargetEntityRelationStorage;
      _loadedSourceDelta = loadedSourceDelta;
      _loadedTargetDelta = loadedTargetDelta;
      _solvedSourceParticipantIds = solvedSourceParticipantIds;
      _solvedTargetParticipantIds = solvedTargetParticipantIds;
      _effectiveSourceDelta = effectiveSourceDelta;
      _effectiveTargetDelta = effectiveTargetDelta;
    }

    public void SolveDeletionConflicts (Func<TTargetEntity, OneWayConflictResolution> resolveSourceDeletionConflict)
    {
      foreach (var sourceDeletion in _loadedSourceDelta.Deleted)
      {
        if (_solvedSourceParticipantIds.Contains (sourceDeletion.Key))
          continue;

        TTargetEntityId targetEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity2ByEntity1 (sourceDeletion.Key, out targetEntityId))
        {
          TTargetEntity targetEntity;
          if (_loadedTargetDelta.Changed.TryGetValue (targetEntityId, out targetEntity))
          {
            if (_solvedTargetParticipantIds.Contains (targetEntityId))
              continue;

            _solvedSourceParticipantIds.Add (sourceDeletion.Key);
            _solvedTargetParticipantIds.Add (targetEntityId);

            switch (resolveSourceDeletionConflict (targetEntity))
            {
              case OneWayConflictResolution.SourceWins:
                _effectiveSourceDelta.Deleted[sourceDeletion.Key] = sourceDeletion.Value;
                break;
              case OneWayConflictResolution.TargetWins:
                _effectiveTargetDelta.Changed[targetEntityId] = targetEntity;
                TTargetEntityId dummy;
                _sourceToTargetEntityRelationStorage.TryRemoveByEntity1 (sourceDeletion.Key, out dummy);
                break;
            }
          }
          else
          {
            _effectiveSourceDelta.Deleted[sourceDeletion.Key] = sourceDeletion.Value;
          }
        }
        else
        {
          _effectiveSourceDelta.Deleted[sourceDeletion.Key] = sourceDeletion.Value;
        }
      }
    }
   
    public void SolveModificationConflicts (Func<TSourceEntity, TTargetEntity, OneWayConflictResolution> resolveSourceModificationConflict)
    {
      foreach (var sourceChange in _loadedSourceDelta.Changed)
      {
        if (_solvedSourceParticipantIds.Contains (sourceChange.Key))
          continue;

        TTargetEntityId targetEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity2ByEntity1 (sourceChange.Key, out targetEntityId))
        {
          TTargetEntity targetEntity;
          if (_loadedTargetDelta.Changed.TryGetValue (targetEntityId, out targetEntity))
          {
            if (_solvedTargetParticipantIds.Contains (targetEntityId))
              continue;

            _solvedSourceParticipantIds.Add (sourceChange.Key);
            _solvedTargetParticipantIds.Add (targetEntityId);

            switch (resolveSourceModificationConflict (sourceChange.Value, targetEntity))
            {
              case OneWayConflictResolution.SourceWins:
                _effectiveSourceDelta.Changed[sourceChange.Key] = sourceChange.Value;
                break;
              case OneWayConflictResolution.TargetWins:
                _effectiveTargetDelta.Changed[targetEntityId] = targetEntity;
                break;
            }
          }
          else
          {
            _effectiveSourceDelta.Changed[sourceChange.Key] = sourceChange.Value;
          }
        }
        else
        {
          _effectiveSourceDelta.Changed[sourceChange.Key] = sourceChange.Value;
        }
      }
    }


  }
}