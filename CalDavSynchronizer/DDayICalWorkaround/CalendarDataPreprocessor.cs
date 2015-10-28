using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;

namespace CalDavSynchronizer.DDayICalWorkaround
{
  public static class CalendarDataPreprocessor
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static void FixTimeZoneDSTRRules (TimeZoneInfo tz, DDay.iCal.iCalTimeZone iCalTz)
    {
      var adjustments = tz.GetAdjustmentRules();
      foreach (var tziItems in iCalTz.TimeZoneInfos)
      {
        var matchingAdj = adjustments.FirstOrDefault(a => (a.DateStart.Year == tziItems.Start.Year)) ?? adjustments.FirstOrDefault();
        if (matchingAdj != null && matchingAdj.DateEnd.Year != 9999)
        {
          if (!(tziItems.Name.Equals ("STANDARD") && matchingAdj == adjustments.Last()))
          {
            tziItems.RecurrenceRules[0].Until = DateTime.SpecifyKind (matchingAdj.DateEnd.Date.AddDays (1).Subtract (tz.BaseUtcOffset), DateTimeKind.Utc);
          }
        }
      }
    }

    public static string FixTimeZoneComponentOrderNoThrow (string iCalenderData)
    {
      if (string.IsNullOrEmpty (iCalenderData))
        return iCalenderData;

      try
      {
        var newCalenderData = iCalenderData;

        for (
            var timeZoneMatch = Regex.Match (iCalenderData, "BEGIN:VTIMEZONE\r?\n(.|\n)*?END:VTIMEZONE\r?\n", RegexOptions.RightToLeft);
            timeZoneMatch.Success;
            timeZoneMatch = timeZoneMatch.NextMatch())
        {
          var sections = new List<Tuple<Match, DateTime>>();

          for (
              var sectionMatch = Regex.Match (timeZoneMatch.Value, "BEGIN:STANDARD\r?\n(.|\n)*?END:STANDARD\r?\n", RegexOptions.RightToLeft);
              sectionMatch.Success;
              sectionMatch = sectionMatch.NextMatch())
          {
            var startMatch = Regex.Match (sectionMatch.Value, "DTSTART:(.*?)\r?\n");
            if (startMatch.Success)
            {
              DateTime date;
              if (DateTime.TryParseExact (startMatch.Groups[1].Value, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
              {
                sections.Add (Tuple.Create (sectionMatch, date));
              }
            }
          }

          for (
              var sectionMatch = Regex.Match (timeZoneMatch.Value, "BEGIN:DAYLIGHT\r?\n(.|\n)*?END:DAYLIGHT\r?\n", RegexOptions.RightToLeft);
              sectionMatch.Success;
              sectionMatch = sectionMatch.NextMatch())
          {
            var startMatch = Regex.Match (sectionMatch.Value, "DTSTART:(.*?)\r?\n");
            if (startMatch.Success)
            {
              DateTime date;
              if (DateTime.TryParseExact (startMatch.Groups[1].Value, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
              {
                sections.Add (Tuple.Create (sectionMatch, date));
              }
            }
          }

          if (sections.Count > 0)
          {
            Match firstSection = null;

            var newTimeZoneData = timeZoneMatch.Value;

            foreach (var section in sections.OrderByDescending (s => s.Item1.Index))
            {
              newTimeZoneData = newTimeZoneData.Remove (section.Item1.Index, section.Item1.Length);
              firstSection = section.Item1;
            }

            foreach (var section in sections.OrderByDescending (s => s.Item2))
            {
              newTimeZoneData = newTimeZoneData.Insert (firstSection.Index, section.Item1.Value);
            }

            newCalenderData = newCalenderData.Remove (timeZoneMatch.Index, timeZoneMatch.Length);
            newCalenderData = newCalenderData.Insert (timeZoneMatch.Index, newTimeZoneData);
          }
        }

        return newCalenderData;
      }
      catch (Exception x)
      {
        s_logger.Error ("Could not process calender data. Using original data", x);
        return iCalenderData;
      }
    }
  }
}