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
using CalDavSynchronizer.Ui.Options.Models;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class SyncSettingsViewModel : ModelBase, IOptionsSection
  {
    private readonly OptionsModel _model;
    
    public SyncSettingsViewModel(OptionsModel model, IViewOptions viewOptions)
    {
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _model = model;
      ViewOptions = viewOptions;

      RegisterPropertyChangePropagation(_model, nameof(_model.Resolution), nameof(Resolution));
      RegisterPropertyChangePropagation(_model, nameof(_model.SynchronizationIntervalInMinutes), nameof(SynchronizationIntervalInMinutes));
      RegisterPropertyChangePropagation(_model, nameof(_model.ChunkSize), nameof(ChunkSize));
      RegisterPropertyChangePropagation(_model, nameof(_model.IsChunkedSynchronizationEnabled), nameof(IsChunkedSynchronizationEnabled));

      RegisterPropertyChangePropagation(_model, nameof(_model.SynchronizationMode), nameof(SynchronizationMode));
      RegisterPropertyChangePropagation(_model, nameof(_model.SynchronizationMode), nameof(ConflictResolutionAvailable));
    }

    public SynchronizationMode SynchronizationMode
    {
      get { return _model.SynchronizationMode; }
      set { _model.SynchronizationMode = value; }
    }

    public bool ConflictResolutionAvailable => SynchronizationMode == SynchronizationMode.MergeInBothDirections;

    public ConflictResolution Resolution
    {
      get { return _model.Resolution; }
      set { _model.Resolution = value; }
    }

    public int SynchronizationIntervalInMinutes
    {
      get { return _model.SynchronizationIntervalInMinutes; }
      set { _model.SynchronizationIntervalInMinutes = value; }
    }

    public int ChunkSize
    {
      get { return _model.ChunkSize; }
      set { _model.ChunkSize = value; }
    }

    public bool IsChunkedSynchronizationEnabled
    {
      get { return _model.IsChunkedSynchronizationEnabled; }
      set { _model.IsChunkedSynchronizationEnabled = value; }
    }

    public bool IsEditOfIsChunkedSynchronizationEnabledAllowed
    {
      get { return _model.IsEditOfIsChunkedSynchronizationEnabledAllowed; }
    
    }

    public IList<Item<int>> AvailableSyncIntervals =>
        new[] { new Item<int>(0, "Manual only") }
            .Union(Enumerable.Range(1, 2).Select(i => new Item<int>(i, i.ToString())))
            .Union(Enumerable.Range(1, 12).Select(i => i * 5).Select(i => new Item<int>(i, i.ToString()))).ToList();

    public IList<Item<ConflictResolution>> AvailableConflictResolutions { get; } = new List<Item<ConflictResolution>>
                                                                                   {
                                                                                       new Item<ConflictResolution> (ConflictResolution.OutlookWins, EnumDisplayNameProvider.Instance.Get(ConflictResolution.OutlookWins) ),
                                                                                       new Item<ConflictResolution> (ConflictResolution.ServerWins,  EnumDisplayNameProvider.Instance.Get(ConflictResolution.ServerWins)),
                                                                                       //new Item<ConflictResolution> (ConflictResolution.Manual, "Manual"),
                                                                                       new Item<ConflictResolution> (ConflictResolution.Automatic,  EnumDisplayNameProvider.Instance.Get(ConflictResolution.Automatic))
                                                                                   };


    public IList<Item<SynchronizationMode>> AvailableSynchronizationModes { get; } = new List<Item<SynchronizationMode>>
                                                                                     {
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.ReplicateOutlookIntoServer,  EnumDisplayNameProvider.Instance.Get(SynchronizationMode.ReplicateOutlookIntoServer)),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.ReplicateServerIntoOutlook,  EnumDisplayNameProvider.Instance.Get(SynchronizationMode.ReplicateServerIntoOutlook)),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeOutlookIntoServer,  EnumDisplayNameProvider.Instance.Get(SynchronizationMode.MergeOutlookIntoServer)),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeServerIntoOutlook,  EnumDisplayNameProvider.Instance.Get(SynchronizationMode.MergeServerIntoOutlook)),
                                                                                         new Item<SynchronizationMode> (SynchronizationMode.MergeInBothDirections,  EnumDisplayNameProvider.Instance.Get(SynchronizationMode.MergeInBothDirections))
                                                                                     };

    public static SyncSettingsViewModel DesignInstance { get; } = new SyncSettingsViewModel(OptionsModel.DesignInstance, OptionsCollectionViewModel.DesignViewOptions)
    {
      SynchronizationMode = SynchronizationMode.MergeInBothDirections,
      Resolution = ConflictResolution.Automatic,
      SynchronizationIntervalInMinutes = 20,
      IsChunkedSynchronizationEnabled = true,
      ChunkSize = 66
    };

    public IViewOptions ViewOptions { get; }
  }
}