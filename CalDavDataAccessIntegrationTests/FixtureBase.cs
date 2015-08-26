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

      var updated = _calDavDataAccess.UpdateEntity (entitiesWithVersion[1], SerializeCalendar (CreateEntity (600)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (2));

      var updateReverted = _calDavDataAccess.UpdateEntity (updated.Id, SerializeCalendar (CreateEntity (2)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (3));

      // It seems that preconditions are not supported on DELETE by all servers
      //Assert.That (
      //    () => _calDavDataAccess.DeleteEntity (entitiesWithVersion[1]),
      //    Throws.Exception.With.Message.Contains ("(412) Precondition Failed"));

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
    
    //[Test]
    //public void UpdateNonExistingEntity_ViaId ()
    //{
    //  var v1 = _calDavDataAccess.CreateEntity (
    //      SerializeCalendar (
    //          CreateEntity (1)));

    //  _calDavDataAccess.DeleteEntity (v1.Id);

    //  _calDavDataAccess.UpdateEntity (
    //      v1.Id,
    //      SerializeCalendar (
    //          CreateEntity (1)));
    //}

    //[Test]
    //public void UpdateNonExistingEntity_ViaIdAndVersion ()
    //{
    //  var v1 = _calDavDataAccess.CreateEntity (
    //      SerializeCalendar (
    //          CreateEntity (1)));

    //  _calDavDataAccess.DeleteEntity (v1.Id);

    //  _calDavDataAccess.UpdateEntity (
    //      v1,
    //      SerializeCalendar (
    //          CreateEntity (1)));
    //}

    [Test]
    public void TestPreconditionOnUpdate ()
    {
      foreach (var evt in _calDavDataAccess.GetEvents (null))
        _calDavDataAccess.DeleteEntity (evt.Id);

      var v1 = _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (50), DateTime.Now.AddDays (150))).Count,
          Is.EqualTo (1));

      var v2 = _calDavDataAccess.UpdateEntity (v1, SerializeCalendar (CreateEntity (6)));

      // The Combination of Id AND Version has to change!
      // ( When update creates a new Id, the Version can remain e.g. '0'. When the Id doesn't change, the Version has to change. Of course both can change.)
      Assert.That (v2.Id != v1.Id || v2.Version != v1.Version, Is.True);

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (50), DateTime.Now.AddDays (150))).Count,
          Is.EqualTo (0));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (550), DateTime.Now.AddDays (650))).Count,
          Is.EqualTo (1));

      Assert.That (
          () => _calDavDataAccess.UpdateEntity (v1, SerializeCalendar (CreateEntity (1))),
          Throws.Exception.With.Message.Contains ("(412) Precondition Failed"));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (50), DateTime.Now.AddDays (150))).Count,
          Is.EqualTo (0));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (550), DateTime.Now.AddDays (650))).Count,
          Is.EqualTo (1));

      var v3 = _calDavDataAccess.UpdateEntity (v2.Id, SerializeCalendar (CreateEntity (1)));
      Assert.That (v3.Id != v1.Id || v3.Version != v1.Version, Is.True);
      Assert.That (v3.Id != v2.Id || v3.Version != v2.Version, Is.True);

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (50), DateTime.Now.AddDays (150))).Count,
          Is.EqualTo (1));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (550), DateTime.Now.AddDays (650))).Count,
          Is.EqualTo (0));
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