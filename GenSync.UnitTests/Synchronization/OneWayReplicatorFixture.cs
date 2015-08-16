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
using NUnit.Framework;

namespace GenSync.UnitTests.Synchronization
{
  [TestFixture]
  internal class OneWayReplicatorFixture : OneWayReplicatorFixtureBase
  {
    [Test]
    public void ReplicateAtoBAddedLocal ()
    {
      _localRepository.Create (v => "Item 1");
      _localRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBAddedServer ()
    {
      _serverRepository.Create (v => "Item 1");
      _serverRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (0);

        AssertServerCount (0);
      });
    }

    [Test]
    public void ReplicateAtoBAddedBoth ()
    {
      InitializeWithTwoEvents();

      _localRepository.Create (v => "Item l");
      _serverRepository.Create (v => "Item s");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

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
    public void ReplicateAtoBDeletedLocal ()
    {
      InitializeWithTwoEvents();

      _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedServer ()
    {
      InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s3", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedBoth ()
    {
      InitializeWithTwoEvents();

      // Schlägt feht Wenns am server gelöscht wird, muss es natürlich wieder hergestellt werden!!
      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (1);
        AssertLocal ("l1", 0, "Item 1");

        AssertServerCount (1);
        AssertServer ("s3", 0, "Item 1");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedBothWithConflict ()
    {
      InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedLocal ()
    {
      InitializeWithTwoEvents();
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedServer ()
    {
      InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedBoth ()
    {
      InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");
      _localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 1, "upd Item 2");

        AssertServerCount (2);
        AssertServer ("s1u", 2, "Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedLocal_ChangeServer_Conflict ()
    {
      InitializeWithTwoEvents();
      _localRepository.Delete ("l1");
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (1);
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (1);
        AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public void ReplicateAtoBDeletedServer_ChangeLocal_Conflict ()
    {
      InitializeWithTwoEvents();

      _serverRepository.Delete ("s1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

        AssertLocalCount (2);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 0, "Item 2");

        AssertServerCount (2);
        AssertServer ("s3", 0, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public void ReplicateAtoBUpdatedBoth_Conflict ()
    {
      InitializeWithTwoEvents();
      _serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize();

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