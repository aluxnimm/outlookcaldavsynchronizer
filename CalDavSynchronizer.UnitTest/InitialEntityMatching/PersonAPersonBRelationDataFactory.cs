using System;
using CalDavSynchronizer.Generic.EntityRelationManagement;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  class PersonAPersonBRelationDataFactory : IEntityRelationDataFactory<int, int, string, string>
  {
    public IEntityRelationData<int, int, string, string> Create (int atypeId, int atypeVersion, string btypeId, string btypeVersion)
    {
      return new PersonAPersonBRelationData (atypeId, atypeVersion, btypeId, btypeVersion);
    }
  }
}