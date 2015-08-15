// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.InitialEntityMatching;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  internal class InitialContactEntityMatcher : InitialEntityMatcherByPropertyGrouping<GenericComObjectWrapper<ContactItem>, string, DateTime, string, vCard, Uri, string, string>
  {
    public InitialContactEntityMatcher (IEqualityComparer<Uri> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (GenericComObjectWrapper<ContactItem> atypeEntity, vCard btypeEntity)
    {
      return true;
    }

    protected override string GetAtypePropertyValue (GenericComObjectWrapper<ContactItem> atypeEntity)
    {
      return atypeEntity.Inner.FirstName + "|" + atypeEntity.Inner.LastName;
    }

    protected override string GetBtypePropertyValue (vCard btypeEntity)
    {
      return btypeEntity.GivenName + "|" + btypeEntity.FamilyName;
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}