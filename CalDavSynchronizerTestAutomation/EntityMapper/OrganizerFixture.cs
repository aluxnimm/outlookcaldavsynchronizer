using System;
using CalDavSynchronizerTestAutomation.Infrastructure;
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.EntityMapper
{
  [TestFixture]
  public class OrganizerFixture
  {
    [Test]
    [ContainsManualAssert]
    [Ignore ("Automated tests have to be run via TestAutomationPlugin")]
    public void TestOrganizerRoundTrip ()
    {
      var eventData = @"
BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.2.16//EN
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
UID:59E2-55170300-17-5DFC2102
SUMMARY:testmeeting
LOCATION:daheim
DESCRIPTION:important meeting 1
CLASS:PUBLIC
PRIORITY:1
CREATED:20150328T194007Z
DTSTAMP:20150328T194007Z
LAST-MODIFIED:20150328T194140Z
DTSTART;TZID=Europe/Vienna:20150429T110000
DTEND;TZID=Europe/Vienna:20150429T123000
TRANSP:OPAQUE
X-SOGO-SEND-APPOINTMENT-NOTIFICATIONS:NO
ORGANIZER;CN=Test Account:mailto:tw13test@technikum-wien.at
ATTENDEE;PARTSTAT=ACCEPTED;ROLE=REQ-PARTICIPANT;CN=Alexander Nimmervoll:mailto:se13m017@technikum-wien.at
ATTENDEE;PARTSTAT=TENTATIVE;ROLE=REQ-PARTICIPANT;CN=Testaccount OBS2009:mailto:tw09test@technikum-wien.at
END:VEVENT
END:VCALENDAR
      ";

      var evt = OutlookTestContext.DeserializeICalendar (eventData);
      using (var outlookEvent = OutlookTestContext.CreateNewAppointment())
      {
        OutlookTestContext.EntityMapper.Map2To1 (evt, outlookEvent);

        _Inspector inspector = outlookEvent.Inner.GetInspector;

        inspector.Activate();

        ManualAssert.Assert ("Check if the Organizer of the Meeting is Equal to: 'Test Account <tw13test@technikum-wien.at>'");

        inspector.Close (OlInspectorClose.olDiscard);

        var newCalendar = OutlookTestContext.EntityMapper.Map1To2 (outlookEvent, new iCalendar());

        Assert.That (newCalendar.Events[0].Organizer.CommonName, Is.EqualTo ("Test Account"));
        Assert.That (newCalendar.Events[0].Organizer.Value.ToString(), Is.EqualTo ("mailto:tw13test@technikum-wien.at"));

        outlookEvent.Inner.Delete();
      }
    }
  }
}