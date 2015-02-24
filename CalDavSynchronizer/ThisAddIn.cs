// This file is Part of CalDavSynchronizer (https://sourceforge.net/projects/outlookcaldavsynchronizer/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Windows.Forms;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using log4net.Config;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;
using Office = Microsoft.Office.Core;

namespace CalDavSynchronizer
{
  public partial class ThisAddIn
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private Office.CommandBar _toolBar;
    private Office.CommandBarButton _optionsButton;
    private Office.CommandBarButton _syncNowButton;
    private Explorers _openExplorers;
    private Scheduler _scheduler;
    private IOptionsDataAccess _optionsDataAccess;

    private void ThisAddIn_Startup (object sender, EventArgs e)
    {
      try
      {
        XmlConfigurator.Configure();

        s_logger.Info ("Startup...");

        _openExplorers = Application.Explorers;
        _openExplorers.NewExplorer += OpenExplorers_NewExplorer;
        AddToolbarToActiveExplorer();

        _optionsDataAccess = new OptionsDataAccess();
        _scheduler = new Scheduler (Application.Session);
        _scheduler.SetOptions (_optionsDataAccess.LoadOptions());
      }
      catch (Exception x)
      {
        s_logger.Error (string.Empty, x);
        throw;
      }

      s_logger.Info ("Startup finnished");
    }


    private void Synchronize ()
    {
      try
      {
        s_logger.Info ("Synchronization manually triggered");
        _scheduler.RunNow();
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void OpenExplorers_NewExplorer (Explorer new_Explorer)
    {
      ((_Explorer) new_Explorer).Activate();
      _toolBar = null;
      AddToolbarToActiveExplorer();
    }


    private void AddToolbarToActiveExplorer ()
    {
      if (_toolBar == null)
      {
        Office.CommandBars cmdBars = Application.ActiveExplorer().CommandBars;
        _toolBar = cmdBars.Add ("CalDav Synchronizer", Office.MsoBarPosition.msoBarTop, false, true);
      }
      try
      {
        Office.CommandBarButton button_1 = (Office.CommandBarButton) _toolBar.Controls.Add (1, missing, missing, missing, missing);
        button_1.Style = Office.MsoButtonStyle.msoButtonCaption;
        button_1.Caption = "Options";
        button_1.Tag = "Options";
        if (_optionsButton == null)
        {
          _optionsButton = button_1;
          _optionsButton.Click += OptionsButton_Click;
        }

        Office.CommandBarButton button_2 = (Office.CommandBarButton) _toolBar.Controls.Add (1, missing, missing, missing, missing);
        button_2.Style = Office.MsoButtonStyle.msoButtonCaption;
        button_2.Caption = "Synchronize now";
        button_2.Tag = "Synchronize now";
        _toolBar.Visible = true;
        if (_syncNowButton == null)
        {
          _syncNowButton = button_2;
          _syncNowButton.Click += SyncNowButton_Click;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show (ex.Message);
      }
    }

    private void SyncNowButton_Click (Office.CommandBarButton Ctrl, ref bool CancelDefault)
    {
      Synchronize();
    }

    private void OptionsButton_Click (Office.CommandBarButton Ctrl, ref bool CancelDefault)
    {
      try
      {
        var options = _optionsDataAccess.LoadOptions();
        if (OptionsForm.EditOptions (Application.Session, options, out options))
        {
          _optionsDataAccess.SaveOptions (options);
          _scheduler.SetOptions (options);
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void ThisAddIn_Shutdown (object sender, EventArgs e)
    {
    }

    #region VSTO generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InternalStartup ()
    {
      Startup += ThisAddIn_Startup;
      Shutdown += ThisAddIn_Shutdown;
    }

    #endregion
  }
}