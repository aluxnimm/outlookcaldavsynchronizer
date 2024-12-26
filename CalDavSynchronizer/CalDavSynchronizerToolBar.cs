﻿// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer
{
    public class CalDavSynchronizerToolBar
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CommandBarButton _toolBarBtnSyncNow;

        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly CommandBarButton _toolBarBtnGeneralOptions;
        private readonly CommandBarButton _toolBarBtnOptions;
        private readonly CommandBarButton _toolBarBtnAboutMe;
        private readonly CommandBarButton _toolBarBtnReports;

        private readonly CommandBarButton _toolBarBtnStatus;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        private CommandBar _toolBar;

        public CalDavSynchronizerToolBar(Explorer explorer, object missing, bool wireClickEvents)
        {
            _toolBar = explorer.CommandBars.Add("CalDav Synchronizer", MsoBarPosition.msoBarTop, false, true);

            _toolBarBtnOptions = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnOptions.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnOptions.Caption = Strings.Get($"Synchronization Profiles");
            _toolBarBtnOptions.FaceId = 222; // builtin icon: hand hovering above a property list
            _toolBarBtnOptions.Tag = Strings.Get($"View or set CalDav Synchronization profiles");
            if (wireClickEvents)
                _toolBarBtnOptions.Click += ToolBarBtn_Options_OnClick;


            _toolBarBtnGeneralOptions = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnGeneralOptions.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnGeneralOptions.Caption = Strings.Get($"General Options");
            _toolBarBtnGeneralOptions.FaceId = 222; // builtin icon: hand hovering above a property list
            _toolBarBtnGeneralOptions.Tag = Strings.Get($"View or set CalDav Synchronizer general options");
            if (wireClickEvents)
                _toolBarBtnGeneralOptions.Click += ToolBarBtn_GeneralOptions_OnClick;

            _toolBarBtnSyncNow = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnSyncNow.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnSyncNow.Caption = Strings.Get($"Synchronize now");
            _toolBarBtnSyncNow.FaceId = 107; // builtin icon: lightning hovering above a calendar table
            _toolBarBtnSyncNow.Tag = Strings.Get($"Start a manual synchronization of all active profiles.");
            if (wireClickEvents)
                _toolBarBtnSyncNow.Click += ToolBarBtn_SyncNow_OnClick;

            _toolBarBtnAboutMe = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnAboutMe.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnAboutMe.Caption = Strings.Get($"About");
            _toolBarBtnAboutMe.FaceId = 487; // builtin icon: blue round sign with "i" letter
            _toolBarBtnAboutMe.Tag = Strings.Get($"About CalDav Synchronizer");
            if (wireClickEvents)
                _toolBarBtnAboutMe.Click += ToolBarBtn_About_OnClick;

            _toolBarBtnReports = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnReports.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnReports.Caption = Strings.Get($"Reports");
            _toolBarBtnReports.FaceId = 433; // builtin icon: statistics
            _toolBarBtnReports.Tag = Strings.Get($"Show reports of last sync runs.");
            if (wireClickEvents)
                _toolBarBtnReports.Click += ToolBarBtn_Reports_OnClick;

            _toolBarBtnStatus = (CommandBarButton) _toolBar.Controls.Add(1, missing, missing, missing, missing);
            _toolBarBtnStatus.Style = MsoButtonStyle.msoButtonIconAndCaption;
            _toolBarBtnStatus.Caption = Strings.Get($"Status");
            _toolBarBtnStatus.FaceId = 433; // builtin icon: statistics
            _toolBarBtnStatus.Tag = Strings.Get($"Show status of sync runs.");
            if (wireClickEvents)
                _toolBarBtnStatus.Click += ToolBarBtn_Status_OnClick;

            _toolBar.Visible = true;
        }

        private void ManualSynchronize()
        {
            try
            {
                _toolBarBtnSyncNow.Enabled = false;
                try
                {
                    ComponentContainer.EnsureSynchronizationContext();
                    ThisAddIn.ComponentContainer.SynchronizeNowAsync();
                }
                finally
                {
                    _toolBarBtnSyncNow.Enabled = true;
                }
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }

        private void ToolBarBtn_Options_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            ToolBarBtn_Options_OnClick();
        }

        private async void ToolBarBtn_Options_OnClick()
        {
            try
            {
                ComponentContainer.EnsureSynchronizationContext();
                await ThisAddIn.ComponentContainer.ShowOptionsAsync();
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }

        private void ToolBarBtn_GeneralOptions_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            ToolBarBtn_GeneralOptions_OnClick();
        }

        private async void ToolBarBtn_GeneralOptions_OnClick()
        {
            try
            {
                ComponentContainer.EnsureSynchronizationContext();
                await ThisAddIn.ComponentContainer.ShowGeneralOptionsAsync();
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }


        private void ToolBarBtn_SyncNow_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            ManualSynchronize();
        }

        private void ToolBarBtn_About_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            try
            {
                ComponentContainer.EnsureSynchronizationContext();
                ThisAddIn.ComponentContainer.ShowAbout();
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }

        private void ToolBarBtn_Reports_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            try
            {
                ComponentContainer.EnsureSynchronizationContext();
                ThisAddIn.ComponentContainer.ShowReports();
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }

        private void ToolBarBtn_Status_OnClick(CommandBarButton Ctrl, ref bool CancelDefault)
        {
            try
            {
                ComponentContainer.EnsureSynchronizationContext();
                ThisAddIn.ComponentContainer.ShowProfileStatuses();
            }
            catch (Exception x)
            {
                ExceptionHandler.Instance.DisplayException(x, s_logger);
            }
        }

        public ToolbarSettings Settings
        {
            get
            {
                return new ToolbarSettings
                {
                    Top = _toolBar.Top,
                    Left = _toolBar.Left,
                    Visible = _toolBar.Visible,
                    Position = _toolBar.Position,
                    RowIndex = _toolBar.RowIndex
                };
            }
            set
            {
                _toolBar.Position = value.Position;

                _toolBar.RowIndex = value.RowIndex;

                _toolBar.Top = value.Top;

                _toolBar.Left = value.Left;

                _toolBar.Visible = value.Visible;
            }
        }
    }
}