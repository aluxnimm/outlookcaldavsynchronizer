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
  public class Sogo : FixtureBase
  {
    protected override string ProfileName
    {
      get { return "TestCal-Sogo"; }
    }
  }

  [TestFixture]
  public abstract class FixtureBase
  {
    private static readonly iCalendarSerializer _calendarSerializer = new iCalendarSerializer();

    private CalDavDataAccess _calDavDataAccess;


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

      _calDavDataAccess = new CalDavDataAccess (
          new Uri (options.CalenderUrl),
          new CalDavClient (
              options.UserName,
              options.Password,
              TimeSpan.FromSeconds (30),
              TimeSpan.FromSeconds (30),
              false,
              false,
              true));
    }


    [Test]
    public void IsResourceCalender ()
    {
      Assert.That (_calDavDataAccess.IsResourceCalender(), Is.True);
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
    public void Test ()
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
          () => _calDavDataAccess.UpdateEntity (entitiesWithVersion[1], SerializeCalendar (CreateEntity (200))),
          Throws.Exception.With.Message.Contains ("(412) Precondition Failed"));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (2));

      var updateReverted = _calDavDataAccess.UpdateEntity (entitiesWithVersion[1].Id, SerializeCalendar (CreateEntity (2)));

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (3));

      // It seems that preconditions are not supported on DELETE by all servers
      //Assert.That (
      //    () => _calDavDataAccess.DeleteEntity (entitiesWithVersion[1]),
      //    Throws.Exception.With.Message.Contains ("(412) Precondition Failed"));

      _calDavDataAccess.DeleteEntity (entitiesWithVersion[1].Id);

      Assert.That (
          _calDavDataAccess.GetEvents (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450))).Count,
          Is.EqualTo (2));

      var entites = _calDavDataAccess.GetEntities (entitiesWithVersion.Take (4).Select (e => e.Id));

      Assert.That (entites.Count, Is.EqualTo (3)); // Only 3, the second was deleted

      CollectionAssert.AreEquivalent (
          entites.Select (e => DeserializeCalendar (e.Entity).Events[0].Summary),
          new[] { "Event1", "Event3", "Event4" });
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