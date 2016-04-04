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

namespace CalDavSynchronizer.Scheduling
{
  public class SynchronizationStatus : ISynchronizationRunLogger
  {
    private int _runCount;

    public event EventHandler<SchedulerStatusEventArgs> StatusChanged;

    public IDisposable LogStartSynchronizationRun ()
    {
      IncrementRunCount();
      return new SynchronizationRunningScope (this);
    }

    private void IncrementRunCount ()
    {
      // No need for synchronization, since all happens in the ui thread
      _runCount++;
      if (_runCount == 1)
        OnStatusChanged (true);
    }

    private void DecrementRunCount ()
    {
      _runCount--;
      if (_runCount == 0)
        OnStatusChanged (false);
    }

    private void OnStatusChanged (bool isRunning)
    {
      StatusChanged?.Invoke (this, new SchedulerStatusEventArgs (isRunning));
    }

    private class SynchronizationRunningScope : IDisposable
    {
      readonly SynchronizationStatus _;

      public SynchronizationRunningScope (SynchronizationStatus context)
      {
        if (context == null)
          throw new ArgumentNullException (nameof (context));

        _ = context;
      }

      public void Dispose ()
      {
        _.DecrementRunCount();
      }
    }
  }
}