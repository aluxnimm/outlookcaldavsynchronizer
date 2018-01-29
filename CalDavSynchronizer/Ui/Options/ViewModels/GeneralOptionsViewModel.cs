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
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using log4net;
using log4net.Appender;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class GeneralOptionsViewModel : ModelBase
  {
    private static readonly ILog s_logger = LogManager.GetLogger(MethodInfo.GetCurrentMethod().DeclaringType);

    public event EventHandler<CloseEventArgs> CloseRequested;

    public GeneralOptionsViewModel()
    {
      OkCommand = new DelegateCommand(_ => Close(true));
      CancelCommand = new DelegateCommand(_ => Close(false));
      ClearLogCommand = new DelegateCommand(_ => ClearLogFile());
      ShowLogCommand = new DelegateCommand(_ => ShowLogFile());
    }

    public IList<Item<ReportLogMode>> AvailableReportLogModes { get; } = new List<Item<ReportLogMode>>
    {
      new Item<ReportLogMode>(ReportLogMode.OnlyWithErrors, "Only sync runs with errors"),
      new Item<ReportLogMode>(ReportLogMode.WarningsOrErrors, "Sync runs with errors or warnings"),
      new Item<ReportLogMode>(ReportLogMode.All, "All sync runs")
    };

    public IList<Item<ReportPopupMode>> AvailableReportPopupModes { get; } = new List<Item<ReportPopupMode>>
    {
      new Item<ReportPopupMode>(ReportPopupMode.NoPopup, "No"),
      new Item<ReportPopupMode>(ReportPopupMode.JustErrors, "Just errors"),
      new Item<ReportPopupMode>(ReportPopupMode.WarningsAndErrors, "Errors and warnings")
    };

    public IList<Item<LogLevel>> AvailableLogLevels { get; } = new List<Item<LogLevel>>
    {
      new Item<LogLevel>(LogLevel.Info, "Info"),
      new Item<LogLevel>(LogLevel.Debug, "Debug")
    };

    public ICommand CancelCommand { get; }
    public ICommand OkCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand ShowLogCommand { get; }
    public GeneralOptions Options { get; set; }

    private void Close(bool shouldSaveNewOptions)
    {
      if (shouldSaveNewOptions)
      {
        string errorMessage;
        if (!Validate(out errorMessage))
        {
          MessageBox.Show(errorMessage, Strings.Get($"Some options contain invalid values"), MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
      }

      CloseRequested?.Invoke(this, new CloseEventArgs(shouldSaveNewOptions));
    }

    private bool Validate(out string errorMessage)
    {
      StringBuilder errorMessageBuilder = new StringBuilder();
      bool isValid = true;


      if (Options.CalDavConnectTimeout <= TimeSpan.Zero)
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- CalDavConnectTimeout must be > 0"));
        isValid = false;
      }
      
      if (Options.MaxReportAgeInDays < 1)
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- Max Report Age must be > 0"));
        isValid = false;
      }
      
      errorMessage = errorMessageBuilder.ToString();
      return isValid;
    }

    public LogLevel LogLevelValue
    {
      get => Options.EnableDebugLog ? LogLevel.Debug : LogLevel.Info;
      set => Options.EnableDebugLog = value == LogLevel.Debug;
    }

    public int CalDavConnectTimeoutInSecs
    {
      get => (int)Options.CalDavConnectTimeout.TotalSeconds;
      set => Options.CalDavConnectTimeout = TimeSpan.FromSeconds(value);
    }

    public ReportLogMode ReportLogModeValue
    {
      get
      {
        if (Options.LogReportsWithoutWarningsOrErrors)
        {
          // HINT: althoug it is possible that in this case LogReportsWithWarnings is false, 
          // the UI doesnt offer that feature to just configure logging of reports without errors and warning 
          return ReportLogMode.All;
        }
        else if (Options.LogReportsWithWarnings)
        {
          return ReportLogMode.WarningsOrErrors;
        }
        else
        {
          return ReportLogMode.OnlyWithErrors;
        }
      }
      set
      {
        Options.LogReportsWithoutWarningsOrErrors = value == ReportLogMode.All;
        Options.LogReportsWithWarnings = value == ReportLogMode.All || value == ReportLogMode.WarningsOrErrors;
      }
    }

    public ReportPopupMode ReportPopupModeValue
    {
      get
      {
        if (Options.ShowReportsWithWarningsImmediately)
        {
          // HINT: althoug it is possible that in this case ShowReportsWithErrorsImmediately is false, 
          // the UI doesnt offer that feature to just show reports with warnings immediately
          return ReportPopupMode.WarningsAndErrors;
        }
        else if (Options.ShowReportsWithErrorsImmediately)
        {
          return ReportPopupMode.JustErrors;
        }
        else
        {
          return ReportPopupMode.NoPopup;
        }
      }
      set
      {
        Options.ShowReportsWithErrorsImmediately = value == ReportPopupMode.JustErrors || value == ReportPopupMode.WarningsAndErrors;
        Options.ShowReportsWithWarningsImmediately = value == ReportPopupMode.WarningsAndErrors;
      }
    }

    private void ShowLogFile()
    {
      MessageBox.Show (
          Strings.Get($"Please be aware that the log file (especially with Log Level Debug) can contain 'Personally identifiable information' (PII) like resource URIs and raw calendar and/or addressbook data but no passwords.\nNever post or attach the log or a sync report in a public forum and mask sensitive data when sending as an e-mail!"),
          ComponentContainer.MessageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning
        );
      ShowLogFileWithouWarning();
    }

    public static void ShowLogFileWithouWarning()
    {
      FileAppender fileAppender = s_logger.Logger.Repository
        .GetAppenders()
        .FirstOrDefault(appender => appender is FileAppender) as FileAppender;

      try
      {
        if (fileAppender != null && File.Exists(((FileAppender) fileAppender).File))
        {
          Process.Start(((FileAppender) fileAppender).File);
        }
      }
      catch (Exception x)
      {
        s_logger.Error(null, x);
      }
    }

    private void ClearLogFile()
    {
      FileAppender fileAppender = s_logger.Logger.Repository
        .GetAppenders()
        .FirstOrDefault(appender => appender is FileAppender) as FileAppender;

      if (fileAppender != null && File.Exists(((FileAppender) fileAppender).File))
      {
        string path = ((FileAppender) fileAppender).File;

        FileStream fs = null;
        try
        {
          fs = new FileStream(path, FileMode.Create);
        }
        catch (Exception ex)
        {
          s_logger.Error("Could not clear the log file!", ex);
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

    public enum ReportLogMode
    {
      OnlyWithErrors,
      WarningsOrErrors,
      All
    }

    public enum ReportPopupMode
    {
      NoPopup,
      JustErrors,
      WarningsAndErrors
    }

    public enum LogLevel
    {
      Info,
      Debug
    }

    public static GeneralOptionsViewModel DesignInstance = new GeneralOptionsViewModel
    {
      Options = new GeneralOptions
      {
        AcceptInvalidCharsInServerResponse = true,
        CalDavConnectTimeout = TimeSpan.FromSeconds(90),
        CheckIfOnline = true,
        DisableCertificateValidation = true,
        EnableAdvancedView = true,
        EnableClientCertificate = true,
        EnableDebugLog = true,
        EnableSsl3 = true,
        EnableTls12 = true,
        EnableTrayIcon = true,
        ExpandAllSyncProfiles = true,
        FixInvalidSettings = true,
        IncludeCustomMessageClasses = true,
        IncludeEntityReportsWithoutErrorsOrWarnings = true,
        LogEntityNames = true,
        LogReportsWithoutWarningsOrErrors = true,
        LogReportsWithWarnings = true,
        MaxReportAgeInDays = 111,
        MaxSucessiveWarnings = 222,
        QueryFoldersJustByGetTable = true,
        ShouldCheckForNewerVersions = true,
        ShowProgressBar = true,
        ShowReportsWithErrorsImmediately = true,
        ShowReportsWithWarningsImmediately = true,
        StoreAppDataInRoamingFolder = true,
        ThresholdForProgressDisplay = 333,
        TriggerSyncAfterSendReceive = true,
        UseUnsafeHeaderParsing = true
      }
    };
  }
}