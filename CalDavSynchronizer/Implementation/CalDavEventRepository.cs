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
  public class CalDavEventRepository : IEntityRepository<IICalendar, Uri, string>
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

    public IReadOnlyDictionary<Uri, IICalendar> Get (ICollection<Uri> ids, ITotalProgress progress)
    {
      if (ids.Count == 0)
      {
        progress.StartStep (0, "").Dispose();
        progress.StartStep (0, "").Dispose();
        return new Dictionary<Uri, IICalendar> ();
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

    private IReadOnlyDictionary<Uri, IICalendar> ParallelDeserialize (IReadOnlyDictionary<Uri, string> serializedEvents)
    {
      var result = new Dictionary<Uri, IICalendar> ();
      Parallel.ForEach (
          serializedEvents,
          () => Tuple.Create (new iCalendarSerializer (), new List<Tuple<Uri, IICalendar>> ()),
          (serialized, loopState, threadLocal) =>
          {
            IICalendar calendar;
            if (TryDeserializeICalEvent (serialized.Value, out calendar, serialized.Key, threadLocal.Item1))
              threadLocal.Item2.Add (Tuple.Create (serialized.Key, calendar));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var calendar in threadLocal.Item2)
                result.Add (calendar.Item1, calendar.Item2);
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

    public EntityIdWithVersion<Uri, string> Update (Uri entityId, IICalendar entityToUpdate, Func<IICalendar, IICalendar> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IICalendar newCalendar = new iCalendar ();
        newCalendar = entityModifier (newCalendar);

        newCalendar.Events[0].Sequence = entityToUpdate.Events[0].Sequence + 1;

        return _calDavDataAccess.UpdateEvent (entityId, SerializeCalEvent (newCalendar));
      }
    }

    public EntityIdWithVersion<Uri, string> Create (Func<IICalendar, IICalendar> entityInitializer)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IICalendar newCalendar = new iCalendar ();
        newCalendar = entityInitializer (newCalendar);
        return _calDavDataAccess.CreateEvent (SerializeCalEvent (newCalendar));
      }
    }


    private string SerializeCalEvent (IICalendar calendar)
    {
      return _calendarSerializer.SerializeToString (calendar);
    }

    private static bool TryDeserializeICalEvent (string iCalData, out IICalendar calendar, Uri uriOfEventForLogging, IStringSerializer calendarSerializer)
    {
      calendar = null;
      try
      {
        calendar = DeserializeICalEvent (iCalData, calendarSerializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserilaize ICalData of '{0}':\r\n{1}", uriOfEventForLogging, iCalData), x);
        return false;
      }
    }

    private static IICalendar DeserializeICalEvent (string iCalData, IStringSerializer calendarSerializer)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
    }
  }
}