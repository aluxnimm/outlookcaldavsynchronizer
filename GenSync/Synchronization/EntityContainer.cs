using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.Utilities;
using log4net;

namespace GenSync.Synchronization
{
    public class EntityContainer<TEntityId, TEntityVersion, TEntity, TContext> : IEntityContainer<TEntityId, TEntity, TContext>
    {
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

        private readonly IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> _repository;
        private readonly Dictionary<TEntityId, TEntity> _cachedEntities;
        private readonly int? _maximumCacheSize;
        private readonly IEqualityComparer<TEntityId> _idComparer;
        private readonly IChunkedExecutor _chunkedExecutor;

        public EntityContainer(IReadOnlyEntityRepository<TEntityId, TEntityVersion, TEntity, TContext> repository, IEqualityComparer<TEntityId> idComparer, int? maximumCacheSize, IChunkedExecutor chunkedExecutor)
        {
            _idComparer = idComparer;
            _repository = repository;
            _maximumCacheSize = maximumCacheSize;
            _chunkedExecutor = chunkedExecutor;
            _cachedEntities = new Dictionary<TEntityId, TEntity>(idComparer);
        }

        public async Task<IReadOnlyDictionary<TEntityId, T>> GetTransformedEntities<T>(ICollection<TEntityId> ids, ILoadEntityLogger logger, TContext context, Func<EntityWithId<TEntityId, TEntity>, T> transform)
        {
            return await _chunkedExecutor.ExecuteAsync(
                new Dictionary<TEntityId, T>(_idComparer),
                ids,
                async (chunk, transformedEntities) =>
                {
                    var entites = CreateEnumerableWhichCachesOrCleansUp(await _repository.Get(chunk, logger, context));
                    foreach (var entity in entites)
                    {
                        if (!transformedEntities.ContainsKey(entity.Id))
                            transformedEntities.Add(entity.Id, transform(entity));
                        else
                            s_logger.WarnFormat("Entitiy '{0}' was contained multiple times in repository response. Ignoring redundant entity", entity.Id);
                    }
                });
        }

        IEnumerable<EntityWithId<TEntityId, TEntity>> CreateEnumerableWhichCachesOrCleansUp(IEnumerable<EntityWithId<TEntityId, TEntity>> entities)
        {
            foreach (var entity in entities)
            {
                yield return entity;

                if (_maximumCacheSize == null || _cachedEntities.Count < _maximumCacheSize)
                    AddToDictionary(_cachedEntities, entity);
                else
                    _repository.Cleanup(entity.Entity);
            }
        }

        public async Task<IReadOnlyDictionary<TEntityId, TEntity>> GetEntities(ICollection<TEntityId> ids, ILoadEntityLogger logger, TContext context)
        {
            var entitesToCleanup = new List<TEntity>();
            var entitiesToLoad = new HashSet<TEntityId>(ids, _idComparer);

            foreach (var kvPair in _cachedEntities.ToArray())
            {
                if (ids.Contains(kvPair.Key))
                {
                    entitiesToLoad.Remove(kvPair.Key);
                }
                else
                {
                    _cachedEntities.Remove(kvPair.Key);
                    entitesToCleanup.Add(kvPair.Value);
                }
            }

            _repository.Cleanup(entitesToCleanup);

            if (entitiesToLoad.Any())
                AddToDictionary(_cachedEntities, await _repository.Get(entitiesToLoad, logger, context));

            return _cachedEntities;
        }

        private static void AddToDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, IEnumerable<EntityWithId<TKey, TValue>> tuples)
        {
            foreach (var tuple in tuples)
            {
                AddToDictionary(dictionary, tuple);
            }
        }

        private static void AddToDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary, EntityWithId<TKey, TValue> tuple)
        {
            if (!dictionary.ContainsKey(tuple.Id))
                dictionary.Add(tuple.Id, tuple.Entity);
            else
                s_logger.WarnFormat("Entitiy '{0}' was contained multiple times in repository response. Ignoring redundant entity", tuple.Id);
        }

        public void Dispose()
        {
            if (_cachedEntities.Any())
            {
                _repository.Cleanup(_cachedEntities.Values);
                _cachedEntities.Clear();
            }
        }
    }
}