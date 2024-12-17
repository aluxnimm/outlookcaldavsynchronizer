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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer
{
    public class LoadErrorComponentContainer : IComponentContainer
    {
        private readonly string _loadExceptionAsString;

        public LoadErrorComponentContainer(string loadExceptionAsString)
        {
            _loadExceptionAsString = loadExceptionAsString;
        }

        public Task ShowOptionsAsync(Guid? initialVisibleProfile = null)
        {
            ShowErrorMessage();
            return Task.FromResult(0);
        }

        public Task ShowGeneralOptionsAsync()
        {
            ShowErrorMessage();
            return Task.FromResult(0);
        }

        public void ShowAbout()
        {
            ShowErrorMessage();
        }

        public void ShowLatestSynchronizationReport(Guid profileId)
        {
            ShowErrorMessage();
        }

        public void ShowProfileStatuses()
        {
            ShowErrorMessage();
        }

        public void SynchronizeNowAsync()
        {
            ShowErrorMessage();
        }

        public void ShowReports()
        {
            ShowErrorMessage();
        }

        public void Dispose()
        {
        }

        public event EventHandler SynchronizationFailedWhileReportsFormWasNotVisible;
        public event EventHandler<SchedulerStatusEventArgs> StatusChanged;

        public Task InitializeSchedulerAndStartAsync()
        {
            return Task.FromResult(0);
        }

        public void SaveToolBarSettings(ToolbarSettings settings)
        {
        }

        public ToolbarSettings LoadToolBarSettings()
        {
            return ToolbarSettings.CreateDefault();
        }

        void ShowErrorMessage()
        {
            if (MessageBox.Show(Strings.Get($"CalDav Synchronizer failed to load due to the following exception: {Environment.NewLine}{_loadExceptionAsString}{Environment.NewLine}{Environment.NewLine}Do you want to open the log file ?"), ComponentContainer.MessageBoxTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                GeneralOptionsViewModel.ShowLogFileWithouWarning();
            }
        }

        void OnSynchronizationFailedWhileReportsFormWasNotVisible()
        {
            SynchronizationFailedWhileReportsFormWasNotVisible?.Invoke(this, EventArgs.Empty);
        }

        void OnStatusChanged(SchedulerStatusEventArgs e)
        {
            StatusChanged?.Invoke(this, e);
        }
    }
}