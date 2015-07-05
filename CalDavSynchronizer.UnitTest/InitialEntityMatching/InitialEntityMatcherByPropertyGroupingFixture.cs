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
using CalDavSynchronizer.Generic.ProgressReport;
using NUnit.Framework;
using System.Linq;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  [TestFixture]
  public class InitialEntityMatcherByPropertyGroupingFixture
  {
    [Test]
    public void PopulateEntityRelationStorage_OneRelationMatches ()
    {
      var aPersons = new[]
                     {
                         new PersonA (new Identifier<int> (1), "Homer", 49, 1),
                         new PersonA (new Identifier<int> (2), "Marge", 49, 1), // Just one of those two is allowed to match
                         new PersonA (new Identifier<int> (4), "Marge", 49, 1), // Just one of those two is allowed to match
                         new PersonA (new Identifier<int> (3), "Bart", 8, 1),
                     };
      var bPersons = new[]
                     {
                         new PersonB (new Identifier<string> ("one"), "Homerx", "49", "1"),
                         new PersonB (new Identifier<string> ("two"), "Marge", "49", "1"),
                         new PersonB (new Identifier<string> ("three"), "Bart", "9", "1"),
                     };

      var atypeIdEqualityComparer = new IdentifierEqualityComparer<int>();
      var btypeIdEqualityComparer = new IdentifierEqualityComparer<string>();

      var atypeEntityVersions = aPersons.ToDictionary (p => p.Id, p => p.Version, atypeIdEqualityComparer);
      var btypeEntityVersions = bPersons.ToDictionary (p => p.Id, p => p.Version, btypeIdEqualityComparer);

      var allAtypeEntities = aPersons.ToDictionary (p => p.Id, atypeIdEqualityComparer);
      var allBtypeEntities = bPersons.ToDictionary (p => p.Id, btypeIdEqualityComparer);

      var foundRelations = new TestInitialEntityMatcher (btypeIdEqualityComparer).FindMatchingEntities (
          new PersonAPersonBRelationDataFactory(),
          allAtypeEntities,
          allBtypeEntities,
          atypeEntityVersions,
          btypeEntityVersions);

      Assert.That (foundRelations.Count, Is.EqualTo (1));
      var relation = foundRelations[0];

      Assert.That (relation.AtypeId.Value, Is.EqualTo (2).Or.EqualTo (4)); // Depends on the implementation which one matches
      Assert.That (relation.AtypeVersion, Is.EqualTo (1));
      Assert.That (relation.BtypeId.Value, Is.EqualTo ("two"));
      Assert.That (relation.BtypeVersion, Is.EqualTo ("1"));
    }
  }
}