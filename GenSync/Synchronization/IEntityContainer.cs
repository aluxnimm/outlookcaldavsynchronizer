using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenSync.Logging;

namespace GenSync.Synchronization
{
    public interface IEntityContainer<TEntityId, TEntity, TContext> : IDisposable
    {
        /// <summary>
        /// Selects entites.
        /// It is not allowed to hold a reference to an entity, which is passed to the transform delegate.
        /// </summary>
        Task<IReadOnlyDictionary<TEntityId, T>> GetTransformedEntities<T>(ICollection<TEntityId> ids, ILoadEntityLogger logger, TContext context, Func<EntityWithId<TEntityId, TEntity>, T> transform);

        /// <summary>
        /// It is not allowed to hold a reference to an entity after another call to this interface
        /// Entities must be cleaned with IReadOnlyEntityRepository.Cleanup
        /// </summary>
        Task<IReadOnlyDictionary<TEntityId, TEntity>> GetEntities(ICollection<TEntityId> ids, ILoadEntityLogger logger, TContext context);
    }
}