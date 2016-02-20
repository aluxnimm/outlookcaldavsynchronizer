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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
  public class ProfileStatusesViewModel
  {
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly Timer _timer;

    public ObservableCollection<ProfileStatusViewModel> Profiles { get; } = new ObservableCollection<ProfileStatusViewModel>();
    private readonly Dictionary<Guid, ProfileStatusViewModel> _profileStatusViewModelsById = new Dictionary<Guid, ProfileStatusViewModel>();
    private readonly ICalDavSynchronizerCommands _calDavSynchronizerCommands;

    public ProfileStatusesViewModel (ICalDavSynchronizerCommands calDavSynchronizerCommands)
    {
      if (calDavSynchronizerCommands == null)
        throw new ArgumentNullException (nameof (calDavSynchronizerCommands));

      _calDavSynchronizerCommands = calDavSynchronizerCommands;
      _timer = new Timer();
      _timer.Tick += delegate
      {
        foreach (var profileStatusViewModel in Profiles)
          profileStatusViewModel.RecalculateLastRunAgoInMinutes();
      };
      _timer.Interval = 50 * 1000;
      _timer.Enabled = true;
    }

    ProfileStatusViewModel GetOrCreateProfileStatusViewModel (Guid profileId)
    {
      ProfileStatusViewModel profileStatusViewModel;
      if (!_profileStatusViewModelsById.TryGetValue (profileId, out profileStatusViewModel))
      {
        profileStatusViewModel = new ProfileStatusViewModel (profileId, _calDavSynchronizerCommands);
        Profiles.Add (profileStatusViewModel);
        _profileStatusViewModelsById.Add (profileId, profileStatusViewModel);
      }
      return profileStatusViewModel;
    }

    void RemoveIfAvailable (Guid profileId)
    {
      ProfileStatusViewModel profileStatusViewModel;
      if (_profileStatusViewModelsById.TryGetValue (profileId, out profileStatusViewModel))
      {
        Profiles.Remove (profileStatusViewModel);
        _profileStatusViewModelsById.Remove (profileId);
      }
    }


    public void EnsureProfilesDisplayed (Contracts.Options[] profiles)
    {
      HashSet<Guid> existingProfiles = new HashSet<Guid>();

      foreach (var profile in profiles)
      {
        existingProfiles.Add (profile.Id);
        var profileStatusViewModel = GetOrCreateProfileStatusViewModel (profile.Id);
        profileStatusViewModel.Update (profile);
      }

      foreach (var kv in _profileStatusViewModelsById)
      {
        if (!existingProfiles.Contains (kv.Key))
          Profiles.Remove (kv.Value);
      }
    }

    public void Update (SynchronizationReport report)
    {
      ProfileStatusViewModel profileStatusViewModel;
      if (_profileStatusViewModelsById.TryGetValue (report.ProfileId, out profileStatusViewModel))
      {
        profileStatusViewModel.Update (report);
      }
    }
    
    public static ProfileStatusesViewModel DesignInstance
    {
      get
      {
        var viewModel = new ProfileStatusesViewModel(NullCalDavSynchronizerCommands.Instance);

        viewModel.Profiles.Add (ProfileStatusViewModel.CreateDesignInstance ("Profile 1", null, null));
        viewModel.Profiles.Add (ProfileStatusViewModel.CreateDesignInstance ("Profile 2", SyncronizationRunResult.Ok, 7));
        viewModel.Profiles.Add (ProfileStatusViewModel.CreateDesignInstance ("Profile 3", SyncronizationRunResult.Warning, 8));
        viewModel.Profiles.Add (ProfileStatusViewModel.CreateDesignInstance ("Profile 4", SyncronizationRunResult.Error, 9));

        return viewModel;
      }
    }
  }
}