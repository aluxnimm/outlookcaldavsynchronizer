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
using System.Reflection;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Common;
using DDay.iCal;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = Microsoft.Office.Interop.Outlook.Exception;
using RecurrencePattern = DDay.iCal.RecurrencePattern;

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class TaskMapper : IEntityMapper<TaskItemWrapper, IICalendar>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly DateTime _dateNull;
    private readonly TimeZoneInfo _localTimeZoneInfo;

    public TaskMapper (string localTimeZoneId)
    {
      _dateNull = new DateTime (4501, 1, 1, 0, 0, 0);
      _localTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (localTimeZoneId);
    }

    public IICalendar Map1To2 (TaskItemWrapper source, IICalendar existingTargetCalender, IEntityMappingLogger logger)
    {
      var newTargetCalender = new iCalendar();
      var localIcalTimeZone = iCalTimeZone.FromSystemTimeZone (_localTimeZoneInfo, new DateTime (1970, 1, 1), true);
     
      DDayICalWorkaround.CalendarDataPreprocessor.FixTimeZoneDSTRRules (_localTimeZoneInfo, localIcalTimeZone);
      newTargetCalender.TimeZones.Add (localIcalTimeZone);

      var existingTargetTodo = existingTargetCalender.Todos.FirstOrDefault (e => e.RecurrenceID == null);

      var newTargetTodo = new Todo();

      if (existingTargetTodo != null)
        newTargetTodo.UID = existingTargetTodo.UID;

      newTargetCalender.Todos.Add (newTargetTodo);

      Map1To2 (source, newTargetTodo, localIcalTimeZone);

      for (int i = 0, newSequenceNumber = existingTargetCalender.Todos.Count > 0 ? existingTargetCalender.Todos.Max (e => e.Sequence) + 1 : 0;
          i < newTargetCalender.Todos.Count;
          i++, newSequenceNumber++)
      {
        newTargetCalender.Todos[i].Sequence = newSequenceNumber;
      }

      return newTargetCalender;
    }

    public void Map1To2 (TaskItemWrapper source, ITodo target, iCalTimeZone localIcalTimeZone)
    {
      target.Summary = source.Inner.Subject;
      target.Description = source.Inner.Body;

      if (source.Inner.StartDate != _dateNull)
      {
        target.Start = new iCalDateTime (source.Inner.StartDate.Year, source.Inner.StartDate.Month, source.Inner.StartDate.Day, true);
        target.Start.SetTimeZone (localIcalTimeZone);
      }

      if (source.Inner.DueDate != _dateNull)
      {
        target.Due = new iCalDateTime (source.Inner.DueDate.Year, source.Inner.DueDate.Month, source.Inner.DueDate.Day, 23, 59, 59);
        target.Due.SetTimeZone (localIcalTimeZone);
        // Workaround for a bug in DDay.iCal, according to RFC5545 DUE must not occur together with DURATION
        target.Properties.Remove (new CalendarProperty ("DURATION"));
      }

      if (source.Inner.Complete && source.Inner.DateCompleted != _dateNull)
      {
        target.Completed = new iCalDateTime (source.Inner.DateCompleted.Year, source.Inner.DateCompleted.Month, source.Inner.DateCompleted.Day, true);
        target.Completed.SetTimeZone (localIcalTimeZone);
      }

      target.PercentComplete = source.Inner.PercentComplete;

      MapRecurrance1To2 (source.Inner, target, localIcalTimeZone);

      target.Properties.Set ("STATUS", MapStatus1To2 (source.Inner.Status));

      target.Priority = CommonEntityMapper.MapPriority1To2 (source.Inner.Importance);

      target.Class = CommonEntityMapper.MapPrivacy1To2 (source.Inner.Sensitivity);

      MapReminder1To2 (source, target);

      MapCategories1To2 (source, target);
    }

    private string MapStatus1To2 (OlTaskStatus value)
    {
      switch (value)
      {
        case OlTaskStatus.olTaskDeferred:
          return "CANCELLED";
        case OlTaskStatus.olTaskComplete:
          return "COMPLETED";
        case OlTaskStatus.olTaskInProgress:
          return "IN-PROCESS";
        case OlTaskStatus.olTaskWaiting:
        case OlTaskStatus.olTaskNotStarted:
          return "NEEDS-ACTION";
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private static void MapCategories1To2 (TaskItemWrapper source, ITodo target)
    {
      if (!string.IsNullOrEmpty (source.Inner.Categories))
      {
        Array.ForEach (
            source.Inner.Categories.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add (c.Trim())
            );
      }
    }

    private void MapReminder1To2 (TaskItemWrapper source, ITodo target)
    {
      if (source.Inner.ReminderSet)
      {
        var trigger = new Trigger();

        if (source.Inner.StartDate != _dateNull)
        {
          trigger.Duration = source.Inner.ReminderTime - source.Inner.StartDate;
          trigger.Parameters.Add ("RELATED", "START");
          trigger.Parameters.Add ("VALUE", "DURATION");

          target.Alarms.Add (
              new Alarm()
              {
                  Trigger = trigger,
                  Description = "This is a task reminder"
              }
              );
          var actionProperty = new CalendarProperty ("ACTION", "DISPLAY");
          target.Alarms[0].Properties.Add (actionProperty);
        }
        else if (source.Inner.DueDate != _dateNull)
        {
          trigger.Duration = source.Inner.ReminderTime - source.Inner.DueDate;
          trigger.Parameters.Add ("RELATED", "END");
          trigger.Parameters.Add ("VALUE", "DURATION");

          target.Alarms.Add (
              new Alarm()
              {
                  Trigger = trigger,
                  Description = "This is a task reminder"
              }
              );
          var actionProperty = new CalendarProperty ("ACTION", "DISPLAY");
          target.Alarms[0].Properties.Add (actionProperty);
        }
      }
    }

    private void MapRecurrance1To2 (TaskItem source, ITodo target, iCalTimeZone localIcalTimeZone)
    {
      if (source.IsRecurring)
      {
        using (var sourceRecurrencePatternWrapper = GenericComObjectWrapper.Create (source.GetRecurrencePattern()))
        {
          var sourceRecurrencePattern = sourceRecurrencePatternWrapper.Inner;
          IRecurrencePattern targetRecurrencePattern = new RecurrencePattern();
          if (!sourceRecurrencePattern.NoEndDate)
          {
            targetRecurrencePattern.Count = sourceRecurrencePattern.Occurrences;
            //Until must not be set if count is set, since outlook always sets Occurrences
            //but sogo wants it as utc end time of the last event not only the enddate at 0000
            //targetRecurrencePattern.Until = sourceRecurrencePattern.PatternEndDate.Add(sourceRecurrencePattern.EndTime.TimeOfDay).ToUniversalTime();
          }
          targetRecurrencePattern.Interval = (sourceRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                              sourceRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth) ? sourceRecurrencePattern.Interval / 12 : sourceRecurrencePattern.Interval;

          switch (sourceRecurrencePattern.RecurrenceType)
          {
            case OlRecurrenceType.olRecursDaily:
              targetRecurrencePattern.Frequency = FrequencyType.Daily;
              break;
            case OlRecurrenceType.olRecursWeekly:
              targetRecurrencePattern.Frequency = FrequencyType.Weekly;
              CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              break;
            case OlRecurrenceType.olRecursMonthly:
              targetRecurrencePattern.Frequency = FrequencyType.Monthly;
              targetRecurrencePattern.ByMonthDay.Add (sourceRecurrencePattern.DayOfMonth);
              break;
            case OlRecurrenceType.olRecursMonthNth:
              targetRecurrencePattern.Frequency = FrequencyType.Monthly;

              if (sourceRecurrencePattern.Instance == 5)
              {
                targetRecurrencePattern.BySetPosition.Add (-1);
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.Instance > 0)
              {
                targetRecurrencePattern.BySetPosition.Add (sourceRecurrencePattern.Instance);
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else
              {
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              break;
            case OlRecurrenceType.olRecursYearly:
              targetRecurrencePattern.Frequency = FrequencyType.Yearly;
              targetRecurrencePattern.ByMonthDay.Add (sourceRecurrencePattern.DayOfMonth);
              targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
              break;
            case OlRecurrenceType.olRecursYearNth:
              targetRecurrencePattern.Frequency = FrequencyType.Yearly;
              if (sourceRecurrencePattern.Instance == 5)
              {
                targetRecurrencePattern.BySetPosition.Add (-1);
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.Instance > 0)
              {
                targetRecurrencePattern.BySetPosition.Add (sourceRecurrencePattern.Instance);
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else
              {
                CommonEntityMapper.MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
              break;
          }

          target.RecurrenceRules.Add (targetRecurrencePattern);
        }
      }
    }

    public TaskItemWrapper Map2To1 (IICalendar sourceCalendar, TaskItemWrapper target, IEntityMappingLogger logger)
    {
      var source = sourceCalendar.Todos[0];
      return Map2To1 (source, target);
    }

    public TaskItemWrapper Map2To1 (ITodo source, TaskItemWrapper target)
    {
      target.Inner.Subject = source.Summary;
      target.Inner.Body = source.Description;

      NodaTime.DateTimeZone localZone = NodaTime.DateTimeZoneProviders.Bcl.GetSystemDefault();

      if (source.Start != null)
      {
        if (source.Start.IsUniversalTime)
        {
          target.Inner.StartDate = NodaTime.Instant.FromDateTimeUtc (source.Start.Value).InZone (localZone).ToDateTimeUnspecified().Date;
        }
        else
        {
          target.Inner.StartDate = source.Start.Date;
        }
      }
       
      if (source.Due != null)
      {
        if (source.Start == null || source.Start.Value <= source.Due.Value)
        {
          if (source.Due.IsUniversalTime)
          {
            target.Inner.DueDate = NodaTime.Instant.FromDateTimeUtc (source.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
          }
          else
          {
            target.Inner.DueDate = source.Due.Date;
          }
        }
          
      }
      if (source.Completed != null)
      {
        if (source.Completed.IsUniversalTime)
        {
          target.Inner.DateCompleted = NodaTime.Instant.FromDateTimeUtc (source.Completed.Value).InZone (localZone).ToDateTimeUnspecified().Date;
        }
        else
        {
          target.Inner.DateCompleted = source.Completed.Date;
        }
        target.Inner.Complete = true;
      }
      else
      {
        target.Inner.Complete = false;
      }

      target.Inner.PercentComplete = source.PercentComplete;

      target.Inner.Importance = CommonEntityMapper.MapPriority2To1 (source.Priority);

      target.Inner.Sensitivity = CommonEntityMapper.MapPrivacy2To1 (source.Class);

      target.Inner.Status = MapStatus2To1 (source.Status);

      MapCategories2To1 (source, target);

      MapReminder2To1 (source, target);

      MapRecurrance2To1 (source, target);

      return target;
    }

    private OlTaskStatus MapStatus2To1 (TodoStatus value)
    {
      switch (value)
      {
        case TodoStatus.Cancelled:
          return OlTaskStatus.olTaskDeferred;
        case TodoStatus.Completed:
          return OlTaskStatus.olTaskComplete;
        case TodoStatus.InProcess:
          return OlTaskStatus.olTaskInProgress;
        case TodoStatus.NeedsAction:
          return OlTaskStatus.olTaskWaiting;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private static void MapCategories2To1 (ITodo source, TaskItemWrapper target)
    {
      target.Inner.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, source.Categories);
    }

    private void MapReminder2To1 (ITodo source, TaskItemWrapper target)
    {
      if (source.Alarms.Count == 0)
      {
        target.Inner.ReminderSet = false;
        return;
      }

      if (source.Alarms.Count > 1)
        s_logger.WarnFormat ("Task '{0}' contains multiple alarms. Ignoring all except first.", source.UID);

      var alarm = source.Alarms[0];

      target.Inner.ReminderSet = true;

      if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.Start && alarm.Trigger.Duration.HasValue && source.Start != null)
      {
        target.Inner.ReminderTime = TimeZoneInfo.ConvertTimeFromUtc (source.Start.UTC, _localTimeZoneInfo).Add (alarm.Trigger.Duration.Value);
      }
      else if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.End && alarm.Trigger.Duration.HasValue && source.Due != null)
      {
        target.Inner.ReminderTime = TimeZoneInfo.ConvertTimeFromUtc (source.Due.UTC, _localTimeZoneInfo).Add (alarm.Trigger.Duration.Value);
      }
      else
      {
        s_logger.WarnFormat ("Task '{0}' alarm is not supported. Ignoring.", source.UID);
        target.Inner.ReminderSet = false;
      }
    }

    private void MapRecurrance2To1 (ITodo source, TaskItemWrapper targetWrapper)
    {
      if (source.RecurrenceRules.Count > 0)
      {
        using (var targetRecurrencePatternWrapper = GenericComObjectWrapper.Create (targetWrapper.Inner.GetRecurrencePattern()))
        {
          var targetRecurrencePattern = targetRecurrencePatternWrapper.Inner;
          if (source.RecurrenceRules.Count > 1)
          {
            s_logger.WarnFormat ("Task '{0}' contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.", source.Url);
          }
          var sourceRecurrencePattern = source.RecurrenceRules[0];

          switch (sourceRecurrencePattern.Frequency)
          {
            case FrequencyType.Daily:
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursDaily;
              break;
            case FrequencyType.Weekly:
              if (sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
              }
              break;
            case FrequencyType.Monthly:
              if (sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthNth;
                if (sourceRecurrencePattern.ByWeekNo.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.Url);
                }
                else if (sourceRecurrencePattern.ByWeekNo.Count > 0)
                {
                  targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];
                }
                else
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.ByDay[0].Offset >= 0) ? sourceRecurrencePattern.ByDay[0].Offset : 5;
                }
                if (sourceRecurrencePattern.BySetPosition.Count > 0)
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.BySetPosition[0] >= 0) ? sourceRecurrencePattern.BySetPosition[0] : 5;
                }
                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.ByMonthDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
                if (sourceRecurrencePattern.ByMonthDay.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.Url);
                }
                targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
              }
              else
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
              }
              break;
            case FrequencyType.Yearly:
              if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByWeekNo.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
                }
                targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                if (sourceRecurrencePattern.ByWeekNo.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.Url);
                }
                targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];

                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByMonthDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
                }
                if (sourceRecurrencePattern.ByMonth[0] != targetRecurrencePattern.MonthOfYear)
                {
                  targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];
                }

                if (sourceRecurrencePattern.ByMonthDay.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.Url);
                }
                if (sourceRecurrencePattern.ByMonthDay[0] != targetRecurrencePattern.DayOfMonth)
                {
                  targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
                }
              }
              else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
                }
                targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                targetRecurrencePattern.Instance = (sourceRecurrencePattern.ByDay[0].Offset >= 0) ? sourceRecurrencePattern.ByDay[0].Offset : 5;
                if (sourceRecurrencePattern.BySetPosition.Count > 0)
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.BySetPosition[0] >= 0) ? sourceRecurrencePattern.BySetPosition[0] : 5;
                }
                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
              }
              break;
            default:
              s_logger.WarnFormat ("Recurring task '{0}' contains the Frequency '{1}', which is not supported by outlook. Ignoring recurrence rule.", source.Url, sourceRecurrencePattern.Frequency);
              targetWrapper.Inner.ClearRecurrencePattern();
              break;
          }

          targetRecurrencePattern.Interval = (targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                              targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth) ? sourceRecurrencePattern.Interval * 12 : sourceRecurrencePattern.Interval;

          if (sourceRecurrencePattern.Count >= 0)
            targetRecurrencePattern.Occurrences = sourceRecurrencePattern.Count;

          if (sourceRecurrencePattern.Until != default(DateTime))
            targetRecurrencePattern.PatternEndDate = sourceRecurrencePattern.Until;
        }

        targetWrapper.SaveAndReload();
      }
    }
  }
}