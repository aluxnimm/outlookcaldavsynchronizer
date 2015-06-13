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
                         new PersonA (1, "Homer", 49, 1),
                         new PersonA (2, "Marge", 49, 1),
                         new PersonA (3, "Bart", 8, 1),
                     };
      var bPersons = new[]
                     {
                         new PersonB ("one", "Homerx", "49", "1"),
                         new PersonB ("two", "Marge", "49", "1"),
                         new PersonB ("three", "Bart", "9", "1"),
                     };


      var atypeRepository = new TestPersonARepository (aPersons);
      var btypeRepository = new TestPersonBRepository (bPersons);


      var atypeEntityVersions = atypeRepository.GetVersions (DateTime.MinValue, DateTime.MinValue);
      var btypeEntityVersions = btypeRepository.GetVersions (DateTime.MinValue, DateTime.MinValue);

      var allAtypeEntities = atypeRepository.Get (atypeEntityVersions.Keys, NullTotalProgressLogger.Instance);
      var allBtypeEntities = btypeRepository.Get (btypeEntityVersions.Keys, NullTotalProgressLogger.Instance);

      var foundRelations = new TestInitialEntityMatcher().PopulateEntityRelationStorage (
          new PersonAPersonBRelationDataFactory(),
          allAtypeEntities,
          allBtypeEntities,
          atypeEntityVersions,
          btypeEntityVersions);

      Assert.That (foundRelations.Count, Is.EqualTo (1));
      var relation = foundRelations[0];

      Assert.That (relation.AtypeId, Is.EqualTo (2));
      Assert.That (relation.AtypeVersion, Is.EqualTo (1));
      Assert.That (relation.BtypeId, Is.EqualTo ("two"));
      Assert.That (relation.BtypeVersion, Is.EqualTo ("1"));
    }
  }
}