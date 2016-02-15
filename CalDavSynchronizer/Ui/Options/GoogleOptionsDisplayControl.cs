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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.Mapping;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class GoogleOptionsDisplayControl : UserControl, IOptionsDisplayControl, IServerSettingsControlDependencies
  {
    public const string ConnectionTestCaption = "Test settings";
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private Guid _optionsId;
    private NetworkAndProxyOptions _networkAndProxyOptions;
    public event EventHandler DeletionRequested;
    public event EventHandler CopyRequested;
    public event EventHandler<HeaderEventArgs> HeaderChanged;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
    private readonly Lazy<IConfigurationFormFactory> _configurationFormFactory;
    private MappingConfigurationBase _mappingConfiguration;

    public GoogleOptionsDisplayControl (
        NameSpace session,
        Func<Guid, string> profileDataDirectoryFactory,
        bool fixInvalidSettings)
    {
      ISettingsFaultFinder faultFinder;
      InitializeComponent();

      if (fixInvalidSettings)
        faultFinder = new SettingsFaultFinder (_syncSettingsControl);
      else
        faultFinder = NullSettingsFaultFinder.Instance;

      _outlookFolderControl.Initialize(session, faultFinder);
      _serverSettingsControl.Initialize (faultFinder, this);

      _profileNameTextBox.TextChanged += _profileNameTextBox_TextChanged;
      _inactiveCheckBox.CheckedChanged += _inactiveCheckBox_CheckedChanged;
      _outlookFolderControl.FolderChanged += OutlookFolderControl_FolderChanged;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;

      _configurationFormFactory = OptionTasks.CreateConfigurationFormFactory(_serverSettingsControl);
    }

    private void OutlookFolderControl_FolderChanged (object sender, EventArgs e)
    {
      OnHeaderChanged();
    }

    public string ProfileName
    {
      get { return _profileNameTextBox.Text; }
    }

    private void _inactiveCheckBox_CheckedChanged (object sender, EventArgs e)
    {
      OnHeaderChanged();
    }

    private void _profileNameTextBox_TextChanged (object sender, EventArgs e)
    {
      OnHeaderChanged();
    }

    private void OnHeaderChanged ()
    {
      if (HeaderChanged != null)
      {
        var args = new HeaderEventArgs (_profileNameTextBox.Text, _inactiveCheckBox.Checked, _outlookFolderControl.OutlookFolderType);
        HeaderChanged (this, args);
      }
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (_serverSettingsControl.UsedServerAdapterType != ServerAdapterType.GoogleTaskApi)
        result &= OptionTasks.ValidateWebDavUrl (_serverSettingsControl.CalendarUrl, errorMessageBuilder, true);

      result &= _outlookFolderControl.Validate (errorMessageBuilder);
      result &= OptionTasks.ValidateGoogleEmailAddress (errorMessageBuilder, _serverSettingsControl.EmailAddress);

      return result;
    }

    public Contracts.Options Options
    {
      set
      {
        _inactiveCheckBox.Checked = value.Inactive;
        _profileNameTextBox.Text = value.Name;
        _optionsId = value.Id;

        _networkAndProxyOptions = new NetworkAndProxyOptions (
            value.CloseAfterEachRequest,
            value.PreemptiveAuthentication,
            value.ProxyOptions ?? new ProxyOptions());
        _mappingConfiguration = value.MappingConfiguration;
        _outlookFolderControl.SetOptions (value);
        _serverSettingsControl.SetOptions (value);
        _syncSettingsControl.SetOptions (value);
        OnHeaderChanged();
      }
      get
      {
        var options = new Contracts.Options()
                      {
                          Name = _profileNameTextBox.Text,
                          Id = _optionsId,
                          Inactive = _inactiveCheckBox.Checked,
                          CloseAfterEachRequest = _networkAndProxyOptions.CloseConnectionAfterEachRequest,
                          PreemptiveAuthentication = _networkAndProxyOptions.PreemptiveAuthentication,
                          ProxyOptions = _networkAndProxyOptions.ProxyOptions,
                          MappingConfiguration = _mappingConfiguration,
                          DisplayType = OptionsDisplayType.Google
                      };

        _outlookFolderControl.FillOptions (options);
        _serverSettingsControl.FillOptions (options);
        _syncSettingsControl.FillOptions (options);
        return options;
      }
    }

    public Control UiControl
    {
      get { return this; }
    }

    private void _deleteButton_Click (object sender, EventArgs e)
    {
      if (DeletionRequested != null)
        DeletionRequested (this, EventArgs.Empty);
    }

    private void _copyButton_Click (object sender, EventArgs e)
    {
      if (CopyRequested != null)
        CopyRequested (this, EventArgs.Empty);
    }

    private void _mappingConfigurationButton_Click (object sender, EventArgs e)
    {
      try
      {
        var mappingConfiguration = OptionTasks.CoreceMappingConfiguration (_outlookFolderControl.OutlookFolderType, _mappingConfiguration);
        if (mappingConfiguration != null)
        {
          var configurationForm = mappingConfiguration.CreateConfigurationForm (_configurationFormFactory.Value);
          if (configurationForm.Display())
            _mappingConfiguration = configurationForm.Options;
        }
        else
        {
          MessageBox.Show ("Mapping configuration not available.");
        }
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    public OlItemType? OutlookFolderType
    {
      get { return _outlookFolderControl.OutlookFolderType; }
    }

    public bool SelectedSynchronizationModeRequiresWriteableServerResource
    {
      get { return _syncSettingsControl.SelectedModeRequiresWriteableServerResource; }
    }

    public string SelectedSynchronizationModeDisplayName
    {
      get { return _syncSettingsControl.SelectedModeDisplayName; }
    }

    private void _browseToProfileCacheDirectoryToolStripMenuItem_Click (object sender, EventArgs e)
    {
      try
      {
        var profileDataDirectory = _profileDataDirectoryFactory (_optionsId);

        if (Directory.Exists (profileDataDirectory))
          Process.Start (profileDataDirectory);
        else
          MessageBox.Show ("The directory does not exist.");
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }

    private void _resetButton_Click (object sender, EventArgs e)
    {
      try
      {
        s_logger.InfoFormat ("Deleting cache for profile '{0}'", ProfileName);

        var profileDataDirectory = _profileDataDirectoryFactory (_optionsId);
        if (Directory.Exists (profileDataDirectory))
          Directory.Delete (profileDataDirectory, true);

        MessageBox.Show ("A new intial sync will be performed with the next sync run!", "Profile cache deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
      }
      catch (Exception x)
      {
        s_logger.Error (null, x);
      }
    }
  }
}