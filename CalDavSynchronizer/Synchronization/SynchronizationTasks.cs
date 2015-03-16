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

    private void TryExecute<T> (Func<T> repositoryOperation, Action<T> cacheUpdateOperation, TTargetEntityId targetIdForLogging)
    {
      TryExecute (repositoryOperation, cacheUpdateOperation, default(TSourceEntityId), targetIdForLogging);
    }

    private void TryExecute<T> (Func<T> repositoryOperation, Action<T> cacheUpdateOperation, TSourceEntityId sourceIdForLogging)
    {
      TryExecute (repositoryOperation, cacheUpdateOperation, sourceIdForLogging, default(TTargetEntityId));
    }

    private void TryExecute<T> (Func<T> repositoryOperation, Action<T> cacheUpdateOperation, TSourceEntityId sourceIdForLogging, TTargetEntityId targetIdForLogging)
    {
      bool success = false;
      T result = default (T);
      try
      {
        result = repositoryOperation();
        success = true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Error occured, skipping entity. Source: '{0}', Target '{1}'", sourceIdForLogging, targetIdForLogging), x);
      }

      if (success)
      {
        cacheUpdateOperation (result);
      }
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

          TryExecute (
              () => _targetEntityRepository.Update (targetEntityId, target => _entityMapper (entityByIdInClosure, target), cachedCurrentTargetEntity),
              newTargetEntityWithVersionId =>
              {
                if (!targetEntityId.Equals (newTargetEntityWithVersionId.Id))
                {
                  _sourceToTargetEntityRelationStorage.TryRemoveByEntity1 (entityById.Key, out targetEntityId);
                  _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVersionId.Id);
                  _knownTargetVersions.DeleteVersion (targetEntityId);
                  _knownTargetVersions.AddVersion (newTargetEntityWithVersionId);
                }
                else
                {
                  _knownTargetVersions.ChangeVersion (newTargetEntityWithVersionId);
                }
              },
              entityById.Key,
              targetEntityId
              );
        }
        else
        {
          var entityByIdInClosure = entityById.Value;
          TryExecute (
              () => _targetEntityRepository.Create (newEntity => _entityMapper (entityByIdInClosure, newEntity)),
              newTargetEntityWithVerisonId =>
              {
                s_logger.DebugFormat ("Created '{0}' in target", targetEntityId);
                _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVerisonId.Id);
                _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
              },
              entityById.Key
              );
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
          TryExecute (
              () => _targetEntityRepository.Delete (targetEntityId),
              _ => _knownTargetVersions.DeleteVersion (targetEntityId),
              deleted.Key,
              targetEntityId
              );
        }
      }
    }

    public void DeleteAdded (IEnumerable<TTargetEntityId> addedVersions)
    {
      foreach (var addedId in addedVersions)
      {
        s_logger.DebugFormat ("Deleting '{0}' in target", addedId);
        TryExecute (
            () => _targetEntityRepository.Delete (addedId),
            _ => _knownTargetVersions.DeleteVersion (addedId),
            addedId
            );
      }
    }

    public void RestoreDeletedInTarget (IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> deletions, IDictionary<TSourceEntityId, TSourceEntity> restoreInformation)
    {
      foreach (var deletion in deletions)
      {
        var sourceEntity = restoreInformation[deletion.Item1];
        TryExecute (
            () => _targetEntityRepository.Create (newEntity => _entityMapper (sourceEntity, newEntity)),
            newTargetEntityWithVerisonId =>
            {
              s_logger.DebugFormat ("Restored '{0}' in target", newTargetEntityWithVerisonId.Id);
              _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
            },
            deletion.Item1
            );
      }
    }

    public void RestoreChangedInTarget (IEnumerable<Tuple<TSourceEntityId, TTargetEntityId>> changes, IDictionary<TSourceEntityId, TSourceEntity> restoreInformation, IDictionary<TTargetEntityId, TTargetEntity> currentTargetEntityCache)
    {
      foreach (var change in changes)
      {
        var sourceEntity = restoreInformation[change.Item1];

        TTargetEntity cachedCurrentTargetEntity;
        currentTargetEntityCache.TryGetValue (change.Item2, out cachedCurrentTargetEntity);


        TryExecute (
            () => _targetEntityRepository.Update (change.Item2, newEntity => _entityMapper (sourceEntity, newEntity), cachedCurrentTargetEntity),
            newTargetEntityWithVerisonId =>
            {
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
            },
            change.Item1,
            change.Item2
            );
      }
    }

    public void SynchronizeAdded (IDictionary<TSourceEntityId, TSourceEntity> addedVersions)
    {
      foreach (var entityById in addedVersions)
      {
        var entityByIdInClosure = entityById.Value;

        TryExecute (
            () => _targetEntityRepository.Create (newEntity => _entityMapper (entityByIdInClosure, newEntity)),
            newTargetEntityWithVerisonId =>
            {
              s_logger.DebugFormat ("Created '{0}' in target", newTargetEntityWithVerisonId.Id);
              _sourceToTargetEntityRelationStorage.AddRelation (entityById.Key, newTargetEntityWithVerisonId.Id);
              _knownTargetVersions.AddVersion (newTargetEntityWithVerisonId);
            },
            entityById.Key
            );
      }
    }

    public IDictionary<TTargetEntityId, TTargetEntity> LoadTargetEntityCache (IDictionary<TSourceEntityId, TSourceEntity> sourceChanges, IDictionary<TTargetEntityId, TTargetEntity> targetChanges)
    {
      try
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
      catch (Exception x)
      {
        s_logger.ErrorFormat ("Could not load target entity cache. Using empty cache, which will result in poor performance", x);
        return new Dictionary<TTargetEntityId, TTargetEntity>();
      }
    }
  }
}