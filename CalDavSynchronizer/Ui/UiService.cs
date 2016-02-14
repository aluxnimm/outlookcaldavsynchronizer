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
using System.Windows.Forms;
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Ui.Reports;
using CalDavSynchronizer.Ui.Reports.ViewModels;
using CalDavSynchronizer.Ui.Reports.Views;

namespace CalDavSynchronizer.Ui
{
  internal class UiService : IUiService
  {
    public void Show (ReportsViewModel reportsViewModel)
    {
      var view = new ReportsView();
      view.DataContext = reportsViewModel;

      var window = new GenericElementHostWindow();

      window.Text = "Synchronization Reports";
      window.Child = view;
      window.Show();
      window.FormClosed += delegate { reportsViewModel.NotifyReportsClosed(); };

      reportsViewModel.RequiresBringToFront += delegate { window.BringToFront(); };

      SetWindowSize (window, 0.75);
    }

    private static void SetWindowSize (GenericElementHostWindow window, double ratioToCurrentScreensize)
    {
      var screenSize = Screen.FromControl (window).Bounds;
      window.Size = new Size (
        (int)(screenSize.Size.Width * ratioToCurrentScreensize), 
        (int)( screenSize.Size.Height * ratioToCurrentScreensize));
    }
  }
}