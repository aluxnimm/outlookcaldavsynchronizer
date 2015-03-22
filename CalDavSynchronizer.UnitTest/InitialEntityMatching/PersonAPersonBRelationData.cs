using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  public class PersonAPersonBRelationData : IEntityRelationData<int, int, string, string>
  {
    public PersonAPersonBRelationData (int atypeId, int atypeVersion, string btypeId, string btypeVersion)
    {
      AtypeId = atypeId;
      AtypeVersion = atypeVersion;
      BtypeId = btypeId;
      BtypeVersion = btypeVersion;
    }

    public int AtypeId { get; set; }

    public int AtypeVersion { get; set; }

    public string BtypeId { get; set; }

    public string BtypeVersion { get; set; }
  }
}