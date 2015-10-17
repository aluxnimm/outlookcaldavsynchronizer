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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using GenSync.Synchronization;
using log4net;

namespace CalDavSynchronizer.Scheduling
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>
  /// This class is NOT threadsafe, but is safe regarding multiple entries of the same thread ( main thread)
  /// which will is a common case when using async methods
  /// </remarks>
  public class SynchronizationProfileRunner
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private DateTime _lastRun;
    private TimeSpan _interval;
    private IOutlookSynchronizer _synchronizer;
    private string _profileName;
    private bool _inactive;
    private readonly ISynchronizerFactory _synchronizerFactory;
    private int _isRunning = 0;

    private volatile bool _fullSyncPending = false;
    // There is no threadsafe Datastructure required, since there will be no concurrent threads
    // The concurrency in this scenario lies in the fact that the MainThread will enter multiple times, because
    // all methods are async
    private readonly List<PartialSync> _pendingPartialSyncs = new List<PartialSync>();

    public SynchronizationProfileRunner (ISynchronizerFactory synchronizerFactory)
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

    public async Task RunAndRescheduleNoThrow (bool runNow)
    {
      if (runNow || !_inactive && _interval > TimeSpan.Zero && DateTime.UtcNow > _lastRun + _interval)
      {
        _fullSyncPending = true;
        await RunAllPendingJobs();
      }
    }

    public async Task RunIfResponsibleNoThrow (string outlookId, string folderEntryId, string folderStoreId)
    {
      _pendingPartialSyncs.Add (new PartialSync (outlookId, folderEntryId, folderStoreId));
      await RunAllPendingJobs();
    }

    private async Task RunAllPendingJobs ()
    {
      // Monitor cannot be used here, since Monitor allows recursive enter for a thread 
      if (Interlocked.CompareExchange (ref _isRunning, 1, 0) == 0)
      {
        try
        {
          while (_fullSyncPending || _pendingPartialSyncs.Count > 0)
          {
            if (_fullSyncPending)
            {
              _fullSyncPending = false;
              Thread.MemoryBarrier(); // should not be required because there is just one thread entering multiple times
              await RunAndRescheduleNoThrow();
            }

            if (_pendingPartialSyncs.Count > 0)
            {
              var pendingPartialSync = _pendingPartialSyncs[0];
              _pendingPartialSyncs.RemoveAt (0);
              Thread.MemoryBarrier(); // should not be required because there is just one thread entering multiple times
              await RunIfResponsibleNoThrow (pendingPartialSync.OutlookId, pendingPartialSync.FolderEntryId, pendingPartialSync.FolderStoreId);
            }
          }
        }
        finally
        {
          Interlocked.Exchange (ref _isRunning, 0);
        }
      }
    }

    private async Task RunAndRescheduleNoThrow ()
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

    private async Task RunIfResponsibleNoThrow (PartialSync syncJob)
    {
      if (_inactive)
        return;

      try
      {
        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("Running synchronization profile '{0}'", _profileName)))
        {
          try
          {
            await _synchronizer.SnychronizeIfResponsible (syncJob.OutlookId, syncJob.FolderEntryId, syncJob.FolderStoreId);
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
    }

    private struct PartialSync
    {
      public readonly string OutlookId;
      public readonly string FolderEntryId;
      public readonly string FolderStoreId;

      public PartialSync (string outlookId, string folderEntryId, string folderStoreId)
          : this()
      {
        OutlookId = outlookId;
        FolderEntryId = folderEntryId;
        FolderStoreId = folderStoreId;
      }
    }
  }
}