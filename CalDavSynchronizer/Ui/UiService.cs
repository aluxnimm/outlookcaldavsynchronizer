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
      SetWindowSizeToQuarterOfScreenSize(window);
    }

    private static void SetWindowSizeToQuarterOfScreenSize (GenericElementHostWindow window)
    {
      var screenSize = Screen.FromControl (window).Bounds;
      window.Size = new System.Drawing.Size (screenSize.Size.Width / 2, screenSize.Size.Height / 2);
    }
  }
}