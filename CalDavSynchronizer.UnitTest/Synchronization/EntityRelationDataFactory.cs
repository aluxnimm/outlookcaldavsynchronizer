using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  internal class EntityRelationDataFactory : IEntityRelationDataFactory<string, int, string, int>
  {
    public IEntityRelationData<string, int, string, int> Create (string atypeId, int atypeVersion, string btypeId, int btypeVersion)
    {
      return new EntityRelationData (atypeId, atypeVersion, btypeId, btypeVersion);
    }
  }
}