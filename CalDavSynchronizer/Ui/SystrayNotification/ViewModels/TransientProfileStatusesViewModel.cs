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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GenSync.Logging;

namespace CalDavSynchronizer.Ui.SystrayNotification.ViewModels
{
    public class TransientProfileStatusesViewModel : ITransientProfileStatusesViewModel
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Timer _timer;

        public ObservableCollection<ProfileStatusViewModel> Profiles { get; } = new ObservableCollection<ProfileStatusViewModel>();
        private readonly Dictionary<Guid, ProfileStatusViewModel> _profileStatusViewModelsById = new Dictionary<Guid, ProfileStatusViewModel>();
        private readonly ICalDavSynchronizerCommands _calDavSynchronizerCommands;

        public event EventHandler Closing;
        public event EventHandler RequestingBringToFront;

        public TransientProfileStatusesViewModel(ICalDavSynchronizerCommands calDavSynchronizerCommands, Contracts.Options[] profiles)
        {
            if (calDavSynchronizerCommands == null)
                throw new ArgumentNullException(nameof(calDavSynchronizerCommands));

            _calDavSynchronizerCommands = calDavSynchronizerCommands;
            _timer = new Timer();
            _timer.Tick += delegate
            {
                foreach (var profileStatusViewModel in Profiles)
                    profileStatusViewModel.RecalculateLastRunAgo();
            };
            _timer.Interval = 5 * 1000;
            _timer.Enabled = true;
            NotifyProfilesChanged(profiles);
        }

        ProfileStatusViewModel GetOrCreateProfileStatusViewModel(Guid profileId)
        {
            ProfileStatusViewModel profileStatusViewModel;
            if (!_profileStatusViewModelsById.TryGetValue(profileId, out profileStatusViewModel))
            {
                profileStatusViewModel = new ProfileStatusViewModel(profileId, _calDavSynchronizerCommands);
                Profiles.Add(profileStatusViewModel);
                _profileStatusViewModelsById.Add(profileId, profileStatusViewModel);
            }

            return profileStatusViewModel;
        }

        public void NotifyProfilesChanged(Contracts.Options[] profiles)
        {
            HashSet<Guid> existingProfiles = new HashSet<Guid>();

            foreach (var profile in profiles)
            {
                existingProfiles.Add(profile.Id);
                var profileStatusViewModel = GetOrCreateProfileStatusViewModel(profile.Id);
                profileStatusViewModel.Update(profile);
            }

            foreach (var kv in _profileStatusViewModelsById)
            {
                if (!existingProfiles.Contains(kv.Key))
                    Profiles.Remove(kv.Value);
            }
        }

        public void Update(Guid profileId, SynchronizationRunSummary summary)
        {
            ProfileStatusViewModel profileStatusViewModel;
            if (_profileStatusViewModelsById.TryGetValue(profileId, out profileStatusViewModel))
            {
                profileStatusViewModel.Update(summary);
            }
        }

        public static TransientProfileStatusesViewModel DesignInstance
        {
            get
            {
                var viewModel = new TransientProfileStatusesViewModel(NullCalDavSynchronizerCommands.Instance, new Contracts.Options[0]);

                viewModel.Profiles.Add(ProfileStatusViewModel.CreateDesignInstance("Profile 1", null, null));
                viewModel.Profiles.Add(ProfileStatusViewModel.CreateDesignInstance("Profile 2", SyncronizationRunResult.Ok, TimeSpan.FromMinutes(7)));
                viewModel.Profiles.Add(ProfileStatusViewModel.CreateDesignInstance("Profile 3", SyncronizationRunResult.Warning, TimeSpan.FromMinutes(839)));
                viewModel.Profiles.Add(ProfileStatusViewModel.CreateDesignInstance("Profile 4", SyncronizationRunResult.Error, TimeSpan.FromMinutes(93)));

                return viewModel;
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
            Closing = null;
            RequestingBringToFront = null;
        }

        public void OnViewClosing()
        {
            Closing?.Invoke(this, EventArgs.Empty);
        }

        public void BringToFront()
        {
            RequestingBringToFront?.Invoke(this, EventArgs.Empty);
        }
    }
}