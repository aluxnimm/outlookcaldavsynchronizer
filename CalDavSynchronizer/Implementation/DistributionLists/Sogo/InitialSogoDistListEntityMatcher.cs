using System;
using System.Collections.Generic;
using CalDavSynchronizer.DataAccess;
using GenSync.InitialEntityMatching;

namespace CalDavSynchronizer.Implementation.DistributionLists.Sogo
{
  internal class InitialSogoDistListEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, DistListMatchData, string, WebResourceName, string, DistributionList, string>
  {
    public InitialSogoDistListEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
      : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual(DistListMatchData atypeEntity, DistributionList btypeEntity)
    {
      return atypeEntity.DlName == btypeEntity.Name;
    }

    protected override string GetAtypePropertyValue(DistListMatchData atypeEntity)
    {
      return atypeEntity.DlName;
    }

    protected override string GetBtypePropertyValue(DistributionList btypeEntity)
    {
      return btypeEntity.Name;
    }

    protected override string MapAtypePropertyValue(string value)
    {
      return value;
    }
  }
}