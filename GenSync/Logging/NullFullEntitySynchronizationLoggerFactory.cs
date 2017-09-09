namespace GenSync.Logging
{
  public class NullFullEntitySynchronizationLoggerFactory<TAtypeEntity, TBtypeEntity> : IFullEntitySynchronizationLoggerFactory<TAtypeEntity, TBtypeEntity>
  {
    public static readonly IFullEntitySynchronizationLoggerFactory<TAtypeEntity, TBtypeEntity> Instance = new NullFullEntitySynchronizationLoggerFactory<TAtypeEntity, TBtypeEntity>();

    private NullFullEntitySynchronizationLoggerFactory()
    {
     
    }

    public IFullEntitySynchronizationLogger<TAtypeEntity, TBtypeEntity> CreateEntitySynchronizationLogger(SynchronizationOperation operation)
    {
      return NullFullEntitySynchronizationLogger<TAtypeEntity, TBtypeEntity>.Instance;
    }
  }
}