using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class EntityRelationData : IEntityRelationData<string, int, string, int>
  {
    public EntityRelationData (string atypeId, int atypeVersion, string btypeId, int btypeVersion)
    {
      AtypeId = atypeId;
      AtypeVersion = atypeVersion;
      BtypeId = btypeId;
      BtypeVersion = btypeVersion;
    }

    public string AtypeId { get; set; }
    public int AtypeVersion { get; set; }
    public string BtypeId { get; set; }
    public int BtypeVersion { get; set; }
  }
}