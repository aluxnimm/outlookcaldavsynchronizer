using System;
using System.Linq;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizerTestAutomation.Infrastructure;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.SynchronizerTests
{
  [TestFixture]
  public class OutlookToCalDav
  {
    [Test]
    public void TestTimeRangeFilter ()
    {
      OutlookTestContext.DeleteAllOutlookEvents();

      OutlookTestContext.CreateEventInOutlook ("before", DateTime.Now.AddDays (-20), DateTime.Now.AddDays (-11));
      OutlookTestContext.CreateEventInOutlook ("after", DateTime.Now.AddDays (11), DateTime.Now.AddDays (20));
      OutlookTestContext.CreateEventInOutlook ("overlapBeginning", DateTime.Now.AddDays (-11), DateTime.Now.AddDays (-9));
      OutlookTestContext.CreateEventInOutlook ("overlapEnd", DateTime.Now.AddDays (9), DateTime.Now.AddDays (11));
      OutlookTestContext.CreateEventInOutlook ("inside", DateTime.Now.AddDays (-5), DateTime.Now.AddDays (5));
      OutlookTestContext.CreateEventInOutlook ("surrounding", DateTime.Now.AddDays (-11), DateTime.Now.AddDays (11));

      var eventDatas = OutlookTestContext.SyncOutlookToCalDav_CalDavIsEmpty (
          null,
          o =>
          {
            o.DaysToSynchronizeInTheFuture = 10;
            o.DaysToSynchronizeInThePast = 10;
            o.IgnoreSynchronizationTimeRange = false;
          });

      var calendars = eventDatas.Select (OutlookTestContext.DeserializeICalendar).ToArray();

      Assert.That (calendars.Length, Is.EqualTo (4));

      CollectionAssert.AreEquivalent (
          new[]
          {
              "overlapBeginning", "overlapEnd", "inside", "surrounding"
          },
          calendars.Select (c => c.Events[0].Summary));
    }

    [Test]
    public void CreateNewInCalDav ()
    {
      OutlookTestContext.DeleteAllOutlookEvents();

      var appointmentId = OutlookTestContext.CreateRecurringEventInOutlook();

      var eventData = OutlookTestContext.SyncOutlookToCalDav_CalDavIsEmpty();

      CheckEvent (eventData[0], appointmentId, new[] { 0, 1, 2 });
    }

    [Test]
    public void ExistsInCalDav ()
    {
      OutlookTestContext.DeleteAllOutlookEvents();

      var appointmentId = OutlookTestContext.CreateRecurringEventInOutlook();

      var eventData = OutlookTestContext.SyncOutlookToCalDav_EventsExistsInCalDav (s_existingEvent, appointmentId);

      CheckEvent (eventData, appointmentId, new[] { 2, 3, 4 });
    }

    private void CheckEvent (string eventData, string appointmentId, int[] expectedSequenceNumbers)
    {
      var calendar = OutlookTestContext.DeserializeICalendar (eventData);

      Assert.That (calendar.Events.Count, Is.EqualTo (3));

      var masterEvent = calendar.Events.Single (e => e.Summary == "Treffen");
      var exeption1Event = calendar.Events.Single (e => e.Summary == "Ex 1");
      var exeption2Event = calendar.Events.Single (e => e.Summary == "Ex 2");

      CollectionAssert.AreEquivalent (expectedSequenceNumbers, calendar.Events.Select (e => e.Sequence));
      Assert.That (exeption1Event.UID, Is.EqualTo (masterEvent.UID));
      Assert.That (exeption2Event.UID, Is.EqualTo (masterEvent.UID));

      using (var masterAppointment = OutlookTestContext.GetOutlookEvent (appointmentId))
      {
        GenericComObjectWrapper<AppointmentItem> exeption1Appointment;
        GenericComObjectWrapper<AppointmentItem> exeption2Appointment;

        using (var r = GenericComObjectWrapper.Create (masterAppointment.Inner.GetRecurrencePattern()))
        {
          exeption1Appointment = GenericComObjectWrapper.Create (r.Inner.Exceptions[1].AppointmentItem);
          exeption2Appointment = GenericComObjectWrapper.Create (r.Inner.Exceptions[2].AppointmentItem);
        }

        Assert.That (masterEvent.Start.Value, Is.EqualTo (masterAppointment.Inner.Start));
        Assert.That (exeption1Event.Start.Value, Is.EqualTo (exeption1Appointment.Inner.Start));
        Assert.That (exeption2Event.Start.Value, Is.EqualTo (exeption2Appointment.Inner.Start));
      }
    }

    private const string s_existingEvent =
        @"BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.3.1//EN
VERSION:2.0
BEGIN:VTIMEZONE
TZID:Europe/Vienna
X-LIC-LOCATION:Europe/Vienna
BEGIN:DAYLIGHT
TZOFFSETFROM:+0100
TZOFFSETTO:+0200
TZNAME:CEST
DTSTART:19700329T020000
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=-1SU
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:+0200
TZOFFSETTO:+0100
TZNAME:CET
DTSTART:19701025T030000
RRULE:FREQ=YEARLY;BYMONTH=10;BYDAY=-1SU
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
UID:DFC-55D85B80-5-654B3980
SUMMARY:Bla
CLASS:PUBLIC
CREATED:20150822T112315Z
DTSTAMP:20150822T112315Z
LAST-MODIFIED:20150822T112315Z
RRULE:FREQ=DAILY;COUNT=10
DTSTART;TZID=Europe/Vienna:20150811T080000
DTEND;TZID=Europe/Vienna:20150811T090000
TRANSP:TRANSPARENT
END:VEVENT
BEGIN:VEVENT
UID:DFC-55D85B80-5-654B3980
SUMMARY:Bla
CLASS:PUBLIC
CREATED:20150822T112349Z
DTSTAMP:20150822T112349Z
LAST-MODIFIED:20150822T112349Z
DTSTART;TZID=Europe/Vienna:20150812T140000
DTEND;TZID=Europe/Vienna:20150812T150000
TRANSP:TRANSPARENT
RECURRENCE-ID:20150812T060000Z
SEQUENCE:1
END:VEVENT
BEGIN:VEVENT
UID:DFC-55D85B80-5-654B3980
SUMMARY:Bla
CLASS:PUBLIC
CREATED:20150822T112411Z
DTSTAMP:20150822T112411Z
LAST-MODIFIED:20150822T112411Z
DTSTART;TZID=Europe/Vienna:20150814T120000
DTEND;TZID=Europe/Vienna:20150814T130000
TRANSP:TRANSPARENT
RECURRENCE-ID:20150814T060000Z
SEQUENCE:1
END:VEVENT
BEGIN:VEVENT
UID:DFC-55D85B80-5-654B3980
SUMMARY:Bla
CLASS:PUBLIC
CREATED:20150822T112425Z
DTSTAMP:20150822T112425Z
LAST-MODIFIED:20150822T112425Z
DTSTART;TZID=Europe/Vienna:20150818T170000
DTEND;TZID=Europe/Vienna:20150818T180000
TRANSP:TRANSPARENT
RECURRENCE-ID:20150818T060000Z
SEQUENCE:1
END:VEVENT
BEGIN:VEVENT
UID:DFC-55D85B80-5-654B3980
SUMMARY:Bla
CLASS:PUBLIC
CREATED:20150822T112442Z
DTSTAMP:20150822T112442Z
LAST-MODIFIED:20150822T112442Z
DTSTART;TZID=Europe/Vienna:20150820T200000
DTEND;TZID=Europe/Vienna:20150820T210000
TRANSP:TRANSPARENT
RECURRENCE-ID:20150820T060000Z
SEQUENCE:1
END:VEVENT
END:VCALENDAR
";
  }
}