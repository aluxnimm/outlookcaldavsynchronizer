// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.IO;
using System.Reflection;
using System.Xml;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using log4net;

namespace TestRunner
{
  internal static class Program
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

  
    private static void Main (string[] args)
    {
      //var x = DeserializeCalendar (s_testEvent, new iCalendarSerializer ());

      var doc = new XmlDocument();
      doc.LoadXml (@"<r> <c><![CDATA[ bla
                  blubb ]]></c></r>");
      var n = doc.SelectSingleNode ("//c");
      Console.WriteLine (n.InnerText);



    }

    private static void TestLogger ()
    {
      s_logger.InfoFormat ("blablubb");
    }

    private static IICalendar DeserializeCalendar (string iCalData, IStringSerializer calendarSerializer)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
    }


    private const string s_testEvent = @"<![CDATA[
BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//Test Calendar//EN
CALSCALE:GREGORIAN
BEGIN:VTIMEZONE
TZID:America/Denver
BEGIN:DAYLIGHT
TZOFFSETFROM:-0700
RRULE:FREQ=YEARLY;BYMONTH=3;BYDAY=2SU
DTSTART:20070311T020000
TZNAME:MDT
TZOFFSETTO:-0600
END:DAYLIGHT
BEGIN:STANDARD
TZOFFSETFROM:-0600
RRULE:FREQ=YEARLY;BYMONTH=11;BYDAY=1SU
DTSTART:20071104T020000
TZNAME:MST
TZOFFSETTO:-0700
END:STANDARD
END:VTIMEZONE
BEGIN:VEVENT
CREATED:20150827T132921Z
LAST-MODIFIED:20150827T134605Z
UID:6e8ace1a-f769-7184-e1bf-4d1c5ab5300b
SUMMARY:Test Event
DTSTART;TZID=America/Denver:20150828T130000
DTEND;TZID=America/Denver:20150828T130500
DTSTAMP:20150828T130000Z
TRANSP:OPAQUE
PRIORITY:5
LOCATION:
DESCRIPTION:Event
URL;VALUE=URI:
BEGIN:VALARM
TRIGGER:-PT5M
ACTION:DISPLAY
ACTION:AUDIO
END:VALARM
END:VEVENT
END:VCALENDAR
]]>
";

    private const string s_stestEventCreatedByDDay = @"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN
BEGIN:VEVENT
ATTENDEE;CN=""Zehetbauer, Gerhard"":MAILTO:gerhard.zehetbauer@bla.blubb
DTEND:20150121T073000
DTSTAMP:20150123T081359Z
DTSTART:20150121T070000
SEQUENCE:0
SUMMARY:bla
UID:32108289-a814-4263-b693-e36959b92cd2
END:VEVENT
END:VCALENDAR";
  }
}