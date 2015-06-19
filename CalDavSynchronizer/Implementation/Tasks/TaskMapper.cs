using System;
using System.Globalization;
using System.Reflection;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Implementation.ComWrappers;
using Microsoft.Office.Interop.Outlook;
using DDay.iCal;
using log4net;

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class TaskMapper : IEntityMapper<TaskItemWrapper, IICalendar>
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly DateTime _dateNull;
    private readonly TimeZoneInfo _localTimeZoneInfo;

    public TaskMapper (string localTimeZoneId)
    {
      _dateNull = new DateTime(4501, 1, 1, 0, 0, 0);
      _localTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (localTimeZoneId);
    }
    public IICalendar Map1To2 (TaskItemWrapper source, IICalendar targetCalender)
    {
      ITodo target = new Todo();
      targetCalender.Todos.Add (target);
      Map1To2 (source, target);
      return targetCalender;
    }

    public void Map1To2 (TaskItemWrapper source, ITodo target)
    {
      target.Summary = source.Inner.Subject;
      target.Description = source.Inner.Body;

      if (source.Inner.StartDate != _dateNull)
      {
        target.Start = new iCalDateTime (source.Inner.StartDate.Year, source.Inner.StartDate.Month, source.Inner.StartDate.Day, false);
      }

      if (source.Inner.DueDate != _dateNull)
      {
        if (source.Inner.DueDate == source.Inner.StartDate)
        {
          target.Duration = default(TimeSpan);
        }
        else
        {
          target.Due = new iCalDateTime (source.Inner.DueDate.Year, source.Inner.DueDate.Month, source.Inner.DueDate.Day, false);
        }
      }

      if (source.Inner.Complete && source.Inner.DateCompleted != _dateNull)
      {
        target.Completed = new iCalDateTime (source.Inner.DateCompleted.Year, source.Inner.DateCompleted.Month, source.Inner.DateCompleted.Day, false);
      }

      target.PercentComplete = source.Inner.PercentComplete;

      target.Properties.Set ("STATUS", MapStatus1To2 (source.Inner.Status));

      target.Priority = MapPriority1To2 (source.Inner.Importance);

      target.Class = MapPrivacy1To2 (source.Inner.Sensitivity);

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

      throw new NotImplementedException (string.Format("Mapping for value '{0}' not implemented.", value));
    }

    private int MapPriority1To2 (OlImportance value)
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

      throw new NotImplementedException (string.Format("Mapping for value '{0}' not implemented.", value));
    }

    private string MapPrivacy1To2 (OlSensitivity value)
    {
      switch (value)
      {
        case OlSensitivity.olNormal:
          return "PUBLIC";
        case OlSensitivity.olPersonal:
          return "PRIVATE"; // not sure
        case OlSensitivity.olPrivate:
          return "PRIVATE";
        case OlSensitivity.olConfidential:
          return "CONFIDENTIAL";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private static void MapCategories1To2 (TaskItemWrapper source, ITodo target)
    {
      if (!string.IsNullOrEmpty(source.Inner.Categories))
      {
        Array.ForEach(
            source.Inner.Categories.Split (new[] {CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add (c)
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

          target.Alarms.Add(
              new Alarm()
              {
                Action = AlarmAction.Display,
                Trigger = trigger
              }
              );
        }
        else if (source.Inner.DueDate != _dateNull)
        {
          trigger.Duration = source.Inner.ReminderTime - source.Inner.DueDate;
          trigger.Parameters.Add ("RELATED", "END");
          trigger.Parameters.Add ("VALUE", "DURATION");

          target.Alarms.Add(
              new Alarm()
              {
                Action = AlarmAction.Display,
                Trigger = trigger
              }
              );
        }
      }
    }

    public TaskItemWrapper Map2To1 (IICalendar sourceCalendar, TaskItemWrapper target)
    {
      var source = sourceCalendar.Todos[0];
      return Map2To1 (source, target);
    }

    public TaskItemWrapper Map2To1 (ITodo source, TaskItemWrapper target)
    {
      target.Inner.Subject = source.Summary;
      target.Inner.Body = source.Description;
     
      if (source.Start != null ) target.Inner.StartDate = source.Start.Date;
      if (source.Due != null)
      {
        if (source.Start == null || source.Start.Value <= source.Due.Value)
        target.Inner.DueDate = source.Due.Date;
      }
      if (source.Completed != null)
      {
        target.Inner.DateCompleted = source.Completed.Date;
        target.Inner.Complete = true;
      }
      else
      {
        target.Inner.Complete = false;
      }

      target.Inner.PercentComplete = source.PercentComplete;
      
      target.Inner.Importance = MapPriority2To1 (source.Priority);

      target.Inner.Sensitivity = MapPrivacy2To1 (source.Class);

      target.Inner.Status = MapStatus2To1 (source.Status);

      MapCategories2To1 (source, target);

      MapReminder2To1 (source, target);

      return target;
    }

    private OlImportance MapPriority2To1 (int value)
    {
      switch (value)
      {
        case 9:
          return OlImportance.olImportanceLow;
        case 0:
        case 5:
          return OlImportance.olImportanceNormal;
        case 1:
          return OlImportance.olImportanceHigh;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
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

    private OlSensitivity MapPrivacy2To1 (string value)
    {
      switch (value)
      {
        case "PUBLIC":
          return OlSensitivity.olNormal;
        case "PRIVATE":
          return OlSensitivity.olPrivate;
        case "CONFIDENTIAL":
          return OlSensitivity.olConfidential;
      }
      return OlSensitivity.olNormal;
    }

    private void MapReminder2To1 (ITodo source, TaskItemWrapper target)
    {
      if (source.Alarms.Count == 0)
      {
        target.Inner.ReminderSet = false;
        return;
      }

      if (source.Alarms.Count > 1)
        s_logger.WarnFormat("Task '{0}' contains multiple alarms. Ignoring all except first.", source.UID);

      var alarm = source.Alarms[0];

      target.Inner.ReminderSet = true;

      if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.Start && alarm.Trigger.Duration.HasValue && source.Start != null)
      {
        target.Inner.ReminderTime = TimeZoneInfo.ConvertTimeFromUtc (source.Start.UTC, _localTimeZoneInfo).Add(alarm.Trigger.Duration.Value);
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
  }
}