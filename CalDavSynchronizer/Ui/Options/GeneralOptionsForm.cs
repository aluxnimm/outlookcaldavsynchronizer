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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using log4net;
using log4net.Appender;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class GeneralOptionsForm : Form
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    private readonly IList<Item<ReportLogMode>> _availableReportLogModes = new List<Item<ReportLogMode>>()
                                                                           {
                                                                               new Item<ReportLogMode> (ReportLogMode.OnlyWithErrors, "Only sync runs with errors"),
                                                                               new Item<ReportLogMode> (ReportLogMode.WarningsOrErrors, "Sync runs with errors or warnings"),
                                                                               new Item<ReportLogMode> (ReportLogMode.All, "All sync runs")
                                                                           };

    private readonly IList<Item<ReportPopupMode>> _availableReportPopupModes = new List<Item<ReportPopupMode>>()
                                                                               {
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.NoPopup, "No"),
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.JustErrors, "Just errors"),
                                                                                   new Item<ReportPopupMode> (ReportPopupMode.WarningsAndErrors, "Errors and warnings")
                                                                               };

    private readonly IList<Item<LogLevel>> _availableLogLevels = new List<Item<LogLevel>>()
                                                                               {
                                                                                   new Item<LogLevel> (LogLevel.Info, "Info"),
                                                                                   new Item<LogLevel> (LogLevel.Debug, "Debug")
                                                                               };

    private enum ReportLogMode
    {
      OnlyWithErrors,
      WarningsOrErrors,
      All
    }

    private enum ReportPopupMode
    {
      NoPopup,
      JustErrors,
      WarningsAndErrors
    }

    private enum LogLevel
    {
      Info,
      Debug
    }


    public GeneralOptionsForm ()
    {
      InitializeComponent();

      Item.BindComboBox (_reportLogModeComboBox, _availableReportLogModes);
      Item.BindComboBox (_reportPopupModeComboBox, _availableReportPopupModes);
      Item.BindComboBox (_logLevelComboBox, _availableLogLevels);
    }

    private void _okButton_Click (object sender, EventArgs e)
    {
      string errorMessage;
      if (Validate (out errorMessage))
      {
        DialogResult = DialogResult.OK;
      }
      else
      { 
        MessageBox.Show (errorMessage, "Some options contain invalid values", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public bool Display ()
    {
      return ShowDialog() == DialogResult.OK;
    }

    private bool Validate (out string errorMessage)
    {
      StringBuilder errorMessageBuilder = new StringBuilder();
      bool isValid = true;

      try
      {
        var timeout = int.Parse (_calDavConnectTimeoutTextBox.Text);
        if (timeout < 1)
        {
          errorMessageBuilder.AppendLine ("- CalDavConnectTimeout must be > 0");
          isValid = false;
        }
        else
        {
          var timespan = TimeSpan.FromSeconds(timeout);
        }
      }
      catch (Exception)
      {
        errorMessageBuilder.AppendLine ("- Invalid CalDavConnectTimeout");
        isValid = false;
      }
      try
      {
        var maxReportAge = int.Parse (_maxReportAgeInDays.Text);
        if (maxReportAge < 1)
        {
          errorMessageBuilder.AppendLine ("- Max Report Age must be > 0");
          isValid = false;
        }
      }
      catch (Exception)
      {
        errorMessageBuilder.AppendLine ("- Invalid Max Report Age");
        isValid = false;
      }

      errorMessage = errorMessageBuilder.ToString();
      return isValid;
    }

    public GeneralOptions Options
    {
      get
      {
        var reportLogMode = ((ReportLogMode) _reportLogModeComboBox.SelectedValue);
        var reportPopupMode = ((ReportPopupMode) _reportPopupModeComboBox.SelectedValue);
        var logLevel = ((LogLevel) _logLevelComboBox.SelectedValue);

        return new GeneralOptions
               {
                   ShouldCheckForNewerVersions = _checkForNewerVersionsCheckBox.Checked,
                   CheckIfOnline = _checkIfOnlineCheckBox.Checked,
                   StoreAppDataInRoamingFolder = _storeDataInRoamingFolderCheckBox.Checked,
                   DisableCertificateValidation = _disableCertificateValidationCheckbox.Checked,
                   EnableTls12 = _enableTls12Checkbox.Checked,
                   EnableSsl3 = _enableSsl3Checkbox.Checked,
                   CalDavConnectTimeout = TimeSpan.FromSeconds (int.Parse (_calDavConnectTimeoutTextBox.Text)),
                   FixInvalidSettings = _fixInvalidSettingsCheckBox.Checked,
                   IncludeCustomMessageClasses = _includeCustomMessageClassesCheckBox.Checked,
                   LogReportsWithoutWarningsOrErrors = reportLogMode == ReportLogMode.All,
                   LogReportsWithWarnings = reportLogMode == ReportLogMode.All || reportLogMode == ReportLogMode.WarningsOrErrors,
                   ShowReportsWithErrorsImmediately = reportPopupMode == ReportPopupMode.JustErrors || reportPopupMode == ReportPopupMode.WarningsAndErrors,
                   ShowReportsWithWarningsImmediately = reportPopupMode == ReportPopupMode.WarningsAndErrors,
                   MaxReportAgeInDays = int.Parse(_maxReportAgeInDays.Text),
                   EnableDebugLog = logLevel == LogLevel.Debug,
                   EnableTrayIcon = _enableTrayIconCheckBox.Checked,
                   AcceptInvalidCharsInServerResponse = _acceptInvalidCharsInServerResponseCheckBox.Checked,
                   TriggerSyncAfterSendReceive = _triggerSyncAfterSendReceiveCheckBox.Checked
        };
      }
      set
      {
        _checkForNewerVersionsCheckBox.Checked = value.ShouldCheckForNewerVersions;
        _checkIfOnlineCheckBox.Checked = value.CheckIfOnline;
        _storeDataInRoamingFolderCheckBox.Checked = value.StoreAppDataInRoamingFolder;
        _disableCertificateValidationCheckbox.Checked = value.DisableCertificateValidation;
        _enableTls12Checkbox.Checked = value.EnableTls12;
        _enableSsl3Checkbox.Checked = value.EnableSsl3;
        _calDavConnectTimeoutTextBox.Text = ((int) value.CalDavConnectTimeout.TotalSeconds).ToString();
        _fixInvalidSettingsCheckBox.Checked = value.FixInvalidSettings;
        _includeCustomMessageClassesCheckBox.Checked = value.IncludeCustomMessageClasses;
        _reportLogModeComboBox.SelectedValue = GetReportLogMode (value);
        _reportPopupModeComboBox.SelectedValue = GetReportPopupMode (value);
        _maxReportAgeInDays.Text = value.MaxReportAgeInDays.ToString();
        _logLevelComboBox.SelectedValue = value.EnableDebugLog ? LogLevel.Debug : LogLevel.Info;
        _enableTrayIconCheckBox.Checked = value.EnableTrayIcon;
        _acceptInvalidCharsInServerResponseCheckBox.Checked = value.AcceptInvalidCharsInServerResponse;
        _triggerSyncAfterSendReceiveCheckBox.Checked = value.TriggerSyncAfterSendReceive;
      }
    }

    private ReportLogMode GetReportLogMode (GeneralOptions options)
    {
      if (options.LogReportsWithoutWarningsOrErrors)
      {
        // HINT: althoug it is possible that in this case LogReportsWithWarnings is false, 
        // the UI doesnt offer that feature to just configure logging of reports without errors and warning 
        return ReportLogMode.All;
      }
      else if (options.LogReportsWithWarnings)
      {
        return ReportLogMode.WarningsOrErrors;
      }
      else
      {
        return ReportLogMode.OnlyWithErrors;
      }
    }

    private ReportPopupMode GetReportPopupMode (GeneralOptions options)
    {
      if (options.ShowReportsWithWarningsImmediately)
      {
        // HINT: althoug it is possible that in this case ShowReportsWithErrorsImmediately is false, 
        // the UI doesnt offer that feature to just show reports with warnings immediately
        return ReportPopupMode.WarningsAndErrors;
      }
      else if (options.ShowReportsWithErrorsImmediately)
      {
        return ReportPopupMode.JustErrors;
      }
      else
      {
        return ReportPopupMode.NoPopup;
      }
    }

    private void _showLogButton_Click (object sender, EventArgs e)
    {
      FileAppender fileAppender = s_logger.Logger.Repository
                 .GetAppenders().FirstOrDefault (appender => appender is FileAppender) as FileAppender;

      try
      {
        if (fileAppender != null && File.Exists (((FileAppender)fileAppender).File))
        {
          Process.Start (((FileAppender)fileAppender).File);
        }
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }

    private void _clearLogButton_Click (object sender, EventArgs e)
    {
      FileAppender fileAppender = s_logger.Logger.Repository
                 .GetAppenders().FirstOrDefault (appender => appender is FileAppender) as FileAppender;


      if (fileAppender != null && File.Exists (((FileAppender)fileAppender).File))
      {
        string path = ((FileAppender)fileAppender).File;

        FileStream fs = null;
        try
        {
          fs = new FileStream (path, FileMode.Create);
        }
        catch (Exception ex)
        {
          s_logger.Error ("Could not clear the log file!", ex);
        }
        finally
        {
          if (fs != null)
          {
            fs.Close();
          }

        }
      }
    }
  }
}