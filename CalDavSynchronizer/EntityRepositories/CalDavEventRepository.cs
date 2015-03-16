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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.EntityVersionManagement;
using DDay.iCal;
using DDay.iCal.Serialization;
using log4net;

namespace CalDavSynchronizer.EntityRepositories
{
  public class CalDavEventRepository : EntityRepositoryBase<IEvent, Uri, string>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly ICalDavDataAccess _calDavDataAccess;
    private readonly IStringSerializer _calendarSerializer;

    public CalDavEventRepository (ICalDavDataAccess calDavDataAccess, IStringSerializer calendarSerializer)
    {
      _calDavDataAccess = calDavDataAccess;
      _calendarSerializer = calendarSerializer;
    }

    public override IEnumerable<EntityIdWithVersion<Uri, string>> GetEntityVersions (DateTime from, DateTime to)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.GetEvents (from, to);
      }
    }

    public override IDictionary<Uri, IEvent> GetEntities (IEnumerable<Uri> sourceEntityIds)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        Dictionary<Uri, IEvent> entitiesByKey = new Dictionary<Uri, IEvent>();

        foreach (var kv in _calDavDataAccess.GetEvents (sourceEntityIds))
        {
          IEvent evt;
          if (TryDeserializeICalEvent (kv.Value, out evt))
            entitiesByKey.Add (kv.Key, evt);
        }

        return entitiesByKey;
      }
    }

    public override bool Delete (Uri entityId)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        return _calDavDataAccess.DeleteEvent (entityId);
      }
    }

    public override EntityIdWithVersion<Uri, string> Update (Uri entityId, Func<IEvent, IEvent> entityModifier, IEvent cachedCurrentTargetEntityIfAvailable)
    {
      using (AutomaticStopwatch.StartDebug (s_logger))
      {
        IEvent newEvent = new Event();
        newEvent = entityModifier (newEvent);

        if (cachedCurrentTargetEntityIfAvailable == null)
        {
          s_logger.WarnFormat ("Event '{0}' is not in cache and has to be loaded from Server. This could cause performance problems", entityId);
          cachedCurrentTargetEntityIfAvailable = GetEvent (entityId);
        }

        newEvent.Sequence = cachedCurrentTargetEntityIfAvailable.Sequence + 1;

        return _calDavDataAccess.UpdateEvent (entityId, SerializeCalEvent (newEvent));
      }
    }

    private IEvent GetEvent (Uri id)
    {
      var currentEventData = _calDavDataAccess.GetEvents (new[] { id }).First().Value;
      return DeserializeICalEvent (currentEventData);
    }

    public override EntityIdWithVersion<Uri, string> Create (Func<IEvent, IEvent> entityInitializer)
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

    private bool TryDeserializeICalEvent (string iCalData, out IEvent evt)
    {
      evt = null;
      try
      {
        evt = DeserializeICalEvent (iCalData);
        return true;
      }
      catch (Exception x)
      {
        s_logger.Error (string.Format ("Could not deserilaize ICalData:\r\n{0}", iCalData), x);
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