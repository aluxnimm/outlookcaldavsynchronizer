using System.Collections.Generic;

namespace GenSync.Synchronization
{
    public class EntitySyncStateChunkCreator<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>
    {
        private readonly int? _maxNumberOfEntitesToLoadPerRepositoryPerBatch;

        public EntitySyncStateChunkCreator(int? maxNumberOfEntitesToLoadPerRepositoryPerBatch)
        {
            _maxNumberOfEntitesToLoadPerRepositoryPerBatch = maxNumberOfEntitesToLoadPerRepositoryPerBatch;
        }

        public IEnumerable<(
            HashSet<TAtypeEntityId> AEntitesToLoad,
            HashSet<TBtypeEntityId> BEntitesToLoad,
            IReadOnlyList<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>> Contexts
            )> CreateChunks(
            IEnumerable<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>> contexts,
            IEqualityComparer<TAtypeEntityId> aIdComparer,
            IEqualityComparer<TBtypeEntityId> bIdComparer)
        {
            var contextsEnumerator = contexts.GetEnumerator();

            for (var currentBach = GetNextBatch(); currentBach.contexts.Count > 0; currentBach = GetNextBatch())
                yield return currentBach;

            (
                HashSet<TAtypeEntityId> aEntitesToLoad,
                HashSet<TBtypeEntityId> bEntitesToLoad,
                IReadOnlyList<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>> contexts
                ) GetNextBatch()
            {
                var currentBatch = new List<IEntitySyncStateContext<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity, TContext>>();
                var aEntitesToLoad = new HashSet<TAtypeEntityId>(aIdComparer);
                var bEntitesToLoad = new HashSet<TBtypeEntityId>(bIdComparer);

                while (
                    (_maxNumberOfEntitesToLoadPerRepositoryPerBatch == null || aEntitesToLoad.Count < _maxNumberOfEntitesToLoadPerRepositoryPerBatch && bEntitesToLoad.Count < _maxNumberOfEntitesToLoadPerRepositoryPerBatch) &&
                    contextsEnumerator.MoveNext())
                {
                    currentBatch.Add(contextsEnumerator.Current);
                    contextsEnumerator.Current.AddRequiredEntitiesToLoad(aEntitesToLoad.Add, bEntitesToLoad.Add);
                }

                return (aEntitesToLoad, bEntitesToLoad, currentBatch);
            }
        }
    }
}