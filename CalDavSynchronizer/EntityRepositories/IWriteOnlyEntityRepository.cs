using System;
using CalDavSynchronizer.Generic.EntityVersionManagement;

namespace CalDavSynchronizer.EntityRepositories
{
  public interface IWriteOnlyEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    bool Delete (TEntityId entityId);
    EntityIdWithVersion<TEntityId, TEntityVersion> Update (TEntityId entityId, TEntity entityToUpdate, Func<TEntity, TEntity> entityModifier);
    EntityIdWithVersion<TEntityId, TEntityVersion> Create (Func<TEntity, TEntity> entityInitializer);
  }
}