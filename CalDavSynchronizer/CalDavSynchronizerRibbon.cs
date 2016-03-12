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
using CalDavSynchronizer.Properties;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Tools.Ribbon;

namespace CalDavSynchronizer
{
  public partial class CalDavSynchronizerRibbon
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private void CalDavSynchronizerRibbon_Load (object sender, RibbonUIEventArgs e)
    {
      ThisAddIn.SynchronizationFailedWhileReportsFormWasNotVisible += SynchronizationFailedWhileReportsFormWasNotVisible;
    }

    private void SynchronizationFailedWhileReportsFormWasNotVisible (object sender, EventArgs e)
    {
      ReportsButton.Image = Resources.SyncError;
    }

    private async void SynchronizeNowButton_ClickAsync (object sender, RibbonControlEventArgs e)
    {
      try
      {
        SynchronizeNowButton.Enabled = false;
        try
        {
          await ThisAddIn.ComponentContainer.SynchronizeNow();
        }
        finally
        {
          SynchronizeNowButton.Enabled = true;
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void OptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        ThisAddIn.ComponentContainer.ShowOptions();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void AboutButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        ThisAddIn.ComponentContainer.ShowAbout();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void GeneralOptionsButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        ThisAddIn.ComponentContainer.ShowGeneralOptions();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void ReportsButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        ReportsButton.Image = Resources.report;
        ThisAddIn.ComponentContainer.ShowReports();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private void StatusesButton_Click (object sender, RibbonControlEventArgs e)
    {
      try
      {
        ThisAddIn.ComponentContainer.ShowProfileStatuses();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }
  }
}