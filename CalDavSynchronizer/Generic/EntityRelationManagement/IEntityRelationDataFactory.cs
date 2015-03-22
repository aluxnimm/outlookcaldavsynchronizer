using System;

namespace CalDavSynchronizer.Generic.EntityRelationManagement
{
  public interface IEntityRelationDataFactory<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion> Create (
        TAtypeEntityId atypeId,
        TAtypeEntityVersion atypeVersion,
        TBtypeEntityId btypeId,
        TBtypeEntityVersion btypeVersion);
  }
}