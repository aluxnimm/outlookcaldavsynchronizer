using System;
using System.Collections.Generic;

namespace CalDavSynchronizer.EntityRepositories
{
  public interface IReadOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    Dictionary<TEntityId, TEntityVersion> GetEntityVersions (DateTime from, DateTime to);
    IDictionary<TEntityId, TEntity> GetEntities (ICollection<TEntityId> sourceEntityIds);
  }
}