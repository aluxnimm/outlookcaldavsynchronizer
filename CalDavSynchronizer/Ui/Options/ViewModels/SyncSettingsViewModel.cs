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
    private int _synchronizationIntervalInMinutes;
    private SynchronizationMode _synchronizationMode;
    private int _chunkSize;
    private bool _isChunkedSynchronizationEnabled;

    public SynchronizationMode SynchronizationMode
    {
      get { return _synchronizationMode; }
      set
      {
        CheckedPropertyChange (ref _synchronizationMode, value);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(nameof(ConflictResolutionAvailable));
      }
    }

    public bool ConflictResolutionAvailable => _synchronizationMode == SynchronizationMode.MergeInBothDirections;

    public ConflictResolution Resolution
    {
      get { return _conflictResolution; }
      set
      {
        CheckedPropertyChange (ref _conflictResolution, value);
      }
    }

    public int SynchronizationIntervalInMinutes
    {
      get { return _synchronizationIntervalInMinutes; }
      set
      {
        CheckedPropertyChange (ref _synchronizationIntervalInMinutes, value);
      }
    }

    public int ChunkSize
    {
      get { return _chunkSize; }
      set { CheckedPropertyChange (ref _chunkSize, value); }
    }

    public bool IsChunkedSynchronizationEnabled
    {
      get { return _isChunkedSynchronizationEnabled; }
      set { CheckedPropertyChange(ref _isChunkedSynchronizationEnabled, value); }
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
                                                                      SynchronizationIntervalInMinutes = 20,
                                                                      IsChunkedSynchronizationEnabled = true,
                                                                      ChunkSize = 66
                                                                  };

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SynchronizationMode = options.SynchronizationMode;
      Resolution = options.ConflictResolution;
      SynchronizationIntervalInMinutes = options.SynchronizationIntervalInMinutes;
      IsChunkedSynchronizationEnabled = options.IsChunkedSynchronizationEnabled;
      ChunkSize = options.ChunkSize;
    }
    
    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.SynchronizationMode = _synchronizationMode;
      options.ConflictResolution = _conflictResolution;
      options.SynchronizationIntervalInMinutes = _synchronizationIntervalInMinutes;
      options.IsChunkedSynchronizationEnabled = IsChunkedSynchronizationEnabled;
      options.ChunkSize = ChunkSize;
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      var isValid = true;

      if (IsChunkedSynchronizationEnabled && ChunkSize < 1)
      {
        isValid = false;
        errorMessageBuilder.AppendLine("- The chunk size hast to be 1 or greater.");
      }

      return isValid;
    }
  }
}