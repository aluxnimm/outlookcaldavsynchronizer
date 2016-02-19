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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.using System;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
  public class ProfileStatusViewModel : INotifyPropertyChanged
  {
    private string _profileName;
    private bool _isActive;

    DateTime? _lastSyncronizationRun;
    SyncronizationRunResult? _lastResult;

    private int? _lastRunMinutesAgo;


    public ProfileStatusViewModel (Guid profileId)
    {
      ProfileId = profileId;
    }

    public Guid ProfileId { get; }

    public int? LastRunMinutesAgo
    {
      get { return _lastRunMinutesAgo; }
      private set
      {
        _lastRunMinutesAgo = value;
        OnPropertyChanged();
      }
    }

    public SyncronizationRunResult? LastResult
    {
      get { return _lastResult; }
      private set
      {
        _lastResult = value;
        OnPropertyChanged();
      }
    }

    public string ProfileName
    {
      get { return _profileName; }
      private set
      {
        _profileName = value;
        OnPropertyChanged();
      }
    }

    public bool IsActive
    {
      get { return _isActive; }
      private set
      {
        _isActive = value;
        OnPropertyChanged();
      }
    }

    public void Update (Contracts.Options profile)
    {
      ProfileName = profile.Name;
      IsActive = !profile.Inactive;
    }

    public void Update (SynchronizationReport report)
    {
      _lastSyncronizationRun = report.StartTime;
      LastResult =
          report.HasErrors
              ? SyncronizationRunResult.Error
              : report.HasWarnings
                  ? SyncronizationRunResult.Warning
                  : SyncronizationRunResult.Ok;
      RecalculateLastRunAgoInMinutes();
    }

    public void RecalculateLastRunAgoInMinutes ()
    {
      LastRunMinutesAgo = (int?) (DateTime.UtcNow - _lastSyncronizationRun)?.TotalMinutes;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }


    public static ProfileStatusViewModel CreateDesignInstance (string profileName, SyncronizationRunResult? status, int? lastRunMinutesAgo)
    {
      var viewModel = new ProfileStatusViewModel (Guid.NewGuid());
      viewModel._profileName = profileName;
      viewModel._lastResult = status;
      viewModel._lastRunMinutesAgo = lastRunMinutesAgo;
      viewModel.IsActive = true;
      return viewModel;
    }
  }
}