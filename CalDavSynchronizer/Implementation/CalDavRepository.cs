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
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.DDayICalWorkaround;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Implementation.TimeRangeFiltering;
using DDay.iCal;
using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using GenSync;
using GenSync.EntityRepositories;
using GenSync.Logging;
using log4net;

namespace CalDavSynchronizer.Implementation
{
  public class CalDavRepository : IEntityRepository<IICalendar, WebResourceName, string, int>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICalDavDataAccess _calDavDataAccess;
    private readonly IStringSerializer _calendarSerializer;
    private readonly EntityType _entityType;
    private readonly IDateTimeRangeProvider _dateTimeRangeProvider;
    private readonly bool _deleteAndCreateOnUpdateError403;

    public enum EntityType
    {
      Event,
      Todo
    }

    public CalDavRepository (
      ICalDavDataAccess calDavDataAccess,
      IStringSerializer calendarSerializer,
      EntityType entityType,
      IDateTimeRangeProvider dateTimeRangeProvider,
      bool deleteAndCreateOnUpdateError403)
    {
      _deleteAndCreateOnUpdateError403 = deleteAndCreateOnUpdateError403;
      _calDavDataAccess = calDavDataAccess;
      _calendarSerializer = calendarSerializer;
      _entityType = entityType;
      _dateTimeRangeProvider = dateTimeRangeProvider;
    }

    public Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetVersions (IEnumerable<IdWithAwarenessLevel<WebResourceName>> idsOfEntitiesToQuery)
    {
      return _calDavDataAccess.GetVersions (idsOfEntitiesToQuery.Select(i => i.Id));
    }

    public async Task<IReadOnlyList<EntityVersion<WebResourceName, string>>> GetAllVersions (IEnumerable<WebResourceName> idsOfknownEntities)
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CalDavRepository.GetVersions"))
      {
        switch (_entityType)
        {
          case EntityType.Event:

            return await _calDavDataAccess.GetEventVersions (_dateTimeRangeProvider.GetRange());
          case EntityType.Todo:
            return await _calDavDataAccess.GetTodoVersions (_dateTimeRangeProvider.GetRange());
          default:
            throw new NotImplementedException (string.Format ("EntityType '{0}' not implemented.", _entityType));
        }
      }
    }

    public async Task<IReadOnlyList<EntityWithId<WebResourceName, IICalendar>>> Get (ICollection<WebResourceName> ids, ILoadEntityLogger logger, int context)
    {
      if (ids.Count == 0)
        return new EntityWithId<WebResourceName, IICalendar>[] { };

      using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CalDavRepository.Get ({0} entitie(s))", ids.Count)))
      {
        var entities = await _calDavDataAccess.GetEntities (ids);
        return await ParallelDeserialize (entities, logger);
      }
    }

    public void Cleanup (IReadOnlyDictionary<WebResourceName, IICalendar> entities)
    {
      // nothing to do
    }

    private Task<IReadOnlyList<EntityWithId<WebResourceName, IICalendar>>> ParallelDeserialize (IReadOnlyList<EntityWithId<WebResourceName, string>> serializedEntities, ILoadEntityLogger logger)
    {
      return Task.Factory.StartNew (() =>
      {
        var result = new List<EntityWithId<WebResourceName, IICalendar>>();

        Parallel.ForEach (
            serializedEntities,
            () => Tuple.Create (new iCalendarSerializer(), new List<Tuple<WebResourceName, IICalendar>>()),
            (serialized, loopState, threadLocal) =>
            {
              IICalendar calendar;
              string normalizedICalData, fixedICalData;

              // fix some linebreak issues with Open-Xchange
              if (serialized.Entity.Contains ("\r\r\n"))
              {
                normalizedICalData = CalendarDataPreprocessor.NormalizeLineBreaks (serialized.Entity);
              }
              else
              {
                normalizedICalData = serialized.Entity;
              }

              // emClient sets DTSTART in VTIMEZONE to year 0001, which causes a 90 sec delay in DDay.iCal to evaluate the recurrence rule.
              // If we find such a DTSTART we replace it 0001 with 1970 since the historic data is not valid anyway and avoid the performance issue.
              if (normalizedICalData.Contains ("DTSTART:00010101"))
              {
                fixedICalData = CalendarDataPreprocessor.FixInvalidDTSTARTInTimeZoneNoThrow (normalizedICalData);
                s_logger.InfoFormat ("Changed DTSTART from year 0001 to 1970 in VTIMEZONE of ICalData '{0}'.", serialized.Id);
              }
              else
              {
                fixedICalData = normalizedICalData;
              }

              if (TryDeserializeCalendar (fixedICalData, out calendar, serialized.Id, threadLocal.Item1, NullLoadEntityLogger.Instance))
              {
                threadLocal.Item2.Add (Tuple.Create (serialized.Id, calendar));
              }
              else
              {
                // maybe deserialization failed because of the iCal-TimeZone-Bug =>  try to fix it
                var fixedICalData2 = CalendarDataPreprocessor.FixTimeZoneComponentOrderNoThrow (fixedICalData);
                if (TryDeserializeCalendar (fixedICalData2, out calendar, serialized.Id, threadLocal.Item1, logger))
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

        IReadOnlyList<EntityWithId<WebResourceName, IICalendar>> readOnlyResult = result;
        return readOnlyResult;
      });
    }


    public async Task Delete (WebResourceName entityId, string version)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        await _calDavDataAccess.DeleteEntity (entityId, version);
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> Update (
        WebResourceName entityId,
        string entityVersion,
        IICalendar entityToUpdate,
        Func<IICalendar, IICalendar> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        var updatedEntity = entityModifier (entityToUpdate);
        try
        {
          return await _calDavDataAccess.UpdateEntity (entityId, entityVersion, SerializeCalendar (updatedEntity));
        }
        catch (WebDavClientException ex)
        {
          if (_deleteAndCreateOnUpdateError403 && ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
          {
            s_logger.Warn ("Server returned '403' ('Forbidden') for update, trying Delete and Recreate instead...");

            await Delete (entityId, entityVersion);

            var uid = Guid.NewGuid().ToString();
            if (updatedEntity.Events.Count > 0)
              updatedEntity.Events[0].UID = uid;
            else
              updatedEntity.Todos[0].UID = uid;

            return await _calDavDataAccess.CreateEntity (SerializeCalendar (updatedEntity), uid);
          }
          else
          {
            throw;
          }
        }
      }
    }

    public async Task<EntityVersion<WebResourceName, string>> Create (Func<IICalendar, IICalendar> entityInitializer)
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

    private static bool TryDeserializeCalendar (
      string iCalData, 
      out IICalendar calendar, 
      WebResourceName uriOfCalendarForLogging, 
      IStringSerializer calendarSerializer,
      ILoadEntityLogger logger)
    {
      calendar = null;
      try
      {
        calendar = DeserializeCalendar (iCalData, calendarSerializer);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserialize ICalData of '{0}'.", uriOfCalendarForLogging.OriginalAbsolutePath));
        s_logger.Debug (string.Format ("ICalData:\r\n{0}", iCalData), x);
        logger.LogSkipLoadBecauseOfError (uriOfCalendarForLogging, x);
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