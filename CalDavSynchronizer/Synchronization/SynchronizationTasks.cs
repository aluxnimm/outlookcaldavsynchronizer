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
using System.Reflection;
using CalDavSynchronizer.EntityRelationManagement;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.EntityVersionManagement;
using log4net;

namespace CalDavSynchronizer.Synchronization
{
  internal class SynchronizationTasks<TSourceEntity, TSourceEntityId, TSourceEntityVersion, TTargetEntity, TTargetEntityId, TTargetEntityVersion>
  {
    // ReSharper disable once StaticFieldInGenericType
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);


    private readonly IVersionStorage<TTargetEntityId, TTargetEntityVersion> _knownTargetVersions;
    private readonly IEntityRelationStorage<TSourceEntityId, TTargetEntityId> _sourceToTargetEntityRelationStorage;
    private readonly EntityRepositoryBase<TTargetEntity, TTargetEntityId, TTargetEntityVersion> _targetEntityRepository;
    private readonly Func<TSourceEntity, TTargetEntity, TTargetEntity> _entityMapper;


    public SynchronizationTasks (
        IEntityRelationStorage<TSourceEntityId, TTargetEntityId> sourceToTargetEntityRelationStorage,
        EntityRepositoryBase<TTargetEntity, TTargetEntityId, TTargetEntityVersion> targetEntityRepository,
        Func<TSourceEntity, TTargetEntity, TTargetEntity> entityMapper,
        IVersionStorage<TTargetEntityId, TTargetEntityVersion> knownTargetVersions)
    {
      _sourceToTargetEntityRelationStorage = sourceToTargetEntityRelationStorage;
      _targetEntityRepository = targetEntityRepository;
      _entityMapper = entityMapper;
      _knownTargetVersions = knownTargetVersions;
    }


    public void SynchronizeChanged (IDictionary<TSourceEntityId, TSourceEntity> changedVersions, IDictionary<TTargetEntityId, TTargetEntity> currentTargetEntityCache)
    {
      foreach (var entityById in changedVersions)
      {
        TTargetEntityId targetEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity2ByEntity1 (entityById.Key, out targetEntityId))
        {
          s_logger.DebugFormat ("Updating '{0}' in target", targetEntityId);
          bool idChanged;
          var entityByIdInClosure = entityById.Value;

          TTargetEntity cachedCurrentTargetEntity;
          currentTargetEntityCache.TryGetValue (targetEntityId, out cachedCurrentTargetEntity);

          var newTargetEntityWithVerisonId = _targetEntityRepository.Update (targetEntityId, target => _entityMapper (entityByIdInClosure, target), cachedCurrentTargetEntity);
          if (!targetEntityId.Equals (newTargetEntityWithVerisonId.Id))
          {
            _sourceToTargetEntityRelationStorage.TryRemoveByEntity1 (entityById.Key, out targetEntityId);
            _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVerisonId.Id);
            _knownTargetVersions.DeleteVersion (targetEntityId);
            _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
          }
          else
          {
            _knownTargetVersions.ChangeVersion (newTargetEntityWithVerisonId);
          }
        }
        else
        {
          var entityByIdInClosure = entityById.Value;
          var newTargetEntityWithVerisonId = _targetEntityRepository.Create (newEntity => _entityMapper (entityByIdInClosure, newEntity));
          s_logger.DebugFormat ("Created '{0}' in target", targetEntityId);
          _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVerisonId.Id);
          _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
        }
      }
    }

    public void SnychronizeDeleted (IDictionary<TSourceEntityId, bool> deletedVersions)
    {
      foreach (var deleted in deletedVersions)
      {
        TTargetEntityId targetEntityId;
        if (_sourceToTargetEntityRelationStorage.TryRemoveByEntity1 (deleted.Key, out targetEntityId))
        {
          s_logger.DebugFormat ("Deleting '{0}' in target", targetEntityId);
          _targetEntityRepository.Delete (targetEntityId);
          _knownTargetVersions.DeleteVersion (targetEntityId);
        }
      }
    }

    public void DeleteAdded (IEnumerable<TTargetEntityId> addedVersions)
    {
      foreach (var addedId in addedVersions)
      {
        s_logger.DebugFormat ("Deleting '{0}' in target", addedId);
        _targetEntityRepository.Delete (addedId);
        _knownTargetVersions.DeleteVersion (addedId);
      }
    }

    public void RestoreDeletedInTarget (IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> deletions, IDictionary<TSourceEntityId, TSourceEntity> restoreInformation)
    {
      foreach (var deletion in deletions)
      {
        var sourceEntity = restoreInformation[deletion.Item1];
        var newTargetEntityWithVerisonId = _targetEntityRepository.Create (newEntity => _entityMapper (sourceEntity, newEntity));

        s_logger.DebugFormat ("Restored '{0}' in target", newTargetEntityWithVerisonId.Id);
        _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
      }
    }

    public void RestoreChangedInTarget (IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> changes, IDictionary<TSourceEntityId, TSourceEntity> restoreInformation, IDictionary<TTargetEntityId, TTargetEntity> currentTargetEntityCache)
    {
      foreach (var change in changes)
      {
        var sourceEntity = restoreInformation[change.Item1];

        TTargetEntity cachedCurrentTargetEntity;
        currentTargetEntityCache.TryGetValue (change.Item2, out cachedCurrentTargetEntity);

        var newTargetEntityWithVerisonId = _targetEntityRepository.Update (change.Item2, newEntity => _entityMapper (sourceEntity, newEntity), cachedCurrentTargetEntity);

        if (!change.Item2.Equals (newTargetEntityWithVerisonId.Id))
        {
          TTargetEntityId targetEntityId;
          _sourceToTargetEntityRelationStorage.TryRemoveByEntity1 (change.Item1, out targetEntityId);
          _sourceToTargetEntityRelationStorage.AddRelation (change.Item1, newTargetEntityWithVerisonId.Id);
          _knownTargetVersions.DeleteVersion (targetEntityId);
          _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
        }
        else
        {
          _knownTargetVersions.ChangeVersion (newTargetEntityWithVerisonId);
        }


        s_logger.DebugFormat ("Restored '{0}' in target", newTargetEntityWithVerisonId.Id);
      }
    }

    public void SynchronizeAdded (IDictionary<TSourceEntityId, TSourceEntity> addedVersions)
    {
      foreach (var entityById in addedVersions)
      {
        var entityByIdInClosure = entityById.Value;
        var newTargetEntityWithVerisonId = _targetEntityRepository.Create (newEntity => _entityMapper (entityByIdInClosure, newEntity));

        s_logger.DebugFormat ("Created '{0}' in target", newTargetEntityWithVerisonId.Id);
        _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVerisonId.Id);
        _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
      }
    }

    public IDictionary<TTargetEntityId, TTargetEntity> LoadTargetEntityCache (IDictionary<TSourceEntityId, TSourceEntity> sourceChanges, IDictionary<TTargetEntityId, TTargetEntity> targetChanges)
    {
      var targetEntityCache = new Dictionary<TTargetEntityId, TTargetEntity>();
      var targetEntitiesToLoad = new List<TTargetEntityId>();

      foreach (var sourceChange in sourceChanges)
      {
        TTargetEntityId targetEntityId;
        if (_sourceToTargetEntityRelationStorage.TryGetEntity2ByEntity1 (sourceChange.Key, out targetEntityId))
        {
          TTargetEntity targetEntity;

          if (targetChanges.TryGetValue (targetEntityId, out targetEntity))
            targetEntityCache.Add (targetEntityId, targetEntity);
          else
            targetEntitiesToLoad.Add (targetEntityId);
        }
      }

      foreach (var kv in _targetEntityRepository.GetEntities (targetEntitiesToLoad))
        targetEntityCache.Add (kv.Key, kv.Value);

      return targetEntityCache;
    }
  }
}