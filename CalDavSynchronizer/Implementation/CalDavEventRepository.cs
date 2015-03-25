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
using System.Reflection;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using DDay.iCal;
using DDay.iCal.Serialization;
using log4net;

namespace CalDavSynchronizer.Implementation
{
  public class CalDavEventRepository : IEntityRepository<IEvent, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICalDavDataAccess _calDavDataAccess;
    private readonly IStringSerializer _calendarSerializer;

    public CalDavEventRepository (ICalDavDataAccess calDavDataAccess, IStringSerializer calendarSerializer)
    {
      _calDavDataAccess = calDavDataAccess;
      _calendarSerializer = calendarSerializer;
    }

    public Dictionary<Uri, string> GetVersions (DateTime from, DateTime to)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CalDavEventRepository.GetVersions"))
      {
        return _calDavDataAccess.GetEvents (from, to);
      }
    }

    public IReadOnlyDictionary<Uri, IEvent> Get (ICollection<Uri> ids, ITotalProgress progress)
    {
      if (ids.Count == 0)
      {
        progress.StartStep (0, "").Dispose ();
        progress.StartStep (0, "").Dispose ();
        return new Dictionary<Uri, IEvent>();
      }

      using (AutomaticStopwatch.StartInfo (s_logger, string.Format("CalDavEventRepository.Get ({0} entitie(s))",ids.Count)))
      {
        Dictionary<Uri, IEvent> entitiesByKey = new Dictionary<Uri, IEvent>();

        Dictionary<Uri, string> events;
        using (var stepProgress = progress.StartStep (ids.Count, string.Format ("Loading {0} entities from CalDav-Server...", ids.Count)))
        {
          events = _calDavDataAccess.GetEvents (ids);
          stepProgress.IncreaseBy (ids.Count);
        }

        using (var stepProgress = progress.StartStep (events.Count, string.Format ("Deserializing {0} CalDav entities...", events.Count)))
        {
          foreach (var kv in events)
          {
            IEvent evt;
          if (TryDeserializeICalEvent (kv.Value, out evt, kv.Key))
              entitiesByKey.Add (kv.Key, evt);
            stepProgress.Increase();
          }
        }

        return entitiesByKey;
      }
    }

    public bool Delete (Uri entityId)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.DeleteEvent (entityId);
      }
    }

    public EntityIdWithVersion<Uri, string> Update (Uri entityId, IEvent entityToUpdate, Func<IEvent, IEvent> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IEvent newEvent = new Event();
        newEvent = entityModifier (newEvent);

        newEvent.Sequence = entityToUpdate.Sequence + 1;

        return _calDavDataAccess.UpdateEvent (entityId, SerializeCalEvent (newEvent));
      }
    }
 
    public EntityIdWithVersion<Uri, string> Create (Func<IEvent, IEvent> entityInitializer)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IEvent newEvent = new Event();
        newEvent = entityInitializer (newEvent);
        return _calDavDataAccess.CreateEvent (SerializeCalEvent (newEvent));
      }
    }


    private string SerializeCalEvent (IEvent evt)
    {
      var calendar = new iCalendar();
      calendar.Events.Add (evt);
      return _calendarSerializer.SerializeToString (calendar);
    }

    private bool TryDeserializeICalEvent (string iCalData, out IEvent evt, Uri uriOfEventForLogging)
    {
      evt = null;
      try
      {
        evt = DeserializeICalEvent (iCalData);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserilaize ICalData of '{0}':\r\n{1}",uriOfEventForLogging, iCalData), x);
        return false;
      }
    }

    private IEvent DeserializeICalEvent (string iCalData)
    {
      IEvent evt;

      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) _calendarSerializer.Deserialize (reader);
        evt = calendarCollection[0].Events[0];
      }
      return evt;
    }
  }
}