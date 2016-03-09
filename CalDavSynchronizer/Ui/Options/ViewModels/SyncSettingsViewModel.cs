using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Implementation;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class SyncSettingsViewModel : ViewModelBase, IOptionsSection
  {
    private ConflictResolution _conflictResolution;
    private int _daysToSynchronizeInTheFuture;
    private int _daysToSynchronizeInThePast;
    private int _synchronizationIntervalInMinutes;
    private SynchronizationMode _synchronizationMode;
    private bool _useSynchronizationTimeRange;

    public SynchronizationMode SynchronizationMode
    {
      get { return _synchronizationMode; }
      set
      {
        _synchronizationMode = value;
        OnPropertyChanged();
      }
    }

    public ConflictResolution Resolution
    {
      get { return _conflictResolution; }
      set
      {
        _conflictResolution = value;
        OnPropertyChanged();
      }
    }

    public int SynchronizationIntervalInMinutes
    {
      get { return _synchronizationIntervalInMinutes; }
      set
      {
        _synchronizationIntervalInMinutes = value;
        OnPropertyChanged();
      }
    }

    public bool UseSynchronizationTimeRange
    {
      get { return _useSynchronizationTimeRange; }
      set
      {
        _useSynchronizationTimeRange = value;
        OnPropertyChanged();
      }
    }

    public int DaysToSynchronizeInThePast
    {
      get { return _daysToSynchronizeInThePast; }
      set
      {
        _daysToSynchronizeInThePast = value;
        OnPropertyChanged();
      }
    }

    public int DaysToSynchronizeInTheFuture
    {
      get { return _daysToSynchronizeInTheFuture; }
      set
      {
        _daysToSynchronizeInTheFuture = value;
        OnPropertyChanged();
      }
    }

    public string SelectedSynchronizationModeDisplayName => AvailableSynchronizationModes.First (m => m.Value == SynchronizationMode).Name;

    public IList<Item<int>> AvailableSyncIntervals =>
        new[] { new Item<int> (0, "Manual only") }
            .Union (Enumerable.Range (1, 2).Select (i => new Item<int> (i, i.ToString())))
            .Union (Enumerable.Range (1, 12).Select (i => i * 5).Select (i => new Item<int> (i, i.ToString()))).ToList();

    public IList<Item<ConflictResolution>> AvailableConflictResolutions { get; } = new List<Item<ConflictResolution>>
                                                                                   {
                                                                                       new Item<ConflictResolution> (ConflictResolution.OutlookWins, "OutlookWins"),
                                                                                       new Item<ConflictResolution> (ConflictResolution.ServerWins, "ServerWins"),
                                                                                       //new Item<ConflictResolution> (ConflictResolution.Manual, "Manual"),
                                                                                       new Item<ConflictResolution> (ConflictResolution.Automatic, "Automatic")
                                                                                   };


    public IList<Item<SynchronizationMode>> AvailableSynchronizationModes { get; } = new List<Item<SynchronizationMode>>
                                                                                     {
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.ReplicateOutlookIntoServer, "Outlook \u2192 Server (Replicate)"),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.ReplicateServerIntoOutlook, "Outlook \u2190 Server (Replicate)"),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeOutlookIntoServer, "Outlook \u2192 Server (Merge)"),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeServerIntoOutlook, "Outlook \u2190 Server (Merge)"),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeInBothDirections, "Outlook \u2190\u2192 Server (Two-Way)")
                                                                                     };

    public static SyncSettingsViewModel DesignInstance { get; } = new SyncSettingsViewModel
                                                                  {
                                                                      SynchronizationMode = SynchronizationMode.MergeInBothDirections,
                                                                      Resolution = ConflictResolution.Automatic,
                                                                      UseSynchronizationTimeRange = true,
                                                                      DaysToSynchronizeInTheFuture = 33,
                                                                      DaysToSynchronizeInThePast = 44,
                                                                      SynchronizationIntervalInMinutes = 20
                                                                  };

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SynchronizationMode = options.SynchronizationMode;
      Resolution = options.ConflictResolution;
      SynchronizationIntervalInMinutes = options.SynchronizationIntervalInMinutes;
      UseSynchronizationTimeRange = !options.IgnoreSynchronizationTimeRange;
      DaysToSynchronizeInThePast = options.DaysToSynchronizeInThePast;
      DaysToSynchronizeInTheFuture = options.DaysToSynchronizeInTheFuture;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.SynchronizationMode = _synchronizationMode;
      options.ConflictResolution = _conflictResolution;
      options.SynchronizationIntervalInMinutes = _synchronizationIntervalInMinutes;
      options.IgnoreSynchronizationTimeRange = !_useSynchronizationTimeRange;
      options.DaysToSynchronizeInThePast = _daysToSynchronizeInThePast;
      options.DaysToSynchronizeInTheFuture = _daysToSynchronizeInTheFuture;
    }

    public bool Validate (StringBuilder errorBuilder)
    {
      return true;
    }
  }
}