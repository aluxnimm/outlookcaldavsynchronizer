using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.Synchronization.States
{
  internal class UpdateBToA<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
      : UpdateBase<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    private readonly TBtypeEntityVersion _newBVersion;

    public UpdateBToA (EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> environment, IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> knownData, TBtypeEntityVersion newBVersion)
        : base(environment, knownData)
    {
      _newBVersion = newBVersion;
    }

    public override IEntitySyncState<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> PerformSyncAction ()
    {
      try
      {
        var newA = _environment.ARepository.Update (_knownData.AtypeId, _aEntity, a => _environment.Mapper.Map2To1 (_bEntity, a));
        return CreateDoNothing (newA.Id, newA.Version, _knownData.BtypeId, _newBVersion);
      }
      catch (Exception x)
      {
        LogException (x);
        return CreateDoNothing();
      }
    }


    public override void AddNewData (Action<IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>> addAction)
    {
      throw new InvalidOperationException();
    }
  }
}