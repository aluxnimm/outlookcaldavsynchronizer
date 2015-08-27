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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using GenSync;
using NUnit.Framework;

namespace CalDavDataAccessIntegrationTests
{
  [TestFixture]
  public abstract class FixtureBase
  {
    private static readonly iCalendarSerializer _calendarSerializer = new iCalendarSerializer();

    private ICalDavDataAccess _calDavDataAccess;


    protected abstract string ProfileName { get; }

    [TestFixtureSetUp]
    public void Initialize ()
    {
      var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

      var optionsDataAccess = new OptionsDataAccess (
          Path.Combine (
              applicationDataDirectory,
              ComponentContainer.GetOrCreateConfigFileName (applicationDataDirectory, "Outlook")
              ));

      var options = optionsDataAccess.LoadOptions().Single (o => o.Name == ProfileName);

      _calDavDataAccess = Wrap (
          new CalDavDataAccess (
              new Uri (options.CalenderUrl),
              new CalDavClient (
                  options.UserName,
                  options.Password,
                  TimeSpan.FromSeconds (30),
                  TimeSpan.FromSeconds (30),
                  false,
                  false,
                  true)));
    }

    protected virtual ICalDavDataAccess Wrap (ICalDavDataAccess dataAccess)
    {
      return dataAccess;
    }

    [Test]
    public void IsResourceCalender ()
    {
      Assert.That (_calDavDataAccess.IsResourceCalender(), Is.True);
    }

    [Test]
    public void DoesSupportCalendarQuery ()
    {
      Assert.That (_calDavDataAccess.DoesSupportCalendarQuery(), Is.True);
    }

    [Test]
    public void IsCalendarAccessSupported ()
    {
      Assert.That (_calDavDataAccess.IsCalendarAccessSupported(), Is.True);
    }

    [Test]
    public void IsWriteable ()
    {
      Assert.That (_calDavDataAccess.IsWriteable(), Is.True);
    }

    [Test]
    public void Test_CRUD ()
    {
      foreach (var evt in _calDavDataAccess.GetEvents (null))
        _calDavDataAccess.DeleteEntity (evt.Id);


      var entitiesWithVersion = new List<EntityIdWithVersion<Uri, string>>();

      for (int i = 1; i <= 5; i++)
      {
        entitiesWithVersion.Add (
            _calDavDataAccess.CreateEntity (
                SerializeCalendar (
                    CreateEntity (i))));
      }

      var queriedEntitesWithVersion = _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)));

      Assert.That (queriedEntitesWithVersion.Count, Is.EqualTo (3));

      CollectionAssert.IsSubsetOf (
          queriedEntitesWithVersion.Select (e => e.Id),
          entitiesWithVersion.Select (e => e.Id));

      var updated = _calDavDataAccess.UpdateEntity (entitiesWithVersion[1].Id, SerializeCalendar (CreateEntity (600)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (2));

      var updateReverted = _calDavDataAccess.UpdateEntity (updated.Id, SerializeCalendar (CreateEntity (2)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (3));

      _calDavDataAccess.DeleteEntity (updateReverted.Id);

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (2));

      var entites = _calDavDataAccess.GetEntities (entitiesWithVersion.Take (4).Select (e => e.Id));

      Assert.That (entites.Count, Is.EqualTo (3)); // Only 3, the second was deleted

      CollectionAssert.AreEquivalent (
          entites.Select (e => DeserializeCalendar (e.Entity).Events[0].Summary),
          new[] { "Event1", "Event3", "Event4" });
    }

    [Test]
    public void UpdateNonExistingEntity ()
    {
      var v = _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)));

      _calDavDataAccess.DeleteEntity (v.Id);

      v = _calDavDataAccess.UpdateEntity (
          v.Id,
          SerializeCalendar (
              CreateEntity (1)));

      Assert.That (
          _calDavDataAccess.GetEntities (new[] { v.Id }).Count,
          Is.EqualTo (1));
    }

    [Test]
    public void DeleteNonExistingEntity ()
    {
      var v = _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)));

      Assert.That (_calDavDataAccess.DeleteEntity (v.Id), Is.True);
      Assert.That (_calDavDataAccess.DeleteEntity (v.Id), Is.False);
    }


    private iCalendar CreateEntity (int startInHundretDays)
    {
      var calendar = new iCalendar();
      var evt = new Event();
      calendar.Events.Add (evt);
      evt.Start = new iCalDateTime (DateTime.Now.AddDays (startInHundretDays * 100));
      evt.End = new iCalDateTime (DateTime.Now.AddDays (startInHundretDays * 100).AddHours (1));
      evt.Summary = "Event" + startInHundretDays;

      return calendar;
    }


    public static IICalendar DeserializeCalendar (string iCalData)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) _calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
    }

    public static string SerializeCalendar (IICalendar calendar)
    {
      return _calendarSerializer.SerializeToString (calendar);
    }
  }
}