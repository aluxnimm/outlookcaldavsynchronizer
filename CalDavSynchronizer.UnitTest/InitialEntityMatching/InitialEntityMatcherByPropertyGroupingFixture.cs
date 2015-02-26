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
using CalDavSynchronizer.EntityVersionManagement;
using NUnit.Framework;

namespace CalDavSynchronizer.UnitTest.InitialEntityMatching
{
  [TestFixture]
  public class InitialEntityMatcherByPropertyGroupingFixture
  {
    private VersionStorage<int, int> _atypeVersionStorage;
    private VersionStorage<string, string> _btypeVersionStorage;

    [Test]
    public void PopulateEntityRelationStorage_OneRelationMatches ()
    {
      var result = Test (
          new[]
          {
              new PersonA (1, "Homer", 49, 1),
              new PersonA (2, "Marge", 49, 1),
              new PersonA (3, "Bart", 8, 1),
          },
          new[]
          {
              new PersonB ("one", "Homerx", "49", "1"),
              new PersonB ("two", "Marge", "49", "1"),
              new PersonB ("three", "Bart", "9", "1"),
          }
          );

      Assert.That (result.BbyA.Count, Is.EqualTo (1));
      Assert.That (result.AbyB.Count, Is.EqualTo (1));

      Assert.That (result.BbyA[2], Is.EqualTo ("two"));
      Assert.That (result.AbyB["two"], Is.EqualTo (2));

      Assert.That (_atypeVersionStorage.KnownVersionsForUnitTests.ContainsKey (1), Is.True);
      Assert.That (_atypeVersionStorage.KnownVersionsForUnitTests.ContainsKey (2), Is.True);
      Assert.That (_atypeVersionStorage.KnownVersionsForUnitTests.ContainsKey (3), Is.True);


      Assert.That (_btypeVersionStorage.KnownVersionsForUnitTests.ContainsKey ("one"), Is.True);
      Assert.That (_btypeVersionStorage.KnownVersionsForUnitTests.ContainsKey ("two"), Is.True);
      Assert.That (_btypeVersionStorage.KnownVersionsForUnitTests.ContainsKey ("three"), Is.True);
    }

    [SetUp ()]
    public void Setup ()
    {
      _atypeVersionStorage = new VersionStorage<int, int>();
      _btypeVersionStorage = new VersionStorage<string, string>();
    }

    public TestRelationStorage Test (IEnumerable<PersonA> aPersons, IEnumerable<PersonB> bPersons)
    {
      var entityRelationStorage = new TestRelationStorage();

      var atypeRepository = new TestPersonARepository (aPersons);
      var btypeRepository = new TestPersonBRepository (bPersons);


      new TestInitialEntityMatcher().PopulateEntityRelationStorage (
          entityRelationStorage,
          atypeRepository,
          btypeRepository,
          atypeRepository.GetEntityVersions (DateTime.MinValue, DateTime.MinValue),
          btypeRepository.GetEntityVersions (DateTime.MinValue, DateTime.MinValue),
          _atypeVersionStorage,
          _btypeVersionStorage
          );


      return entityRelationStorage;
    }
  }
}