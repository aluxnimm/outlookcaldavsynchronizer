using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.Implementation
{
  public class OutlookEventRelationDataFactory : IEntityRelationDataFactory<string, DateTime, Uri, string>
  {
    public IEntityRelationData<string, DateTime, Uri, string> Create (string atypeId, DateTime atypeVersion, Uri btypeId, string btypeVersion)
    {
      return new OutlookEventRelationData()
             {
                 AtypeId = atypeId,
                 AtypeVersion = atypeVersion,
                 BtypeId = btypeId,
                 BtypeVersion = btypeVersion
             };
    }
  }
}