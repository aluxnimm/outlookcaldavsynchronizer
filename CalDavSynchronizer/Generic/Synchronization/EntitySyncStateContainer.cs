using System;
using System.Collections.Generic;
using System.Linq;
using CalDavSynchronizer.Generic.Synchronization.States;

namespace CalDavSynchronizer.Generic.Synchronization
{
  /// <summary>
  /// A container for syncstates, that tracks all created states and disposes them on demand
  /// </summary>
  /// <remarks>
  /// Disposing a SyncState means that the state will  set all references to entities to their default value (which is null for ref types)
  /// This may be helpful, for implementations where the Entity has an underlying COM-Object
  /// </remarks>
  internal class EntitySyncStateContainer<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> : IDisposable
  {
    private List<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>> _entitySyncStates = new List<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>>();
    private readonly HashSet<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>> _allSyncStatesThatWereCreated = new HashSet<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>>();

    public void Add (IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> syncState)
    {
      _entitySyncStates.Add (syncState);
      _allSyncStatesThatWereCreated.Add (syncState);
    }

    public void DoTransition (Func<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>, IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>> transitionFunction)
    {
      _entitySyncStates = _entitySyncStates.Select (transitionFunction).ToList();
      _entitySyncStates.ForEach (s => _allSyncStatesThatWereCreated.Add (s));
    }

    public void Execute (Action<IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>> action)
    {
      _entitySyncStates.ForEach (action);
    }

    public void Dispose ()
    {
      foreach (var syncState in _allSyncStatesThatWereCreated)
        syncState.Dispose ();
    }

    public int Count
    {
      get { return _entitySyncStates.Count; }
    }
  }
}