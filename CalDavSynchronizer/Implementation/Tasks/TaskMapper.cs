using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Generic.EntityMapping;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;

namespace CalDavSynchronizer.Implementation.Tasks
{
  class TaskMapper :  IEntityMapper<TaskItemWrapper, ITodo>
  {
    public ITodo Map1To2 (TaskItemWrapper source, ITodo target)
    {
      throw new NotImplementedException();
    }

    public TaskItemWrapper Map2To1 (ITodo source, TaskItemWrapper target)
    {
      throw new NotImplementedException();
    }
  }
}
