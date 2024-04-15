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
using System.IO;
using System.Linq;
using System.Windows.Input;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.ProfileTypes;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
    public class SelectProfileViewModel
    {
        public event EventHandler<CloseEventArgs> CloseRequested;
        private readonly IUiService _uiService;

        public SelectProfileViewModel(IReadOnlyCollection<IProfileType> profileTypes, IUiService uiService)
        {
            if (profileTypes == null) throw new ArgumentNullException(nameof(profileTypes));
            if (uiService == null) throw new ArgumentNullException(nameof(uiService));

            _uiService = uiService;
            ProfileTypes = profileTypes.Select(p => new ProfileViewModel(p)).ToList();
            ProfileTypes.First().IsSelected = true;

            OkCommand = new DelegateCommand(_ => Close(true));
            CancelCommand = new DelegateCommand(_ => Close(false));
        }

        public ICommand CancelCommand { get; }
        public ICommand OkCommand { get; }
        public IReadOnlyList<ProfileViewModel> ProfileTypes { get; }
        public IProfileType SelectedProfile { get; private set; }

        void Close(bool okPressed)
        {
            if (okPressed)
            {
                if (ProfileTypes.Count(p => p.IsSelected) != 1)
                {
                    _uiService.ShowErrorDialog(Strings.Get($"Please select exactly one option."), ComponentContainer.MessageBoxTitle);
                    return;
                }

                if (StringComparer.InvariantCultureIgnoreCase.Equals(ProfileTypes.Single(p => p.IsSelected).ProfileType.Name, "Open-Xchange"))
                {
                    _uiService.ShowOXInfoDialog();
                    return;
                }

                SelectedProfile = ProfileTypes.Single(p => p.IsSelected).ProfileType;
            }

            CloseRequested?.Invoke(this, new CloseEventArgs(okPressed));
        }

        public static SelectProfileViewModel DesignInstance => new SelectProfileViewModel(
            new[]
                {
                    "Generic CalDAV_CardDAV",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_iCloud.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_cozy.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_easyproject.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_fruux.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_gmx.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_google.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_landmarks.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_nextcloud.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_posteo.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_sarenet.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_sogo.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_yandex.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_mailbox.org.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_webde.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_smartermail.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_kolab.png",
                    "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_untermStrich.png"
                }
                .Select(u => new DesignProfileType(Path.GetFileName(u), u))
                .ToArray(),
            NullUiService.Instance);
    }
}