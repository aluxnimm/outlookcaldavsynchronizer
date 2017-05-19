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
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Reports;
using CalDavSynchronizer.Synchronization;
using CalDavSynchronizer.Utilities;
using CalDavSynchronizer.Ui.ConnectionTests;
using GenSync;
using GenSync.EntityRepositories;
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
  public partial class SynchronizationProfileRunner
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    /// <summary>
    /// Since events for itemchange are raised by outlook multiple times, the partialsync is delayed.
    /// This will prevent that subsequent change events for the same item trigger subsequent sync runs, provided they are within this time frame
    /// </summary>
    private readonly TimeSpan _partialSyncDelay = TimeSpan.FromSeconds (10);

    private readonly Guid _profileId;
    private readonly ISynchronizationReportSink _reportSink;
    private readonly ISynchronizerFactory _synchronizerFactory;
    private readonly IFolderChangeWatcherFactory _folderChangeWatcherFactory;
    private readonly Action _ensureSynchronizationContext;
    private readonly ISynchronizationRunLogger _runLogger;
    private readonly IDateTimeProvider _dateTimeProvider;
   
    private ProfileData _profile;

    private volatile bool _fullSyncPending;
    /// <summary>
    /// There is no threadsafe Datastructure required, since there will be no concurrent threads
    /// The concurrency in this scenario lies in the fact that the MainThread will enter multiple times, because
    /// all methods are async
    /// </summary>
    private readonly ConcurrentDictionary<string, IOutlookId> _pendingOutlookItems = new ConcurrentDictionary<string, IOutlookId> ();
    private DateTime _lastRun;
    private int _isRunning;

    private ErrorHandlingStrategy _errorHandlingStrategy;
    
    public SynchronizationProfileRunner (
        ISynchronizerFactory synchronizerFactory,
        ISynchronizationReportSink reportSink,
        IFolderChangeWatcherFactory folderChangeWatcherFactory,
        Action ensureSynchronizationContext, 
        ISynchronizationRunLogger runLogger,
        IDateTimeProvider dateTimeProvider, 
        Guid profileId)
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
      if (dateTimeProvider == null) throw new ArgumentNullException(nameof(dateTimeProvider));

      _profileId = profileId;
      _synchronizerFactory = synchronizerFactory;
      _reportSink = reportSink;
      _folderChangeWatcherFactory = folderChangeWatcherFactory;
      _ensureSynchronizationContext = ensureSynchronizationContext;
      _runLogger = runLogger;
      _dateTimeProvider = dateTimeProvider;
      // Set to min, to ensure that it runs on the first run after startup
      _lastRun = DateTime.MinValue;
      _errorHandlingStrategy = new ErrorHandlingStrategy(_profile, _dateTimeProvider, 0);
    }

    public async Task UpdateOptions(Options options, GeneralOptions generalOptions)
    {
      if (options == null)
        throw new ArgumentNullException(nameof(options));
      if (generalOptions == null)
        throw new ArgumentNullException(nameof(generalOptions));
      if (_profileId != options.Id)
        throw new ArgumentException($"Cannot update runner for profile '{_profileId}' with options of profile '{options.Id}'");

      if (_isRunning == 1)
        s_logger.Info($"Applying options to profile '{options.Name}' ({_profileId}) which is currently running.");

      _pendingOutlookItems.Clear();
      _fullSyncPending = false;

      if (!_profile.IsEmpty)
      {
        _profile.FolderChangeWatcher.ItemSavedOrDeleted -= FolderChangeWatcher_ItemSavedOrDeleted;
        _profile.FolderChangeWatcher.Dispose();
      }

      _profile = new ProfileData(
        options.Inactive,
        options.Name,
        generalOptions.CheckIfOnline,
        TimeSpan.FromMinutes(options.SynchronizationIntervalInMinutes),
        options.ProxyOptions,
        options.Inactive
          ? NullOutlookSynchronizer.Instance
          : await _synchronizerFactory.CreateSynchronizer(options, generalOptions),
        !options.Inactive && options.EnableChangeTriggeredSynchronization
          ? _folderChangeWatcherFactory.Create(options.OutlookFolderEntryId, options.OutlookFolderStoreId)
          : NullItemCollectionChangeWatcher.Instance);

      _profile.FolderChangeWatcher.ItemSavedOrDeleted += FolderChangeWatcher_ItemSavedOrDeleted;

      _errorHandlingStrategy = new ErrorHandlingStrategy (_profile, _dateTimeProvider, generalOptions.MaxSucessiveWarnings);
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
        if(_errorHandlingStrategy.ShouldPostponeSyncRun())
          return;

        _pendingOutlookItems.AddOrUpdate (e.EntryId.EntryId, e.EntryId, (key, existingValue) => e.EntryId.Version > existingValue.Version ? e.EntryId : existingValue);
        if (s_logger.IsDebugEnabled)
        {
          s_logger.Debug ($"Partial sync:  '{_pendingOutlookItems.Count}' items pending after registering item '{e.EntryId.EntryId}' as pending sync item.");
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

    public async Task RunAndRescheduleNoThrow (bool wasManuallyTriggered)
    {
      try
      {
        if (_profile.Inactive)
          return;

        if (wasManuallyTriggered ||
          _profile.Interval > TimeSpan.Zero && _dateTimeProvider.Now > _lastRun + _profile.Interval && !_errorHandlingStrategy.ShouldPostponeSyncRun ())
        {
          _errorHandlingStrategy.NotifySyncRunStarting(wasManuallyTriggered);
          _fullSyncPending = true;

          await RunAllPendingJobs();

          _errorHandlingStrategy.NotifySyncRunFinished();
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }

    private async Task RunAllPendingJobs ()
    {
      if (_profile.CheckIfOnline && !ConnectionTester.IsOnline (_profile.ProxyOptionsOrNull))
      {
        s_logger.WarnFormat ("Skipping synchronization profile '{0}' (Id: '{1}') because network is not available", _profile.ProfileName, _profileId);
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
                s_logger.Debug ($"Partial sync: Going to sync '{itemsToSync.Length}' pending items ( {string.Join (", ", itemsToSync.Select (id => id.EntryId))} ).");
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
        using (var logger = new SynchronizationLogger(_profileId, _profile.ProfileName, _reportSink))
        {
          using (AutomaticStopwatch.StartInfo(s_logger, string.Format("Running synchronization profile '{0}'", _profile.ProfileName)))
          {
            try
            {
              await _profile.Synchronizer.Synchronize(logger);
            }
            catch (Exception x)
            {
              _errorHandlingStrategy.HandleException (x, logger);
            }
          }

          GC.Collect();
          GC.WaitForPendingFinalizers();
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
      finally
      {
        _lastRun = _dateTimeProvider.Now;
      }
    }
    
    private async Task RunPartialNoThrow (IOutlookId[] itemsToSync)
    {
      try
      {
        using (var logger = new SynchronizationLogger(_profileId, _profile.ProfileName, _reportSink))
        {
          using (AutomaticStopwatch.StartInfo(s_logger, string.Format("Partial sync: Running synchronization profile '{0}'", _profile.ProfileName)))
          {
            try
            {
              await _profile.Synchronizer.SynchronizePartial(itemsToSync, logger);
            }
            catch (Exception x)
            {
              _errorHandlingStrategy.HandleException(x, logger);
            }
          }

          GC.Collect();
          GC.WaitForPendingFinalizers();
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
    
    struct ProfileData
    {
      public readonly bool Inactive;
      public readonly string ProfileName;
      public readonly bool CheckIfOnline;
      public readonly TimeSpan Interval;
      public readonly ProxyOptions ProxyOptionsOrNull;
      public readonly IOutlookSynchronizer Synchronizer;
      public readonly IItemCollectionChangeWatcher FolderChangeWatcher;

      public ProfileData (bool inactive, string profileName, bool checkIfOnline, TimeSpan interval, ProxyOptions proxyOptionsOrNull, IOutlookSynchronizer synchronizer, IItemCollectionChangeWatcher folderChangeWatcher)
      {
        if (profileName == null)
          throw new ArgumentNullException (nameof (profileName));
        if (synchronizer == null)
          throw new ArgumentNullException (nameof (synchronizer));
        if (folderChangeWatcher == null)
          throw new ArgumentNullException (nameof (folderChangeWatcher));

        Inactive = inactive;
        ProfileName = profileName;
        CheckIfOnline = checkIfOnline;
        Interval = interval;
        ProxyOptionsOrNull = proxyOptionsOrNull;
        Synchronizer = synchronizer;
        FolderChangeWatcher = folderChangeWatcher;
      }

      public bool IsEmpty => FolderChangeWatcher == null;
    }
  }
}