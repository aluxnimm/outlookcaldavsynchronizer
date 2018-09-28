namespace GenSync.Logging
{
  public class NullFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> : IFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>
    {
    public static readonly IFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> Instance = new NullFullEntitySynchronizationLoggerFactory<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>();

    private NullFullEntitySynchronizationLoggerFactory()
    {
     
    }

    public IFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity> CreateEntitySynchronizationLogger(SynchronizationOperation operation)
    {
      return NullFullEntitySynchronizationLogger<TAtypeEntityId, TAtypeEntity, TBtypeEntityId, TBtypeEntity>.Instance;
    }
  }
}