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
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using DDay.iCal;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{

  class EventSynchronizerFixture : EventSynchronizerFixtureBase
  {
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeToServer_SomeEventsAreOutsideTimeRangeFilter_SyncsJustEventsWhichMatchTimeRangeFilter()
    {
      var options = GetOptions("IntegrationTest/General/Sogo");
      options.DaysToSynchronizeInTheFuture = 10;
      options.DaysToSynchronizeInThePast = 10;
      options.IgnoreSynchronizationTimeRange = false;
      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;

      await InitializeFor (options);
      await ClearEventRepositoriesAndCache ();
      
      await CreateEventInOutlook("before", DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-11));
      await CreateEventInOutlook("after", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await CreateEventInOutlook("overlapBeginning", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await CreateEventInOutlook("overlapEnd", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));
      await CreateEventInOutlook("inside", DateTime.Now.AddDays(-5), DateTime.Now.AddDays(5));
      await CreateEventInOutlook("surrounding", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(11));

      var reportSink = new TestReportSink();

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await Synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 4 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 4 , Update 0 , Delete 0"));

      var events = await Server.GetAllEntities();
      
      CollectionAssert.AreEquivalent(
        new[]
        {
          "overlapBeginning", "overlapEnd", "inside", "surrounding"
        },
        events.Select(e => e.Entity.Events[0].Summary));
    }
    
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeTwoWay_LocalEventChanges_IsSyncedToServerAndPreservesExtendedPropertiesAndUid()
    {
      var options = GetOptions ("IntegrationTest/General/Sogo");
      await InitializeFor(options);
      await ClearEventRepositoriesAndCache ();

      string initialUid = null;

      await CreateEventOnServer(
        "XXXX",
        DateTime.Now.AddDays(11),
        DateTime.Now.AddDays(20),
        e =>
        {
          e.Properties.Add(new CalendarProperty("X-CALDAVSYNCHRONIZER-INTEGRATIONTEST", "TheValueBlaBLubb"));
          initialUid = e.UID;
        });
     
      await Synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      using (var outlookEvent = (await Outlook.GetAllEntities()).Single().Entity)
      {
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save();
      }

      await Synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      var serverEvent = (await Server.GetAllEntities()).Single().Entity;

      Assert.That(serverEvent.Events[0].Summary, Is.EqualTo("TheNewSubject"));
      Assert.That(serverEvent.Events[0].UID, Is.EqualTo(initialUid));

      //Assert.That(
      //  serverEvent.Events[0].Properties.SingleOrDefault(p => p.Name == "X-CALDAVSYNCHRONIZER-INTEGRATIONTEST")?.Value,
      //  Is.EqualTo("TheValueBlaBLubb"));

    }

    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeTwoWay_CacheIsClearedAfterFirstRun_FindsMatchingEntitiesInSecondRun()
    {
      var options = GetOptions("IntegrationTest/General/Sogo");

      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;

      await InitializeFor(options);
      await ClearEventRepositoriesAndCache();
      
      await CreateEventInOutlook("first", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await CreateEventInOutlook("second", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await CreateEventInOutlook("third", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));

      await Synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      var relations = Components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId}).ToArray();
      Assert.That(relations.Length, Is.EqualTo(3));

      ClearCache();

      await Synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      CollectionAssert.AreEquivalent(
        relations,
        Components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId})
      );
    }


    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task Synchronize_ServerEventContainsOrganizer_IsSyncedToOutlookAndBackToServer()
    {
      var options = GetOptions("IntegrationTest/General/Sogo");
      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;

      await InitializeFor(options);
      await ClearEventRepositoriesAndCache();


      await CreateEventOnServer("bla", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20), e => e.Organizer = new Organizer("theOrgainzer@bla.com"));
      await Synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      using (var outlookEvent = (await Outlook.GetAllEntities()).Single().Entity)
      {
        Assert.That(outlookEvent.Inner.Organizer, Is.EqualTo("theOrgainzer@bla.com"));
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save();
      }

      await Synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      var serverEvent = (await Server.GetAllEntities()).Single().Entity;

      Assert.That(serverEvent.Events[0].Summary, Is.EqualTo("TheNewSubject"));
      Assert.That(serverEvent.Events[0].Organizer.Value.ToString(), Is.EqualTo("mailto:theOrgainzer@bla.com"));

    }
  }


}
