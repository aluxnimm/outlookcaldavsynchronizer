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
using System.Threading.Tasks;
using GenSync.UnitTests.Synchronization.Stubs;
using NUnit.Framework;

namespace GenSync.UnitTests.Synchronization
{
  [TestFixture]
  internal class TwoWaySynchronizerFixture : SynchronizerFixtureBase
  {
    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_AddedLocal (GenericConflictResolution conflictWinner)
    {
      await _localRepository.Create (v => "Item 1");
      await _localRepository.Create (v => "Item 2");

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
    public async Task TwoWaySynchronize_AddedServer (GenericConflictResolution conflictWinner)
    {
      await _serverRepository.Create (v => "Item 1");
      await _serverRepository.Create (v => "Item 2");

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
    public async Task TwoWaySynchronize_AddedBoth (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      await _localRepository.Create (v => "Item l");
      await _serverRepository.Create (v => "Item s");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (4);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");
        AssertLocal (0, "Item l");
        AssertLocal (0, "Item s");

        AssertServerCount (4);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
        AssertServer (0, "Item l");
        AssertServer (0, "Item s");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedLocal (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      await _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedServer (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      await _serverRepository.Delete ("s1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedBoth (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      await _serverRepository.Delete ("s1");
      await _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (0);

        AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedBothWithConflict (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      await _serverRepository.Delete ("s1");
      await _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_UpdatedLocal (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_UpdatedServer (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (2);
        AssertLocal ("l1u", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_UpdatedBoth (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");
      _localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (conflictWinner);
        AssertLocalCount (2);
        AssertLocal ("l1u", 1, "upd Item 1");
        AssertLocal ("l2", 1, "upd Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 1, "upd Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_DeletedLocal_ChangeServer_Conflict_LocalWins ()
    {
      await InitializeWithTwoEvents();

      await _localRepository.Delete ("l1");
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.AWins);
        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_DeletedLocal_ChangeServer_Conflict_ServerWins ()
    {
      await InitializeWithTwoEvents();

      await _localRepository.Delete ("l1");
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.BWins);
        AssertLocalCount (2);
        AssertLocal ("l3", 0, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_DeletedServer_ChangeLocal_Conflict_LocalWins ()
    {
      await InitializeWithTwoEvents();

      await _serverRepository.Delete ("s1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.AWins);
        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s3", 0, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_DeletedServer_ChangeLocal_Conflict_ServerWins ()
    {
      await InitializeWithTwoEvents();

      await _serverRepository.Delete ("s1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.BWins);
        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_UpdatedBoth_Conflict_LocalWins ()
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.AWins);
        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd loc Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "upd loc Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task TwoWaySynchronize_UpdatedBoth_Conflict_ServerWins ()
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.BWins);
        AssertLocalCount (2);
        AssertLocal ("l1u", 2, "upd srv Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 1, "upd srv Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }
  }
}