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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.InitialEntityMatching;

namespace CalDavSynchronizer.Implementation.Events
{
  internal class InitialEventEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, AppointmentItemWrapper, string, WebResourceName, string, IICalendar, string>
  {
    public InitialEventEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (AppointmentItemWrapper atypeEntity, IICalendar btypeEntity)
    {
      var evt = btypeEntity.Events[0];

      if (evt.Summary == atypeEntity.Inner.Subject)
      {
        if (evt.IsAllDay && atypeEntity.Inner.AllDayEvent)
        {
          if (evt.Start.Value == atypeEntity.Inner.Start)
          {
            if (evt.End == null)
              return evt.Start.Value.AddDays(1) == atypeEntity.Inner.End;
            else
              return evt.End.Value == atypeEntity.Inner.End;
          }
          else return false;
        }
        else if (!evt.IsAllDay)
        {
          if (evt.Start.IsUniversalTime)
          {
            if (evt.Start.Value == atypeEntity.Inner.StartUTC)
            {
              if (evt.DTEnd == null)
                return evt.Start.Value == atypeEntity.Inner.EndUTC;
              else
              {
                if (evt.DTEnd.IsUniversalTime)
                  return evt.DTEnd.Value == atypeEntity.Inner.EndUTC;
                else
                  return evt.DTEnd.Value == atypeEntity.Inner.EndInEndTimeZone;
              }
            }
            else return false;
          }
          else if (evt.Start.Value == atypeEntity.Inner.StartInStartTimeZone)
          {
            if (evt.DTEnd == null)
              return evt.Start.Value == atypeEntity.Inner.EndInEndTimeZone;
            else
            {
              if (evt.DTEnd.IsUniversalTime)
                return evt.DTEnd.Value == atypeEntity.Inner.EndUTC;
              else
                return evt.DTEnd.Value == atypeEntity.Inner.EndInEndTimeZone;
            }
          }
          else
            return false;
        }
      }
      return false;
    }

    protected override string GetAtypePropertyValue (AppointmentItemWrapper atypeEntity)
    {
      return (atypeEntity.Inner.Subject != null ? atypeEntity.Inner.Subject.ToLower() : string.Empty);
    }

    protected override string GetBtypePropertyValue (IICalendar btypeEntity)
    {
      return (btypeEntity.Events[0].Summary != null ? btypeEntity.Events[0].Summary.ToLower() : string.Empty);
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}