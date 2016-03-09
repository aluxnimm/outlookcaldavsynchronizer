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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.Views;
using CalDavSynchronizer.Ui.Reports;
using CalDavSynchronizer.Ui.Reports.ViewModels;
using CalDavSynchronizer.Ui.Reports.Views;
using CalDavSynchronizer.Ui.SystrayNotification.ViewModels;
using CalDavSynchronizer.Ui.SystrayNotification.Views;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui
{
  internal class UiService : IUiService
  {
    private readonly GenericElementHostWindow _profileStatusesWindow;

    public UiService (      ProfileStatusesViewModel viewModel)
    {
      if (viewModel == null)
        throw new ArgumentNullException (nameof (viewModel));
  
      var view = new ProfileStatusesView();
      view.DataContext = viewModel;
      _profileStatusesWindow = new GenericElementHostWindow();
      _profileStatusesWindow.Text = "Synchronization Status";
      _profileStatusesWindow.Icon = Resources.ApplicationIcon;
      _profileStatusesWindow.ShowIcon = true;
      _profileStatusesWindow.BackColor = SystemColors.Window;
      _profileStatusesWindow.Child = view;
      _profileStatusesWindow.Size = new Size (400, 300);
      _profileStatusesWindow.FormClosing += (sender, e) =>
      {
        e.Cancel = true;
        _profileStatusesWindow.Visible = false;
      };
    }


    public void Show (ReportsViewModel reportsViewModel)
    {
      var view = new ReportsView();
      view.DataContext = reportsViewModel;

      var window = new GenericElementHostWindow();

      window.Text = "Synchronization Reports";
      window.Icon = Resources.ApplicationIcon;
      window.ShowIcon = true;
      window.BackColor = SystemColors.Window;
      window.Child = view;
      window.Show();
      window.FormClosed += delegate { reportsViewModel.NotifyReportsClosed(); };

      reportsViewModel.RequiresBringToFront += delegate { window.BringToFront(); };

      SetWindowSize (window, 0.75);
    }

    public void ShowProfileStatusesWindow ()
    {
      if (_profileStatusesWindow.Visible)
        _profileStatusesWindow.BringToFront();
      else
        _profileStatusesWindow.Visible = true;
    }

    public bool ShowOptions (OptionsCollectionViewModel viewModel)
    {
      var window = new OptionsWindow();
      window.DataContext = viewModel;
      window.Icon = BitmapFrame.Create (new Uri ("pack://application:,,,/CalDavSynchronizer;component/Resources/ApplicationIcon.ico"));
      ElementHost.EnableModelessKeyboardInterop (window);

      viewModel.RequestBringIntoView += delegate
      {
        window.BringIntoView();
      };

      return window.ShowDialog().GetValueOrDefault (false);
    }

  

    private static void SetWindowSize (GenericElementHostWindow window, double ratioToCurrentScreensize)
    {
      var screenSize = Screen.FromControl (window).Bounds;
      window.Size = new Size (
          (int) (screenSize.Size.Width * ratioToCurrentScreensize),
          (int) (screenSize.Size.Height * ratioToCurrentScreensize));
    }
  }
}