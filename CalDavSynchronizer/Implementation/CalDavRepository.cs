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
using System.Reflection;
using System.Threading.Tasks;
using CalDavSynchronizer.DataAccess;
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

    public IReadOnlyList<EntityIdWithVersion<Uri, string>> GetVersions ()
    {
      using (AutomaticStopwatch.StartInfo (s_logger, "CalDavRepository.GetVersions"))
      {
        switch (_entityType)
        {
          case EntityType.Event:

            return _calDavDataAccess.GetEvents (_dateTimeRangeProvider.GetRange());
          case EntityType.Todo:
            return _calDavDataAccess.GetTodos (_dateTimeRangeProvider.GetRange());
          default:
            throw new NotImplementedException (string.Format ("EntityType '{0}' not implemented.", _entityType));
        }
      }
    }

    public Task<IReadOnlyList<EntityWithVersion<Uri, IICalendar>>> Get (ICollection<Uri> ids)
    {
      return Task.Factory.StartNew (() =>
      {
        if (ids.Count == 0)
          return new EntityWithVersion<Uri, IICalendar>[] { };

        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("CalDavRepository.Get ({0} entitie(s))", ids.Count)))
        {
          var entities = _calDavDataAccess.GetEntities (ids);
          return ParallelDeserialize (entities);
        }
      });
    }

    public void Cleanup (IReadOnlyDictionary<Uri, IICalendar> entities)
    {
      // nothing to do
    }

    private IReadOnlyList<EntityWithVersion<Uri, IICalendar>> ParallelDeserialize (IReadOnlyList<EntityWithVersion<Uri, string>> serializedEntities)
    {
      var result = new List<EntityWithVersion<Uri, IICalendar>>();

      Parallel.ForEach (
          serializedEntities,
          () => Tuple.Create (new iCalendarSerializer(), new List<Tuple<Uri, IICalendar>>()),
          (serialized, loopState, threadLocal) =>
          {
            IICalendar calendar;

            if (TryDeserializeCalendar (serialized.Entity, out calendar, serialized.Id, threadLocal.Item1))
              threadLocal.Item2.Add (Tuple.Create (serialized.Id, calendar));
            return threadLocal;
          },
          threadLocal =>
          {
            lock (result)
            {
              foreach (var calendar in threadLocal.Item2)
                result.Add (EntityWithVersion.Create (calendar.Item1, calendar.Item2));
            }
          });

      return result;
    }


    public bool Delete (Uri entityId)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.DeleteEntity (entityId);
      }
    }

    public EntityIdWithVersion<Uri, string> Update (Uri entityId, IICalendar entityToUpdate, Func<IICalendar, IICalendar> entityModifier)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        var updatedEntity = entityModifier (entityToUpdate);
        return _calDavDataAccess.UpdateEntity (entityId, SerializeCalendar (updatedEntity));
      }
    }

    public EntityIdWithVersion<Uri, string> Create (Func<IICalendar, IICalendar> entityInitializer)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IICalendar newCalendar = new iCalendar();
        newCalendar = entityInitializer (newCalendar);
        return _calDavDataAccess.CreateEntity (SerializeCalendar (newCalendar));
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
        s_logger.Error (string.Format ("Could not deserilaize ICalData of '{0}':\r\n{1}", uriOfCalendarForLogging, iCalData), x);
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