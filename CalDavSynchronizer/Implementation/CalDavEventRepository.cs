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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.EntityRepositories;
using CalDavSynchronizer.Generic.EntityVersionManagement;
using CalDavSynchronizer.Generic.ProgressReport;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
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
        progress.StartStep (0, "").Dispose();
        progress.StartStep (0, "").Dispose();
        return new Dictionary<Uri, IEvent>();
      }

      using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CalDavEventRepository.Get ({0} entitie(s))", ids.Count)))
      {
        Dictionary<Uri, string> events;
        using (var stepProgress = progress.StartStep (ids.Count, string.Format ("Loading {0} entities from CalDav-Server...", ids.Count)))
        {
          events = _calDavDataAccess.GetEvents (ids);
          stepProgress.IncreaseBy (ids.Count);
        }

        using (var stepProgress = progress.StartStep (events.Count, string.Format ("Deserializing {0} CalDav entities...", events.Count)))
        {
          return ParallelDeserialize (events);
        }
      }
    }

    private IReadOnlyDictionary<Uri, IEvent> ParallelDeserialize (IReadOnlyDictionary<Uri, string> serializedEvents)
    {
      var result = new Dictionary<Uri, IEvent>();
      Parallel.ForEach (
          serializedEvents,
          () => Tuple.Create (new iCalendarSerializer(), new List<Tuple<Uri, IEvent>>()),
          (serialized, loopState, threadLocal) =>
          {
            IEvent evt;
            if (TryDeserializeICalEvent (serialized.Value, out evt, serialized.Key, threadLocal.Item1))
              threadLocal.Item2.Add (Tuple.Create (serialized.Key, evt));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var evt in threadLocal.Item2)
                result.Add (evt.Item1, evt.Item2);
            }
          });

      return result;
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

    private static bool TryDeserializeICalEvent (string iCalData, out IEvent evt, Uri uriOfEventForLogging, IStringSerializer calendarSerializer)
    {
      evt = null;
      try
      {
        evt = DeserializeICalEvent (iCalData, calendarSerializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserilaize ICalData of '{0}':\r\n{1}", uriOfEventForLogging, iCalData), x);
        return false;
      }
    }

    private static IEvent DeserializeICalEvent (string iCalData, IStringSerializer calendarSerializer)
    {
      IEvent evt;

      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) calendarSerializer.Deserialize (reader);
        evt = calendarCollection[0].Events[0];
      }
      return evt;
    }
  }
}