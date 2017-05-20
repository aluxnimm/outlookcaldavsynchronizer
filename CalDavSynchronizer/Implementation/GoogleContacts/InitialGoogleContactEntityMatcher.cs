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
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.InitialEntityMatching;
using Google.Contacts;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  internal class InitialGoogleContactEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, IContactItemWrapper, string, string, GoogleContactVersion, GoogleContactWrapper , string>
  {
    public InitialGoogleContactEntityMatcher (IEqualityComparer<string> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (IContactItemWrapper atypeEntity, GoogleContactWrapper btypeEntity)
    {
      return true;
    }

    protected override string GetAtypePropertyValue (IContactItemWrapper atypeEntity)
    {
      return atypeEntity.Inner.FirstName + "|" + atypeEntity.Inner.LastName;
    }

    protected override string GetBtypePropertyValue (GoogleContactWrapper btypeEntity)
    {
      return btypeEntity.Contact.Name.GivenName + "|" + btypeEntity.Contact.Name.FamilyName;
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}