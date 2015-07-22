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
using GenSync.InitialEntityMatching;

namespace GenSync.UnitTests.InitialEntityMatching
{
  internal class TestInitialEntityMatcher : InitialEntityMatcherByPropertyGrouping<PersonA, Identifier<int>, int, int, PersonB, Identifier<string>, string, string>
  {
    public TestInitialEntityMatcher (IEqualityComparer<Identifier<string>> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (PersonA atypeEntity, PersonB btypeEntity)
    {
      return atypeEntity.Name == btypeEntity.Name &&
             atypeEntity.Age.ToString() == btypeEntity.Age;
    }

    protected override int GetAtypePropertyValue (PersonA atypeEntity)
    {
      return atypeEntity.Age;
    }

    protected override string GetBtypePropertyValue (PersonB btypeEntity)
    {
      return btypeEntity.Age;
    }

    protected override string MapAtypePropertyValue (int value)
    {
      return value.ToString();
    }
  }
}