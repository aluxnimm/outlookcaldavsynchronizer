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
  internal class TwoWaySynchronizerFixture : TwoWaySynchronizerFixtureBase
  {
    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_AddedLocal (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup._localRepository.Create (v => "Item 1");
      _synchronizerSetup._localRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);

        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_AddedServer (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup._serverRepository.Create (v => "Item 1");
      _synchronizerSetup._serverRepository.Create (v => "Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_AddedBoth (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Create (v => "Item l");
      _synchronizerSetup._serverRepository.Create (v => "Item s");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (4);
        _synchronizerSetup.AssertLocal ("l1", 0, "Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");
        _synchronizerSetup.AssertLocal (0, "Item l");
        _synchronizerSetup.AssertLocal (0, "Item s");

        _synchronizerSetup.AssertServerCount (4);
        _synchronizerSetup.AssertServer ("s1", 0, "Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
        _synchronizerSetup.AssertServer (0, "Item l");
        _synchronizerSetup.AssertServer (0, "Item s");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_DeletedLocal (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_DeletedServer (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_DeletedBoth (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.Delete ("l2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (0);

        _synchronizerSetup.AssertServerCount (0);
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_DeletedBothWithConflict (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.Delete ("l1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_UpdatedLocal (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 1, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_UpdatedServer (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1u", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 1, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public void TwoWaySynchronize_UpdatedBoth (GenericConflictResolution conflictWinner)
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (conflictWinner);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1u", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 1, "upd Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 1, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2u", 1, "upd Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_DeletedLocal_ChangeServer_Conflict_LocalWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Delete ("l1");
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.AWins);
        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_DeletedLocal_ChangeServer_Conflict_ServerWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._localRepository.Delete ("l1");
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.BWins);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l3", 0, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 1, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_DeletedServer_ChangeLocal_Conflict_LocalWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.AWins);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s3", 0, "upd Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_DeletedServer_ChangeLocal_Conflict_ServerWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();

      _synchronizerSetup._serverRepository.Delete ("s1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.BWins);
        _synchronizerSetup.AssertLocalCount (1);
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (1);
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_UpdatedBoth_Conflict_LocalWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.AWins);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1", 1, "upd loc Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1u", 2, "upd loc Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }

    [Test]
    public void TwoWaySynchronize_UpdatedBoth_Conflict_ServerWins ()
    {
      _synchronizerSetup.InitializeWithTwoEvents();
      _synchronizerSetup._serverRepository.UpdateWithoutIdChange ("s1", v => "upd srv Item 1");
      _synchronizerSetup._localRepository.UpdateWithoutIdChange ("l1", v => "upd loc Item 1");

      ExecuteMultipleTimes (() =>
      {
        Synchronize (GenericConflictResolution.BWins);
        _synchronizerSetup.AssertLocalCount (2);
        _synchronizerSetup.AssertLocal ("l1u", 2, "upd srv Item 1");
        _synchronizerSetup.AssertLocal ("l2", 0, "Item 2");

        _synchronizerSetup.AssertServerCount (2);
        _synchronizerSetup.AssertServer ("s1", 1, "upd srv Item 1");
        _synchronizerSetup.AssertServer ("s2", 0, "Item 2");
      });
    }
  }
}