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
  internal class InitialTaskEntityMatcher : InitialEntityMatcherByPropertyGrouping<string, DateTime, TaskEntityMatchData, string, WebResourceName, string, IICalendar, string>
  {
    public InitialTaskEntityMatcher (IEqualityComparer<WebResourceName> btypeIdEqualityComparer)
        : base (btypeIdEqualityComparer)
    {
    }

    protected override bool AreEqual (TaskEntityMatchData atypeEntity, IICalendar btypeEntity)
    {
      var task = btypeEntity.Todos[0];

      if (atypeEntity.Subject == task.Summary)
      {
        NodaTime.DateTimeZone localZone = NodaTime.DateTimeZoneProviders.Bcl.GetSystemDefault();
        DateTime dateNull = new DateTime (4501, 1, 1, 0, 0, 0);

        if (task.Start != null)
        {
          if (task.Start.IsUniversalTime)
          {
            if (atypeEntity.StartDate == NodaTime.Instant.FromDateTimeUtc (task.Start.Value).InZone (localZone).ToDateTimeUnspecified().Date)
            {
              if (task.Due != null)
              {
                if (task.Due.IsUniversalTime)
                {
                  return atypeEntity.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
                }
                else
                {
                  return atypeEntity.DueDate == task.Due.Date;
                }
              }
              return atypeEntity.DueDate == dateNull;
            }
            else
              return false;
          }
          else
          {
            if (atypeEntity.StartDate == task.Start.Date)
            {
              if (task.Due != null)
              {
                if (task.Due.IsUniversalTime)
                {
                  return atypeEntity.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
                }
                else
                {
                  return atypeEntity.DueDate == task.Due.Date;
                }
              }
              return atypeEntity.DueDate == dateNull;
            }
            else
              return false;
          }
        }
        else if (task.Due != null)
        {
          if (task.Due.IsUniversalTime)
          {
            return atypeEntity.StartDate == dateNull && atypeEntity.DueDate == NodaTime.Instant.FromDateTimeUtc (task.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
          }
          else
          {
            return atypeEntity.StartDate == dateNull && atypeEntity.DueDate == task.Due.Date;
          }
        }
        else
          return atypeEntity.StartDate == dateNull && atypeEntity.DueDate == dateNull;
      }
      return false;
    }

    protected override string GetAtypePropertyValue (TaskEntityMatchData atypeEntity)
    {
      return (atypeEntity.Subject ?? string.Empty).ToLower();
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