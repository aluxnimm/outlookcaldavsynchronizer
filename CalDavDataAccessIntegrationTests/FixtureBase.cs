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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
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

    protected virtual ServerAdapterType? ServerAdapterTypeOverride
    {
      get { return null; }
    }

    [OneTimeSetUp]
    public void Initialize ()
    {
      var applicationDataDirectory = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "CalDavSynchronizer");

      var optionsDataAccess = new OptionsDataAccess (ComponentContainer.GetOrCreateDataDirectory (applicationDataDirectory, "Outlook").ConfigFilePath);

      var options = optionsDataAccess.Load().Single (o => o.Name == ProfileName);

      if (ServerAdapterTypeOverride.HasValue)
        options.ServerAdapterType = ServerAdapterTypeOverride.Value;

      _calDavDataAccess = new CalDavDataAccess (
          new Uri (options.CalenderUrl),
          SynchronizerFactory.CreateWebDavClient (
              options,
              new OutlookAccountPasswordProvider ("Outlook", "16.0.0.4266"),
              new GeneralOptionsDataAccess().LoadOptions()));
    }

    [Test]
    public virtual async Task IsResourceCalender ()
    {
      Assert.That (await _calDavDataAccess.IsResourceCalender(), Is.True);
    }

    [Test]
    public virtual async Task DoesSupportCalendarQuery ()
    {
      Assert.That (await _calDavDataAccess.DoesSupportCalendarQuery(), Is.True);
    }

    [Test]
    public async Task IsCalendarAccessSupported ()
    {
      Assert.That (await _calDavDataAccess.IsCalendarAccessSupported(), Is.True);
    }

    [Test]
    public async Task IsWriteable ()
    {
      Assert.That (await _calDavDataAccess.GetPrivileges(), Is.EqualTo (AccessPrivileges.All));
    }

    [Test]
    public virtual async Task Test_CRUD ()
    {
      foreach (var evt in await _calDavDataAccess.GetEventVersions (null))
        await _calDavDataAccess.TryDeleteEntity (evt.Id, evt.Version);

      var entitiesWithVersion = new List<EntityVersion<WebResourceName, string>>();

      var uids = new List<string>();

      for (int i = 1; i <= 5; i++)
      {
        var iCalendar = CreateEntity (i);
        uids.Add (iCalendar.Events[0].UID);
        entitiesWithVersion.Add (
            await _calDavDataAccess.CreateEntity (
                SerializeCalendar (
                    iCalendar), iCalendar.Events[0].UID));
      }

      var queriedEntitesWithVersion = await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)));

      Assert.That (queriedEntitesWithVersion.Count, Is.EqualTo (3));

      CollectionAssert.IsSubsetOf (
          queriedEntitesWithVersion.Select (e => e.Id),
          entitiesWithVersion.Select (e => e.Id));

      var updatedCalendar = CreateEntity (600);
      updatedCalendar.Events[0].UID = uids[1];
      var updated = await _calDavDataAccess.TryUpdateEntity (
          entitiesWithVersion[1].Id,
          entitiesWithVersion[1].Version,
          SerializeCalendar (updatedCalendar));

      Assert.That (
          (await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)))).Count,
          Is.EqualTo (2));

      var updatedRevertedCalendar = CreateEntity (2);
      updatedRevertedCalendar.Events[0].UID = uids[1];
      var updateReverted = await _calDavDataAccess.TryUpdateEntity (
          updated.Id,
          updated.Version,
          SerializeCalendar (updatedRevertedCalendar));

      Assert.That (
          (await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)))).Count,
          Is.EqualTo (3));

      await _calDavDataAccess.TryDeleteEntity (updateReverted.Id, updateReverted.Version);

      Assert.That (
          (await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)))).Count,
          Is.EqualTo (2));

      var entites = await _calDavDataAccess.GetEntities (entitiesWithVersion.Take (4).Select (e => e.Id));

      if (!DeletedEntitesAreJustMarkedAsDeletedAndStillAvailableViaCalendarMultigetReport)
      {
        Assert.That (entites.Count, Is.EqualTo (3)); // Only 3, the second was deleted

        CollectionAssert.AreEquivalent (
            entites.Select (e => DeserializeCalendar (e.Entity).Events[0].Summary),
            new[] { "Event1", "Event3", "Event4" });
      }
      else
      {
        CollectionAssert.AreEquivalent (
            entites.Select (e => DeserializeCalendar (e.Entity).Events[0].Summary),
            new[] { "Event1", "Event2", "Event3", "Event4" });
      }
    }


    public virtual async Task Test_CRUD_WithoutTimeRangeFilter ()
    {
      foreach (var evt in await _calDavDataAccess.GetEventVersions (null))
        await _calDavDataAccess.TryDeleteEntity (evt.Id, evt.Version);

      var entitiesWithVersion = new List<EntityVersion<WebResourceName, string>> ();

      var uids = new List<string> ();

      for (int i = 1; i <= 5; i++)
      {
        var iCalendar = CreateEntity (i);
        uids.Add (iCalendar.Events[0].UID);
        entitiesWithVersion.Add (
            await _calDavDataAccess.CreateEntity (
                SerializeCalendar (
                    iCalendar), iCalendar.Events[0].UID));
      }

      var queriedEntitesWithVersion = await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)));

      Assert.That (queriedEntitesWithVersion.Count, Is.EqualTo (5));

      CollectionAssert.AreEquivalent (
          queriedEntitesWithVersion.Select (e => e.Id),
          entitiesWithVersion.Select (e => e.Id));

      var updatedCalendar = CreateEntity (600);
      updatedCalendar.Events[0].UID = uids[1];
      var updated = await _calDavDataAccess.TryUpdateEntity (
          entitiesWithVersion[1].Id,
          entitiesWithVersion[1].Version,
          SerializeCalendar (updatedCalendar));

      var queried2 = await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)));

      var updatedVersion = queried2.FirstOrDefault (v => WebResourceName.Comparer.Equals(v.Id,updated.Id));
      Assert.That (updatedVersion, Is.Not.Null);
      Assert.That (updatedVersion.Version, Is.EqualTo (updated.Version));


      await _calDavDataAccess.TryDeleteEntity (updated.Id, updated.Version);

      var queried3 = await _calDavDataAccess.GetEventVersions (new DateTimeRange (DateTime.Now.AddDays (150), DateTime.Now.AddDays (450)));
      Assert.That (queried3.Count,Is.EqualTo (4));
    }

    protected virtual bool DeletedEntitesAreJustMarkedAsDeletedAndStillAvailableViaCalendarMultigetReport
    {
      get { return false; }
    }
   

    [Test]
    public async Task TryDeleteNonExistingEntity ()
    {
      var v = await _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)), Guid.NewGuid().ToString());

      Assert.That (
          await _calDavDataAccess.TryDeleteEntity (v.Id, v.Version),
          Is.True);

      Assert.That (
          await _calDavDataAccess.TryDeleteEntity (v.Id, @"""bla"""),
          Is.False);
    }

    [Test]
    public virtual async Task TryDeleteEntityWithWrongVersion ()
    {
      var calendar = CreateEntity (1);
      var v = await _calDavDataAccess.CreateEntity (
          SerializeCalendar (calendar),
          calendar.Events[0].UID);
      calendar.Events[0].Summary += "xxx";
      var v2 = await _calDavDataAccess.TryUpdateEntity (
          v.Id,
          v.Version,
          SerializeCalendar (calendar));

      Assert.That (
          await _calDavDataAccess.TryDeleteEntity (v.Id, v.Version),
          Is.False);

      Assert.That (
          await _calDavDataAccess.TryDeleteEntity (v2.Id, v2.Version),
          Is.True);
    }
    
    [Test]
    public async Task UpdateEntityWithWrongVersion ()
    {
      var calendar = CreateEntity (1);
      var v = await _calDavDataAccess.CreateEntity (
          SerializeCalendar (calendar),
          calendar.Events[0].UID);

      calendar.Events[0].Summary += "xxx";

      var v2 = await _calDavDataAccess.TryUpdateEntity (
          v.Id,
          v.Version,
          SerializeCalendar (calendar));

      if (ShouldWaitForNewEtag)
      {
        const int maxRetries = 5;
        for (var i = 0; i < maxRetries && v2.Version == v.Version; i++)
        {
          await Task.Delay(300);
          v2 = (await _calDavDataAccess.GetVersions(new[] {v.Id}))[0];
        }

        if (v2.Version == v.Version)
          Assert.Fail("Server didn't change etag after update"); // Some servers (e.g. Google) don't change etag sometimes
      }

      calendar.Events[0].Summary += "xxx";

      Assert.That (
          await _calDavDataAccess.TryUpdateEntity (
              v.Id,
              v.Version,
              SerializeCalendar (calendar)),
          Is.Null);
    }

    /// <summary>
    /// Some servers do change the entity on a PUT Request before storing it
    /// and therefore must not return an etag (see: https://tools.ietf.org/html/rfc7231#section-4.3.4)
    /// 
    /// </summary>
    protected virtual bool ShouldWaitForNewEtag => false;

    [Test]
    public virtual async Task UpdateNonExistingEntity ()
    {
      var v = await _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)), Guid.NewGuid ().ToString ());

      await _calDavDataAccess.TryDeleteEntity (v.Id, v.Version);

      Assert.That (
          await _calDavDataAccess.TryUpdateEntity (
              v.Id,
              v.Version,
              SerializeCalendar (
                  CreateEntity (1))),
          Is.Null);
    }

    [Test]
    public async Task CreateInvalidEntity ()
    {
      Exception exception = null;
      try
      {
        await _calDavDataAccess.CreateEntity("Invalix CalDav Entity", Guid.NewGuid().ToString());
      }
      catch (Exception x)
      {
        exception = x;
      }

      Assert.That (exception, Is.Not.Null);
      Assert.That(exception, Is.InstanceOf<WebDavClientException>());
    }

    [Test]
    public async Task InvalidUpdateEntity ()
    {
      var v = await _calDavDataAccess.CreateEntity (
          SerializeCalendar (
              CreateEntity (1)), Guid.NewGuid().ToString());

      Exception exception = null;
      try
      {
        await  _calDavDataAccess.TryUpdateEntity (v.Id, v.Version, "Invalid ICal");
      }
      catch (Exception x)
      {
        exception = x;
      }

      Assert.That (exception, Is.Not.Null);
      Assert.That (exception, Is.InstanceOf<WebDavClientException> ());
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