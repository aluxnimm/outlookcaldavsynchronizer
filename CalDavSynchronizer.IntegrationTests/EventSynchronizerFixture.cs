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
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.IntegrationTests.Infrastructure;
using CalDavSynchronizer.IntegrationTests.TestBase;
using CalDavSynchronizer.Scheduling;
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
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();

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

        [Test]
        [TestCase(false)]
        //[TestCase(true)] => This does currently not work, since sogo returns a wrong collection sync report
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task Synchronize_ContainsDuplicates_DuplicatesAreDeletedIfEnabled(bool useWebDavCollectionSync)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();

            options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
            options.UseWebDavCollectionSync = useWebDavCollectionSync;

            var synchronizer = await CreateSynchronizer(options);
            await synchronizer.ClearEventRepositoriesAndCache();


            synchronizer = await CreateSynchronizer(options);

            var now = DateTime.Now;
            var date1 = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddDays(3);
            var date2 = date1.AddHours(1);
            var date3 = date1.AddHours(2);

            var event1 = ("e1", date1, date3);
            var event2 = ("e1", date1, date3);
            var event3 = ("e1", date1, date3);

            var event4 = ("e2", date1, date3);
            var event5 = ("e2", date1, date3);
            var event6 = ("e2", date2, date3);

            var event7 = ("e3", date1, date3);
            var event8 = ("e3", date1, date2);
            var event9 = ("e3x", date1, date3);
            var allEvents = new[] {event1, event2, event3, event4, event5, event6, event7, event8, event9};

            foreach (var evt in allEvents)
                await synchronizer.CreateEventInOutlook(evt.Item1, evt.Item2, evt.Item3);

            await synchronizer.SynchronizeAndCheck(
                unchangedA: 0, addedA: 9, changedA: 0, deletedA: 0,
                unchangedB: 0, addedB: 0, changedB: 0, deletedB: 0,
                createA: 0, updateA: 0, deleteA: 0,
                createB: 9, updateB: 0, deleteB: 0);

            var entities = (await synchronizer.Outlook.GetAllEntities()).Select(e => e.Entity).ToList();
            Assert.That(
                entities.Select(e => (e.Inner.Subject, e.Inner.Start, e.Inner.End)),
                Is.EquivalentTo(allEvents));
            entities.ForEach(e => e.Dispose());

            ((EventMappingConfiguration) options.MappingConfiguration).CleanupDuplicateEvents = true;

            synchronizer = await CreateSynchronizer(options);

            await synchronizer.SynchronizeAndCheck(
                unchangedA: 9, addedA: 0, changedA: 0, deletedA: 0,
                unchangedB: 9, addedB: 0, changedB: 0, deletedB: 0,
                createA: 0, updateA: 0, deleteA: 0,
                createB: 0, updateB: 0, deleteB: 0);

            var entitiesAfterClenaup = (await synchronizer.Outlook.GetAllEntities()).Select(e => e.Entity).ToList();
            Assert.That(
                entitiesAfterClenaup.Select(e => (e.Inner.Subject, e.Inner.Start, e.Inner.End)),
                Is.EquivalentTo(new[] {event1, event4, event6, event7, event8, event9}));
            entities.ForEach(e => e.Dispose());
            entitiesAfterClenaup.ForEach(e => e.Dispose());

            _testComponentContainer.AssertNoComObjectInstancesOpen();
        }

        [Ignore("This does currently not work")]
        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents_Google()
        {
            await SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents(_testComponentContainer.TestOptionsFactory.CreateGoogleEvents());
        }

        [Test]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents_Sogo()
        {
            await SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents(_testComponentContainer.TestOptionsFactory.CreateSogoEvents());
        }

        async Task SynchronizeToServer_AllDayEventsWithTimeRangeFilter_DoesntDuplicateOrDeleteBoundaryEvents(Options options)
        {
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
        [TestCase(false)]
        [TestCase(true)]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task SynchronizeTwoWay_LocalEventChanges_IsSyncedToServerAndPreservesExtendedPropertiesAndUid(bool useWebDavCollectionSync)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();
            options.UseWebDavCollectionSync = useWebDavCollectionSync;
            var synchronizer = await CreateSynchronizer(options);
            await synchronizer.ClearEventRepositoriesAndCache();

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

        [Test]
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task CreateOutlookEntity_ExceptionOccurs_DoesNotLeaveEmptyEntityInRepository(bool saveAndReload, bool useWebDavCollectionSync)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();
            options.UseWebDavCollectionSync = useWebDavCollectionSync;
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
        [TestCase(true)]
        [TestCase(false)]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task SynchronizeTwoWay_CacheIsClearedAfterFirstRun_FindsMatchingEntitiesInSecondRun(bool useWebDavCollectionSync)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();
            options.UseWebDavCollectionSync = useWebDavCollectionSync;
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
        [TestCase(true)]
        [TestCase(false)]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task Synchronize_ServerEventContainsOrganizer_IsSyncedToOutlookAndBackToServer(bool useWebDavCollectionSync)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateSogoEvents();
            options.UseWebDavCollectionSync = useWebDavCollectionSync;
            options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
            ((EventMappingConfiguration) options.MappingConfiguration).MapAttendees = true;

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

        [Test]
        [TestCase(null)]
        [TestCase("cat1")]
        [Apartment(System.Threading.ApartmentState.STA)]
        public async Task Synchronizer_ServerMasterEventIsMissing_IsReconstructed(string category)
        {
            var options = _testComponentContainer.TestOptionsFactory.CreateLocalFolderEvents();
            var synchronizer = await CreateSynchronizer(options);

            await synchronizer.ClearEventRepositoriesAndCache();

            await synchronizer.CreateEventInOutlook(
                "Meeting",
                DateTime.Now.AddDays(10),
                DateTime.Now.AddDays(10).AddHours(1),
                false,
                e =>
                {
                    if (category != null)
                        e.Inner.Categories = "cat1";
                    using (var recurrencePatternWrapper = GenericComObjectWrapper.Create(e.Inner.GetRecurrencePattern()))
                    {
                        var recurrencePattern = recurrencePatternWrapper.Inner;
                        recurrencePattern.RecurrenceType = Microsoft.Office.Interop.Outlook.OlRecurrenceType.olRecursDaily;
                        recurrencePattern.Occurrences = 10;
                    }

                    e.SaveAndReload();

                    using (var recurrencePatternWrapper = GenericComObjectWrapper.Create(e.Inner.GetRecurrencePattern()))
                    {
                        var recurrencePattern = recurrencePatternWrapper.Inner;

                        foreach (var exDay in new[] {2, 4, 6})
                        {
                            using (var wrapper = GenericComObjectWrapper.Create(recurrencePattern.GetOccurrence(e.Inner.Start.AddDays(exDay))))
                            {
                                wrapper.Inner.Subject = $"Long Meeting on day {exDay}";
                                wrapper.Inner.End = wrapper.Inner.End.AddHours(1);
                                wrapper.Inner.Save();
                            }
                        }
                    }
                });

            await synchronizer.SynchronizeAndAssertNoErrors();
            synchronizer.ClearCache();
            await synchronizer.Outlook.DeleteAllEntities();


            var entityVersion = (await synchronizer.Components.CalDavDataAccess.GetEventVersions(null)).Single();
            var entity = (await synchronizer.Components.CalDavRepository.Get(new[] {entityVersion.Id}, NullLoadEntityLogger.Instance, NullEventSynchronizationContext.Instance)).Single();
            await synchronizer.Components.CalDavRepository.TryUpdate(
                entityVersion.Id,
                entityVersion.Version,
                entity.Entity,
                c =>
                {
                    var master = c.Events.Single(e => e.Summary == "Meeting");
                    c.Events.Remove(master);
                    return Task.FromResult(c);
                },
                NullEventSynchronizationContext.Instance,
                NullEntitySynchronizationLogger.Instance);

            // Now a server event was set up without an master event
            var report = await synchronizer.Synchronize();

            Assert.That(report.HasErrors, Is.False);
            Assert.That(
                report.EntitySynchronizationReports.SingleOrDefault()?.Warnings.FirstOrDefault(w => w == "CalDav Ressources contains only exceptions. Reconstructing master event."),
                Is.Not.Null);

            using (var outlookEvent = (await synchronizer.Outlook.GetAllEntities()).Single().Entity)
            {
                // the name of the recostructed master event has been taken from the first exception
                Assert.That(outlookEvent.Inner.Subject, Is.EqualTo("Long Meeting on day 2"));
                Assert.That(outlookEvent.Inner.Categories, Is.EqualTo(category));
            }
        }


        private async Task<EventTestSynchronizer> CreateSynchronizer(Options options)
        {
            var synchronizer = new EventTestSynchronizer(options, _testComponentContainer);
            await synchronizer.Initialize();
            return synchronizer;
        }
    }
}
