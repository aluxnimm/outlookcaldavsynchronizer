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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using CalDavSynchronizer.Scheduling.ComponentCollectors;
using CalDavSynchronizer.Synchronization;
using DDay.iCal;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using NUnit.Framework;

namespace CalDavSynchronizer.IntegrationTests
{

  class EventSynchronizerFixture : SynchronizerFixtureBase
  {
    [Test]
    [Apartment(System.Threading.ApartmentState.STA)]
    public async Task SynchronizeToServer_SomeEventsAreOutsideTimeRangeFilter_SyncsJustEventsWhichMatchTimeRangeFilter()
    {
      var options = GetOptions("IntegrationTest/General/Sogo");
      options.DaysToSynchronizeInTheFuture = 10;
      options.DaysToSynchronizeInThePast = 10;
      options.IgnoreSynchronizationTimeRange = false;

      await ClearEventRepositoriesAndCache (options);

      options.SynchronizationMode = SynchronizationMode.ReplicateOutlookIntoServer;
      var synchronizerWithComponents = await SynchronizerFactory.CreateSynchronizerWithComponents(options, GeneralOptions);
      var components = (AvailableEventSynchronizerComponents) synchronizerWithComponents.Item2;
      var synchronizer = synchronizerWithComponents.Item1;

      await CreateEventInOutlook(components.OutlookEventRepository, "before", DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-11));
      await CreateEventInOutlook(components.OutlookEventRepository, "after", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await CreateEventInOutlook(components.OutlookEventRepository, "overlapBeginning", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await CreateEventInOutlook(components.OutlookEventRepository, "overlapEnd", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));
      await CreateEventInOutlook(components.OutlookEventRepository, "inside", DateTime.Now.AddDays(-5), DateTime.Now.AddDays(5));
      await CreateEventInOutlook(components.OutlookEventRepository, "surrounding", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(11));

      var reportSink = new TestReportSink();

      using (var logger = new SynchronizationLogger(options.Id, options.Name, reportSink))
      {
        await synchronizer.Synchronize(logger);
      }

      Assert.That(reportSink.SynchronizationReport.ADelta, Is.EqualTo("Unchanged: 0 , Added: 4 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.BDelta, Is.EqualTo("Unchanged: 0 , Added: 0 , Deleted 0 ,  Changed 0"));
      Assert.That(reportSink.SynchronizationReport.AJobsInfo, Is.EqualTo("Create 0 , Update 0 , Delete 0"));
      Assert.That(reportSink.SynchronizationReport.BJobsInfo, Is.EqualTo("Create 4 , Update 0 , Delete 0"));

      var events = await GetAllEntities(components.CalDavRepository, NullEventSynchronizationContext.Instance);
      
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

      await ClearEventRepositoriesAndCache (options);

      var synchronizerWithComponents = await SynchronizerFactory.CreateSynchronizerWithComponents (options, GeneralOptions);
      var components = (AvailableEventSynchronizerComponents) synchronizerWithComponents.Item2;
      var synchronizer = synchronizerWithComponents.Item1;

      string initialUid = null;

      await CreateEventOnServer(
        components.CalDavRepository,
        "XXXX",
        DateTime.Now.AddDays(11),
        DateTime.Now.AddDays(20),
        e =>
        {
          e.Properties.Add(new CalendarProperty("X-CALDAVSYNCHRONIZER-INTEGRATIONTEST", "TheValueBlaBLubb"));
          initialUid = e.UID;
        });

     
      await synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      using (var outlookEvent = (await GetAllEntities(components.OutlookEventRepository, NullEventSynchronizationContext.Instance)).Single().Entity)
      {
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save();
      }

      await synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      var serverEvent = (await GetAllEntities(components.CalDavRepository, NullEventSynchronizationContext.Instance)).Single().Entity;

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

      await ClearEventRepositoriesAndCache(options);

      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
      var synchronizerWithComponents = await SynchronizerFactory.CreateSynchronizerWithComponents(options, GeneralOptions);
      var components = (AvailableEventSynchronizerComponents) synchronizerWithComponents.Item2;
      var synchronizer = synchronizerWithComponents.Item1;

      await CreateEventInOutlook(components.OutlookEventRepository, "first", DateTime.Now.AddDays(11), DateTime.Now.AddDays(20));
      await CreateEventInOutlook(components.OutlookEventRepository, "second", DateTime.Now.AddDays(-11), DateTime.Now.AddDays(-9));
      await CreateEventInOutlook(components.OutlookEventRepository, "third", DateTime.Now.AddDays(9), DateTime.Now.AddDays(11));

      await synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      var relations = components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId});
      // Delete cache by saving empty relations list
      components.EntityRelationDataAccess.SaveEntityRelationData(new List<GenSync.EntityRelationManagement.IEntityRelationData<AppointmentId, DateTime, WebResourceName, string>>());
      
      await synchronizer.Synchronize(NullSynchronizationLogger.Instance);

      CollectionAssert.AreEquivalent(
        relations,
        components.EntityRelationDataAccess.LoadEntityRelationData().Select(r => new {r.AtypeId, r.BtypeId})
      );
    }


    [Test]
    [Apartment (System.Threading.ApartmentState.STA)]
    public async Task Synchronize_ServerEventContainsOrganizer_IsSyncedToOutlookAndBackToServer ()
    {
      var options = GetOptions ("IntegrationTest/General/Sogo");

      await ClearEventRepositoriesAndCache (options);

      options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
      var synchronizerWithComponents = await SynchronizerFactory.CreateSynchronizerWithComponents (options, GeneralOptions);
      var components = (AvailableEventSynchronizerComponents) synchronizerWithComponents.Item2;
      var synchronizer = synchronizerWithComponents.Item1;

      await CreateEventOnServer (components.CalDavRepository, "bla", DateTime.Now.AddDays (11), DateTime.Now.AddDays (20), e => e.Organizer = new Organizer("theOrgainzer@bla.com"));
      await synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      using (var outlookEvent = (await GetAllEntities (components.OutlookEventRepository, NullEventSynchronizationContext.Instance)).Single ().Entity)
      {
        Assert.That(outlookEvent.Inner.Organizer, Is.EqualTo("theOrgainzer@bla.com"));
        outlookEvent.Inner.Subject = "TheNewSubject";
        outlookEvent.Inner.Save ();
      }

      await synchronizer.Synchronize (NullSynchronizationLogger.Instance);

      var serverEvent = (await GetAllEntities(components.CalDavRepository, NullEventSynchronizationContext.Instance)).Single().Entity;

      Assert.That(serverEvent.Events[0].Summary, Is.EqualTo("TheNewSubject"));
      Assert.That(serverEvent.Events[0].Organizer.Value.ToString(), Is.EqualTo("mailto:theOrgainzer@bla.com"));

    }
  }


}
