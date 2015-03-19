using System;
using CalDavSynchronizer.EntityVersionManagement;

namespace CalDavSynchronizer.EntityRepositories
{
  public interface IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    bool Delete (TEntityId entityId);
    EntityIdWithVersion<TEntityId, TEntityVersion> Update (TEntityId entityId, Func<TEntity, TEntity> entityModifier, TEntity cachedCurrentTargetEntityIfAvailable);
    EntityIdWithVersion<TEntityId, TEntityVersion> Create (Func<TEntity, TEntity> entityInitializer);
  }
}