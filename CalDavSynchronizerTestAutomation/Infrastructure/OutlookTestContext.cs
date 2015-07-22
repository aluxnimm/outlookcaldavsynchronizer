using System;
using System.Configuration;
using System.IO;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Events;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync.EntityMapping;
using GenSync.EntityRepositories;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public class OutlookTestContext
  {
    private static EventEntityMapper _entityMapper;
    private static OutlookEventRepository _outlookRepository;
    private static readonly iCalendarSerializer _calendarSerializer = new iCalendarSerializer();
    private static NameSpace s_mapiNameSpace;
    private static string s_outlookFolderEntryId;
    private static string s_outlookFolderStoreId;

    public static void Initialize (NameSpace mapiNameSpace)
    {
      s_mapiNameSpace = mapiNameSpace;
      const string testerServerEmailAddress = "tester@server.com";

      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      _entityMapper = new EventEntityMapper (mapiNameSpace.CurrentUser.Address, new Uri ("mailto:" + testerServerEmailAddress), mapiNameSpace.Application.TimeZones.CurrentTimeZone.ID, mapiNameSpace.Application.Version);

      s_outlookFolderEntryId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderEntryId", Environment.MachineName)];
      s_outlookFolderStoreId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderStoreId", Environment.MachineName)];

      _outlookRepository = new OutlookEventRepository (mapiNameSpace, s_outlookFolderEntryId, s_outlookFolderStoreId);
    }

    public static IEntityMapper<AppointmentItemWrapper, IICalendar> EntityMapper
    {
      get { return _entityMapper; }
    }

    public static IEntityRepository<AppointmentItemWrapper, string, DateTime> OutlookRepository
    {
      get { return _outlookRepository; }
    }

    public static IICalendar DeserializeICalEvent (string iCalData)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) _calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
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