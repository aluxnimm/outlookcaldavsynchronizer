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
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.EntityRepositories
{
  public abstract class EntityRepositoryBase<TEntity, TEntityId, TEntityVersion> : IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>, IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion> 
  {
    public abstract IEnumerable<EntityIdWithVersion<TEntityId, TEntityVersion>> GetEntityVersions (DateTime from, DateTime to);
    public abstract IDictionary<TEntityId, TEntity> GetEntities (ICollection<TEntityId> sourceEntityIds);

    public abstract bool Delete (TEntityId entityId);
    public abstract EntityIdWithVersion<TEntityId, TEntityVersion> Update (TEntityId entityId, Func<TEntity, TEntity> entityModifier, TEntity cachedCurrentTargetEntityIfAvailable);
    public abstract EntityIdWithVersion<TEntityId, TEntityVersion> Create (Func<TEntity, TEntity> entityInitializer);

    public EntityDelta<TEntity, TEntityId> LoadDelta (VersionDelta<TEntityId, TEntityVersion> delta)
    {
      var addedAndChanged = GetEntities (delta.Added.Union (delta.Changed).Select (v => v.Id).ToArray());

      var added = new Dictionary<TEntityId, TEntity>();
      foreach (var addedVersion in delta.Added)
      {
        TEntity addedEntity;
        // if entity coulkd not be deserialized, then it will not be present in the dictionary
        if (addedAndChanged.TryGetValue (addedVersion.Id, out addedEntity))
          added.Add (addedVersion.Id, addedEntity);
      }

      var changed = new Dictionary<TEntityId, TEntity> ();
      foreach (var changedVersion in delta.Changed)
      {
        TEntity changedEntity;
        // if entity coulkd not be deserialized, then it will not be present in the dictionary
        if (addedAndChanged.TryGetValue (changedVersion.Id, out changedEntity))
          changed.Add (changedVersion.Id, changedEntity);
      }
      
      var deleted = delta.Deleted.ToDictionary (k => k.Id, v => true);
      return new EntityDelta<TEntity, TEntityId> (added, changed, deleted);
    }
  }
}