using System;
using System.Collections.Generic;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.EntityRepositories
{
  public interface IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    IEnumerable<EntityIdWithVersion<TEntityId, TEntityVersion>> GetEntityVersions (DateTime from, DateTime to);
    IDictionary<TEntityId, TEntity> GetEntities (IEnumerable<TEntityId> sourceEntityIds);
    EntityDelta<TEntity, TEntityId> LoadDelta (VersionDelta<TEntityId, TEntityVersion> delta);
  }
}