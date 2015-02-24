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
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.ConflictManagement
{
  internal class OneWayConflictSolutionTasks<TSourceEntity, TSourceEntityId, TTargetEntityId, TTargetEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IEntityRelationStorage<TSourceEntityId, TTargetEntityId> _sourceToTargetEntityRelationStorage;

    public OneWayConflictSolutionTasks (IEntityRelationStorage<TSourceEntityId, TTargetEntityId> sourceToTargetEntityRelationStorage)
    {
      _sourceToTargetEntityRelationStorage = sourceToTargetEntityRelationStorage;
    }

    public IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> GetTargetChangesWithoutConflict (IReadOnlyList<EntityIdWithVersion<TTargetEntityId, TTargetEntityVersion>> targetChanges, IDictionary<TSourceEntityId, bool> sourceDeletes, IDictionary<TSourceEntityId, TSourceEntity> sourceChanges)
    {
      var targetChangesWithoutConflict = new List<Tuple<TSourceEntityId, TTargetEntityId>>();
      foreach (var targetChange in targetChanges)
      {
        TSourceEntityId sourceEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity1ByEntity2 (targetChange.Id, out sourceEntityId))
        {
          if (!sourceDeletes.ContainsKey (sourceEntityId))
          {
            if (!sourceChanges.ContainsKey (sourceEntityId))
            {
              targetChangesWithoutConflict.Add (Tuple.Create (sourceEntityId, targetChange.Id));
            }
          }
        }
      }
      return targetChangesWithoutConflict;
    }

    public IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> GetTargetDeletionsWithoutConflict (IReadOnlyList<EntityIdWithVersion<TTargetEntityId, TTargetEntityVersion>> targetdeletions, IDictionary<TSourceEntityId, bool> sourceDeletes, IDictionary<TSourceEntityId, TSourceEntity> sourceChanges)
    {
      var targetDeletionsWithoutConflict = new List<Tuple<TSourceEntityId, TTargetEntityId>>();
      foreach (var targetDeletion in targetdeletions)
      {
        TSourceEntityId sourceEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity1ByEntity2 (targetDeletion.Id, out sourceEntityId))
        {
          if (!sourceDeletes.ContainsKey (sourceEntityId))
          {
            if (!sourceChanges.ContainsKey (sourceEntityId))
            {
              targetDeletionsWithoutConflict.Add (Tuple.Create (sourceEntityId, targetDeletion.Id));
            }
          }
        }
      }
      return targetDeletionsWithoutConflict;
    }

    public void ClearTargetDeletionConflicts (IReadOnlyList<EntityIdWithVersion<TTargetEntityId, TTargetEntityVersion>> targetDeletions, IDictionary<TSourceEntityId, TSourceEntity> sourceChanges)
    {
      foreach (var targetDeletion in targetDeletions)
      {
        TSourceEntityId sourceEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity1ByEntity2 (targetDeletion.Id, out sourceEntityId))
        {
          TSourceEntity sourceEntity;
          if (sourceChanges.TryGetValue (sourceEntityId, out sourceEntity))
          {
            TSourceEntityId dummy;
            _sourceToTargetEntityRelationStorage.TryRemoveByEntity2 (targetDeletion.Id, out dummy);
          }
        }
      }
    }
  }
}