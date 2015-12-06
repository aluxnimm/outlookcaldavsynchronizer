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
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.DDayICalWorkaround;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using GenSync;
using GenSync.EntityRepositories;
using log4net;

namespace CalDavSynchronizer.Implementation
{
  public class CalDavRepository : IEntityRepository<IICalendar, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICalDavDataAccess _calDavDataAccess;
    private readonly IStringSerializer _calendarSerializer;
    private readonly EntityType _entityType;
    private readonly IDateTimeRangeProvider _dateTimeRangeProvider;

    public enum EntityType
    {
      Event,
      Todo
    }

    public CalDavRepository (ICalDavDataAccess calDavDataAccess, IStringSerializer calendarSerializer, EntityType entityType, IDateTimeRangeProvider dateTimeRangeProvider)
    {
      _calDavDataAccess = calDavDataAccess;
      _calendarSerializer = calendarSerializer;
      _entityType = entityType;
      _dateTimeRangeProvider = dateTimeRangeProvider;
    }

    public Task<IReadOnlyList<EntityVersion<Uri, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<Uri>> idsOfEntitiesToQuery)
    {
      return _calDavDataAccess.GetVersions (idsOfEntitiesToQuery.Select(i => i.Id));
    }

    public Task<IReadOnlyList<EntityVersion<Uri, string>>> GetAllVersions (IEnumerable<Uri> idsOfknownEntities)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CalDavRepository.GetVersions"))
      {
        switch (_entityType)
        {
          case EntityType.Event:

            return _calDavDataAccess.GetEventVersions (_dateTimeRangeProvider.GetRange());
          case EntityType.Todo:
            return _calDavDataAccess.GetTodoVersions (_dateTimeRangeProvider.GetRange());
          default:
            throw new NotImplementedException (string.Format ("EntityType '{0}' not implemented.", _entityType));
        }
      }
    }

    public async Task<IReadOnlyList<EntityWithId<Uri, IICalendar>>> Get (ICollection<Uri> ids)
    {
      if (ids.Count == 0)
        return new EntityWithId<Uri, IICalendar>[] { };

      using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CalDavRepository.Get ({0} entitie(s))", ids.Count)))
      {
        var entities = await _calDavDataAccess.GetEntities (ids);
        return await ParallelDeserialize (entities);
      }
    }

    public void Cleanup (IReadOnlyDictionary<Uri, IICalendar> entities)
    {
      // nothing to do
    }

    private Task<IReadOnlyList<EntityWithId<Uri, IICalendar>>> ParallelDeserialize (IReadOnlyList<EntityWithId<Uri, string>> serializedEntities)
    {
      return Task.Factory.StartNew (() =>
      {
        var result = new List<EntityWithId<Uri, IICalendar>>();

        Parallel.ForEach (
            serializedEntities,
            () => Tuple.Create (new iCalendarSerializer(), new List<Tuple<Uri, IICalendar>>()),
            (serialized, loopState, threadLocal) =>
            {
              IICalendar calendar;
              string normalizedICalData;

              // fix some linebreak issues with Open-Xchange
              if (serialized.Entity.Contains ("\r\r\n"))
              {
                normalizedICalData = CalendarDataPreprocessor.NormalizeLineBreaks (serialized.Entity);
              }
              else
              {
                normalizedICalData = serialized.Entity;
              }

              if (TryDeserializeCalendar (normalizedICalData, out calendar, serialized.Id, threadLocal.Item1))
              {
                threadLocal.Item2.Add (Tuple.Create (serialized.Id, calendar));
              }
              else
              {
                // maybe deserialization failed because of the iCal-TimeZone-Bug =>  try to fix it
                var fixedICalData = CalendarDataPreprocessor.FixTimeZoneComponentOrderNoThrow (normalizedICalData);
                if (TryDeserializeCalendar (fixedICalData, out calendar, serialized.Id, threadLocal.Item1))
                {
                  threadLocal.Item2.Add (Tuple.Create (serialized.Id, calendar));
                  s_logger.Info (string.Format ("Deserialized ICalData with reordering of TimeZone data '{0}'.", serialized.Id));
                }
              }

              return threadLocal;
            },
            threadLocal =>
            {
              lock (result)
              {
                foreach (var calendar in threadLocal.Item2)
                  result.Add (EntityWithId.Create (calendar.Item1, calendar.Item2));
              }
            });

        IReadOnlyList<EntityWithId<Uri, IICalendar>> readOnlyResult = result;
        return readOnlyResult;
      });
    }


    public Task Delete (Uri entityId, string version)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.DeleteEntity (entityId, version);
      }
    }

    public Task<EntityVersion<Uri, string>> Update (
        Uri entityId,
        string entityVersion,
        IICalendar entityToUpdate,
        Func<IICalendar, IICalendar> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        var updatedEntity = entityModifier (entityToUpdate);
        return _calDavDataAccess.UpdateEntity (entityId, entityVersion, SerializeCalendar (updatedEntity));
      }
    }

    public async Task<EntityVersion<Uri, string>> Create (Func<IICalendar, IICalendar> entityInitializer)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IICalendar newCalendar = new iCalendar();
        newCalendar = entityInitializer (newCalendar);
        var uid = (newCalendar.Events.Count > 0) ? newCalendar.Events[0].UID : newCalendar.Todos[0].UID;
        return await _calDavDataAccess.CreateEntity (SerializeCalendar (newCalendar), uid);
      }
    }


    private string SerializeCalendar (IICalendar calendar)
    {
      return _calendarSerializer.SerializeToString (calendar);
    }

    private static bool TryDeserializeCalendar (string iCalData, out IICalendar calendar, Uri uriOfCalendarForLogging, IStringSerializer calendarSerializer)
    {
      calendar = null;
      try
      {
        calendar = DeserializeCalendar (iCalData, calendarSerializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserialize ICalData of '{0}'.", uriOfCalendarForLogging));
        s_logger.Debug (string.Format ("ICalData:\r\n{0}", iCalData), x);
        return false;
      }
    }

    private static IICalendar DeserializeCalendar (string iCalData, IStringSerializer calendarSerializer)
    {
      using (var reader = new StringReader (iCalData))
      {
        var calendarCollection = (iCalendarCollection) calendarSerializer.Deserialize (reader);
        return calendarCollection[0];
      }
    }
  }
}