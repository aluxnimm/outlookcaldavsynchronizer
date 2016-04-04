using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Scheduling
{
  public class SchedulerStatusEventArgs : EventArgs
  {
    public SchedulerStatusEventArgs (bool isRunning)
    {
      IsRunning = isRunning;
    }

    public bool IsRunning { get; }
  }
}
