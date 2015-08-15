using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenSync.EntityRepositories
{
  public class EntityRepositoryDeleteCreateInstaedOfUpdateWrapper<TEntity, TEntityId, TEntityVersion> : IEntityRepository<TEntity, TEntityId, TEntityVersion>
  {
    private readonly IEntityRepository<TEntity, TEntityId, TEntityVersion> _inner;

    public EntityRepositoryDeleteCreateInstaedOfUpdateWrapper (IEntityRepository<TEntity, TEntityId, TEntityVersion> inner)
    {
      if (inner == null)
        throw new ArgumentNullException ("inner");

      _inner = inner;
    }

    public bool Delete (TEntityId entityId)
    {
      return _inner.Delete (entityId);
    }

    public EntityIdWithVersion<TEntityId, TEntityVersion> Update (TEntityId entityId, TEntity entityToUpdate, Func<TEntity, TEntity> entityModifier)
    {
      return _inner.Create (newEntity =>
      {
        _inner.Delete (entityId);
        return entityModifier (newEntity);
      });
    }

    public EntityIdWithVersion<TEntityId, TEntityVersion> Create (Func<TEntity, TEntity> entityInitializer)
    {
      return _inner.Create (entityInitializer);
    }


    public IReadOnlyList<EntityIdWithVersion<TEntityId, TEntityVersion>> GetVersions ()
    {
      return _inner.GetVersions();
    }

    public Task<IReadOnlyList<EntityWithVersion<TEntityId, TEntity>>> Get (ICollection<TEntityId> ids)
    {
      return _inner.Get (ids);
    }

    public void Cleanup (IReadOnlyDictionary<TEntityId, TEntity> entities)
    {
      _inner.Cleanup (entities);
    }
  }
}