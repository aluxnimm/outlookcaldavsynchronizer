// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
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

namespace CalDavSynchronizer.UnitTest.Synchronization
{
  [TestFixture]
  internal class OneWayReplicatorFixture : OneWayReplicatorFixtureBase
  {
    [Test]
    public void ReplicateAtoBAddedLocal ()
    {
      _synchronizerSetup._localRepository.Create (v => "Item 1");
      _synchronizerSetup._localRepository.Create (v => "Item 2");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBAddedServer ()
    {
      _synchronizerSetup._serverRepository.Create (v => "Item 1");
      _synchronizerSetup._serverRepository.Create (v => "Item 2");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (0);

        _synchronizerSetup.AssertServerCount (0);
      });
    }

    [Test]
    public void ReplicateAtoBAddedBoth ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Create (v => "Item l");
      _synchronizerSetup._serverRepository.Create (v => "Item s");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (3);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");
        _synchronizerSetup.AssertLocal (0, "Item l");

        _synchronizerSetup.AssertServerCount (3);
        _synchronizerSetup.AssertServer ("s1", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
        _synchronizerSetup.AssertServer (0, "Item l");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedLocal ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Delete ("l1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedServer ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s3", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedBoth ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      // Schlägt feht Wenns am server gelöscht wird, muss es natürlich wieder hergestellt werden!!
      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.Delete ("l2");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s3", 0, "Item 1");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedBothWithConflict ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.Delete ("l1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedLocal ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 1, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedServer ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 2, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBUpdatedBoth ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 1, "upd Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 2, "Item 1");
        _synchronizerSetup.AssertServer ("s2u", 1, "upd Item 2");
      });
    }

    [Test]
    public void ReplicateAtoBDeletedLocal_ChangeServer_Conflict ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._localRepository.Delete ("l1");
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public void ReplicateAtoBDeletedServer_ChangeLocal_Conflict ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s3", 0, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }


    [Test]
    public void ReplicateAtoBUpdatedBoth_Conflict ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteTwice (() =>
      {
        Synchronize();

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd loc Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 2, "upd loc Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }
  }
}