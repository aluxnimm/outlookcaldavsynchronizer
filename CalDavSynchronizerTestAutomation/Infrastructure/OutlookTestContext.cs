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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Implementation.TimeZones;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Synchronization;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.Logging;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using Microsoft.Office.Interop.Outlook;
using Rhino.Mocks;
using Application = System.Windows.Forms.Application;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public static class OutlookTestContext
  {
    private static EventEntityMapper s_entityMapper;
    private static readonly iCalendarSerializer _calendarSerializer = new iCalendarSerializer();
    private static NameSpace s_mapiNameSpace;
    private static string s_outlookFolderEntryId;
    private static string s_outlookFolderStoreId;
    private static SynchronizerFactory s_synchronizerFactory;
    private static OutlookEventRepository s_outlookEventRepository;

    public static void Initialize (NameSpace mapiNameSpace)
    {
      s_mapiNameSpace = mapiNameSpace;
      const string testerServerEmailAddress = "tester@server.com";

      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      var globalTimeZoneCache = new GlobalTimeZoneCache();

      var eventMappingConfiguration = new EventMappingConfiguration();
      s_entityMapper = new EventEntityMapper (
          mapiNameSpace.CurrentUser.Address,
          new Uri ("mailto:" + testerServerEmailAddress),
          mapiNameSpace.Application.TimeZones.CurrentTimeZone.ID,
          mapiNameSpace.Application.Version,
          new TimeZoneCache (null, false, globalTimeZoneCache), 
          eventMappingConfiguration,
          null);

      s_outlookFolderEntryId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderEntryId", Environment.MachineName)];
      s_outlookFolderStoreId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderStoreId", Environment.MachineName)];

      var daslFilterProvider = new DaslFilterProvider (false);

      s_synchronizerFactory = new SynchronizerFactory (
          _ => @"a:\invalid path",
          NullTotalProgressFactory.Instance,
          s_mapiNameSpace,
          daslFilterProvider,
          new OutlookAccountPasswordProvider (mapiNameSpace.CurrentProfileName, mapiNameSpace.Application.Version),
          globalTimeZoneCache,
          QueryOutlookFolderByRequestingItemStrategy.Instance);

      s_outlookEventRepository = new OutlookEventRepository (
          s_mapiNameSpace,
          s_outlookFolderEntryId,
          s_outlookFolderStoreId,
          NullDateTimeRangeProvider.Instance,
          eventMappingConfiguration,
          daslFilterProvider,
          QueryOutlookFolderByRequestingItemStrategy.Instance);
    }

    public static IOutlookSynchronizer CreateEventSynchronizer (
        SynchronizationMode mode,
        ICalDavDataAccess calDavDataAccess,
        IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess = null,
        Action<Options> optionsModifier = null)
    {
      var options = new Options()
                    {
                        ConflictResolution = ConflictResolution.Automatic,
                        EmailAddress = "tester@test.com",
                        IgnoreSynchronizationTimeRange = true,
                        OutlookFolderEntryId = s_outlookFolderEntryId,
                        OutlookFolderStoreId = s_outlookFolderStoreId,
                        SynchronizationMode = mode,
                        CalenderUrl = "http://invalidurl.net"
                    };

      if (optionsModifier != null)
        optionsModifier (options);

      return s_synchronizerFactory.CreateEventSynchronizer (
          options,
          calDavDataAccess,
          entityRelationDataAccess ?? MockRepository.GenerateStub<IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string>>()).Result;
    }


    public static IEntityMapper<AppointmentItemWrapper, IICalendar, IEventSynchronizationContext> EntityMapper
    {
      get { return s_entityMapper; }
    }

    public static OutlookEventRepository EventRepository
    {
      get { return s_outlookEventRepository; }
    }

    public static IICalendar DeserializeICalendar (string iCalData)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) _calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
    }

    public static string SerializeICalendar (IICalendar calendar)
    {
      return _calendarSerializer.SerializeToString (calendar);
    }

    public static AppointmentItemWrapper CreateNewAppointment ()
    {
      using (var folderWrapper = GenericComObjectWrapper.Create ((Folder) s_mapiNameSpace.GetFolderFromID (s_outlookFolderEntryId, s_outlookFolderStoreId)))
      {
        return OutlookEventRepository.CreateNewAppointmentForTesting (folderWrapper.Inner, s_mapiNameSpace, s_outlookFolderStoreId);
      }
    }

    public static string SyncCalDavToOutlookAndBackToCalDav (string eventData)
    {
      var entityRelationStorage = new InMemoryEntityRelationStorage<AppointmentId, DateTime, IEntityRelationData<AppointmentId, DateTime, WebResourceName, string>, WebResourceName, string>();

      SyncCalDavToOutlook (eventData, entityRelationStorage);

      var relation = entityRelationStorage.LoadEntityRelationData().First();
      var newRelation = new OutlookEventRelationData()
                        {
                            AtypeId = relation.AtypeId,
                            AtypeVersion = relation.AtypeVersion.AddHours (-1),
                            BtypeId = relation.BtypeId,
                            BtypeVersion = relation.BtypeVersion
                        };
      entityRelationStorage.SaveEntityRelationData (new List<IEntityRelationData<AppointmentId, DateTime, WebResourceName, string>>() { newRelation });

      return SyncOutlookToCalDav_EventsExistsInCalDav (eventData, entityRelationStorage);
    }

    public static void SyncCalDavToOutlook (string eventData, IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess)
    {
      var calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();

      var entityUri = new WebResourceName("/e1");

      calDavDataAccess
          .Expect (r => r.GetEventVersions (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>> (
              new[] { EntityVersion.Create (entityUri, "v1") }));

      calDavDataAccess
          .Expect (r => r.GetEntities (Arg<ICollection<WebResourceName>>.List.Equal (new[] { entityUri })))
          .Return (Task.FromResult<IReadOnlyList<EntityWithId<WebResourceName, string>>> (
              new[] { EntityWithId.Create (entityUri, eventData) }));

      var synchronizer = OutlookTestContext.CreateEventSynchronizer (
          SynchronizationMode.ReplicateServerIntoOutlook,
          calDavDataAccess,
          entityRelationDataAccess);

      WaitForTask (synchronizer.Synchronize (NullSynchronizationLogger.Instance));
    }

    /// <remarks>
    /// Task.Wait() resp. Task.Result cannot be used, because it will deadlock!
    /// 
    /// Note: In some cases, this might lead to a deadlock: Your call to Result blocks the main thread, 
    /// thereby preventing the remainder of the async code to execute. 
    /// (see http://stackoverflow.com/questions/22628087/calling-async-method-synchronously)
    /// </remarks>
    public static void WaitForTask (Task task)
    {
      while (!task.IsCompleted)
      {
        task.Wait (100);
        Application.DoEvents();
      }
    }

    public static List<string> SyncOutlookToCalDav_CalDavIsEmpty (
        IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess = null,
        Action<Options> optionsModifier = null)
    {
      var calDavEvents = new List<string>();

      ICalDavDataAccess calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();


      calDavDataAccess
          .Expect (r => r.GetEventVersions (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>> (
              new EntityVersion<WebResourceName, string>[] { }));
      calDavDataAccess
          .Expect (r => r.CreateEntity (null, Guid.NewGuid().ToString()))
          .IgnoreArguments()
          .Return (Task.FromResult (
              EntityVersion.Create (new WebResourceName("http://bla.com"), "blubb")))
          .WhenCalled (a => calDavEvents.Add ((string) a.Arguments[0]));
      var synchronizer = CreateEventSynchronizer (
          SynchronizationMode.ReplicateOutlookIntoServer,
          calDavDataAccess,
          entityRelationDataAccess,
          optionsModifier);
      WaitForTask (synchronizer.Synchronize (NullSynchronizationLogger.Instance));
      return calDavEvents;
    }

    public static string SyncOutlookToCalDav_EventsExistsInCalDav (string existingEventData, AppointmentId existingAppointmentId)
    {
      var entityRelationStorage = new InMemoryEntityRelationStorage<AppointmentId, DateTime, IEntityRelationData<AppointmentId, DateTime, WebResourceName, string>, WebResourceName, string>();
      entityRelationStorage.SaveEntityRelationData (new List<IEntityRelationData<AppointmentId, DateTime, WebResourceName, string>>()
                                                    {
                                                        new OutlookEventRelationData()
                                                        {
                                                            AtypeId = existingAppointmentId,
                                                            AtypeVersion = new DateTime (1),
                                                            BtypeId = new WebResourceName("/e1"),
                                                            BtypeVersion = "v1"
                                                        }
                                                    });

      return SyncOutlookToCalDav_EventsExistsInCalDav (existingEventData, entityRelationStorage);
    }

    public static string SyncOutlookToCalDav_EventsExistsInCalDav (string existingEventData, IEntityRelationDataAccess<AppointmentId, DateTime, WebResourceName, string> entityRelationDataAccess = null)
    {
      string roundTrippedData = null;
      ICalDavDataAccess calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();
      var entityUri = new WebResourceName("/e1");
      calDavDataAccess
          .Expect (r => r.GetEventVersions (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityVersion<WebResourceName, string>>> (
              new[] { EntityVersion.Create (entityUri, "v1") }));

      calDavDataAccess
          .Expect (r => r.GetEntities (Arg<ICollection<WebResourceName>>.List.Equal (new[] { entityUri })))
          .Return (Task.FromResult<IReadOnlyList<EntityWithId<WebResourceName, string>>> (
              new[] { EntityWithId.Create (entityUri, existingEventData) }));

      calDavDataAccess
          .Expect (r => r.TryUpdateEntity (
              new WebResourceName("http://bla.com"),
              null,
              null))
          .IgnoreArguments()
          .Return (Task.FromResult<EntityVersion<WebResourceName, string>> (
              EntityVersion.Create (new WebResourceName("http://bla.com"), "blubb")))
          .WhenCalled (a => { roundTrippedData = (string) a.Arguments[2]; });

      var synchronizer = OutlookTestContext.CreateEventSynchronizer (
          SynchronizationMode.ReplicateOutlookIntoServer,
          calDavDataAccess,
          entityRelationDataAccess);

      WaitForTask (synchronizer.Synchronize (NullSynchronizationLogger.Instance));

      return roundTrippedData;
    }

    private static GenericComObjectWrapper<Folder> CreateFolderWrapper ()
    {
      return GenericComObjectWrapper.Create ((Folder) s_mapiNameSpace.GetFolderFromID (s_outlookFolderEntryId, s_outlookFolderStoreId));
    }

    private const string c_entryIdColumnName = "EntryID";

    public static void DeleteAllOutlookEvents ()
    {
      foreach (var e in GetAllOutlookEvents())
        e.Inner.Delete();
    }

    public static IEnumerable<GenericComObjectWrapper<AppointmentItem>> GetAllOutlookEvents ()
    {
      using (var calendarFolderWrapper = CreateFolderWrapper())
      {
        using (var tableWrapper = GenericComObjectWrapper.Create ((Table) calendarFolderWrapper.Inner.GetTable()))
        {
          var table = tableWrapper.Inner;
          table.Columns.RemoveAll();
          table.Columns.Add (c_entryIdColumnName);

          var storeId = calendarFolderWrapper.Inner.StoreID;

          while (!table.EndOfTable)
          {
            var row = table.GetNextRow();
            var entryId = (string) row[c_entryIdColumnName];
            using (var appointmentWrapper = GenericComObjectWrapper.Create ((AppointmentItem) s_mapiNameSpace.GetItemFromID (entryId, storeId)))
            {
              yield return appointmentWrapper;
            }
          }
        }
      }
    }

    public static AppointmentId CreateRecurringEventInOutlook ()
    {
      using (var w = CreateNewAppointment())
      {
        w.Inner.Start = DateTime.Now.Date.AddDays (2).Date.AddHours (9);
        w.Inner.End = w.Inner.Start.AddHours (1);
        w.Inner.Subject = "Treffen";


        using (var r = GenericComObjectWrapper.Create (w.Inner.GetRecurrencePattern()))
        {
          r.Inner.RecurrenceType = OlRecurrenceType.olRecursWeekly;
          r.Inner.DayOfWeekMask = OlDaysOfWeek.olMonday;
        }

        w.SaveAndReload();

        using (var r = GenericComObjectWrapper.Create (w.Inner.GetRecurrencePattern()))
        {
          using (var ex = GenericComObjectWrapper.Create (r.Inner.GetOccurrence (w.Inner.Start.AddDays (7))))
          {
            ex.Inner.Start = ex.Inner.Start.AddDays (1);
            ex.Inner.Subject = "Ex 1";
            ex.Inner.Save();
          }

          using (var ex = GenericComObjectWrapper.Create (r.Inner.GetOccurrence (w.Inner.Start.AddDays (14))))
          {
            ex.Inner.Start = ex.Inner.Start.AddDays (2);
            ex.Inner.Subject = "Ex 2";
            ex.Inner.Save();
          }
        }

        w.Inner.Save();

        return new AppointmentId(w.Inner.EntryID, w.Inner.GlobalAppointmentID);
      }
    }

    public static string CreateEventInOutlook (string subject, DateTime start, DateTime end)
    {
      using (var w = CreateNewAppointment())
      {
        w.Inner.Start = start;
        w.Inner.End = end;
        w.Inner.Subject = subject;

        w.Inner.Save();

        return w.Inner.EntryID;
      }
    }

    public static AppointmentItemWrapper GetOutlookEvent (AppointmentId id)
    {
      return OutlookEventRepository.GetOutlookEventForTesting (id.EntryId, s_mapiNameSpace, s_outlookFolderStoreId);
    }
  }
}