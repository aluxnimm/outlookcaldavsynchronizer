using System;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;

namespace CalDavSynchronizer.Implementation.Tasks
{
  internal class TaskMapper : IEntityMapper<TaskItemWrapper, IICalendar>
  {
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
    }

    public TaskItemWrapper Map2To1 (IICalendar sourceCalendar, TaskItemWrapper target)
    {
      var source = sourceCalendar.Todos[0];
      return Map2To1 (source, target);
    }

    public TaskItemWrapper Map2To1 (ITodo source, TaskItemWrapper target)
    {
      target.Inner.Subject = source.Summary;
      return target;
    }
  }
}