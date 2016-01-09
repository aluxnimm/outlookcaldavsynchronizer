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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using GenSync.Logging;
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
    private readonly ISynchronizationReportRepository _synchronizationReportRepository;
    private string _profileName;
    private Guid _profileId;
    private bool _inactive;
    private readonly ISynchronizerFactory _synchronizerFactory;
    private int _isRunning = 0;
    private bool _changeTriggeredSynchronizationEnabled;

    private volatile bool _fullSyncPending = false;
    // There is no threadsafe Datastructure required, since there will be no concurrent threads
    // The concurrency in this scenario lies in the fact that the MainThread will enter multiple times, because
    // all methods are async
    private readonly List<string> _pendingOutlookItems = new List<string>();

    public SynchronizationProfileRunner (
        ISynchronizerFactory synchronizerFactory,
        ISynchronizationReportRepository synchronizationReportRepository)
    {
      _synchronizerFactory = synchronizerFactory;
      _synchronizationReportRepository = synchronizationReportRepository;
      // Set to min, to ensure that it runs on the first run after startup
      _lastRun = DateTime.MinValue;
    }

    public void UpdateOptions (Options options)
    {
      _pendingOutlookItems.Clear();
      _fullSyncPending = false;

      _profileName = options.Name;
      _profileId = options.Id;
      _synchronizer = _synchronizerFactory.CreateSynchronizer (options);
      _interval = TimeSpan.FromMinutes (options.SynchronizationIntervalInMinutes);
      _inactive = options.Inactive;
      _changeTriggeredSynchronizationEnabled = options.EnableChangeTriggeredSynchronization;
    }

    public async Task RunAndRescheduleNoThrow (bool runNow)
    {
      try
      {
        if (_inactive)
          return;

        if (runNow || _interval > TimeSpan.Zero && DateTime.UtcNow > _lastRun + _interval)
        {
          _fullSyncPending = true;
          await RunAllPendingJobs();
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    public async Task RunIfResponsibleNoThrow (string outlookId, string folderEntryId, string folderStoreId)
    {
      try
      {
        if (!_changeTriggeredSynchronizationEnabled)
          return;

        if (_inactive)
          return;

        if (!_synchronizer.IsResponsible (folderEntryId, folderStoreId))
          return;

        _pendingOutlookItems.Add (outlookId);
        await RunAllPendingJobs();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private async Task RunAllPendingJobs ()
    {
      if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
      {
        s_logger.WarnFormat ("Skipping synchronization profile '{0}' (Id: '{1}') because network is not available", _profileName, _profileId);
        return;
      }

      // Monitor cannot be used here, since Monitor allows recursive enter for a thread 
      if (Interlocked.CompareExchange (ref _isRunning, 1, 0) == 0)
      {
        try
        {
          while (_fullSyncPending || _pendingOutlookItems.Count > 0)
          {
            if (_fullSyncPending)
            {
              _fullSyncPending = false;
              Thread.MemoryBarrier(); // should not be required because there is just one thread entering multiple times
              await RunAndRescheduleNoThrow();
            }

            if (_pendingOutlookItems.Count > 0)
            {
              var itemsToSync = _pendingOutlookItems.ToArray();
              _pendingOutlookItems.Clear();
              Thread.MemoryBarrier(); // should not be required because there is just one thread entering multiple times
              await RunIfResponsibleNoThrow (itemsToSync);
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
      try
      {
        var logger = new SynchronizationLogger (_profileId, _profileName);
        
        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("Running synchronization profile '{0}'", _profileName)))
        {
            await _synchronizer.SynchronizeNoThrow (logger);
        }

        GC.Collect ();
        GC.WaitForPendingFinalizers ();
        var synchronizationReport = logger.GetReport();
        _synchronizationReportRepository.AddReport (synchronizationReport);
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

    private async Task RunIfResponsibleNoThrow (IEnumerable<string> itemsToSync)
    {
      try
      {
        var logger = new SynchronizationLogger (_profileId, _profileName);
        
        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("Running synchronization profile '{0}'", _profileName)))
        {
          await _synchronizer.SnychronizePartialNoThrow (itemsToSync, logger);
        }

        GC.Collect ();
        GC.WaitForPendingFinalizers ();
        var synchronizationReport = logger.GetReport();
        _synchronizationReportRepository.AddReport (synchronizationReport);
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }
  }
}