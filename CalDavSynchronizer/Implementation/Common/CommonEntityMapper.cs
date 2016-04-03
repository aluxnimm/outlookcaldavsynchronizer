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
using System.Globalization;
using System.Linq;
using DDay.iCal;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  public static class CommonEntityMapper
  {

    public static void MapDayOfWeek1To2 (OlDaysOfWeek source, IList<IWeekDay> target)
    {
      if ((source & OlDaysOfWeek.olMonday) > 0)
        target.Add (new WeekDay (DayOfWeek.Monday));
      if ((source & OlDaysOfWeek.olTuesday) > 0)
        target.Add (new WeekDay (DayOfWeek.Tuesday));
      if ((source & OlDaysOfWeek.olWednesday) > 0)
        target.Add (new WeekDay (DayOfWeek.Wednesday));
      if ((source & OlDaysOfWeek.olThursday) > 0)
        target.Add (new WeekDay (DayOfWeek.Thursday));
      if ((source & OlDaysOfWeek.olFriday) > 0)
        target.Add (new WeekDay (DayOfWeek.Friday));
      if ((source & OlDaysOfWeek.olSaturday) > 0)
        target.Add (new WeekDay (DayOfWeek.Saturday));
      if ((source & OlDaysOfWeek.olSunday) > 0)
        target.Add (new WeekDay (DayOfWeek.Sunday));
    }

    public static OlDaysOfWeek MapDayOfWeek2To1 (IList<IWeekDay> source)
    {
      OlDaysOfWeek target = 0;

      foreach (var day in source)
      {
        switch (day.DayOfWeek)
        {
          case DayOfWeek.Monday:
            target |= OlDaysOfWeek.olMonday;
            break;
          case DayOfWeek.Tuesday:
            target |= OlDaysOfWeek.olTuesday;
            break;
          case DayOfWeek.Wednesday:
            target |= OlDaysOfWeek.olWednesday;
            break;
          case DayOfWeek.Thursday:
            target |= OlDaysOfWeek.olThursday;
            break;
          case DayOfWeek.Friday:
            target |= OlDaysOfWeek.olFriday;
            break;
          case DayOfWeek.Saturday:
            target |= OlDaysOfWeek.olSaturday;
            break;
          case DayOfWeek.Sunday:
            target |= OlDaysOfWeek.olSunday;
            break;
        }
      }
      return target;
    }

    public static string MapPrivacy1To2 (OlSensitivity value, bool mapPrivateToConfidential)
    {
      switch (value)
      {
        case OlSensitivity.olNormal:
          return "PUBLIC";
        case OlSensitivity.olPersonal:
          return "PRIVATE"; // not sure
        case OlSensitivity.olPrivate:
          return mapPrivateToConfidential ? "CONFIDENTIAL" : "PRIVATE";
        case OlSensitivity.olConfidential:
          return "CONFIDENTIAL";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    public static OlSensitivity MapPrivacy2To1 (string value, bool mapConfidentialToPrivate)
    {
      switch (value)
      {
        case "PUBLIC":
          return OlSensitivity.olNormal;
        case "PRIVATE":
          return OlSensitivity.olPrivate;
        case "CONFIDENTIAL":
          return mapConfidentialToPrivate ? OlSensitivity.olPrivate : OlSensitivity.olConfidential;
      }
      return OlSensitivity.olNormal;
    }

    public static int MapPriority1To2 (OlImportance value)
    {
      switch (value)
      {
        case OlImportance.olImportanceLow:
          return 9;
        case OlImportance.olImportanceNormal:
          return 5;
        case OlImportance.olImportanceHigh:
          return 1;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    public static OlImportance MapPriority2To1 (int value)
    {
      switch (value)
      {
        case 6:
        case 7:
        case 8:
        case 9:
          return OlImportance.olImportanceLow;
        case 0:
        case 5:
          return OlImportance.olImportanceNormal;
        case 1:
        case 2:
        case 3:
        case 4:
          return OlImportance.olImportanceHigh;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    public static string[] SplitCategoryString (string categoryStringOrNull)
    {
      if (!string.IsNullOrEmpty (categoryStringOrNull))
      {
        return
            categoryStringOrNull
                .Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries)
                .Select (c => c.Trim ())
                .ToArray ();
      }
      else
      {
        return new string[] { };
      }
    }
  }
}
