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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CalDavSynchronizer.ChangeWatching;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Diagnostics;
using CalDavSynchronizer.Reports;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using CalDavSynchronizer.Ui.ConnectionTests;
using GenSync;
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
    private readonly ISynchronizationReportSink _reportSink;
    private string _profileName;
    private Guid _profileId;
    private ProxyOptions _proxyOptions;
    private bool _inactive;
    private readonly ISynchronizerFactory _synchronizerFactory;
    private int _isRunning = 0;
    private bool _checkIfOnline;
    private readonly IFolderChangeWatcherFactory _folderChangeWatcherFactory;
    private IItemCollectionChangeWatcher _folderChangeWatcher;
    private readonly Action _ensureSynchronizationContext;
    private readonly ISynchronizationRunLogger _runLogger;

    private volatile bool _fullSyncPending = false;
    // There is no threadsafe Datastructure required, since there will be no concurrent threads
    // The concurrency in this scenario lies in the fact that the MainThread will enter multiple times, because
    // all methods are async
    private readonly ConcurrentDictionary<string, IIdWithHints<string, DateTime>> _pendingOutlookItems = new ConcurrentDictionary<string, IIdWithHints<string, DateTime>>();
    // Since events for itemchange are raised by outlook multiple times, the partialsync is delayed.
    // This will prevent that subsequent change events for the same item trigger subsequent sync runs, provided they are within this time frame
    private readonly TimeSpan _partialSyncDelay = TimeSpan.FromSeconds (10);

    public SynchronizationProfileRunner (
        ISynchronizerFactory synchronizerFactory,
        ISynchronizationReportSink reportSink,
        IFolderChangeWatcherFactory folderChangeWatcherFactory,
        Action ensureSynchronizationContext, 
        ISynchronizationRunLogger runLogger)
    {
      if (synchronizerFactory == null)
        throw new ArgumentNullException (nameof (synchronizerFactory));
      if (reportSink == null)
        throw new ArgumentNullException (nameof (reportSink));
      if (folderChangeWatcherFactory == null)
        throw new ArgumentNullException (nameof (folderChangeWatcherFactory));
      if (ensureSynchronizationContext == null)
        throw new ArgumentNullException (nameof (ensureSynchronizationContext));
      if (runLogger == null)
        throw new ArgumentNullException (nameof (runLogger));

      _synchronizerFactory = synchronizerFactory;
      _reportSink = reportSink;
      _folderChangeWatcherFactory = folderChangeWatcherFactory;
      _ensureSynchronizationContext = ensureSynchronizationContext;
      _runLogger = runLogger;
      // Set to min, to ensure that it runs on the first run after startup
      _lastRun = DateTime.MinValue;
    }

    public async Task UpdateOptions (Options options, GeneralOptions generalOptions)
    {
      if (options == null)
        throw new ArgumentNullException (nameof (options));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));

      _pendingOutlookItems.Clear();
      _fullSyncPending = false;

      _profileName = options.Name;
      _profileId = options.Id;
      _proxyOptions = options.ProxyOptions;
      _synchronizer = options.Inactive ? NullOutlookSynchronizer.Instance : await _synchronizerFactory.CreateSynchronizer (options, generalOptions);
      _interval = TimeSpan.FromMinutes (options.SynchronizationIntervalInMinutes);
      _inactive = options.Inactive;
      _checkIfOnline = generalOptions.CheckIfOnline;

      if (_folderChangeWatcher != null)
      {
        _folderChangeWatcher.ItemSavedOrDeleted -= FolderChangeWatcher_ItemSavedOrDeleted;
        _folderChangeWatcher.Dispose();
        _folderChangeWatcher = null;
      }

      if (!_inactive && options.EnableChangeTriggeredSynchronization)
      {
        _folderChangeWatcher =
            _folderChangeWatcherFactory.Create (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
        _folderChangeWatcher.ItemSavedOrDeleted += FolderChangeWatcher_ItemSavedOrDeleted;
      }
    }

    private void FolderChangeWatcher_ItemSavedOrDeleted (object sender, ItemSavedEventArgs e)
    {
      try
      {
        _ensureSynchronizationContext();
        FolderChangeWatcher_ItemSavedOrDeletedAsync (e);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }

    private async void FolderChangeWatcher_ItemSavedOrDeletedAsync (ItemSavedEventArgs e)
    {
      try
      {
        _pendingOutlookItems.AddOrUpdate (e.EntryId.Id, e.EntryId, (key, existingValue) => e.EntryId.VersionHint > existingValue.VersionHint ? e.EntryId : existingValue);
        if (s_logger.IsDebugEnabled)
        {
          s_logger.Debug ($"Partial sync:  '{_pendingOutlookItems.Count}' items pending after registering item '{e.EntryId.Id}' as pending sync item.");
        }
        await Task.Delay (_partialSyncDelay);
        using (_runLogger.LogStartSynchronizationRun())
        {
          await RunAllPendingJobs();
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
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
        s_logger.Error (null, x);
      }
    }

    private async Task RunAllPendingJobs ()
    {
      if (_checkIfOnline && !ConnectionTester.IsOnline (_proxyOptions))
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
              var itemsToSync = _pendingOutlookItems.Values.ToArray();
              _pendingOutlookItems.Clear();
              if (s_logger.IsDebugEnabled)
              {
                s_logger.Debug ($"Partial sync: Going to sync '{itemsToSync.Length}' pending items ( {string.Join (", ", itemsToSync.Select (id => id.Id))} ).");
              }
              Thread.MemoryBarrier(); // should not be required because there is just one thread entering multiple times
              await RunPartialNoThrow (itemsToSync);
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

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var synchronizationReport = logger.GetReport();
        _reportSink.PostReport (synchronizationReport);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
      finally
      {
        _lastRun = DateTime.UtcNow;
      }
    }

    private async Task RunPartialNoThrow (IIdWithHints<string, DateTime>[] itemsToSync)
    {
      try
      {
        var logger = new SynchronizationLogger (_profileId, _profileName);

        using (AutomaticStopwatch.StartInfo (s_logger, string.Format ("Partial sync: Running synchronization profile '{0}'", _profileName)))
        {
          await _synchronizer.SnychronizePartialNoThrow (itemsToSync, logger);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        var synchronizationReport = logger.GetReport();
        _reportSink.PostReport (synchronizationReport);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
  }
}