// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer
// Copyright (c) 2015 Alexander Nimmervoll
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.InitialEntityMatching;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.DistributionLists
{
  internal class InitialSogoDistListEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, GenericComObjectWrapper<DistListItem>, string, WebResourceName, string, DistributionList, string>
  {
    public InitialSogoDistListEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual(GenericComObjectWrapper<DistListItem> atypeEntity, DistributionList btypeEntity)
    {
      return atypeEntity.Inner.DLName == btypeEntity.Name;
    }

    protected override string GetAtypePropertyValue(GenericComObjectWrapper<DistListItem> atypeEntity)
    {
      return atypeEntity.Inner.DLName;
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

  internal class InitialDistListEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, GenericComObjectWrapper<DistListItem>, string, WebResourceName, string, vCard, string>
  {
    public InitialDistListEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (GenericComObjectWrapper<DistListItem> atypeEntity, vCard btypeEntity)
    {
      return atypeEntity.Inner.DLName == btypeEntity.DisplayName;
    }

    protected override string GetAtypePropertyValue (GenericComObjectWrapper<DistListItem> atypeEntity)
    {
      return atypeEntity.Inner.DLName;
    }

    protected override string GetBtypePropertyValue (vCard btypeEntity)
    {
      return btypeEntity.DisplayName;
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}