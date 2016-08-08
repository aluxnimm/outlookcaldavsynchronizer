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

      Assert.That (roundTrippedData, Does.Contain ("X-CALDAVSYNCHRONIZER-TEST:This is a test property"));
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

      Assert.That (roundTrippedData, Does.Contain ("UID:59E2-55170300-17-5DFC2102"));
    }
  }
}