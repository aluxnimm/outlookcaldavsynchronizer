using System;

namespace CalDavSynchronizer.Generic.EntityRelationManagement
{
  public interface IEntityRelationData<TAtypeEntityId, TAtypeEntityVersion, TBtypeEntityId, TBtypeEntityVersion>
  {
    TAtypeEntityId AtypeId { get; set; }
    TAtypeEntityVersion AtypeVersion { get; set; }
    TBtypeEntityId BtypeId { get; set; }
    TBtypeEntityVersion BtypeVersion { get; set; }
  }
}