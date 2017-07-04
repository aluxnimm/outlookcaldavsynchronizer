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
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using DDay.iCal;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{

  class EventSynchronizerFixture 
  {
    private TestComponentContainer _testComponentContainer;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
      _testComponentContainer = new TestComponentContainer();
    }

    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeToServer_SomeEventsAreOutsideTimeRangeFilter_SyncsJustEventsWhichMatchTimeRangeFilter()
    {
      var options = TestComponentContainer.GetOptions("IntegrationTest/Events/Sogo");
 
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      options.DaysToSynchronizeInTheFuture = 10;
      options.DaysToSynchronizeInThePast = 10;
      options.IgnoreSynchronizationTimeRange = false;
      synchronizer = await CreateSynchronizer(options);

      await synchronizer.CreateEventInOutlook("before", DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-11));
      await synchronizer.CreateEventInOutlook("after", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await synchronizer.CreateEventInOutlook("overlapBeginning", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await synchronizer.CreateEventInOutlook("overlapEnd", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));
      await synchronizer.CreateEventInOutlook("inside", DateTime.Now.AddDays(-5), DateTime.Now.AddDays(5));
      await synchronizer.CreateEventInOutlook("surrounding", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(11));

      await synchronizer.SynchronizeAndCheck(
        unchangedA: 0, addedA: 4, changedA: 0, deletedA: 0,
        unchangedB: 0, addedB: 0, changedB: 0, deletedB: 0,
        createA: 0, updateA: 0, deleteA: 0,
        createB: 4, updateB: 0, deleteB: 0);
      
      options.IgnoreSynchronizationTimeRange = true;
      synchronizer = await CreateSynchronizer(options);

      var events = await synchronizer.Server.GetAllEntities();
      
      CollectionAssert.AreEquivalent(
        new[]
        {
          "overlapBeginning", "overlapEnd", "inside", "surrounding"
        },
        events.Select(e => e.Entity.Events[0].Summary));
    }

    [TestCase("IntegrationTest/Events/Sogo")]
    // [TestCase("IntegrationTest/Events/Google")] => This does currently not work
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents(string profileName)
    {
      var options = TestComponentContainer.GetOptions(profileName);

      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      await synchronizer.CreateEventInOutlook("Event -5", DateTime.Today.AddDays(-5), DateTime.Now.AddDays(-5), true);
      await synchronizer.CreateEventInOutlook("Event -4", DateTime.Today.AddDays(-4), DateTime.Now.AddDays(-4), true);
      await synchronizer.CreateEventInOutlook("Event -3", DateTime.Today.AddDays(-3), DateTime.Now.AddDays(-3), true);
      await synchronizer.CreateEventInOutlook("Event -2", DateTime.Today.AddDays(-2), DateTime.Now.AddDays(-2), true);
      await synchronizer.CreateEventInOutlook("Event -1", DateTime.Today.AddDays(-1), DateTime.Now.AddDays(-1), true);
      await synchronizer.CreateEventInOutlook("Event 0", DateTime.Today.AddDays(0), DateTime.Now.AddDays(0), true);
      await synchronizer.CreateEventInOutlook("Event 1", DateTime.Today.AddDays(1), DateTime.Now.AddDays(1), true);
      await synchronizer.CreateEventInOutlook("Event 2", DateTime.Today.AddDays(2), DateTime.Now.AddDays(2), true);
      await synchronizer.CreateEventInOutlook("Event 3", DateTime.Today.AddDays(3), DateTime.Now.AddDays(3), true);
      await synchronizer.CreateEventInOutlook("Event 4", DateTime.Today.AddDays(4), DateTime.Now.AddDays(4), true);
      await synchronizer.CreateEventInOutlook("Event 5", DateTime.Today.AddDays(5), DateTime.Now.AddDays(5), true);

      await synchronizer.SynchronizeAndCheck(
        unchangedA: 0, addedA: 11, changedA: 0, deletedA: 0,
        unchangedB: 0, addedB: 0, changedB: 0, deletedB: 0,
        createA: 0, updateA: 0, deleteA: 0,
        createB: 11, updateB: 0, deleteB: 0);

      synchronizer.ClearCache();

      for (int rangeInDays = 1; rangeInDays <= 5; rangeInDays++)
      {
        options.DaysToSynchronizeInTheFuture = rangeInDays;
        options.DaysToSynchronizeInThePast = rangeInDays;
        options.IgnoreSynchronizationTimeRange = false;
        synchronizer = await CreateSynchronizer(options);

        var expectedNumberOfEvents = rangeInDays * 2 + 2 - (rangeInDays == 5 ? 1 : 0);

        await synchronizer.SynchronizeAndCheck(
          unchangedA: expectedNumberOfEvents, addedA: 0, changedA: 0, deletedA: 0,
          unchangedB: expectedNumberOfEvents, addedB: 0, changedB: 0, deletedB: 0,
          createA: 0, updateA: 0, deleteA: 0,
          createB: 0, updateB: 0, deleteB: 0);
      }
    }
    
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeTwoWay_LocalEventChanges_IsSyncedToServerAndPreservesExtendedPropertiesAndUid()
    {
      var options = TestComponentContainer.GetOptions ("IntegrationTest/Events/Sogo");
      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache ();

      string initialUid = null;

      await synchronizer.CreateEventOnServer(
        "XXXX",
        DateTime.Now.AddDays(11),
        DateTime.Now.AddDays(20),
        e =>
        {
          e.Properties.Add(new CalendarProperty("X-CALDAVSYNCHRONIZER-INTEGRATIONTEST", "TheValueBlaBLubb"));
          initialUid = e.UID;
        });

      await synchronizer.SynchronizeAndAssertNoErrors();

      using (var outlookEvent = (await synchronizer.Outlook.GetAllEntities()).Single().Entity)
      {
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverEvent = (await synchronizer.Server.GetAllEntities()).Single().Entity;

      Assert.That(serverEvent.Events[0].Summary, Is.EqualTo("TheNewSubject"));
      Assert.That(serverEvent.Events[0].UID, Is.EqualTo(initialUid));

      //Assert.That(
      //  serverEvent.Events[0].Properties.SingleOrDefault(p => p.Name == "X-CALDAVSYNCHRONIZER-INTEGRATIONTEST")?.Value,
      //  Is.EqualTo("TheValueBlaBLubb"));

    }

    [TestCase(true)]
    [TestCase(false)]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task CreateOutlookEntity_ExceptionOccurs_DoesNotLeaveEmptyEntityInRepository(bool saveAndReload)
    {
      var options = TestComponentContainer.GetOptions("IntegrationTest/Events/Sogo");
      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();

      bool exceptionCatched = false;
      var exception = new Exception("bla");
      try
      {
        await synchronizer.Components.OutlookEventRepository.Create(
          w =>
          {
            if (saveAndReload)
              w.SaveAndReload();
            throw exception;
          },
          NullEventSynchronizationContext.Instance);
      }
      catch (Exception x)
      {
        if (ReferenceEquals(x, exception))
          exceptionCatched = true;
      }

      Assert.That(exceptionCatched, Is.EqualTo(true));

      Assert.That(
        (await synchronizer.Outlook.GetAllEntities()).Count(),
        Is.EqualTo(0));
    }

    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeTwoWay_CacheIsClearedAfterFirstRun_FindsMatchingEntitiesInSecondRun()
    {
      var options = TestComponentContainer.GetOptions("IntegrationTest/Events/Sogo");

      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();
      
      await synchronizer.CreateEventInOutlook("first", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await synchronizer.CreateEventInOutlook("second", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await synchronizer.CreateEventInOutlook("third", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));

      await synchronizer.SynchronizeAndAssertNoErrors();

      var relations = synchronizer.Components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId}).ToArray();
      Assert.That(relations.Length, Is.EqualTo(3));

      synchronizer.ClearCache();

      await synchronizer.SynchronizeAndAssertNoErrors();

      CollectionAssert.AreEquivalent(
        relations,
        synchronizer.Components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId})
      );
    }


    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Synchronize_ServerEventContainsOrganizer_IsSyncedToOutlookAndBackToServer()
    {
      var options = TestComponentContainer.GetOptions("IntegrationTest/Events/Sogo");
      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;

      var synchronizer = await CreateSynchronizer(options);
      await synchronizer.ClearEventRepositoriesAndCache();


      await synchronizer.CreateEventOnServer("bla", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20), e => e.Organizer = new Organizer("theOrgainzer@bla.com"));
      await synchronizer.SynchronizeAndAssertNoErrors();

      using (var outlookEvent = (await synchronizer.Outlook.GetAllEntities()).Single().Entity)
      {
        Assert.That(outlookEvent.Inner.Organizer, Is.EqualTo("theOrgainzer@bla.com"));
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save();
      }

      await synchronizer.SynchronizeAndAssertNoErrors();

      var serverEvent = (await synchronizer.Server.GetAllEntities()).Single().Entity;

      Assert.That(serverEvent.Events[0].Summary, Is.EqualTo("TheNewSubject"));
      Assert.That(serverEvent.Events[0].Organizer.Value.ToString(), Is.EqualTo("mailto:theOrgainzer@bla.com"));

    }


    private async Task<EventTestSynchronizer> CreateSynchronizer(Options options)
    {
      var synchronizer = new EventTestSynchronizer(options, _testComponentContainer);
      await synchronizer.Initialize();
      return synchronizer;
    }
  }
}
