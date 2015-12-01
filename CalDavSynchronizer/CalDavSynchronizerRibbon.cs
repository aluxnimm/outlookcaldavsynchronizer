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
using System.Reflection;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizer
{
  public partial class CalDavSynchronizerRibbon
  {
    private void CalDavSynchronizerRibbon_Load (object sender, RibbonUIEventArgs e)
    {
    }

    private async void SynchronizeNowButton_Click (object sender, RibbonControlEventArgs e)
    {
      SynchronizeNowButton.Enabled = false;
      try
      {
        await ThisAddIn.ComponentContainer.SynchronizeNowNoThrow();
      }
      finally
      {
        SynchronizeNowButton.Enabled = true;
      }
    }

    private void OptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      ThisAddIn.ComponentContainer.ShowOptionsNoThrow();
    }

    private void AboutButton_Click (object sender, RibbonControlEventArgs e)
    {
      using (var aboutForm = new AboutForm())
      {
        aboutForm.ShowDialog();
      }
    }

    private void GeneralOptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      ThisAddIn.ComponentContainer.ShowGeneralOptionsNoThrow ();
    }
  }
}