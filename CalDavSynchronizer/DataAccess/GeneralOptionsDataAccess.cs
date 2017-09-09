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
using System.IO;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using Microsoft.Win32;
using System.Configuration;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Core;
using Microsoft.Office.Core;

namespace CalDavSynchronizer.DataAccess
{
  public class GeneralOptionsDataAccess : IGeneralOptionsDataAccess
  {
    private const string ValueNameShouldCheckForNewerVersions = "CheckForNewerVersions";
    private const string ValueNameCheckIfOnline = "CheckIfOnline";
    private const string ValueNameStoreAppDataInRoamingFolder = "StoreAppDataInRoamingFolder";
    private const string ValueNameDisableCertificateValidation = "DisableCertificateValidation";
    private const string ValueNameEnableClientCertificate = "EnableClientCertificate";
    private const string ValueNameEnableTls12 = "EnableTls12";
    private const string ValueNameEnableSsl3 = "EnableSsl3";
    private const string ValueNameCalDavConnectTimeout = "CalDavConnectTimeout";
    private const string ValueNameFixInvalidSettings = "FixInvalidSettings";
    private const string ValueNameOptionsRegistryKey = @"Software\CalDavSynchronizer";
    private const string ValueNameIncludeCustomMessageClasses = "IncludeCustomMessageClasses";
    private const string ValueNameIncludeEntityReportsWithoutErrorsOrWarnings = "LogAllEntitySyncReports";
    private const string ValueNameLogEntityNames = "LogEntityNames";
    
    private const string ValueNameQueryFoldersJustByGetTable = "QueryFoldersJustByGetTable";
    private const string ValueNameLogReportsWithoutWarningsOrErrors = "LogReportsWithoutWarningsOrErrors";
    private const string ValueNameLogReportsWithWarnings = "LogReportsWithWarnings";
    private const string ValueNameShowReportsWithWarningsImmediately = "ShowReportsWithWarningsImmediately";
    private const string ValueNameShowReportsWithErrorsImmediately = "ShowReportsWithErrorsImmediately";
    private const string ValueNameMaxReportAgeInDays = "MaxReportAgeInDays";

    private const string ValueNameEnableDebugLog = "EnableDebugLog";
    private const string ValueNameEnableTrayIcon = "EnableTrayIcon";
    private const string ValueNameEntityCacheVersion = "EntityCacheVersion";
    private const string ValueNameAcceptInvalidCharsInServerResponse = "AcceptInvalidCharsInServerResponse";
    private const string ValueNameUseUnsafeHeaderParsing = "UseUnsafeHeaderParsing";
    private const string ValueNameTriggerSyncAfterSendReceive = "TriggerSyncAfterSendReceive";
    private const string ValueNameExpandAllSyncProfiles = "ExpandAllSyncProfiles";
    private const string ValueNameEnableAdvancedView = "EnableAdvancedView";
    private const string ValueNameToolbarSettings = "ToolbarSettings";

    private const string ValueNameShowProgressBar = "ShowProgressBar";
    private const string ValueNameThresholdForProgressDisplay = "ThresholdForProgressDisplay";
    private const string ValueNameMaxSucessiveWarnings = "MaxSucessiveWarnings";

    public GeneralOptions LoadOptions ()
    {
      using (var key = OpenOptionsKey())
      {
        int debugEnabledInConfig = ((Hierarchy) LogManager.GetRepository()).Root.Level == Level.Debug ? 1 : 0;

        return new GeneralOptions()
               {
                   ShouldCheckForNewerVersions = (int) (key.GetValue (ValueNameShouldCheckForNewerVersions) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["checkForNewerVersions"] ?? bool.TrueString))) != 0,
                   CheckIfOnline = (int) (key.GetValue (ValueNameCheckIfOnline) ?? 1) != 0,
                   StoreAppDataInRoamingFolder = (int) (key.GetValue (ValueNameStoreAppDataInRoamingFolder) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["storeAppDataInRoamingFolder"] ?? bool.FalseString))) != 0,
                   DisableCertificateValidation = (int) (key.GetValue (ValueNameDisableCertificateValidation) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["disableCertificateValidation"] ?? bool.FalseString))) != 0,
                   EnableClientCertificate = (int) (key.GetValue (ValueNameEnableClientCertificate) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["enableClientCertificate"] ?? bool.FalseString))) != 0,
                   EnableTls12 = (int) (key.GetValue (ValueNameEnableTls12) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["enableTls12"] ?? bool.TrueString))) != 0,
                   EnableSsl3 = (int) (key.GetValue (ValueNameEnableSsl3) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["enableSsl3"] ?? bool.FalseString))) != 0,
                   CalDavConnectTimeout = TimeSpan.Parse ((string)(key.GetValue (ValueNameCalDavConnectTimeout) ?? ConfigurationManager.AppSettings["caldavConnectTimeout"] ?? "01:30")),
                   FixInvalidSettings = (int) (key.GetValue (ValueNameFixInvalidSettings) ?? 1) != 0,
                   IncludeCustomMessageClasses = (int) (key.GetValue (ValueNameIncludeCustomMessageClasses) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["includeCustomMessageClasses"] ?? bool.FalseString))) != 0,
                   LogReportsWithoutWarningsOrErrors = (int) (key.GetValue (ValueNameLogReportsWithoutWarningsOrErrors) ?? 0) != 0,
                   IncludeEntityReportsWithoutErrorsOrWarnings = (int)(key.GetValue (ValueNameIncludeEntityReportsWithoutErrorsOrWarnings) ?? 0) != 0 ,
                   LogEntityNames = (int)(key.GetValue (ValueNameLogEntityNames) ?? 0) != 0,
                   LogReportsWithWarnings = (int) (key.GetValue (ValueNameLogReportsWithWarnings) ?? 1) != 0,
                   ShowReportsWithWarningsImmediately = (int) (key.GetValue (ValueNameShowReportsWithWarningsImmediately) ?? 0) != 0,
                   ShowReportsWithErrorsImmediately = (int) (key.GetValue (ValueNameShowReportsWithErrorsImmediately) ?? 1) != 0,
                   MaxReportAgeInDays = (int) (key.GetValue (ValueNameMaxReportAgeInDays) ?? 1),
                   EnableDebugLog = (int) (key.GetValue (ValueNameEnableDebugLog) ?? debugEnabledInConfig) != 0,
                   EnableTrayIcon = (int) (key.GetValue (ValueNameEnableTrayIcon) ?? 1) != 0,
                   AcceptInvalidCharsInServerResponse = (int) (key.GetValue (ValueNameAcceptInvalidCharsInServerResponse) ?? 0) != 0,
                   UseUnsafeHeaderParsing = (int) (key.GetValue (ValueNameUseUnsafeHeaderParsing) ?? Convert.ToInt32 (SystemNetSettings.UseUnsafeHeaderParsing)) !=0,
                   TriggerSyncAfterSendReceive = (int) (key.GetValue (ValueNameTriggerSyncAfterSendReceive) ?? 0) != 0,
                   ExpandAllSyncProfiles = (int) (key.GetValue (ValueNameExpandAllSyncProfiles) ?? 1) != 0,
                   EnableAdvancedView = (int)(key.GetValue(ValueNameEnableAdvancedView) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["enableAdvancedView"] ?? bool.FalseString))) != 0,
                   QueryFoldersJustByGetTable = (int) (key.GetValue (ValueNameQueryFoldersJustByGetTable) ?? 1) != 0,
                   ShowProgressBar = (int) (key.GetValue (ValueNameShowProgressBar) ?? Convert.ToInt32 (Boolean.Parse (ConfigurationManager.AppSettings["showProgressBar"] ?? bool.TrueString))) != 0,
                   ThresholdForProgressDisplay = (int) (key.GetValue (ValueNameThresholdForProgressDisplay) ?? int.Parse(ConfigurationManager.AppSettings["loadOperationThresholdForProgressDisplay"] ?? "50")),
                   MaxSucessiveWarnings = (int) (key.GetValue (ValueNameMaxSucessiveWarnings) ?? 2)
        };
      }
    }

    public static bool WpfRenderModeSoftwareOnly => bool.Parse(ConfigurationManager.AppSettings["wpfRenderModeSoftwareOnly"] ?? bool.FalseString);

    public void SaveOptions (GeneralOptions options)
    {
      using (var key = OpenOptionsKey())
      {
        key.SetValue (ValueNameShouldCheckForNewerVersions, options.ShouldCheckForNewerVersions ? 1 : 0);
        key.SetValue (ValueNameCheckIfOnline, options.CheckIfOnline ? 1 : 0);
        key.SetValue (ValueNameStoreAppDataInRoamingFolder, options.StoreAppDataInRoamingFolder ? 1 : 0);
        key.SetValue (ValueNameDisableCertificateValidation, options.DisableCertificateValidation ? 1 : 0);
        key.SetValue (ValueNameEnableClientCertificate, options.EnableClientCertificate ? 1 : 0);
        key.SetValue (ValueNameEnableTls12, options.EnableTls12 ? 1 : 0);
        key.SetValue (ValueNameEnableSsl3, options.EnableSsl3 ? 1 : 0);
        key.SetValue (ValueNameCalDavConnectTimeout, options.CalDavConnectTimeout.ToString());
        key.SetValue (ValueNameFixInvalidSettings, options.FixInvalidSettings ? 1 : 0);
        key.SetValue (ValueNameIncludeCustomMessageClasses, options.IncludeCustomMessageClasses ? 1 : 0);
        key.SetValue (ValueNameLogReportsWithoutWarningsOrErrors, options.LogReportsWithoutWarningsOrErrors ? 1 : 0);
        key.SetValue (ValueNameIncludeEntityReportsWithoutErrorsOrWarnings, options.IncludeEntityReportsWithoutErrorsOrWarnings ? 1 : 0);
        key.SetValue (ValueNameLogEntityNames, options.LogEntityNames ? 1 : 0);
        key.SetValue (ValueNameLogReportsWithWarnings, options.LogReportsWithWarnings ? 1 : 0);
        key.SetValue (ValueNameShowReportsWithWarningsImmediately, options.ShowReportsWithWarningsImmediately ? 1 : 0);
        key.SetValue (ValueNameShowReportsWithErrorsImmediately, options.ShowReportsWithErrorsImmediately ? 1 : 0);
        key.SetValue (ValueNameMaxReportAgeInDays, options.MaxReportAgeInDays);
        key.SetValue (ValueNameEnableDebugLog, options.EnableDebugLog ? 1 : 0);
        key.SetValue (ValueNameEnableTrayIcon, options.EnableTrayIcon ? 1 : 0);
        key.SetValue (ValueNameAcceptInvalidCharsInServerResponse, options.AcceptInvalidCharsInServerResponse ? 1 : 0);
        key.SetValue (ValueNameUseUnsafeHeaderParsing, options.UseUnsafeHeaderParsing ? 1 : 0);
        key.SetValue (ValueNameTriggerSyncAfterSendReceive, options.TriggerSyncAfterSendReceive ? 1 : 0);
        key.SetValue (ValueNameExpandAllSyncProfiles, options.ExpandAllSyncProfiles ? 1 : 0);
        key.SetValue (ValueNameEnableAdvancedView, options.EnableAdvancedView ? 1 : 0);
        key.SetValue (ValueNameQueryFoldersJustByGetTable, options.QueryFoldersJustByGetTable ? 1 : 0);
        key.SetValue (ValueNameShowProgressBar, options.ShowProgressBar ? 1: 0);
        key.SetValue (ValueNameThresholdForProgressDisplay, options.ThresholdForProgressDisplay);
        key.SetValue (ValueNameMaxSucessiveWarnings, options.MaxSucessiveWarnings);
      }
    }

    public Version IgnoreUpdatesTilVersion
    {
      get
      {
        using (var key = OpenOptionsKey())
        {
          var versionString = (string) key.GetValue ("IgnoreUpdatesTilVersion");
          if (!string.IsNullOrEmpty (versionString))
            return new Version (versionString);
          else
            return null;
        }
      }
      set
      {
        using (var key = OpenOptionsKey())
        {
          if (value != null)
            key.SetValue ("IgnoreUpdatesTilVersion", value.ToString());
          else
            key.DeleteValue ("IgnoreUpdatesTilVersion");
        }
      }
    }
    
    public int EntityCacheVersion
    {
      get
      {
        using (var key = OpenOptionsKey())
        {
          return (int) (key.GetValue (ValueNameEntityCacheVersion) ?? 0);
        }
      }
      set
      {
        using (var key = OpenOptionsKey())
        {
          key.SetValue (ValueNameEntityCacheVersion, value);
        }
      }
    }

    public static void SaveToolBarSettings(ToolbarSettings settings)
    {
      using (var key = OpenOptionsKey())
      {
        if (settings != null)
          key.SetValue(ValueNameToolbarSettings, Serializer<ToolbarSettings>.Serialize(settings));
        else
          key.DeleteValue(ValueNameToolbarSettings);
      }
    }

    public static ToolbarSettings LoadToolBarSettings()
    {
      using (var key = OpenOptionsKey())
      {
        var settings = (string) key.GetValue(ValueNameToolbarSettings);
        if (!string.IsNullOrEmpty(settings))
          return Serializer<ToolbarSettings>.Deserialize(settings);
        else
          return ToolbarSettings.CreateDefault();
      }
    }

    private static RegistryKey OpenOptionsKey ()
    {
      var key = Registry.CurrentUser.OpenSubKey (ValueNameOptionsRegistryKey, true);
      if (key == null)
        key = Registry.CurrentUser.CreateSubKey (ValueNameOptionsRegistryKey);
      return key;
    }
  }
}