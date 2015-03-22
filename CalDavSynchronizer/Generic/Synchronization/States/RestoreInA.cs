using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.Synchronization.States
{
  internal class RestoreInA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    public RestoreInA (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData)
        : base(environment, knownData)
    {
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> PerformSyncAction ()
    {
      try
      {
        var newA = _environment.ARepository.Update (_knownData.AtypeId, _aEntity, a => _environment.Mapper.Map2To1 (_bEntity, a));
        return CreateDoNothing (newA.Id, newA.Version, _knownData.BtypeId, _knownData.BtypeVersion);
      }
      catch (Exception x)
      {
        LogException (x);
        return CreateDoNothing ();
      }
    }
  }
}