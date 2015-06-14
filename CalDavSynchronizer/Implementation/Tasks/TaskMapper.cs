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
     
      if (source.Start != null ) target.Inner.StartDate = source.Start.Value;
      if (source.Due != null) target.Inner.DueDate = source.Due.Value;

      if (source.Completed != null)
      {
        target.Inner.DateCompleted = source.Completed.Value;
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
        s_logger.WarnFormat("Task '{0}' contains multiple alarms. Ignoring all except first.", source.Url);

      var alarm = source.Alarms[0];

      target.Inner.ReminderSet = true;

      if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.Start && alarm.Trigger.Duration.HasValue)
      {
        target.Inner.ReminderTime = source.Start.Value.Add (alarm.Trigger.Duration.Value);
      }
      else if (alarm.Trigger.IsRelative && alarm.Trigger.Related == TriggerRelation.End && alarm.Trigger.Duration.HasValue)
      {
        target.Inner.ReminderTime = source.Due.Value.Add (alarm.Trigger.Duration.Value);
      }
      else
      {
        s_logger.WarnFormat ("Task '{0}' alarm is not supported. Ignoring.", source.Url);
        target.Inner.ReminderSet = false;
      }
    }
  }
}