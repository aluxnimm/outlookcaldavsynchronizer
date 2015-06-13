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
using CalDavSynchronizer.Generic.InitialEntityMatching;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  internal class TestInitialEntityMatcher : InitialEntityMatcherByPropertyGrouping<PersonA, int, int, int, PersonB, string, string, string>
  {
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