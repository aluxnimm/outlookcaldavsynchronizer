using System;
using System.Configuration;
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Scheduling;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityMapping;
using GenSync.EntityRelationManagement;
using GenSync.EntityRepositories;
using GenSync.ProgressReport;
using GenSync.Synchronization;
using Microsoft.Office.Interop.Outlook;
using Rhino.Mocks;

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
          TimeSpan.Zero,
          false,
          false,
          true);
    }
    
    public static ISynchronizer CreateEventSynchronizer (SynchronizationMode mode, ICalDavDataAccess calDavDataAccess)
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

      return s_synchronizerFactory.CreateEventSynchronizer (
          options,
          calDavDataAccess,
          MockRepository.GenerateStub<IEntityRelationDataAccess<string, DateTime, Uri, string>>());
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
        return OutlookEventRepository.CreateNewAppointmentForTesting (folderWrapper.Inner, s_mapiNameSpace);
      }
    }
  }
}