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
using CalDavSynchronizer.Ui;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer
{
  internal class CalDavSynchronizerToolBar
  {
    private readonly CommandBarButton _toolBarBtnSyncNow;
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    private readonly CommandBarButton _toolBarBtnGeneralOptions;
    private readonly CommandBarButton _toolBarBtnOptions;
    private readonly CommandBarButton _toolBarBtnAboutMe;
    // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

    private readonly ComponentContainer _componentContainer;

    public CalDavSynchronizerToolBar (Application application, ComponentContainer componentContainer, object missing)
    {
      _componentContainer = componentContainer;

      var toolBar = application.ActiveExplorer().CommandBars.Add ("CalDav Synchronizer", MsoBarPosition.msoBarTop, false, true);

      _toolBarBtnOptions = (CommandBarButton) toolBar.Controls.Add (1, missing, missing, missing, missing);
      _toolBarBtnOptions.Style = MsoButtonStyle.msoButtonIconAndCaption;
      _toolBarBtnOptions.Caption = "Synchronization Profiles";
      _toolBarBtnOptions.FaceId = 222; // builtin icon: hand hovering above a property list
      _toolBarBtnOptions.Tag = "View or set CalDav Synchronization Profiles";
      _toolBarBtnOptions.Click += ToolBarBtn_Options_OnClick;


      _toolBarBtnGeneralOptions = (CommandBarButton) toolBar.Controls.Add (1, missing, missing, missing, missing);
      _toolBarBtnGeneralOptions.Style = MsoButtonStyle.msoButtonIconAndCaption;
      _toolBarBtnGeneralOptions.Caption = "General Options";
      _toolBarBtnGeneralOptions.FaceId = 222; // builtin icon: hand hovering above a property list
      _toolBarBtnGeneralOptions.Tag = "View or set CalDav Synchronizer general options";
      _toolBarBtnGeneralOptions.Click += ToolBarBtn_GeneralOptions_OnClick;

      _toolBarBtnSyncNow = (CommandBarButton) toolBar.Controls.Add (1, missing, missing, missing, missing);
      _toolBarBtnSyncNow.Style = MsoButtonStyle.msoButtonIconAndCaption;
      _toolBarBtnSyncNow.Caption = "Synchronize";
      _toolBarBtnSyncNow.FaceId = 107; // builtin icon: lightning hovering above a calendar table
      _toolBarBtnSyncNow.Tag = "Synchronize now";
      _toolBarBtnSyncNow.Click += ToolBarBtn_SyncNow_OnClick;

      _toolBarBtnAboutMe = (CommandBarButton) toolBar.Controls.Add (1, missing, missing, missing, missing);
      _toolBarBtnAboutMe.Style = MsoButtonStyle.msoButtonIconAndCaption;
      _toolBarBtnAboutMe.Caption = "About";
      _toolBarBtnAboutMe.FaceId = 487; // builtin icon: blue round sign with "i" letter
      _toolBarBtnAboutMe.Tag = "About CalDav Synchronizer";
      _toolBarBtnAboutMe.Click += ToolBarBtn_About_OnClick;

      toolBar.Visible = true;
    }

    private async void ManualSynchronize ()
    {
      _toolBarBtnSyncNow.Enabled = false;
      try
      {
        await _componentContainer.SynchronizeNowNoThrow();
      }
      finally
      {
        _toolBarBtnSyncNow.Enabled = true;
      }
    }

    private void ToolBarBtn_Options_OnClick (CommandBarButton Ctrl, ref bool CancelDefault)
    {
      _componentContainer.ShowOptionsNoThrow();
    }

    private void ToolBarBtn_GeneralOptions_OnClick (CommandBarButton Ctrl, ref bool CancelDefault)
    {
      _componentContainer.ShowGeneralOptionsNoThrow ();
    }

    private void ToolBarBtn_SyncNow_OnClick (CommandBarButton Ctrl, ref bool CancelDefault)
    {
      ManualSynchronize();
    }

    private void ToolBarBtn_About_OnClick (CommandBarButton Ctrl, ref bool CancelDefault)
    {
      using (var aboutForm = new AboutForm())
      {
        aboutForm.ShowDialog();
      }
    }
  }
}