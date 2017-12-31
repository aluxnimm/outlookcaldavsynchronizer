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
using System.Threading.Tasks;
using System.Windows.Input;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
  public class ProfileStatusViewModel : ModelBase
  {
    private string _profileName;
    private bool _isActive;

    DateTime? _lastSyncronizationRun;
    SyncronizationRunResult? _lastResult;

    private TimeSpan? _lastRunAgo;
    private readonly ICalDavSynchronizerCommands _calDavSynchronizerCommands;

    public ProfileStatusViewModel (Guid profileId, ICalDavSynchronizerCommands calDavSynchronizerCommands)
    {
      if (calDavSynchronizerCommands == null)
        throw new ArgumentNullException (nameof (calDavSynchronizerCommands));

      ProfileId = profileId;
      _calDavSynchronizerCommands = calDavSynchronizerCommands;

      ShowOptionsCommand = new DelegateCommand (_ =>
      {
        _calDavSynchronizerCommands.ShowOptionsAsync (ProfileId);
      });
      ShowLatestSynchronizationReportCommand = new DelegateCommand (_ =>
      {
        _calDavSynchronizerCommands.ShowLatestSynchronizationReport (ProfileId);
      });
    }
    
    public Guid ProfileId { get; }
    public ICommand ShowOptionsCommand { get; }
    public ICommand ShowLatestSynchronizationReportCommand { get; }
    
    public TimeSpan? LastRunAgo
    {
      get { return _lastRunAgo; }
      private set
      {
        CheckedPropertyChange (ref _lastRunAgo, value);
      }
    }

    public SyncronizationRunResult? LastResult
    {
      get { return _lastResult; }
      private set
      {
        CheckedPropertyChange (ref _lastResult, value);
      }
    }

    public string ProfileName
    {
      get { return _profileName; }
      private set
      {
        CheckedPropertyChange (ref _profileName, value);
      }
    }

    public bool IsActive
    {
      get { return _isActive; }
      private set
      {
        CheckedPropertyChange (ref _isActive, value);
      }
    }

    public void Update (Contracts.Options profile)
    {
      ProfileName = profile.Name;
      IsActive = !profile.Inactive;
    }

    public void Update (SynchronizationRunSummary summary)
    {
      _lastSyncronizationRun = summary.StartTimeUtc;
      LastResult = summary.Result;
      RecalculateLastRunAgo();
    }

    public void RecalculateLastRunAgo()
    {
      var lastRunAgoSeconds = (int?) (DateTime.UtcNow - _lastSyncronizationRun)?.TotalSeconds;
      LastRunAgo = lastRunAgoSeconds.HasValue ? TimeSpan.FromSeconds(lastRunAgoSeconds.Value) : (TimeSpan?) null;
    }

    public static ProfileStatusViewModel CreateDesignInstance (string profileName, SyncronizationRunResult? status, TimeSpan? lastRunAgo)
    {
      var viewModel = new ProfileStatusViewModel (Guid.NewGuid(), NullCalDavSynchronizerCommands.Instance);
      viewModel._profileName = profileName;
      viewModel._lastResult = status;
      viewModel._lastRunAgo = lastRunAgo;
      viewModel.IsActive = true;
      return viewModel;
    }
  }
}