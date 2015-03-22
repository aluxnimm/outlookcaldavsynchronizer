using System;
using System.Collections.Generic;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.Synchronization.States
{
  internal abstract class UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : StateWithKnownData<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
   
    protected TAtypeEntity _aEntity;
    protected TBtypeEntity _bEntity;


    protected UpdateBase (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
        : base(environment, knownData)
    {
    }

    public override void AddRequiredEntitiesToLoad (Func<TAtypeEntityId, bool> a, Func<TBtypeEntityId, bool> b)
    {
      a (_knownData.AtypeId);
      b (_knownData.BtypeId);
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> FetchRequiredEntities (IDictionary<TAtypeEntityId, TAtypeEntity> aEntities, IDictionary<TBtypeEntityId, TBtypeEntity> bEntites)
    {
      if (!aEntities.TryGetValue (_knownData.AtypeId, out _aEntity))
        return CreateDoNothing();

      if (!bEntites.TryGetValue (_knownData.BtypeId, out _bEntity))
        return CreateDoNothing();

      return this;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> Resolve ()
    {
      return this;
    }


    public override void AddNewData (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      throw new InvalidOperationException();
    }
  }
}