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
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DDayICalWorkaround;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Implementation.Common;
using DDay.iCal;
using GenSync.EntityMapping;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using NodaTime;
using Exception = Microsoft.Office.Interop.Outlook.Exception;
using RecurrencePattern = DDay.iCal.RecurrencePattern;

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class TaskMapper : IEntityMapper<ITaskItemWrapper, IICalendar, int>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly TimeZoneInfo _localTimeZoneInfo;
    private readonly TaskMappingConfiguration _configuration;

    public TaskMapper (string localTimeZoneId, TaskMappingConfiguration configuration)
    {
      _localTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (localTimeZoneId);
      _configuration = configuration;
    }

    public Task<IICalendar> Map1To2 (ITaskItemWrapper source, IICalendar existingTargetCalender, IEntitySynchronizationLogger logger, int context)
    {
      var newTargetCalender = new iCalendar();
      var localIcalTimeZone = iCalTimeZone.FromSystemTimeZone (_localTimeZoneInfo, new DateTime (1970, 1, 1), true);
     
      DDayICalWorkaround.CalendarDataPreprocessor.FixTimeZoneDSTRRules (_localTimeZoneInfo, localIcalTimeZone);
      if (!_configuration.MapStartAndDueAsFloating) newTargetCalender.TimeZones.Add (localIcalTimeZone);

      var existingTargetTodo = existingTargetCalender.Todos.FirstOrDefault (e => e.RecurrenceID == null);

      var newTargetTodo = new Todo();

      if (existingTargetTodo != null)
        newTargetTodo.UID = existingTargetTodo.UID;

      newTargetCalender.Todos.Add (newTargetTodo);

      Map1To2 (source, newTargetTodo, localIcalTimeZone, logger);

      for (int i = 0, newSequenceNumber = existingTargetCalender.Todos.Count > 0 ? existingTargetCalender.Todos.Max (e => e.Sequence) + 1 : 0;
          i < newTargetCalender.Todos.Count;
          i++, newSequenceNumber++)
      {
        newTargetCalender.Todos[i].Sequence = newSequenceNumber;
      }

      return Task.FromResult<IICalendar>(newTargetCalender);
    }

    public void Map1To2 (ITaskItemWrapper source, ITodo target, iCalTimeZone localIcalTimeZone, IEntitySynchronizationLogger logger)
    {
      target.Summary = CalendarDataPreprocessor.EscapeBackslash (source.Inner.Subject);

      if (_configuration.MapBody)
        target.Description = CalendarDataPreprocessor.EscapeBackslash (source.Inner.Body);

      if (source.Inner.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
      {
        target.Start = new iCalDateTime (source.Inner.StartDate.Year, source.Inner.StartDate.Month, source.Inner.StartDate.Day, true);
        if (!_configuration.MapStartAndDueAsFloating) target.Start.SetTimeZone (localIcalTimeZone);
      }

      if (source.Inner.Complete && source.Inner.DateCompleted != OutlookUtility.OUTLOOK_DATE_NONE)
      {
        target.Completed = new iCalDateTime (source.Inner.DateCompleted.ToUniversalTime()) { IsUniversalTime = true, HasTime = true};
      }

      target.PercentComplete = source.Inner.PercentComplete;

      if (_configuration.MapRecurringTasks)
        MapRecurrance1To2 (source.Inner, target, localIcalTimeZone);

      if (source.Inner.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
      {
        target.Due = new iCalDateTime (source.Inner.DueDate.Year, source.Inner.DueDate.Month, source.Inner.DueDate.Day, 23, 59, 59);
        if (!_configuration.MapStartAndDueAsFloating) target.Due.SetTimeZone (localIcalTimeZone);
        
        // Workaround for a bug in DDay.iCal, according to RFC5545 DUE must not occur together with DURATION
        target.Properties.Remove (new CalendarProperty ("DURATION"));
      }

      target.Properties.Set ("STATUS", MapStatus1To2 (source.Inner.Status));

      if (_configuration.MapPriority)
        target.Priority = CommonEntityMapper.MapPriority1To2 (source.Inner.Importance);

      target.Class = CommonEntityMapper.MapPrivacy1To2 (source.Inner.Sensitivity, false, false);

      MapReminder1To2 (source, target);

      MapCategories1To2 (source, target);

      if (_configuration.MapCustomProperties || _configuration.UserDefinedCustomPropertyMappings.Length > 0)
      {
        using (var userPropertiesWrapper = GenericComObjectWrapper.Create (source.Inner.UserProperties))
        {
          CommonEntityMapper.MapCustomProperties1To2 (userPropertiesWrapper, target.Properties, _configuration.MapCustomProperties,_configuration.UserDefinedCustomPropertyMappings ,logger, s_logger);
        }
      }
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

    private void MapCategories1To2 (ITaskItemWrapper source, ITodo target)
    {
      if (!string.IsNullOrEmpty (source.Inner.Categories))
      {
        var useTaskCategoryAsFilter = _configuration.UseTaskCategoryAsFilter;

        var sourceCategories = CommonEntityMapper.SplitCategoryString (source.Inner.Categories)
                .Where(c => !useTaskCategoryAsFilter || c != _configuration.TaskCategory);

        foreach (var sourceCategory in sourceCategories)
        {
          target.Categories.Add (sourceCategory);
        }
      }
    }

    private void MapReminder1To2 (ITaskItemWrapper source, ITodo target)
    {
      if (_configuration.MapReminder == ReminderMapping.@false)
        return;

      if (source.Inner.ReminderSet)
      {
        if (_configuration.MapReminder == ReminderMapping.JustUpcoming
            && source.Inner.ReminderTime <= DateTime.Now)
          return;

        var trigger = new Trigger();

        if (!_configuration.MapReminderAsDateTime && source.Inner.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
        {
          trigger.Duration = source.Inner.ReminderTime - source.Inner.StartDate;
          trigger.Parameters.Add ("RELATED", "START");
          trigger.Parameters.Add ("VALUE", "DURATION");

          target.Alarms.Add (
              new Alarm()
              {
                  Description = "This is a task reminder"
              }
              );
          // Fix DDay.iCal TimeSpan 0 serialization
          if (trigger.Duration.Value == TimeSpan.Zero)
          {
            target.Alarms[0].Properties.Add (new CalendarProperty ("TRIGGER", "-P0D"));
          }
          else
          {
            target.Alarms[0].Trigger = trigger;
          }
          var actionProperty = new CalendarProperty ("ACTION", "DISPLAY");
          target.Alarms[0].Properties.Add (actionProperty);
        }
        else if (!_configuration.MapReminderAsDateTime && source.Inner.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
        {
          var dueTime = new DateTime (source.Inner.DueDate.Year, source.Inner.DueDate.Month, source.Inner.DueDate.Day, 23, 59, 59);
          trigger.Duration = source.Inner.ReminderTime - dueTime;
          trigger.Parameters.Add ("RELATED", "END");
          trigger.Parameters.Add ("VALUE", "DURATION");

          target.Alarms.Add (
            new Alarm()
            {
              Description = "This is a task reminder"
            }
            );
          // Fix DDay.iCal TimeSpan 0 serialization
          if (trigger.Duration.Value == TimeSpan.Zero)
          {
            target.Alarms[0].Properties.Add (new CalendarProperty ("TRIGGER", "P0D"));
          }
          else
          {
            target.Alarms[0].Trigger = trigger;
          }
          var actionProperty = new CalendarProperty ("ACTION", "DISPLAY");
          target.Alarms[0].Properties.Add (actionProperty);
        }
        else
        {
          trigger.DateTime = new iCalDateTime (source.Inner.ReminderTime.ToUniversalTime()) { IsUniversalTime = true, HasTime = true };
          trigger.Parameters.Add ("VALUE", "DATE-TIME");
          target.Alarms.Add (
            new Alarm()
            {
              Description = "This is a task reminder"
            }
            );
          target.Alarms[0].Trigger = trigger;
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

          // Recurring task must have a DTSTART according to the RFC but Outlook may have no task start date set, use PatternStartDate in this case
          if (source.StartDate == OutlookUtility.OUTLOOK_DATE_NONE)
          {
            target.Start = new iCalDateTime ( sourceRecurrencePattern.PatternStartDate.Year,
                                              sourceRecurrencePattern.PatternStartDate.Month, sourceRecurrencePattern.PatternStartDate.Day, true);
            if (!_configuration.MapStartAndDueAsFloating) target.Start.SetTimeZone (localIcalTimeZone);
          }
          IRecurrencePattern targetRecurrencePattern = new RecurrencePattern();

          // Don't set Count if pattern has NoEndDate or invalid Occurences for some reason.
          if (!sourceRecurrencePattern.NoEndDate && sourceRecurrencePattern.Occurrences > 0)
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

    public Task<ITaskItemWrapper> Map2To1 (IICalendar sourceCalendar, ITaskItemWrapper target, IEntitySynchronizationLogger logger, int context)
    {
      var source = sourceCalendar.Todos[0];
      return Task.FromResult(Map2To1 (source, target, logger));
    }

    public ITaskItemWrapper Map2To1 (ITodo source, ITaskItemWrapper target, IEntitySynchronizationLogger logger)
    {
      target.Inner.Subject = source.Summary;

      target.Inner.Body = _configuration.MapBody ? source.Description : string.Empty;

      DateTimeZone localZone = DateTimeZoneProviders.Bcl.GetSystemDefault();

      if (source.Start != null)
      {
        if (source.Start.IsUniversalTime)
        {
          target.Inner.StartDate = Instant.FromDateTimeUtc (source.Start.Value).InZone (localZone).ToDateTimeUnspecified().Date;
        }
        else
        {
          target.Inner.StartDate = source.Start.Date;
        }
      }
      else
      {
        target.Inner.StartDate = OutlookUtility.OUTLOOK_DATE_NONE;
      }

      if (source.Due != null)
      {
        if (source.Start == null || source.Start.Value <= source.Due.Value)
        {
          if (source.Due.IsUniversalTime)
          {
            target.Inner.DueDate = Instant.FromDateTimeUtc (source.Due.Value).InZone (localZone).ToDateTimeUnspecified().Date;
          }
          else
          {
            target.Inner.DueDate = source.Due.Date;
          }
        }
      }
      else
      {
        target.Inner.DueDate = OutlookUtility.OUTLOOK_DATE_NONE;
      }

      if (source.Completed != null)
      {
        if (source.Completed.IsUniversalTime)
        {
          target.Inner.DateCompleted = Instant.FromDateTimeUtc (source.Completed.Value).InZone (localZone).ToDateTimeUnspecified().Date;
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

      target.Inner.Status = (target.Inner.Complete && target.Inner.PercentComplete == 100) ? OlTaskStatus.olTaskComplete : MapStatus2To1 (source.Status);

      target.Inner.PercentComplete = source.PercentComplete;

      if (_configuration.MapPriority)
        target.Inner.Importance = CommonEntityMapper.MapPriority2To1 (source.Priority);

      target.Inner.Sensitivity = CommonEntityMapper.MapPrivacy2To1 (source.Class, false, false);

      MapCategories2To1 (source, target);

      MapReminder2To1 (source, target, logger);

      if (_configuration.MapCustomProperties || _configuration.UserDefinedCustomPropertyMappings.Length > 0)
      {
        using (var userPropertiesWrapper = GenericComObjectWrapper.Create (target.Inner.UserProperties))
        {
          CommonEntityMapper.MapCustomProperties2To1 (source.Properties, userPropertiesWrapper, _configuration.MapCustomProperties, _configuration.UserDefinedCustomPropertyMappings, logger, s_logger);
        }
      }

      if (_configuration.MapRecurringTasks)
        MapRecurrance2To1 (source, target, logger);

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
          return OlTaskStatus.olTaskNotStarted;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private void MapCategories2To1 (ITodo source, ITaskItemWrapper target)
    {
      var categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, source.Categories);

      if (_configuration.UseTaskCategoryAsFilter && !_configuration.InvertTaskCategoryFilter
          && source.Categories.All(a => a != _configuration.TaskCategory))
      {
        target.Inner.Categories = categories + CultureInfo.CurrentCulture.TextInfo.ListSeparator + _configuration.TaskCategory;
      }
      else
      {
        target.Inner.Categories = categories;
      }
    }

    private void MapReminder2To1 (ITodo source, ITaskItemWrapper target, IEntitySynchronizationLogger logger)
    {
      target.Inner.ReminderSet = false;

      if (_configuration.MapReminder == ReminderMapping.@false) return;

      if (source.Alarms.Count == 0) return;

      if (source.Alarms.Count > 1)
      {
        s_logger.WarnFormat ("Task '{0}' contains multiple alarms. Ignoring all except first.", source.UID);
      }
      var alarm = source.Alarms[0];
      var localZone = DateTimeZoneProviders.Bcl.GetSystemDefault();

      if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.Start && alarm.Trigger.Duration.HasValue && source.Start != null)
      {
        var reminderInstant = Instant.FromDateTimeUtc (source.Start.AsUtc().Add (alarm.Trigger.Duration.Value)).InZone (localZone);
        var reminderTime = reminderInstant.ToDateTimeUnspecified();

        if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
        target.Inner.ReminderSet = true;
        target.Inner.ReminderTime = reminderTime;
      }
      else if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.End && alarm.Trigger.Duration.HasValue && source.Due != null)
      {
        var reminderInstant = Instant.FromDateTimeUtc (source.Due.AsUtc().Add (alarm.Trigger.Duration.Value)).InZone (localZone);
        var reminderTime = reminderInstant.ToDateTimeUnspecified();

        if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
        target.Inner.ReminderSet = true;
        target.Inner.ReminderTime = reminderTime;
      }
      else if (alarm.Trigger.DateTime != null)
      {
        var reminderInstant = Instant.FromDateTimeUtc (alarm.Trigger.DateTime.AsUtc()).InZone (localZone);
        var reminderTime = reminderInstant.ToDateTimeUnspecified();
     
        if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
        target.Inner.ReminderSet = true;
        target.Inner.ReminderTime = reminderTime;
      }
      else
      {
        s_logger.WarnFormat ("Task '{0}' alarm is not supported. Ignoring.", source.UID);
        logger.LogWarning ("Task alarm is not supported. Ignoring.");
      }
    }

    private void MapRecurrance2To1 (ITodo source, ITaskItemWrapper targetWrapper, IEntitySynchronizationLogger logger)
    {
      if (source.RecurrenceRules.Count > 0)
      {
        using (var targetRecurrencePatternWrapper = GenericComObjectWrapper.Create (targetWrapper.Inner.GetRecurrencePattern()))
        {
          var targetRecurrencePattern = targetRecurrencePatternWrapper.Inner;
          if (source.RecurrenceRules.Count > 1)
          {
            s_logger.WarnFormat ("Task '{0}' contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.", source.UID);
            logger.LogWarning ("Task contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.");
          }
          var sourceRecurrencePattern = source.RecurrenceRules[0];

          switch (sourceRecurrencePattern.Frequency)
          {
            case FrequencyType.Daily:
              if (sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursDaily;
              }
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
                  s_logger.WarnFormat ("Task '{0}' contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.");
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
                  s_logger.WarnFormat ("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.");
                }
                try
                {
                  targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
                }
                catch (COMException ex)
                {
                  s_logger.Warn ($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                  logger.LogWarning ($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                }
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
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                }
                if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                {
                  s_logger.Warn ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                  logger.LogWarning ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                }
                else
                  targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                if (sourceRecurrencePattern.ByWeekNo.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.");
                }
                targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];

                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByMonthDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                }
                if (sourceRecurrencePattern.ByMonth[0] != targetRecurrencePattern.MonthOfYear)
                {
                  if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                  {
                    s_logger.Warn ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                    logger.LogWarning ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                  }
                  else
                    targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];
                }

                if (sourceRecurrencePattern.ByMonthDay.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.");
                }
                if (sourceRecurrencePattern.ByMonthDay[0] != targetRecurrencePattern.DayOfMonth)
                {
                  try
                  {
                    targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
                  }
                  catch (COMException ex)
                  {
                    s_logger.Warn ($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                    logger.LogWarning ($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                  }
                }
              }
              else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                  logger.LogWarning ("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                }
                if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                {
                  s_logger.Warn ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                  logger.LogWarning ($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                }
                else
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
              s_logger.WarnFormat ("Recurring task '{0}' contains the Frequency '{1}', which is not supported by outlook. Ignoring recurrence rule.", source.UID, sourceRecurrencePattern.Frequency);
              logger.LogWarning ($"Recurring task contains the Frequency '{sourceRecurrencePattern.Frequency}', which is not supported by outlook. Ignoring recurrence rule.");
              targetWrapper.Inner.ClearRecurrencePattern();
              break;
          }

          try
          {
            targetRecurrencePattern.Interval = (targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                                targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth) ? sourceRecurrencePattern.Interval * 12 : sourceRecurrencePattern.Interval;
          }
          catch (COMException ex)
          {
            s_logger.Warn (string.Format ("Recurring task '{0}' contains the Interval '{1}', which is not supported by outlook. Ignoring interval.", source.UID, sourceRecurrencePattern.Interval), ex);
            logger.LogWarning ($"Recurring task contains the Interval '{sourceRecurrencePattern.Interval}', which is not supported by outlook. Ignoring interval.", ex);
          }

          try
          {
            if (sourceRecurrencePattern.Count > 0)
            {
              targetRecurrencePattern.Occurrences = sourceRecurrencePattern.Count;
            }
            else if (sourceRecurrencePattern.Count == 0)
            {
              s_logger.Warn ($"Recurring task '{source.UID}' contains COUNT=0, which is invalid. Ignoring the occurence count.");
              logger.LogWarning ($"Recurring task '{source.UID}' contains COUNT=0, which is invalid. Ignoring the occurence count.");
            }

            if (sourceRecurrencePattern.Until != default (DateTime))
            {
              targetRecurrencePattern.PatternEndDate = sourceRecurrencePattern.Until.Date >= targetRecurrencePattern.PatternStartDate
                ? sourceRecurrencePattern.Until.Date
                : targetRecurrencePattern.PatternStartDate;
            }
          }
          catch (COMException ex)
          {
            s_logger.Warn ($"Recurring task '{source.UID}' contains occurence count or end date, which is not supported by outlook. Ignoring.", ex);
            logger.LogWarning ($"Recurring task contains occurence count or end date, which is not supported by outlook. Ignoring.", ex);
          }
        }

        targetWrapper.SaveAndReload();
      }
    }
  }
}