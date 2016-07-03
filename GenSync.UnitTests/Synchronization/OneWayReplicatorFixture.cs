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
using System.Threading.Tasks;
using GenSync.Synchronization;
using NUnit.Framework;

namespace GenSync.UnitTests.Synchronization
{
  [TestFixture]
  internal class OneWayReplicatorFixture : SynchronizerFixtureBase
  {
    [Test]
    public async Task ReplicateAtoBAddedLocal ()
    {
      await _localRepository.Create (v => "Item 1", NullSynchronizationContextFactory.Instance.Create ().Result);
      await _localRepository.Create (v => "Item 2", NullSynchronizationContextFactory.Instance.Create().Result);

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBAddedServer ()
    {
      await _serverRepository.Create (v => "Item 1", NullSynchronizationContextFactory.Instance.Create ().Result);
      await _serverRepository.Create (v => "Item 2", NullSynchronizationContextFactory.Instance.Create ().Result);

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (0);

        AssertServerCount (0);
      });
    }

    [Test]
    public async Task ReplicateAtoBAddedBoth ()
    {
      await InitializeWithTwoEvents();

      await _localRepository.Create (v => "Item l", NullSynchronizationContextFactory.Instance.Create ().Result);
      await _serverRepository.Create (v => "Item s", NullSynchronizationContextFactory.Instance.Create ().Result);

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (3);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");
        AssertLocal (0, "Item l");

        AssertServerCount (3);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
        AssertServer (0, "Item l");
      });
    }

    [Test]
    public async Task ReplicateAtoBDeletedLocal ()
    {
      await InitializeWithTwoEvents();

      _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBDeletedServer ()
    {
      await InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s3", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBDeletedBoth ()
    {
      await InitializeWithTwoEvents();

      // Schlägt feht Wenns am server gelöscht wird, muss es natürlich wieder hergestellt werden!!
      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (1);
        AssertLocal ("l1", 0, "Item 1");

        AssertServerCount (1);
        AssertServer ("s3", 0, "Item 1");
      });
    }

    [Test]
    public async Task ReplicateAtoBDeletedBothWithConflict ()
    {
      await InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBUpdatedLocal ()
    {
      await InitializeWithTwoEvents();
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBUpdatedServer ()
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBUpdatedBoth ()
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");
      _localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 1, "upd Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
      });
    }

    [Test]
    public async Task ReplicateAtoBDeletedLocal_ChangeServer_Conflict ()
    {
      await InitializeWithTwoEvents();
      _localRepository.Delete ("l1");
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public async Task ReplicateAtoBDeletedServer_ChangeLocal_Conflict ()
    {
      await InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s3", 0, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public async Task ReplicateAtoBUpdatedBoth_Conflict ()
    {
      await InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        SynchronizeOneWay();

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd loc Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "upd loc Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }
  }
}