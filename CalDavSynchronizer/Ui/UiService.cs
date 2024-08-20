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
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.Views;
using CalDavSynchronizer.Ui.Reports;
using CalDavSynchronizer.Ui.Reports.ViewModels;
using CalDavSynchronizer.Ui.Reports.Views;
using CalDavSynchronizer.Ui.SystrayNotification.ViewModels;
using CalDavSynchronizer.Ui.SystrayNotification.Views;
using CalDavSynchronizer.Ui.ViewModels;
using CalDavSynchronizer.Ui.Views;
using GenSync.ProgressReport;
using Microsoft.Office.Interop.Outlook;
using Size = System.Drawing.Size;
using SystemColors = System.Drawing.SystemColors;

namespace CalDavSynchronizer.Ui
{
    internal class UiService : IUiService
    {
        private const string c_exportedProfilesFilesFilter = "CalDav Synchronizer profiles (*.cdsp)|*.cdsp";

        public void Show(TransientProfileStatusesViewModel viewModel)
        {
            if (viewModel == null) throw new ArgumentNullException(nameof(viewModel));

            var view = new ProfileStatusesView();
            view.DataContext = viewModel;
            var profileStatusesWindow = new GenericElementHostWindow();
            profileStatusesWindow.Text = Strings.Get($"Synchronization Status");
            profileStatusesWindow.Icon = Resources.ApplicationIcon;
            profileStatusesWindow.ShowIcon = true;
            profileStatusesWindow.BackColor = SystemColors.Window;
            profileStatusesWindow.Child = view;
            profileStatusesWindow.Size = new Size(400, 300);
            profileStatusesWindow.FormClosing += (sender, e) => { viewModel.OnViewClosing(); };
            viewModel.RequestingBringToFront += (sender, e) => { profileStatusesWindow.BringToFront(); };
            profileStatusesWindow.Show();
        }

        public void Show(ReportsViewModel reportsViewModel)
        {
            var view = new ReportsView();
            view.DataContext = reportsViewModel;

            var window = new GenericElementHostWindow();

            window.Text = Strings.Get($"Synchronization Reports");
            window.Icon = Resources.ApplicationIcon;
            window.ShowIcon = true;
            window.BackColor = SystemColors.Window;
            window.Child = view;
            window.Show();
            window.FormClosed += delegate { reportsViewModel.NotifyReportsClosed(); };

            reportsViewModel.RequiresBringToFront += delegate { window.BringToFront(); };

            SetWindowSize(window, 0.75);
        }

        public bool ShowGeneralOptions(GeneralOptionsViewModel generalOptionsViewModel)
        {
            var window = new GeneralOptionsWindow();
            window.DataContext = generalOptionsViewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            ElementHost.EnableModelessKeyboardInterop(window);
            return window.ShowDialog().GetValueOrDefault(false);
        }

        public bool ShowOptions(OptionsCollectionViewModel viewModel)
        {
            var window = new OptionsWindow();
            window.DataContext = viewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            ElementHost.EnableModelessKeyboardInterop(window);

            viewModel.RequestBringIntoView += delegate { window.BringIntoView(); };

            return window.ShowDialog().GetValueOrDefault(false);
        }

        public IProfileType QueryProfileType(IReadOnlyCollection<IProfileType> profileTypes)
        {
            var viewModel = new SelectProfileViewModel(profileTypes, this);

            var window = new SelectProfileWindow();
            window.DataContext = viewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            ElementHost.EnableModelessKeyboardInterop(window);

            if (window.ShowDialog() ?? false)
                return viewModel.SelectedProfile;
            else
                return null;
        }

        public void ShowErrorDialog(string errorMessage, string title)
        {
            System.Windows.MessageBox.Show(errorMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowOXInfoDialog()
        {
            var viewModel = new OXInfoDialogViewModel(this);

            var window = new OXInfoDialog();
            window.DataContext = viewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            ElementHost.EnableModelessKeyboardInterop(window);
            window.ShowDialog();
        }

        public string ShowSaveDialog(string title)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog {Title = title, Filter = c_exportedProfilesFilesFilter};
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public string ShowOpenDialog(string title)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog {Title = title, Filter = c_exportedProfilesFilesFilter};
            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        public void ShowReport(string title, string reportText)
        {
            var viewModel = new GenericReportViewModel();
            viewModel.Title = title;
            viewModel.ReportText = reportText;

            var window = new GenericReportWindow();
            window.DataContext = viewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            ElementHost.EnableModelessKeyboardInterop(window);
            window.ShowDialog();
        }

        private static void SetWindowSize(GenericElementHostWindow window, double ratioToCurrentScreensize)
        {
            var screenSize = Screen.FromControl(window).Bounds;
            window.Size = new Size(
                (int) (screenSize.Size.Width * ratioToCurrentScreensize),
                (int) (screenSize.Size.Height * ratioToCurrentScreensize));
        }

        public IProgressUi Create(int maxValue)
        {
            var window = new ProgressWindow();
            var viewModel = new ProgressViewModel();
            window.DataContext = viewModel;
            window.Icon = BitmapFrame.Create(new Uri("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
            viewModel.SetMaximun(maxValue);
            window.ShowActivated = false;
            window.Show();
            return viewModel;
        }
    }
}