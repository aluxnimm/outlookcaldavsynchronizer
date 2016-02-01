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

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class InitialTaskEntityMatcher : InitialEntityMatcherByPropertyGrouping<TaskItemWrapper, string, DateTime, string, IICalendar, WebResourceName, string, string>
  {
    public InitialTaskEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (TaskItemWrapper atypeEntity, IICalendar btypeEntity)
    {
      var task = btypeEntity.Todos[0];

      if (atypeEntity.Inner.Subject == task.Summary)
      {
        NodaTime.DateTimeZone localZone = NodaTime.DateTimeZoneProviders.Bcl.GetSystemDefault();
        DateTime dateNull = new DateTime (4501, 1, 1, 0, 0, 0);

        if (task.Start != null)
        {
          if (task.Start.IsUniversalTime)
          {
            if (atypeEntity.Inner.StartDate == NodaTime.Instant.FromDateTimeUtc (task.Start.Value).InZone (localZone).ToDateTimeUnspecified().Date)
            {
              if (task.Due != null)
              {
                if (task.Due.IsUniversalTime)
                {
                  return atypeEntity.Inner.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
                }
                else
                {
                  return atypeEntity.Inner.DueDate == task.Due.Date;
                }
              }
              return atypeEntity.Inner.DueDate == dateNull;
            }
            else
              return false;
          }
          else
          {
            if (atypeEntity.Inner.StartDate == task.Start.Date)
            {
              if (task.Due != null)
              {
                if (task.Due.IsUniversalTime)
                {
                  return atypeEntity.Inner.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
                }
                else
                {
                  return atypeEntity.Inner.DueDate == task.Due.Date;
                }
              }
              return atypeEntity.Inner.DueDate == dateNull;
            }
            else
              return false;
          }
        }
        else if (task.Due != null)
        {
          if (task.Due.IsUniversalTime)
          {
            return atypeEntity.Inner.StartDate == dateNull && atypeEntity.Inner.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
          }
          else
          {
            return atypeEntity.Inner.StartDate == dateNull && atypeEntity.Inner.DueDate == task.Due.Date;
          }
        }
        else
          return atypeEntity.Inner.StartDate == dateNull && atypeEntity.Inner.DueDate == dateNull;
      }
      return false;
    }

    protected override string GetAtypePropertyValue (TaskItemWrapper atypeEntity)
    {
      return (atypeEntity.Inner.Subject ?? string.Empty).ToLower();
    }

    protected override string GetBtypePropertyValue (IICalendar btypeEntity)
    {
      return (btypeEntity.Todos[0].Summary ?? string.Empty).ToLower();
    }

    protected override string MapAtypePropertyValue (string value)
    {
      return value;
    }
  }
}