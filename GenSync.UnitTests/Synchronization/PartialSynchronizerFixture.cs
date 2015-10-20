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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenSync.UnitTests.Synchronization.Stubs;
using NUnit.Framework;

namespace GenSync.UnitTests.Synchronization
{
  [TestFixture]
  internal class PartialSynchronizerFixture : SynchronizerFixtureBase
  {
    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_ChangedLocal (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      _localRepository.UpdateWithoutIdChange ("l1", _ => "Item 1 upd");
      _localRepository.UpdateWithoutIdChange ("l2", _ => "Item 2 upd");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            C ("l1"),
            C());

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "Item 1 upd");
        AssertLocal ("l2", 1, "Item 2 upd");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "Item 1 upd");
        AssertServer ("s2", 0, "Item 2");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "Item 1 upd");
        AssertLocal ("l2", 1, "Item 2 upd");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "Item 1 upd");
        AssertServer ("s2u", 1, "Item 2 upd");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_ChangedLocal_SpecifiedTwice (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      _localRepository.UpdateWithoutIdChange ("l1", _ => "Item 1 upd");
      _localRepository.UpdateWithoutIdChange ("l2", _ => "Item 2 upd");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            C ("l1"),
            C ("s1"));

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "Item 1 upd");
        AssertLocal ("l2", 1, "Item 2 upd");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "Item 1 upd");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    private static Identifier[] C (params string[] values)
    {
      return values.Select (v => new Identifier (v)).ToArray();
    }
  }
}