using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Scheduling;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using Microsoft.Office.Interop.Outlook;
using Rhino.Mocks;
using Application = System.Windows.Forms.Application;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public static class OutlookTestContext
  {
    private static EventEntityMapper _entityMapper;
    private static OutlookEventRepository _outlookRepository;
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

      _entityMapper = new EventEntityMapper (mapiNameSpace.CurrentUser.Address, new Uri ("mailto:" + testerServerEmailAddress), mapiNameSpace.Application.TimeZones.CurrentTimeZone.ID, mapiNameSpace.Application.Version);

      s_outlookFolderEntryId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderEntryId", Environment.MachineName)];
      s_outlookFolderStoreId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderStoreId", Environment.MachineName)];

      _outlookRepository = new OutlookEventRepository (mapiNameSpace, s_outlookFolderEntryId, s_outlookFolderStoreId, NullDateTimeRangeProvider.Instance);

      s_synchronizerFactory = new SynchronizerFactory (
          @"a:\invalid path",
          NullTotalProgressFactory.Instance,
          s_mapiNameSpace,
          TimeSpan.Zero,
          TimeSpan.Zero);

      s_outlookEventRepository = new OutlookEventRepository (
          s_mapiNameSpace,
          s_outlookFolderEntryId,
          s_outlookFolderStoreId,
          NullDateTimeRangeProvider.Instance);
    }

    public static ISynchronizer CreateEventSynchronizer (
        SynchronizationMode mode,
        ICalDavDataAccess calDavDataAccess,
        IEntityRelationDataAccess<string, DateTime, Uri, string> entityRelationDataAccess = null,
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
          entityRelationDataAccess ?? MockRepository.GenerateStub<IEntityRelationDataAccess<string, DateTime, Uri, string>>());
    }


    public static IEntityMapper<AppointmentItemWrapper, IICalendar> EntityMapper
    {
      get { return _entityMapper; }
    }

    public static IEntityRepository<AppointmentItemWrapper, string, DateTime> OutlookRepository
    {
      get { return _outlookRepository; }
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
      var entityRelationStorage = new InMemoryEntityRelationStorage<string, DateTime, IEntityRelationData<string, DateTime, Uri, string>, Uri, string>();

      SyncCalDavToOutlook (eventData, entityRelationStorage);

      var relation = entityRelationStorage.LoadEntityRelationData().First();
      var newRelation = new OutlookEventRelationData()
                        {
                            AtypeId = relation.AtypeId,
                            AtypeVersion = relation.AtypeVersion.AddHours (-1),
                            BtypeId = relation.BtypeId,
                            BtypeVersion = relation.BtypeVersion
                        };
      entityRelationStorage.SaveEntityRelationData (new List<IEntityRelationData<string, DateTime, Uri, string>>() { newRelation });

      return SyncOutlookToCalDav_EventsExistsInCalDav (eventData, entityRelationStorage);
    }

    public static void SyncCalDavToOutlook (string eventData, IEntityRelationDataAccess<string, DateTime, Uri, string> entityRelationDataAccess)
    {
      var calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();

      var entityUri = new Uri ("/e1", UriKind.Relative);

      calDavDataAccess
          .Expect (r => r.GetEvents (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityIdWithVersion<Uri, string>>> (
              new[] { EntityIdWithVersion.Create (entityUri, "v1") }));

      calDavDataAccess
          .Expect (r => r.GetEntities (Arg<ICollection<Uri>>.List.Equal (new[] { entityUri })))
          .Return (Task.FromResult<IReadOnlyList<EntityWithVersion<Uri, string>>> (
              new[] { EntityWithVersion.Create (entityUri, eventData) }));

      var synchronizer = OutlookTestContext.CreateEventSynchronizer (
          SynchronizationMode.ReplicateServerIntoOutlook,
          calDavDataAccess,
          entityRelationDataAccess);

      WaitForTask (synchronizer.Synchronize());
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
        IEntityRelationDataAccess<string, DateTime, Uri, string> entityRelationDataAccess = null,
        Action<Options> optionsModifier = null)
    {
      var calDavEvents = new List<string>();

      ICalDavDataAccess calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();


      calDavDataAccess
          .Expect (r => r.GetEvents (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityIdWithVersion<Uri, string>>> (
              new EntityIdWithVersion<Uri, string>[] { }));
      calDavDataAccess
          .Expect (r => r.CreateEntity (null))
          .IgnoreArguments()
          .Return (Task.FromResult (
              EntityIdWithVersion.Create (new Uri ("http://bla.com"), "blubb")))
          .WhenCalled (a => calDavEvents.Add ((string) a.Arguments[0]));
      ISynchronizer synchronizer = CreateEventSynchronizer (
          SynchronizationMode.ReplicateOutlookIntoServer,
          calDavDataAccess,
          entityRelationDataAccess,
          optionsModifier);
      WaitForTask (synchronizer.Synchronize());
      return calDavEvents;
    }

    public static string SyncOutlookToCalDav_EventsExistsInCalDav (string existingEventData, string existingAppointmentId)
    {
      var entityRelationStorage = new InMemoryEntityRelationStorage<string, DateTime, IEntityRelationData<string, DateTime, Uri, string>, Uri, string>();
      entityRelationStorage.SaveEntityRelationData (new List<IEntityRelationData<string, DateTime, Uri, string>>()
                                                    {
                                                        new OutlookEventRelationData()
                                                        {
                                                            AtypeId = existingAppointmentId,
                                                            AtypeVersion = new DateTime (1),
                                                            BtypeId = new Uri ("/e1", UriKind.Relative),
                                                            BtypeVersion = "v1"
                                                        }
                                                    });

      return SyncOutlookToCalDav_EventsExistsInCalDav (existingEventData, entityRelationStorage);
    }

    public static string SyncOutlookToCalDav_EventsExistsInCalDav (string existingEventData, IEntityRelationDataAccess<string, DateTime, Uri, string> entityRelationDataAccess = null)
    {
      string roundTrippedData = null;
      ICalDavDataAccess calDavDataAccess = MockRepository.GenerateMock<ICalDavDataAccess>();
      var entityUri = new Uri ("/e1", UriKind.Relative);
      calDavDataAccess
          .Expect (r => r.GetEvents (null))
          .IgnoreArguments()
          .Return (Task.FromResult<IReadOnlyList<EntityIdWithVersion<Uri, string>>> (
              new[] { EntityIdWithVersion.Create (entityUri, "v1") }));

      calDavDataAccess
          .Expect (r => r.GetEntities (Arg<ICollection<Uri>>.List.Equal (new[] { entityUri })))
          .Return (Task.FromResult<IReadOnlyList<EntityWithVersion<Uri, string>>> (
              new[] { EntityWithVersion.Create (entityUri, existingEventData) }));

      calDavDataAccess
          .Expect (r => r.UpdateEntity (new Uri ("http://bla.com"), null))
          .IgnoreArguments()
          .Return (Task.FromResult<EntityIdWithVersion<Uri, string>> (
              EntityIdWithVersion.Create (new Uri ("http://bla.com"), "blubb")))
          .WhenCalled (a => { roundTrippedData = (string) a.Arguments[1]; });

      ISynchronizer synchronizer = OutlookTestContext.CreateEventSynchronizer (
          SynchronizationMode.ReplicateOutlookIntoServer,
          calDavDataAccess,
          entityRelationDataAccess);

      WaitForTask (synchronizer.Synchronize());

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

    public static string CreateRecurringEventInOutlook ()
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

        return w.Inner.EntryID;
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

    public static AppointmentItemWrapper GetOutlookEvent (string id)
    {
      return OutlookEventRepository.GetOutlookEventForTesting (id, s_mapiNameSpace, s_outlookFolderStoreId);
    }

   
  }
}