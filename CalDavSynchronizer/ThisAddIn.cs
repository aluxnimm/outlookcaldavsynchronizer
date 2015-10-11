// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using System.Windows.Forms;
using Microsoft.Office.Core;
using Microsoft.Office.Tools.Ribbon;
using Microsoft.Office.Interop.Outlook;
using CalDavSynchronizer.Ui;

namespace CalDavSynchronizer
{
  public partial class ThisAddIn
  {
    private CommandBar       _toolBar           = null; // Pierre-Marie Baty -- only for Outlook 2007
    private CommandBarButton _toolBarBtnOptions = null;
    private CommandBarButton _toolBarBtnSyncNow = null;
    private CommandBarButton _toolBarBtnAboutMe = null;

    public static ComponentContainer ComponentContainer { get; private set; }

    private void ThisAddIn_Startup (object sender, EventArgs e)
    {
      ComponentContainer.ConfigureServicePointManager();
      ComponentContainer = new ComponentContainer (Application.Session);

      // Pierre-Marie Baty -- test Outlook version. If major version < 14 (Outlook 2010), we need a toolbar.
      if (Convert.ToInt32(Globals.ThisAddIn.Application.Version.Split(new char[] { '.' })[0]) < 14)
      {
        try // We're in Outlook < 2010. Create a toolbar with 3 buttons...
        {
          _toolBar = Application.ActiveExplorer().CommandBars.Add("CalDav Synchronizer", MsoBarPosition.msoBarTop, false, true);
          _toolBarBtnOptions = (CommandBarButton)_toolBar.Controls.Add(1, missing, missing, missing, missing);
          _toolBarBtnOptions.Style = MsoButtonStyle.msoButtonIconAndCaption;
          _toolBarBtnOptions.Caption = "Options";
          _toolBarBtnOptions.FaceId = 222; // builtin icon: hand hovering above a property list
          _toolBarBtnOptions.Tag = "View or set CalDav Synchronizer options";
          _toolBarBtnOptions.Click += ToolBarBtn_Options_OnClick;
          _toolBarBtnSyncNow = (CommandBarButton)_toolBar.Controls.Add(1, missing, missing, missing, missing);
          _toolBarBtnSyncNow.Style = MsoButtonStyle.msoButtonIconAndCaption;
          _toolBarBtnSyncNow.Caption = "Synchronize";
          _toolBarBtnSyncNow.FaceId = 107; // builtin icon: lightning hovering above a calendar table
          _toolBarBtnSyncNow.Tag = "Synchronize now";
          _toolBarBtnSyncNow.Click += ToolBarBtn_SyncNow_OnClick;
          _toolBarBtnAboutMe = (CommandBarButton)_toolBar.Controls.Add(1, missing, missing, missing, missing);
          _toolBarBtnAboutMe.Style = MsoButtonStyle.msoButtonIconAndCaption;
          _toolBarBtnAboutMe.Caption = "About";
          _toolBarBtnAboutMe.FaceId = 487; // builtin icon: blue round sign with "i" letter
          _toolBarBtnAboutMe.Tag = "About CalDav Synchronizer";
          _toolBarBtnAboutMe.Click += ToolBarBtn_About_OnClick;
          _toolBar.Visible = true; // once the toolbar is populated with its buttons, show it
        }
        catch (System.Exception ex)
        {
          MessageBox.Show(ex.Message); // if unable to create toolbar with buttons, display at least a messagebox telling why
        }
      }
    }

    private async void ManualSynchronize()
    {
      _toolBarBtnSyncNow.Enabled = false; // first, disable the "sync now" button
      try { await ComponentContainer.SynchronizeNowNoThrow(); } // trigger a sync and wait for it to finish...
      finally { _toolBarBtnSyncNow.Enabled = true; } // ...when it's done, enable the "sync now" button back
    }

    private void ToolBarBtn_Options_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
    {
      ComponentContainer.ShowOptionsNoThrow(); // fire up the Options dialog box
    }

    private void ToolBarBtn_SyncNow_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
    {
      ManualSynchronize(); // call an async function (that must mean "threaded" in C# language) so that the UI is always responsive
    }

    private void ToolBarBtn_About_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
    {
      using (var aboutForm = new AboutForm())
      {
        aboutForm.ShowDialog();
      }
    }

    private void ThisAddIn_Shutdown (object sender, EventArgs e)
    {
    }

    protected override IRibbonExtension[] CreateRibbonObjects ()
    {
      return new IRibbonExtension[] { new CalDavSynchronizerRibbon() };
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
