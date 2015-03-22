using System;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Generic.Synchronization.States
{
  /// <summary>
  /// Environment for the state
  /// NOTE: this is not the Context from the State-Pattern !!!
  /// </summary>
  public class EntitySyncStateEnvironment<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity>
  {
    public IEntityMapper<TAtypeEntity, TBtypeEntity> Mapper { get; private set; }
    public IWriteOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> ARepository { get; private set; }
    public IWriteOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> BRepository { get; private set; }
    public IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> DataFactory { get; private set; }
    public IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> StateFactory { get; private set; }

    public EntitySyncStateEnvironment (IEntityMapper<TAtypeEntity, TBtypeEntity> mapper, IWriteOnlyEntityRepository<TAtypeEntity, TAtypeEntityId, TAtypeEntityVersion> aRepository, IWriteOnlyEntityRepository<TBtypeEntity, TBtypeEntityId, TBtypeEntityVersion> bRepository, IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> dataFactory, IEntitySyncStateFactory<TAtypeEntityId, TAtypeEntityVersion, TAtypeEntity, TBtypeEntityId, TBtypeEntityVersion, TBtypeEntity> stateFactory)
    {
      Mapper = mapper;
      ARepository = aRepository;
      BRepository = bRepository;
      DataFactory = dataFactory;
      StateFactory = stateFactory;
    }

  }
}