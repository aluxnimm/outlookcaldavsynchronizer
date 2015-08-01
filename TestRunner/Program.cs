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
using System.Reflection;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using log4net;

namespace TestRunner
{
  internal static class Program
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private static readonly ICalDavDataAccess s_dataAccess = new CalDavDataAccess (
        new Uri ("XXXXXXXXXXXXXXXXX"),
        new CalDavWebClient (
            "XXXXXXXXX",
            "XXXXXXXXX",
            TimeSpan.FromMinutes (1),
            TimeSpan.FromMinutes (5)));

    private static void Main (string[] args)
    {
      Console.WriteLine (new Uri ("mailto:Gerhard@blablubb.com").AbsolutePath);

      //XmlConfigurator.Configure();

      //TestLogger();
      TestCalDavDataAccess();
      //TestStorageDataAccess();

      //TestCalDavChangeWatcher();
      //TestCalDavDataAccess();
    }

    private static void TestLogger ()
    {
      s_logger.InfoFormat ("blablubb");
    }


    private static void TestCalDavDataAccess ()
    {
      var eventRepository = new CalDavRepository (s_dataAccess, new DDay.iCal.Serialization.iCalendar.iCalendarSerializer(), CalDavRepository.EntityType.Event);

      var versions = eventRepository.GetVersions (DateTime.Now.AddDays (-1000), DateTime.Now.AddDays (1000));
      //var events = eventRepository.Get (versions.Keys);


      //foreach (var e in s_dataAccess.GetEvents (null, null))
      //  s_dataAccess.DeleteEvent (e);

      //var evt = eventRepository.Create (e =>
      //{
      //  e.Summary = "bla";
      //  e.Location = "iwo";
      //  e.Start = new iCalDateTime (DateTime.UtcNow.AddHours (-4));
      //  e.DTEnd = new iCalDateTime (DateTime.UtcNow.AddHours (-3));
      //  return e;
      //});


      //foreach (var evt in x)
      //{
      //  Console.WriteLine ("Value:{0}, {1} ",evt.Summary, evt.Priority);
      //}


      //eventRepository.Update (evt.Id, e =>
      //{
      //  e.Summary = "blubb";
      //  e.Location = "iwo";
      //  e.Start = new iCalDateTime (DateTime.UtcNow.AddHours (-4));
      //  e.DTEnd = new iCalDateTime (DateTime.UtcNow.AddHours (-3));
      //  return e;
      //});
    }


    private const string s_testEvent = @"BEGIN:VCALENDAR
PRODID:-//Inverse inc./SOGo 2.2.13//EN
VERSION:2.0
BEGIN:VEVENT
UID:FFF-54B53A80-1-7919440
SUMMARY:TestEvent3333
LOCATION:Irgendwo
CLASS:PUBLIC
DTSTART:20150114T130000
DTEND:20150114T140000
END:VEVENT
END:VCALENDAR";

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