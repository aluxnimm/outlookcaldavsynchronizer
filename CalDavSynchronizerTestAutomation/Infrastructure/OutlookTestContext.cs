using System;
using System.Configuration;
using System.IO;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Generic.EntityRepositories;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizerTestAutomation.Infrastructure
{
  public class OutlookTestContext
  {
    private static AppointmentEventEntityMapper _entityMapper;
    private static OutlookAppointmentRepository _outlookRepository;
    private static readonly iCalendarSerializer _calendarSerializer = new iCalendarSerializer ();
    private static Folder s_calendarFolder;
    private static NameSpace s_mapiNameSpace;

    public static void Initialize (NameSpace mapiNameSpace)
    {
      s_mapiNameSpace = mapiNameSpace;
      const string testerServerEmailAddress = "tester@server.com";

      if (mapiNameSpace == null)
        throw new ArgumentNullException ("mapiNameSpace");

      _entityMapper = new AppointmentEventEntityMapper (mapiNameSpace.CurrentUser.Address, new Uri ("mailto:" + testerServerEmailAddress), mapiNameSpace.Application.TimeZones.CurrentTimeZone.ID, mapiNameSpace.Application.Version);

      var outlookFolderEntryId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderEntryId", Environment.MachineName)];
      var outlookFolderStoreId = ConfigurationManager.AppSettings[string.Format ("{0}.OutlookFolderStoreId", Environment.MachineName)];

      s_calendarFolder = (Folder) mapiNameSpace.GetFolderFromID (outlookFolderEntryId, outlookFolderStoreId);
      _outlookRepository = new OutlookAppointmentRepository (s_calendarFolder, mapiNameSpace);
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
      return OutlookAppointmentRepository.CreateNewAppointmentForTesting (s_calendarFolder, s_mapiNameSpace);
    }
  }
}