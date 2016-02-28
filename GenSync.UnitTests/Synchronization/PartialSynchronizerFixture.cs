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
            CreateIdentifiers ("l1"),
            CreateIdentifiers());

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
    public async Task TwoWaySynchronize_ChangedLocal_VersionSpecified (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      _localRepository.UpdateWithoutIdChange ("l1", _ => "Item 1 upd");
      _localRepository.UpdateWithoutIdChange ("l2", _ => "Item 2 upd");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) 0, null), // Same version as knyown version specified => isn't synced
                IdWithHints.Create (new Identifier ("l2"), (int?) 999, null) // Other version as knyown version specified => is synced
            },
            CreateIdentifiers());

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "Item 1 upd");
        AssertLocal ("l2", 1, "Item 2 upd");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2u", 1, "Item 2 upd");
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
    public async Task TwoWaySynchronize_DeletedLocal_WithoutHint (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents ();

      _localRepository.Delete ("l1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) null, null), 
            },
            CreateIdentifiers ());

        AssertLocalCount (0);

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (0);
        AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedLocal_WithHint (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents ();

      _localRepository.Delete ("l1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) null, true), 
            },
            CreateIdentifiers ());

        AssertLocalCount (0);

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (0);
        AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedLocal_WithWrongHint1 (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents ();

      _localRepository.Delete ("l1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) null, false), 
            },
            CreateIdentifiers ());

        AssertLocalCount (0);

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (0);
        AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedLocal_WithWrongHint2 (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents ();

      _localRepository.Delete ("l1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) 666, null),
            },
            CreateIdentifiers ());

        AssertLocalCount (0);

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (0);
        AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_AddedLocal_WithoutHint (GenericConflictResolution conflictWinner)
    {
      await _localRepository.Create (v => "Item 1");
      await _localRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) null, null),
            },
            CreateIdentifiers ());

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s1", 0, "Item 1");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_AddedLocal_WithHint (GenericConflictResolution conflictWinner)
    {
      await _localRepository.Create (v => "Item 1");
      await _localRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) 0, null),
            },
            CreateIdentifiers ());

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s1", 0, "Item 1");
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_AddedLocal_WithWrongHint (GenericConflictResolution conflictWinner)
    {
      await _localRepository.Create (v => "Item 1");
      await _localRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizePartialTwoWay (
            conflictWinner,
            new[]
            {
                IdWithHints.Create (new Identifier ("l1"), (int?) null, true), // specifying deletion hint true for a new item causes the item not to be synced
            },
            CreateIdentifiers ());

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (0);
      });

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
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
            CreateIdentifiers ("l1"),
            CreateIdentifiers ("s1"));

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "Item 1 upd");
        AssertLocal ("l2", 1, "Item 2 upd");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "Item 1 upd");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    private static IIdWithHints<Identifier, int>[] CreateIdentifiers (params string[] values)
    {
      return values.Select (v => IdWithHints.Create (new Identifier (v), (int?) null, null)).ToArray();
    }
  }
}