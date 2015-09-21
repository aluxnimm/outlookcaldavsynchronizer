using System;
using CalDavSynchronizerTestAutomation.Infrastructure;
using NUnit.Framework;

namespace CalDavSynchronizerTestAutomation.SynchronizerTests
{
  [TestFixture]
  public class CalDavToOutlookToCalDavRoundTrip
  {
    //[Test]
    public void KeepsCustomProperties ()
    {
      var eventData = @"
BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.2.16//EN
VERSION:2.0
BEGIN:VEVENT
UID:59E2-55170300-17-5DFC2102
SUMMARY:testmeeting
DTSTART;TZID=Europe/Vienna:20150429T110000
DTEND;TZID=Europe/Vienna:20150429T123000
X-CALDAVSYNCHRONIZER-TEST:This is a test property
END:VEVENT
END:VCALENDAR
      ";

      var roundTrippedData = OutlookTestContext.SyncCalDavToOutlookAndBackToCalDav (eventData);

      Assert.That (roundTrippedData, Is.StringContaining ("X-CALDAVSYNCHRONIZER-TEST:This is a test property"));
    }

    [Test]
    [Ignore ("Automated tests have to be run via TestAutomationPlugin")]
    public void KeepsUid ()
    {
      var eventData = @"
BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.2.16//EN
VERSION:2.0
BEGIN:VEVENT
UID:59E2-55170300-17-5DFC2102
SUMMARY:testmeeting
DTSTART;TZID=Europe/Vienna:20150429T110000
DTEND;TZID=Europe/Vienna:20150429T123000
X-CALDAVSYNCHRONIZER-TEST:This is a test property
END:VEVENT
END:VCALENDAR
      ";

      var roundTrippedData = OutlookTestContext.SyncCalDavToOutlookAndBackToCalDav (eventData);

      Assert.That (roundTrippedData, Is.StringContaining ("UID:59E2-55170300-17-5DFC2102"));
    }
  }
}