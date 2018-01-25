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

using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{
  [TestFixture]
  public class GoogleContactFixture 
  {
    private TestComponentContainer _testComponentContainer;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    [Test]
    [TestCase(40, 201 , 50)]
    [TestCase(40, 201 , 100)]
    [TestCase(40, 201 , 700)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task GenericTest(int numberOfGroups, int numberOfContacts, int chunkSize)
    {
      var options = _testComponentContainer.TestOptionsFactory.CreateGoogleContacts();
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;
      options.IsChunkedSynchronizationEnabled = true;
      options.ChunkSize = chunkSize;

      var synchronizer = await CreateSynchronizer(options);

      await synchronizer.ClearEventRepositoriesAndCache();

      var groupNames = synchronizer.GetOrCreateGoogleGroups(numberOfGroups);
      await synchronizer.CreateContactsInOutlook(synchronizer.CreateTestContactData(groupNames, numberOfContacts));

      await synchronizer.SynchronizeAndCheck(
        unchangedA: 0, addedA: numberOfContacts, changedA: 0, deletedA: 0,
        unchangedB: 0, addedB: 0, changedB: 0, deletedB: 0,
        createA: 0, updateA: 0, deleteA: 0,
        createB: numberOfContacts, updateB: 0, deleteB: 0);

      var outlook1IdsByGoogleId = synchronizer.Components.GoogleContactsEntityRelationDataAccess.LoadEntityRelationData().ToDictionary(r => r.BtypeId, r => r.AtypeId);
      Assert.That(outlook1IdsByGoogleId.Count, Is.EqualTo(numberOfContacts));

      await synchronizer.Outlook.DeleteAllEntities();

      options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
      synchronizer = await CreateSynchronizer(options);

      await synchronizer.SynchronizeAndCheck(
        unchangedA: 0, addedA: 0, changedA: 0, deletedA: numberOfContacts,
        unchangedB: numberOfContacts, addedB: 0, changedB: 0, deletedB: 0,
        createA: numberOfContacts, updateA: 0, deleteA: 0,
        createB: 0, updateB: 0, deleteB: 0);
    }

    private async Task<GoogleContactTestSynchronizer> CreateSynchronizer(Options options)
    {
      var synchronizer = new GoogleContactTestSynchronizer(options, _testComponentContainer);
      await synchronizer.Initialize();
      return synchronizer;
    }
  }
}