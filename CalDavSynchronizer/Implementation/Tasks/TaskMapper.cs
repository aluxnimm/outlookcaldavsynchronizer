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
        private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
        private static readonly ISet<string> s_internalOutlookProperties =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                RecurringTaskSeries.SeriesIdPropertyName
            };

        private readonly TimeZoneInfo _localTimeZoneInfo;
        private readonly TaskMappingConfiguration _configuration;

        public TaskMapper(string localTimeZoneId, TaskMappingConfiguration configuration)
        {
            _localTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);
            _configuration = configuration;
        }

        public Task<IICalendar> Map1To2(ITaskItemWrapper source, IICalendar existingTargetCalender, IEntitySynchronizationLogger logger, int context)
        {
            var localIcalTimeZone = iCalTimeZone.FromSystemTimeZone(_localTimeZoneInfo, new DateTime(1970, 1, 1), true);

            DDayICalWorkaround.CalendarDataPreprocessor.FixTimeZoneDSTRRules(_localTimeZoneInfo, localIcalTimeZone);

            // Outlook handles the final occurrence of a finite recurring task differently
            // from all earlier occurrences. PidLidTaskDeadOccurrence is TRUE for that
            // final item and there is no next live master to aggregate it with. Exporting
            // that item as a new master would overwrite the CalDAV series with a single
            // completed VTODO and can also lose the original COUNT/end condition. When
            // an existing recurring CalDAV master is available, keep that master and add
            // the final Outlook item as one more completed RECURRENCE-ID exception.
            if (TryMapFinalCompletedOccurrence1To2(
                    source,
                    existingTargetCalender,
                    localIcalTimeZone,
                    logger))
            {
                return Task.FromResult(existingTargetCalender);
            }

            var newTargetCalender = new iCalendar();
            if (!_configuration.MapStartAndDueAsFloating) newTargetCalender.TimeZones.Add(localIcalTimeZone);

            var existingTargetTodo = existingTargetCalender.Todos.FirstOrDefault(e => e.RecurrenceID == null);

            var newTargetTodo = new Todo();

            if (existingTargetTodo != null)
                newTargetTodo.UID = existingTargetTodo.UID;

            newTargetCalender.Todos.Add(newTargetTodo);

            Map1To2(source, newTargetTodo, localIcalTimeZone, logger);
            foreach (var recurrenceException in source.RecurrenceExceptions)
            {
                DateTime recurrenceIdDate;

                if (recurrenceException.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
                {
                    recurrenceIdDate = recurrenceException.StartDate.Date;
                }
                else if (recurrenceException.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
                {
                    recurrenceIdDate = recurrenceException.DueDate.Date;
                }
                else
                {
                    s_logger.Warn(
                        $"Recurring task exception '{recurrenceException.EntryID}' has neither StartDate nor DueDate. Ignoring exception.");

                    logger.LogWarning(
                        $"Recurring task exception has neither StartDate nor DueDate. Ignoring exception.");

                    continue;
                }

                var targetException = new Todo
                {
                    UID = newTargetTodo.UID
                };

                MapTaskItem1To2(
                    recurrenceException,
                    targetException,
                    localIcalTimeZone,
                    logger,
                    false);

                targetException.RecurrenceID =
                    new iCalDateTime(recurrenceIdDate)
                    {
                        HasTime = false
                    };

                newTargetCalender.Todos.Add(targetException);
            }

            for (int i = 0, newSequenceNumber = existingTargetCalender.Todos.Count > 0 ? existingTargetCalender.Todos.Max(e => e.Sequence) + 1 : 0;
                i < newTargetCalender.Todos.Count;
                i++, newSequenceNumber++)
            {
                newTargetCalender.Todos[i].Sequence = newSequenceNumber;
            }

            return Task.FromResult<IICalendar>(newTargetCalender);
        }

        private bool TryMapFinalCompletedOccurrence1To2(
            ITaskItemWrapper source,
            IICalendar existingTargetCalendar,
            iCalTimeZone localIcalTimeZone,
            IEntitySynchronizationLogger logger)
        {
            if (!_configuration.MapRecurringTasks ||
                existingTargetCalendar == null ||
                !source.Inner.IsRecurring ||
                !source.Inner.Complete ||
                !RecurringTaskSeries.IsDeadOccurrence(source.Inner))
            {
                return false;
            }

            var existingMaster =
                existingTargetCalendar.Todos.FirstOrDefault(todo =>
                    todo.RecurrenceID == null &&
                    todo.RecurrenceRules.Count > 0);

            if (existingMaster == null)
                return false;

            DateTime recurrenceDate;
            if (source.Inner.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                recurrenceDate = source.Inner.StartDate.Date;
            }
            else if (source.Inner.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                recurrenceDate = source.Inner.DueDate.Date;
            }
            else
            {
                s_logger.Warn(
                    "Final completed recurring Outlook task has neither StartDate nor DueDate; " +
                    "falling back to normal task mapping.");
                logger.LogWarning(
                    "Final completed recurring Outlook task has neither StartDate nor DueDate; " +
                    "falling back to normal task mapping.");
                return false;
            }

            var targetException =
                existingTargetCalendar.Todos.FirstOrDefault(todo =>
                    todo.RecurrenceID != null &&
                    todo.RecurrenceID.Date.Date == recurrenceDate);

            var isNewException = targetException == null;
            if (isNewException)
            {
                targetException = new Todo
                {
                    UID = existingMaster.UID
                };

                existingTargetCalendar.Todos.Add(targetException);
            }

            MapTaskItem1To2(
                source.Inner,
                targetException,
                localIcalTimeZone,
                logger,
                false);

            // Never let properties of Outlook's terminal item turn the exception into
            // another series. The existing CalDAV master remains the authoritative
            // recurrence definition and retains its original DTSTART/RRULE/COUNT.
            targetException.UID = existingMaster.UID;
            targetException.RecurrenceRules.Clear();
            targetException.RecurrenceID =
                new iCalDateTime(recurrenceDate)
                {
                    HasTime = false
                };

            var nextSequence =
                existingTargetCalendar.Todos
                    .Where(todo => !ReferenceEquals(todo, targetException))
                    .Select(todo => todo.Sequence)
                    .DefaultIfEmpty(-1)
                    .Max() + 1;

            targetException.Sequence = nextSequence;

            s_logger.InfoFormat(
                "Mapped final completed Outlook recurrence {0:d} as RECURRENCE-ID on existing CalDAV series '{1}'.",
                recurrenceDate,
                existingMaster.UID);

            return true;
        }

        public void Map1To2(
            ITaskItemWrapper source,
            ITodo target,
            iCalTimeZone localIcalTimeZone,
            IEntitySynchronizationLogger logger)
        {
            MapTaskItem1To2(
                source.Inner,
                target,
                localIcalTimeZone,
                logger,
                true);
        }

        private void MapTaskItem1To2(
            TaskItem source,
            ITodo target,
            iCalTimeZone localIcalTimeZone,
            IEntitySynchronizationLogger logger,
            bool mapRecurrence)
        {
            target.Summary =
                CalendarDataPreprocessor.EncodeString(source.Subject);

            if (_configuration.MapBody)
                target.Description =
                    CalendarDataPreprocessor.EncodeString(source.Body);

            if (source.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                target.Start = new iCalDateTime(
                    source.StartDate.Year,
                    source.StartDate.Month,
                    source.StartDate.Day,
                    true);

                if (!_configuration.MapStartAndDueAsFloating)
                    target.Start.SetTimeZone(localIcalTimeZone);
            }

            if (source.Complete &&
                source.DateCompleted != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                target.Completed =
                    new iCalDateTime(source.DateCompleted.ToUniversalTime())
                    {
                        IsUniversalTime = true,
                        HasTime = true
                    };
            }

            target.PercentComplete = source.PercentComplete;

            if (_configuration.MapRecurringTasks && mapRecurrence)
                MapRecurrance1To2(
                    source,
                    target,
                    localIcalTimeZone);

            if (source.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                target.Due = new iCalDateTime(
                    source.DueDate.Year,
                    source.DueDate.Month,
                    source.DueDate.Day,
                    23,
                    59,
                    59);

                if (!_configuration.MapStartAndDueAsFloating)
                    target.Due.SetTimeZone(localIcalTimeZone);

                target.Properties.Remove(
                    new CalendarProperty("DURATION"));
            }

            // Outlook exposes a recurring task at its current outstanding occurrence.
            // After SkipRecurrence(), StartDate/DueDate therefore move forward while
            // RecurrencePattern.PatternStartDate keeps the original series anchor.
            // iCalendar RRULE COUNT is evaluated from DTSTART, so exporting the moving
            // Outlook StartDate would silently extend a finite CalDAV series on every
            // roundtrip. Keep DTSTART at the recurrence pattern's original start and
            // preserve the task's start-to-due offset.
            if (_configuration.MapRecurringTasks &&
                mapRecurrence &&
                source.IsRecurring &&
                source.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
            {
                MapRecurringSeriesAnchor1To2(
                    source,
                    target,
                    localIcalTimeZone);
            }

            target.Properties.Set(
                "STATUS",
                MapStatus1To2(source.Status));

            if (_configuration.MapPriority)
                target.Priority =
                    CommonEntityMapper.MapPriority1To2(
                        source.Importance);

            target.Class =
                CommonEntityMapper.MapPrivacy1To2(
                    source.Sensitivity,
                    false,
                    false);

            MapReminder1To2(source, target);
            MapCategories1To2(source, target);

            if (_configuration.MapCustomProperties ||
                _configuration.UserDefinedCustomPropertyMappings.Length > 0)
            {
                using (var userPropertiesWrapper =
                       GenericComObjectWrapper.Create(
                           source.UserProperties))
                {
                    CommonEntityMapper.MapCustomProperties1To2(
                        userPropertiesWrapper,
                        target.Properties,
                        _configuration.MapCustomProperties,
                        _configuration.UserDefinedCustomPropertyMappings,
                        logger,
                        s_logger,
                        s_internalOutlookProperties);
                }
            }
        }

        private string MapStatus1To2(OlTaskStatus value)
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

            throw new NotImplementedException(string.Format("Mapping for value '{0}' not implemented.", value));
        }

        private void MapCategories1To2(
    TaskItem source,
    ITodo target)
        {
            if (!string.IsNullOrEmpty(source.Categories))
            {
                var useTaskCategoryAsFilter = _configuration.UseTaskCategoryAsFilter;

                var sourceCategories = CommonEntityMapper.SplitCategoryString(source.Categories)
                    .Where(c => !useTaskCategoryAsFilter || c != _configuration.TaskCategory);

                foreach (var sourceCategory in sourceCategories)
                {
                    target.Categories.Add(sourceCategory);
                }
            }
        }

        private void MapReminder1To2(
    TaskItem source,
    ITodo target)
        {
            if (_configuration.MapReminder == ReminderMapping.@false)
                return;

            if (source.ReminderSet)
            {
                if (_configuration.MapReminder == ReminderMapping.JustUpcoming
                    && source.ReminderTime <= DateTime.Now)
                    return;

                var trigger = new Trigger();

                if (!_configuration.MapReminderAsDateTime && source.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
                {
                    trigger.Duration = source.ReminderTime - source.StartDate;
                    trigger.Parameters.Add("RELATED", "START");
                    trigger.Parameters.Add("VALUE", "DURATION");

                    target.Alarms.Add(
                        new Alarm()
                        {
                            Description = "This is a task reminder"
                        }
                    );
                    // Fix DDay.iCal TimeSpan 0 serialization
                    if (trigger.Duration.Value == TimeSpan.Zero)
                    {
                        target.Alarms[0].Properties.Add(new CalendarProperty("TRIGGER", "-P0D"));
                    }
                    else
                    {
                        target.Alarms[0].Trigger = trigger;
                    }

                    var actionProperty = new CalendarProperty("ACTION", "DISPLAY");
                    target.Alarms[0].Properties.Add(actionProperty);
                }
                else if (!_configuration.MapReminderAsDateTime && source.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
                {
                    var dueTime = new DateTime(source.DueDate.Year, source.DueDate.Month, source.DueDate.Day, 23, 59, 59);
                    trigger.Duration = source.ReminderTime - dueTime;
                    trigger.Parameters.Add("RELATED", "END");
                    trigger.Parameters.Add("VALUE", "DURATION");

                    target.Alarms.Add(
                        new Alarm()
                        {
                            Description = "This is a task reminder"
                        }
                    );
                    // Fix DDay.iCal TimeSpan 0 serialization
                    if (trigger.Duration.Value == TimeSpan.Zero)
                    {
                        target.Alarms[0].Properties.Add(new CalendarProperty("TRIGGER", "P0D"));
                    }
                    else
                    {
                        target.Alarms[0].Trigger = trigger;
                    }

                    var actionProperty = new CalendarProperty("ACTION", "DISPLAY");
                    target.Alarms[0].Properties.Add(actionProperty);
                }
                else
                {
                    trigger.DateTime = new iCalDateTime(source.ReminderTime.ToUniversalTime()) {IsUniversalTime = true, HasTime = true};
                    trigger.Parameters.Add("VALUE", "DATE-TIME");
                    target.Alarms.Add(
                        new Alarm()
                        {
                            Description = "This is a task reminder"
                        }
                    );
                    target.Alarms[0].Trigger = trigger;
                    var actionProperty = new CalendarProperty("ACTION", "DISPLAY");
                    target.Alarms[0].Properties.Add(actionProperty);
                }
            }
        }

        private void MapRecurringSeriesAnchor1To2(
            TaskItem source,
            ITodo target,
            iCalTimeZone localIcalTimeZone)
        {
            using (var recurrencePatternWrapper =
                   GenericComObjectWrapper.Create(source.GetRecurrencePattern()))
            {
                var patternStart =
                    recurrencePatternWrapper.Inner.PatternStartDate.Date;
                var currentStart = source.StartDate.Date;

                target.Start = new iCalDateTime(
                    patternStart.Year,
                    patternStart.Month,
                    patternStart.Day,
                    true);

                if (!_configuration.MapStartAndDueAsFloating)
                    target.Start.SetTimeZone(localIcalTimeZone);

                if (source.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
                {
                    var dueOffset =
                        source.DueDate.Date - currentStart;
                    var anchoredDue =
                        patternStart.Add(dueOffset);

                    target.Due = new iCalDateTime(
                        anchoredDue.Year,
                        anchoredDue.Month,
                        anchoredDue.Day,
                        23,
                        59,
                        59);

                    if (!_configuration.MapStartAndDueAsFloating)
                        target.Due.SetTimeZone(localIcalTimeZone);

                    target.Properties.Remove(
                        new CalendarProperty("DURATION"));
                }
            }
        }

        private void MapRecurrance1To2(TaskItem source, ITodo target, iCalTimeZone localIcalTimeZone)
        {
            if (source.IsRecurring)
            {
                using (var sourceRecurrencePatternWrapper = GenericComObjectWrapper.Create(source.GetRecurrencePattern()))
                {
                    var sourceRecurrencePattern = sourceRecurrencePatternWrapper.Inner;

                    // Recurring task must have a DTSTART according to the RFC but Outlook may have no task start date set, use PatternStartDate in this case
                    if (source.StartDate == OutlookUtility.OUTLOOK_DATE_NONE)
                    {
                        target.Start = new iCalDateTime(sourceRecurrencePattern.PatternStartDate.Year,
                            sourceRecurrencePattern.PatternStartDate.Month, sourceRecurrencePattern.PatternStartDate.Day, true);
                        if (!_configuration.MapStartAndDueAsFloating) target.Start.SetTimeZone(localIcalTimeZone);
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
                                                        sourceRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth)
                        ? sourceRecurrencePattern.Interval / 12
                        : sourceRecurrencePattern.Interval;

                    switch (sourceRecurrencePattern.RecurrenceType)
                    {
                        case OlRecurrenceType.olRecursDaily:
                            targetRecurrencePattern.Frequency = FrequencyType.Daily;
                            break;
                        case OlRecurrenceType.olRecursWeekly:
                            targetRecurrencePattern.Frequency = FrequencyType.Weekly;
                            CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            break;
                        case OlRecurrenceType.olRecursMonthly:
                            targetRecurrencePattern.Frequency = FrequencyType.Monthly;
                            targetRecurrencePattern.ByMonthDay.Add(sourceRecurrencePattern.DayOfMonth);
                            break;
                        case OlRecurrenceType.olRecursMonthNth:
                            targetRecurrencePattern.Frequency = FrequencyType.Monthly;

                            if (sourceRecurrencePattern.Instance == 5)
                            {
                                targetRecurrencePattern.BySetPosition.Add(-1);
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }
                            else if (sourceRecurrencePattern.Instance > 0)
                            {
                                targetRecurrencePattern.BySetPosition.Add(sourceRecurrencePattern.Instance);
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }
                            else
                            {
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }

                            break;
                        case OlRecurrenceType.olRecursYearly:
                            targetRecurrencePattern.Frequency = FrequencyType.Yearly;
                            targetRecurrencePattern.ByMonthDay.Add(sourceRecurrencePattern.DayOfMonth);
                            targetRecurrencePattern.ByMonth.Add(sourceRecurrencePattern.MonthOfYear);
                            break;
                        case OlRecurrenceType.olRecursYearNth:
                            targetRecurrencePattern.Frequency = FrequencyType.Yearly;
                            if (sourceRecurrencePattern.Instance == 5)
                            {
                                targetRecurrencePattern.BySetPosition.Add(-1);
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }
                            else if (sourceRecurrencePattern.Instance > 0)
                            {
                                targetRecurrencePattern.BySetPosition.Add(sourceRecurrencePattern.Instance);
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }
                            else
                            {
                                CommonEntityMapper.MapDayOfWeek1To2(sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
                            }

                            targetRecurrencePattern.ByMonth.Add(sourceRecurrencePattern.MonthOfYear);
                            break;
                    }

                    target.RecurrenceRules.Add(targetRecurrencePattern);
                }
            }
        }

        public Task<ITaskItemWrapper> Map2To1(IICalendar sourceCalendar, ITaskItemWrapper target, IEntitySynchronizationLogger logger, int context)
        {
            if (sourceCalendar == null)
                throw new ArgumentNullException(nameof(sourceCalendar));
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            var master = sourceCalendar.Todos.FirstOrDefault(todo => todo.RecurrenceID == null);
            if (master == null)
            {
                master = sourceCalendar.Todos.FirstOrDefault();
                if (master == null)
                    return Task.FromResult(target);

                s_logger.WarnFormat(
                    "Task calendar contains no master VTODO. Using first VTODO '{0}' as master.",
                    master.UID);
                logger.LogWarning("Task calendar contains no master VTODO. Using first VTODO as master.");
            }

            Map2To1(master, target, logger);

            if (_configuration.MapRecurringTasks && master.RecurrenceRules.Count > 0)
            {
                ReconcileRecurrenceExceptions2To1(sourceCalendar, target, logger);
            }

            return Task.FromResult(target);
        }

        private void ReconcileRecurrenceExceptions2To1(
            IICalendar sourceCalendar,
            ITaskItemWrapper target,
            IEntitySynchronizationLogger logger)
        {
            var completedSourceExceptions =
                sourceCalendar.Todos
                    .Where(todo =>
                        todo.RecurrenceID != null &&
                        (todo.Status == TodoStatus.Completed ||
                         todo.PercentComplete == 100 ||
                         todo.Completed != null))
                    .GroupBy(todo => todo.RecurrenceID.Date)
                    .ToDictionary(group => group.Key, group => group.OrderByDescending(todo => todo.Sequence).First());

            var unsupportedExceptions =
                sourceCalendar.Todos
                    .Where(todo =>
                        todo.RecurrenceID != null &&
                        todo.Status != TodoStatus.Completed &&
                        todo.PercentComplete != 100 &&
                        todo.Completed == null)
                    .ToList();

            foreach (var unsupported in unsupportedExceptions)
            {
                s_logger.WarnFormat(
                    "Recurring task '{0}' contains a non-completed RECURRENCE-ID exception '{1}'. " +
                    "Outlook recurring tasks cannot represent arbitrary non-completed exceptions; ignoring it.",
                    unsupported.UID,
                    unsupported.RecurrenceID);
                logger.LogWarning(
                    "Recurring task contains a non-completed RECURRENCE-ID exception. " +
                    "Outlook recurring tasks cannot represent arbitrary non-completed exceptions; ignoring it.");
            }

            // Every recurring Outlook master needs a persistent identifier before copies
            // are created. Outlook copies user properties to completed occurrences and
            // we also set it explicitly below for robustness.
            var seriesId = RecurringTaskSeries.EnsureSeriesId(target.Inner);

            var existingByDate =
                target.RecurrenceExceptions
                    .Select(task => new
                    {
                        Task = task,
                        Date = GetOutlookRecurrenceExceptionDate(task)
                    })
                    .Where(item => item.Date.HasValue)
                    .GroupBy(item => item.Date.Value)
                    .ToDictionary(group => group.Key, group => group.Select(item => item.Task).ToList());

            var matchedExisting = new HashSet<TaskItem>();

            foreach (var sourceExceptionPair in completedSourceExceptions)
            {
                var recurrenceDate = sourceExceptionPair.Key;
                var sourceException = sourceExceptionPair.Value;

                TaskItem existingException = null;
                List<TaskItem> existingForDate;
                if (existingByDate.TryGetValue(recurrenceDate, out existingForDate))
                {
                    existingException = existingForDate.FirstOrDefault();
                    if (existingForDate.Count > 1)
                    {
                        s_logger.WarnFormat(
                            "Recurring Outlook task has {0} completed copies for occurrence {1:d}. " +
                            "Keeping the first and deleting duplicates.",
                            existingForDate.Count,
                            recurrenceDate);
                    }
                }

                if (existingException != null)
                {
                    matchedExisting.Add(existingException);
                    MapRecurrenceException2To1(sourceException, existingException, seriesId, logger);
                }
                else
                {
                    CreateRecurrenceException2To1(sourceException, target.Inner, seriesId, logger);
                }
            }

            // Remove Outlook completed copies which are no longer represented by a
            // RECURRENCE-ID component on the server. This also removes duplicate copies.
            foreach (var existingException in target.RecurrenceExceptions)
            {
                if (matchedExisting.Contains(existingException))
                    continue;

                var existingDate = GetOutlookRecurrenceExceptionDate(existingException);
                if (existingDate.HasValue && completedSourceExceptions.ContainsKey(existingDate.Value))
                {
                    // A duplicate for a date for which another copy was kept.
                    existingException.Delete();
                    continue;
                }

                existingException.Delete();
            }

            // CalDAV keeps DTSTART/DUE on the recurring master at the original series
            // start. Outlook, on the other hand, exposes a recurring task at its
            // current outstanding occurrence. Advance the Outlook master over all
            // completed leading occurrences represented by RECURRENCE-ID components.
            AdvanceMasterPastCompletedOccurrences(
                target.Inner,
                completedSourceExceptions.Keys,
                logger);
        }

        private void AdvanceMasterPastCompletedOccurrences(
            TaskItem master,
            IEnumerable<DateTime> completedRecurrenceDates,
            IEntitySynchronizationLogger logger)
        {
            var completedDates =
                new HashSet<DateTime>(
                    completedRecurrenceDates.Select(date => date.Date));

            if (completedDates.Count == 0 || !master.IsRecurring)
                return;

            // At most one SkipRecurrence call per completed exception. Besides being
            // a natural upper bound, this also protects us from a COM/Outlook state
            // which for some reason does not advance the current occurrence.
            var maximumAdvances = completedDates.Count;
            var advances = 0;

            while (advances < maximumAdvances)
            {
                var currentDate = GetOutlookRecurrenceExceptionDate(master);
                if (!currentDate.HasValue ||
                    !completedDates.Contains(currentDate.Value.Date))
                {
                    break;
                }

                var previousDate = currentDate.Value.Date;
                bool advanced;

                try
                {
                    advanced = master.SkipRecurrence();
                }
                catch (COMException ex)
                {
                    s_logger.Warn(
                        $"Could not advance recurring Outlook task past completed occurrence {previousDate:d}.",
                        ex);
                    logger.LogWarning(
                        $"Could not advance recurring Outlook task past completed occurrence {previousDate:d}.",
                        ex);
                    break;
                }

                if (!advanced)
                {
                    // Outlook reports false when the current item is the last
                    // occurrence in the recurrence. There is no next occurrence to
                    // expose as the master in that case.
                    s_logger.InfoFormat(
                        "Recurring Outlook task reached its final occurrence while advancing past completed occurrence {0:d}.",
                        previousDate);
                    break;
                }

                advances++;

                var nextDate = GetOutlookRecurrenceExceptionDate(master);
                if (nextDate.HasValue && nextDate.Value.Date == previousDate)
                {
                    s_logger.WarnFormat(
                        "SkipRecurrence did not advance recurring Outlook task from occurrence {0:d}; stopping to avoid a loop.",
                        previousDate);
                    logger.LogWarning(
                        "SkipRecurrence did not advance recurring Outlook task; stopping to avoid a loop.");
                    break;
                }
            }

            if (advances > 0)
                master.Save();
        }

        private void CreateRecurrenceException2To1(
            ITodo sourceException,
            TaskItem master,
            string seriesId,
            IEntitySynchronizationLogger logger)
        {
            TaskItem copy = null;

            try
            {
                copy = (TaskItem) master.Copy();

                // Mark the copy as a dead occurrence before setting completion fields.
                // This mirrors Outlook's own model for completed recurring tasks and
                // prevents the copy from generating another occurrence.
                RecurringTaskSeries.SetSeriesId(copy, seriesId);
                RecurringTaskSeries.SetDeadOccurrence(copy, true);

                using (var borrowedWrapper = new BorrowedTaskItemWrapper(copy))
                {
                    Map2To1(sourceException, borrowedWrapper, logger);
                }

                copy.Save();
            }
            finally
            {
                if (copy != null && Marshal.IsComObject(copy))
                    Marshal.FinalReleaseComObject(copy);
            }
        }

        private void MapRecurrenceException2To1(
            ITodo sourceException,
            TaskItem targetException,
            string seriesId,
            IEntitySynchronizationLogger logger)
        {
            RecurringTaskSeries.SetSeriesId(targetException, seriesId);
            RecurringTaskSeries.SetDeadOccurrence(targetException, true);

            using (var borrowedWrapper = new BorrowedTaskItemWrapper(targetException))
            {
                Map2To1(sourceException, borrowedWrapper, logger);
            }

            targetException.Save();
        }

        private static DateTime? GetOutlookRecurrenceExceptionDate(TaskItem task)
        {
            if (task.StartDate != OutlookUtility.OUTLOOK_DATE_NONE)
                return task.StartDate.Date;

            if (task.DueDate != OutlookUtility.OUTLOOK_DATE_NONE)
                return task.DueDate.Date;

            return null;
        }

        private sealed class BorrowedTaskItemWrapper : ITaskItemWrapper
        {
            private TaskItem _inner;
            private static readonly IReadOnlyList<TaskItem> s_emptyRecurrenceExceptions =
                new TaskItem[0];

            public BorrowedTaskItemWrapper(TaskItem inner)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public TaskItem Inner => _inner;

            public IReadOnlyList<TaskItem> RecurrenceExceptions => s_emptyRecurrenceExceptions;

            public void AddRecurrenceException(TaskItem recurrenceException)
            {
                throw new NotSupportedException();
            }

            public void SaveAndReload()
            {
                _inner.Save();
            }

            public void Replace(TaskItem inner)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            public void Dispose()
            {
                // The caller owns the COM object.
            }
        }

        public ITaskItemWrapper Map2To1(ITodo source, ITaskItemWrapper target, IEntitySynchronizationLogger logger)
        {
            target.Inner.Subject = source.Summary;

            target.Inner.Body = _configuration.MapBody ? source.Description : string.Empty;

            DateTimeZone localZone = DateTimeZoneProviders.Bcl.GetSystemDefault();

            if (source.Start != null)
            {
                if (source.Start.IsUniversalTime)
                {
                    target.Inner.StartDate = Instant.FromDateTimeUtc(source.Start.Value).InZone(localZone).ToDateTimeUnspecified().Date;
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
                        target.Inner.DueDate = Instant.FromDateTimeUtc(source.Due.Value).InZone(localZone).ToDateTimeUnspecified().Date;
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
                    target.Inner.DateCompleted = Instant.FromDateTimeUtc(source.Completed.Value).InZone(localZone).ToDateTimeUnspecified().Date;
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

            target.Inner.Status = (target.Inner.Complete && target.Inner.PercentComplete == 100) ? OlTaskStatus.olTaskComplete : MapStatus2To1(source.Status);

            // Only set PercentComplete if source is actually set and status is not already completed to avoid overwriting the status again
            if (source.PercentComplete != 0 && target.Inner.Status != OlTaskStatus.olTaskComplete)
            {
                target.Inner.PercentComplete = source.PercentComplete;
            }

            if (_configuration.MapPriority)
                target.Inner.Importance = CommonEntityMapper.MapPriority2To1(source.Priority);

            target.Inner.Sensitivity = CommonEntityMapper.MapPrivacy2To1(source.Class, false, false);

            MapCategories2To1(source, target);

            MapReminder2To1(source, target, logger);

            if (_configuration.MapCustomProperties || _configuration.UserDefinedCustomPropertyMappings.Length > 0)
            {
                using (var userPropertiesWrapper = GenericComObjectWrapper.Create(target.Inner.UserProperties))
                {
                    CommonEntityMapper.MapCustomProperties2To1(
                        source.Properties,
                        userPropertiesWrapper,
                        _configuration.MapCustomProperties,
                        _configuration.UserDefinedCustomPropertyMappings,
                        logger,
                        s_logger,
                        s_internalOutlookProperties);
                }
            }

            if (_configuration.MapRecurringTasks)
                MapRecurrance2To1(source, target, logger);

            return target;
        }

        private OlTaskStatus MapStatus2To1(TodoStatus value)
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

            throw new NotImplementedException(string.Format("Mapping for value '{0}' not implemented.", value));
        }

        private void MapCategories2To1(ITodo source, ITaskItemWrapper target)
        {
            var categories = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, source.Categories);

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

        private void MapReminder2To1(ITodo source, ITaskItemWrapper target, IEntitySynchronizationLogger logger)
        {
            target.Inner.ReminderSet = false;

            if (_configuration.MapReminder == ReminderMapping.@false) return;

            if (source.Alarms.Count == 0) return;

            if (source.Alarms.Count > 1)
            {
                s_logger.WarnFormat("Task '{0}' contains multiple alarms. Ignoring all except first.", source.UID);
            }

            var alarm = source.Alarms[0];
            var localZone = DateTimeZoneProviders.Bcl.GetSystemDefault();

            if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.Start && alarm.Trigger.Duration.HasValue && source.Start != null)
            {
                var reminderInstant = Instant.FromDateTimeUtc(source.Start.AsUtc().Add(alarm.Trigger.Duration.Value)).InZone(localZone);
                var reminderTime = reminderInstant.ToDateTimeUnspecified();

                if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
                target.Inner.ReminderSet = true;
                target.Inner.ReminderTime = reminderTime;
            }
            else if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.End && alarm.Trigger.Duration.HasValue && source.Due != null)
            {
                var reminderInstant = Instant.FromDateTimeUtc(source.Due.AsUtc().Add(alarm.Trigger.Duration.Value)).InZone(localZone);
                var reminderTime = reminderInstant.ToDateTimeUnspecified();

                if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
                target.Inner.ReminderSet = true;
                target.Inner.ReminderTime = reminderTime;
            }
            else if (alarm.Trigger.DateTime != null)
            {
                var reminderInstant = Instant.FromDateTimeUtc(alarm.Trigger.DateTime.AsUtc()).InZone(localZone);
                var reminderTime = reminderInstant.ToDateTimeUnspecified();

                if (_configuration.MapReminder == ReminderMapping.JustUpcoming && reminderTime < DateTime.Now) return;
                target.Inner.ReminderSet = true;
                target.Inner.ReminderTime = reminderTime;
            }
            else
            {
                s_logger.WarnFormat("Task '{0}' alarm is not supported. Ignoring.", source.UID);
                logger.LogWarning("Task alarm is not supported. Ignoring.");
            }
        }

        private void MapRecurrance2To1(ITodo source, ITaskItemWrapper targetWrapper, IEntitySynchronizationLogger logger)
        {
            if (source.RecurrenceRules.Count > 0)
            {
                using (var targetRecurrencePatternWrapper = GenericComObjectWrapper.Create(targetWrapper.Inner.GetRecurrencePattern()))
                {
                    var targetRecurrencePattern = targetRecurrencePatternWrapper.Inner;
                    if (source.RecurrenceRules.Count > 1)
                    {
                        s_logger.WarnFormat("Task '{0}' contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.", source.UID);
                        logger.LogWarning("Task contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.");
                    }

                    var sourceRecurrencePattern = source.RecurrenceRules[0];

                    switch (sourceRecurrencePattern.Frequency)
                    {
                        case FrequencyType.Daily:
                            if (sourceRecurrencePattern.ByDay.Count > 0)
                            {
                                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
                                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1(sourceRecurrencePattern.ByDay);
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
                                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1(sourceRecurrencePattern.ByDay);
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
                                    s_logger.WarnFormat("Task '{0}' contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.");
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

                                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1(sourceRecurrencePattern.ByDay);
                            }
                            else if (sourceRecurrencePattern.ByMonthDay.Count > 0)
                            {
                                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
                                if (sourceRecurrencePattern.ByMonthDay.Count > 1)
                                {
                                    s_logger.WarnFormat("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.");
                                }

                                try
                                {
                                    targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
                                }
                                catch (COMException ex)
                                {
                                    s_logger.Warn($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                                    logger.LogWarning($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
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
                                    s_logger.WarnFormat("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                                }

                                if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                                {
                                    s_logger.Warn($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                    logger.LogWarning($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                }
                                else
                                    targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                                if (sourceRecurrencePattern.ByWeekNo.Count > 1)
                                {
                                    s_logger.WarnFormat("Task '{0}' contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.");
                                }

                                targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];

                                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1(sourceRecurrencePattern.ByDay);
                            }
                            else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByMonthDay.Count > 0)
                            {
                                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
                                if (sourceRecurrencePattern.ByMonth.Count > 1)
                                {
                                    s_logger.WarnFormat("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                                }

                                if (sourceRecurrencePattern.ByMonth[0] != targetRecurrencePattern.MonthOfYear)
                                {
                                    if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                                    {
                                        s_logger.Warn($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                        logger.LogWarning($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                    }
                                    else
                                        targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];
                                }

                                if (sourceRecurrencePattern.ByMonthDay.Count > 1)
                                {
                                    s_logger.WarnFormat("Task '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.");
                                }

                                if (sourceRecurrencePattern.ByMonthDay[0] != targetRecurrencePattern.DayOfMonth)
                                {
                                    try
                                    {
                                        targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
                                    }
                                    catch (COMException ex)
                                    {
                                        s_logger.Warn($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                                        logger.LogWarning($"Recurring task '{source.UID}' contains invalid BYMONTHDAY '{sourceRecurrencePattern.ByMonthDay[0]}', which will be ignored.", ex);
                                    }
                                }
                            }
                            else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByDay.Count > 0)
                            {
                                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
                                if (sourceRecurrencePattern.ByMonth.Count > 1)
                                {
                                    s_logger.WarnFormat("Task '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.UID);
                                    logger.LogWarning("Task contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.");
                                }

                                if (sourceRecurrencePattern.ByMonth[0] < 1 || sourceRecurrencePattern.ByMonth[0] > 12)
                                {
                                    s_logger.Warn($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                    logger.LogWarning($"Recurring task '{source.UID}' contains invalid BYMONTH '{sourceRecurrencePattern.ByMonth[0]}', which will be ignored.");
                                }
                                else
                                    targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                                targetRecurrencePattern.Instance = (sourceRecurrencePattern.ByDay[0].Offset >= 0) ? sourceRecurrencePattern.ByDay[0].Offset : 5;
                                if (sourceRecurrencePattern.BySetPosition.Count > 0)
                                {
                                    targetRecurrencePattern.Instance = (sourceRecurrencePattern.BySetPosition[0] >= 0) ? sourceRecurrencePattern.BySetPosition[0] : 5;
                                }

                                targetRecurrencePattern.DayOfWeekMask = CommonEntityMapper.MapDayOfWeek2To1(sourceRecurrencePattern.ByDay);
                            }
                            else
                            {
                                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
                            }

                            break;
                        default:
                            s_logger.WarnFormat("Recurring task '{0}' contains the Frequency '{1}', which is not supported by outlook. Ignoring recurrence rule.", source.UID, sourceRecurrencePattern.Frequency);
                            logger.LogWarning($"Recurring task contains the Frequency '{sourceRecurrencePattern.Frequency}', which is not supported by outlook. Ignoring recurrence rule.");
                            targetWrapper.Inner.ClearRecurrencePattern();
                            break;
                    }

                    try
                    {
                        targetRecurrencePattern.Interval = (targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                                            targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth)
                            ? sourceRecurrencePattern.Interval * 12
                            : sourceRecurrencePattern.Interval;
                    }
                    catch (COMException ex)
                    {
                        s_logger.Warn(string.Format("Recurring task '{0}' contains the Interval '{1}', which is not supported by outlook. Ignoring interval.", source.UID, sourceRecurrencePattern.Interval), ex);
                        logger.LogWarning($"Recurring task contains the Interval '{sourceRecurrencePattern.Interval}', which is not supported by outlook. Ignoring interval.", ex);
                    }

                    try
                    {
                        if (sourceRecurrencePattern.Count > 0)
                        {
                            targetRecurrencePattern.Occurrences = sourceRecurrencePattern.Count;
                        }
                        else if (sourceRecurrencePattern.Count == 0)
                        {
                            s_logger.Warn($"Recurring task '{source.UID}' contains COUNT=0, which is invalid. Ignoring the occurence count.");
                            logger.LogWarning($"Recurring task '{source.UID}' contains COUNT=0, which is invalid. Ignoring the occurence count.");
                        }

                        if (sourceRecurrencePattern.Until != default(DateTime))
                        {
                            targetRecurrencePattern.PatternEndDate = sourceRecurrencePattern.Until.Date >= targetRecurrencePattern.PatternStartDate
                                ? sourceRecurrencePattern.Until.Date
                                : targetRecurrencePattern.PatternStartDate;
                        }
                        else if (sourceRecurrencePattern.Count <= 0)
                        {
                            // set explicitly NoEndDate only if there is no positive COUNT and no UNTIL set
                            targetRecurrencePattern.NoEndDate = true;
                        }
                    }
                    catch (COMException ex)
                    {
                        s_logger.Warn($"Recurring task '{source.UID}' contains occurence count or end date, which is not supported by outlook. Ignoring.", ex);
                        logger.LogWarning($"Recurring task contains occurence count or end date, which is not supported by outlook. Ignoring.", ex);
                    }
                }

                targetWrapper.SaveAndReload();
            }
        }
    }
}