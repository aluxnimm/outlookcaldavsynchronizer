// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Utilities;
using GenSync.Synchronization;
using log4net;

namespace CalDavSynchronizer.Scheduling
{
  public class SynchronizationWorker
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private DateTime _lastRun;
    private TimeSpan _interval;
    private ISynchronizer _synchronizer;
    private string _profileName;
    private bool _inactive;
    private readonly ISynchronizerFactory _synchronizerFactory;
    private int _workInProgress = 0;

    public SynchronizationWorker (ISynchronizerFactory synchronizerFactory)
    {
      _synchronizerFactory = synchronizerFactory;
      // Set to min, to ensure that it runs on the first run after startup
      _lastRun = DateTime.MinValue;
    }

    public void UpdateOptions (Options options)
    {
      _profileName = options.Name;
      _synchronizer = _synchronizerFactory.CreateSynchronizer (options);
      _interval = TimeSpan.FromMinutes (options.SynchronizationIntervalInMinutes);
      _inactive = options.Inactive;
    }

    public async Task RunIfRequiredAndReschedule ()
    {
      if (!_inactive && _interval > TimeSpan.Zero && DateTime.UtcNow > _lastRun + _interval)
      {
        await RunNoThrowAndRescheduleIfNotRunning();
      }
    }

    public async Task RunNoThrowAndRescheduleIfNotRunning ()
    {
      // Monitor cannot be used here, since Monitor allows recursive enter for a thread (which can easily happen in an async scenario)
      if (Interlocked.CompareExchange (ref _workInProgress, 1, 0) == 0)
      {
        try
        {
          await RunNoThrowAndReschedule();
        }
        finally
        {
          Interlocked.Exchange (ref _workInProgress, 0);
        }
      }
      else
      {
        s_logger.InfoFormat ("Skipping run of Synchronization profile '{0}', because it is currently already running.", _profileName);
      }
    }


    private async Task RunNoThrowAndReschedule ()
    {
      if (_inactive)
        return;

      try
      {
        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("Running synchronization profile '{0}'", _profileName)))
        {
          try
          {
            await _synchronizer.Synchronize();
          }
          finally
          {
            GC.Collect();
            GC.WaitForPendingFinalizers();
          }
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
      finally
      {
        _lastRun = DateTime.UtcNow;
      }
    }
  }
}