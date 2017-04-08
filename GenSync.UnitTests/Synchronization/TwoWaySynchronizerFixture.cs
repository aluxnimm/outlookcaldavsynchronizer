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
using System.Threading.Tasks;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Synchronization;
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
      await _localRepository.Create (v => Task.FromResult ("Item 1"), NullSynchronizationContextFactory.Instance.Create ().Result);
      await _localRepository.Create (v => Task.FromResult ("Item 2"), NullSynchronizationContextFactory.Instance.Create ().Result);

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
      await _serverRepository.Create (v => Task.FromResult("Item 1"), NullSynchronizationContextFactory.Instance.Create ().Result);
      await _serverRepository.Create (v => Task.FromResult("Item 2"), NullSynchronizationContextFactory.Instance.Create ().Result);

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

      await _localRepository.Create (v => Task.FromResult("Item l"), NullSynchronizationContextFactory.Instance.Create ().Result);
      await _serverRepository.Create (v => Task.FromResult("Item s"), NullSynchronizationContextFactory.Instance.Create ().Result);

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
    public async Task TwoWaySynchronize_AddedBoth_NewEntitiesAreMatching (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      var addedLocal = await _localRepository.Create (v => Task.FromResult("Item l"), NullSynchronizationContextFactory.Instance.Create ().Result);
      var addedServer = await _serverRepository.Create (v => Task.FromResult("Item s"), NullSynchronizationContextFactory.Instance.Create ().Result);
      await _serverRepository.Create (v => Task.FromResult("Item s2"), NullSynchronizationContextFactory.Instance.Create ().Result);

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (
            conflictWinner,
            new List<IEntityRelationData<Identifier, int, Identifier, int>>
            {
                new EntityRelationData (addedLocal.Id, addedLocal.Version, addedServer.Id, addedServer.Version)
            });
        AssertLocalCount (4);
        AssertLocal ("l1", 0, "Item 1");
        AssertLocal ("l2", 0, "Item 2");
        AssertLocal (0, "Item l");
        AssertLocal ("l4", 0, "Item s2");

        AssertServerCount (4);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2", 0, "Item 2");
        AssertServer (0, "Item s");
        AssertServer ("s4", 0, "Item s2");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_AddedBothWithoutExisting_NewEntitiesAreMatching (GenericConflictResolution conflictWinner)
    {
      var addedLocal = await _localRepository.Create (v => Task.FromResult("Item l"), NullSynchronizationContextFactory.Instance.Create ().Result);
      var addedServer = await _serverRepository.Create (v => Task.FromResult("Item s"), NullSynchronizationContextFactory.Instance.Create ().Result);

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (
            conflictWinner,
            new List<IEntityRelationData<Identifier, int, Identifier, int>>
            {
                new EntityRelationData (addedLocal.Id, addedLocal.Version, addedServer.Id, addedServer.Version)
            });
        AssertLocalCount (1);
        AssertLocal (0, "Item l");

        AssertServerCount (1);
        AssertServer (0, "Item s");
      });
    }

    [TestCase (GenericConflictResolution.AWins)]
    [TestCase (GenericConflictResolution.BWins)]
    public async Task TwoWaySynchronize_DeletedLocal (GenericConflictResolution conflictWinner)
    {
      await InitializeWithTwoEvents();

      _localRepository.Delete ("l1");

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

      _serverRepository.Delete ("s1");

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

      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l2");

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

      _serverRepository.Delete ("s1");
      _localRepository.Delete ("l1");

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

      _localRepository.Delete ("l1");
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

      _localRepository.Delete ("l1");
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

      _serverRepository.Delete ("s1");
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

      _serverRepository.Delete ("s1");
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

    [Test]
    public async Task TwoWaySynchronize_RepositoryOverloadOnUpdate_KeepsAlreadyUpdatedRelations ()
    {
      await InitializeWithEvents (4);
    

      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");
      _localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");
      _localRepository.UpdateWithoutIdChange ("l3", v => "upd Item 3");
      _serverRepository.EntityWhichCausesAbortExceptionOnUpdate = "s2";

      ExecuteMultipleTimes (() =>
      {
        Assert.That(
          SynchronizeTwoWay(GenericConflictResolution.AWins),
          Is.InstanceOf<TestAbortException>());

        AssertLocalCount (4);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 1, "upd Item 2");
        AssertLocal ("l3", 1, "upd Item 3");
        AssertLocal ("l4", 0, "Item 4");

        AssertServerCount (4);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2", 0, "Item 2");
        AssertServer ("s3", 0, "Item 3");
        AssertServer ("s4", 0, "Item 4");

        AssertRelations(
          new EntityRelationData("l1", 1, "s1u", 1),
          new EntityRelationData("l2", 0, "s2", 0), // => still local version 0, since there was an overload on updating
          new EntityRelationData("l3", 0, "s3", 0),
          new EntityRelationData("l4", 0, "s4", 0));
      });

      _serverRepository.EntityWhichCausesAbortExceptionOnUpdate = null;

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay(GenericConflictResolution.AWins);

        AssertLocalCount (4);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 1, "upd Item 2");
        AssertLocal ("l3", 1, "upd Item 3");
        AssertLocal ("l4", 0, "Item 4");

        AssertServerCount (4);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
        AssertServer ("s3u", 1, "upd Item 3");
        AssertServer ("s4", 0, "Item 4");

        AssertRelations (
          new EntityRelationData ("l1", 1, "s1u", 1),
          new EntityRelationData ("l2", 1, "s2u", 1), 
          new EntityRelationData ("l3", 1, "s3u", 1),
          new EntityRelationData ("l4", 0, "s4", 0));
      });
    }

    [Test]
    public async Task TwoWaySynchronize_AllOperationsWithErrorsInFirstRuns ()
    {
      await InitializeWithEvents (8);

      var addedLocal1 = await _localRepository.Create (v => Task.FromResult ("Item add l1"), NullSynchronizationContextFactory.Instance.Create ().Result);
      var addedLocal2 = await _localRepository.Create (v => Task.FromResult ("Item add l2"), NullSynchronizationContextFactory.Instance.Create ().Result);
      _localRepository.UpdateWithoutIdChange ("l1", v => "upd Item 1");
      _localRepository.UpdateWithoutIdChange ("l2", v => "upd Item 2");
      _localRepository.Delete("l3");
      _localRepository.Delete("l4");

      _localRepository.SetNumberOfEntitesWhichCanBeCreatedBeforeExceptionsOccur(1);
      _localRepository.EntityWhichCausesExceptionOnUpdate = "l5";
      _localRepository.EntityWhichCausesExceptionOnDelete = "l7";

      var addedServer1 = await _serverRepository.Create (v => Task.FromResult ("Item add s1"), NullSynchronizationContextFactory.Instance.Create ().Result);
      var addedServer2 = await _serverRepository.Create (v => Task.FromResult ("Item add s2"), NullSynchronizationContextFactory.Instance.Create ().Result);
      _serverRepository.UpdateWithoutIdChange ("s5", v => "upd Item 5");
      _serverRepository.UpdateWithoutIdChange ("s6", v => "upd Item 6");
      _serverRepository.Delete ("s7");
      _serverRepository.Delete ("s8");

      _serverRepository.SetNumberOfEntitesWhichCanBeCreatedBeforeExceptionsOccur(1);
      _serverRepository.EntityWhichCausesExceptionOnUpdate = "s1";
      _serverRepository.EntityWhichCausesExceptionOnDelete = "s3";

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.AWins);

        AssertLocalCount (8);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 1, "upd Item 2");
        AssertLocal ("l5", 0, "Item 5");
        AssertLocal ("l6u", 1, "upd Item 6");
        AssertLocal ("l7", 0, "Item 7");
        AssertLocal ("l9", 0, "Item add l1");
        AssertLocal ("l10", 0, "Item add l2");
        AssertLocal ("l11", 0, "Item add s1");

        AssertServerCount (8);
        AssertServer ("s1", 0, "Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
        AssertServer ("s3", 0, "Item 3");
        AssertServer ("s5", 1, "upd Item 5");
        AssertServer ("s6", 1, "upd Item 6");
        AssertServer ("s9", 0, "Item add s1");
        AssertServer ("s10", 0, "Item add s2");
        AssertServer ("s11", 0, "Item add l1");
      });

      _localRepository.SetNumberOfEntitesWhichCanBeCreatedBeforeExceptionsOccur (null);
      _localRepository.EntityWhichCausesExceptionOnUpdate = null;
      _localRepository.EntityWhichCausesExceptionOnDelete = null;
      _serverRepository.SetNumberOfEntitesWhichCanBeCreatedBeforeExceptionsOccur (null);
      _serverRepository.EntityWhichCausesExceptionOnUpdate = null;
      _serverRepository.EntityWhichCausesExceptionOnDelete = null;

      ExecuteMultipleTimes (() =>
      {
        SynchronizeTwoWay (GenericConflictResolution.AWins);

        AssertLocalCount (8);
        AssertLocal ("l1", 1, "upd Item 1");
        AssertLocal ("l2", 1, "upd Item 2");
        AssertLocal ("l5u", 1, "upd Item 5");
        AssertLocal ("l6u", 1, "upd Item 6");
        AssertLocal ("l9", 0, "Item add l1");
        AssertLocal ("l10", 0, "Item add l2");
        AssertLocal ("l11", 0, "Item add s1");
        AssertLocal ("l12", 0, "Item add s2");

        AssertServerCount (8);
        AssertServer ("s1u", 1, "upd Item 1");
        AssertServer ("s2u", 1, "upd Item 2");
        AssertServer ("s5", 1, "upd Item 5");
        AssertServer ("s6", 1, "upd Item 6");
        AssertServer ("s9", 0, "Item add s1");
        AssertServer ("s10", 0, "Item add s2");
        AssertServer ("s11", 0, "Item add l1");
        AssertServer ("s12", 0, "Item add l2");
      });
    }
  }
}