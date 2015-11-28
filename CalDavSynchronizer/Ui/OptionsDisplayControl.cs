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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui
{
  public partial class OptionsDisplayControl : UserControl, IOptionsDisplayControl, IServerSettingsControlDependencies
  {
    public const string ConnectionTestCaption = "Test settings";
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private OlItemType? _folderType;
    private string _folderEntryId;
    private string _folderStoreId;
    private readonly NameSpace _session;
    private Guid _optionsId;
    private readonly ISettingsFaultFinder _faultFinder;
    private AdvancedOptions _advancedOptions;

    public event EventHandler DeletionRequested;
    public event EventHandler CopyRequested;
    public event EventHandler<HeaderEventArgs> HeaderChanged;
    private readonly Func<Guid, string> _profileDataDirectoryFactory;
  
    public OptionsDisplayControl (
      NameSpace session, 
      Func<Guid, string> profileDataDirectoryFactory,
      bool fixInvalidSettings)
    {
      InitializeComponent();

      if (fixInvalidSettings)
        _faultFinder = new SettingsFaultFinder (_syncSettingsControl);
      else
        _faultFinder = NullSettingsFaultFinder.Instance;

      _serverSettingsControl.Initialize (_faultFinder, this);

      _session = session;
      _profileDataDirectoryFactory = profileDataDirectoryFactory;
    
      _selectOutlookFolderButton.Click += _selectOutlookFolderButton_Click;

      _profileNameTextBox.TextChanged += _profileNameTextBox_TextChanged;
      _inactiveCheckBox.CheckedChanged += _inactiveCheckBox_CheckedChanged;
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
        var args = new HeaderEventArgs (_profileNameTextBox.Text, _inactiveCheckBox.Checked, _folderType);
        HeaderChanged (this, args);
      }
    }

    private void _selectOutlookFolderButton_Click (object sender, EventArgs e)
    {
      SelectFolder();
    }

  
    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = ValidateCalendarUrl (errorMessageBuilder, true);

      if (string.IsNullOrWhiteSpace (_folderStoreId) || string.IsNullOrWhiteSpace (_folderEntryId))
      {
        errorMessageBuilder.AppendLine ("- There is no Outlook Folder selected.");
        result = false;
      }

      try
      {
        var uri = new Uri ("mailto:" + _serverSettingsControl.EmailAddress).ToString ();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The Email Address is invalid. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }

    private bool ValidateCalendarUrl (StringBuilder errorMessageBuilder, bool requiresTrailingSlash)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (_serverSettingsControl.CalendarUrl))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url is empty.");
        return false;
      }

      if (_serverSettingsControl.CalendarUrl.Trim () != _serverSettingsControl.CalendarUrl)
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url cannot end/start with whitespaces.");
        result = false;
      }

      if (requiresTrailingSlash && !_serverSettingsControl.CalendarUrl.EndsWith ("/"))
      {
        errorMessageBuilder.AppendLine ("- The CalDav/CardDav Url has to end with a slash ('/').");
        result = false;
      }

      try
      {
        var uri = new Uri (_serverSettingsControl.CalendarUrl).ToString ();
      }
      catch (Exception x)
      {
        errorMessageBuilder.AppendFormat ("- The CalDav/CardDav Url is not a well formed Url. ({0})", x.Message);
        errorMessageBuilder.AppendLine();
        result = false;
      }

      return result;
    }


    public Options Options
    {
      set
      {
        _inactiveCheckBox.Checked = value.Inactive;
        _profileNameTextBox.Text = value.Name;
        _optionsId = value.Id;

        _synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Checked = value.EnableChangeTriggeredSynchronization;

        _advancedOptions = new AdvancedOptions (
            value.CloseAfterEachRequest,
            value.ProxyOptions ?? new ProxyOptions(),
            value.MappingConfiguration);

        UpdateFolder (value.OutlookFolderEntryId, value.OutlookFolderStoreId);
        _serverSettingsControl.SetOptions (value);
        _syncSettingsControl.SetOptions (value);
        OnHeaderChanged();
      }
      get
      {
        var options = new Options()
               {
                   Name = _profileNameTextBox.Text,
                   OutlookFolderEntryId = _folderEntryId,
                   OutlookFolderStoreId = _folderStoreId,
                   Id = _optionsId,
                   Inactive = _inactiveCheckBox.Checked,
                   CloseAfterEachRequest = _advancedOptions.CloseConnectionAfterEachRequest,
                   EnableChangeTriggeredSynchronization = _synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Checked,
                   ProxyOptions = _advancedOptions.ProxyOptions,
                   MappingConfiguration = _advancedOptions.MappingConfiguration
               };
        _serverSettingsControl.FillOptions (options);
        _syncSettingsControl.FillOptions (options);
        return options;
      }
    }

    public Control UiControl
    {
      get { return this; }
    }


    private bool IsTaskSynchronizationEnabled
    {
      get
      {
        bool enabled;
        if (bool.TryParse (ConfigurationManager.AppSettings["enableTaskSynchronization"], out enabled))
          return enabled;
        else
          return false;
      }
    }

    private void UpdateFolder (MAPIFolder folder)
    {
      if (IsTaskSynchronizationEnabled)
      {
        if (folder.DefaultItemType != OlItemType.olAppointmentItem && folder.DefaultItemType != OlItemType.olTaskItem && folder.DefaultItemType != OlItemType.olContactItem)
        {
          string wrongFolderMessage = string.Format ("Wrong ItemType in folder '{0}'. It should be a calendar, task or contact folder.", folder.Name);
          MessageBox.Show (wrongFolderMessage, "Configuration Error");
          return;
        }
      }
      else
      {
        if (folder.DefaultItemType != OlItemType.olAppointmentItem && folder.DefaultItemType != OlItemType.olContactItem)
        {
          string wrongFolderMessage = string.Format ("Wrong ItemType in folder '{0}'. It should be a calendar or contact folder.", folder.Name);
          MessageBox.Show (wrongFolderMessage, "Configuration Error");
          return;
        }
      }

      _folderEntryId = folder.EntryID;
      _folderStoreId = folder.StoreID;
      _outoookFolderNameTextBox.Text = folder.Name;
      _folderType = folder.DefaultItemType;
      OnHeaderChanged();
    }

    private void UpdateFolder (string folderEntryId, string folderStoreId)
    {
      if (!string.IsNullOrEmpty (folderEntryId) && !string.IsNullOrEmpty (folderStoreId))
      {
        try
        {
          using (var folderWrapper = GenericComObjectWrapper.Create (_session.GetFolderFromID (folderEntryId, folderStoreId)))
          {
            UpdateFolder (folderWrapper.Inner);
          }
        }
        catch (Exception x)
        {
          s_logger.Error (null, x);
          _outoookFolderNameTextBox.Text = "<ERROR>";
          _folderType = null;
        }
      }
      else
      {
        _outoookFolderNameTextBox.Text = "<MISSING>";
        _folderType = null;
      }
    }

    private void SelectFolder ()
    {
      var folder = _session.PickFolder();
      if (folder != null)
      {
        using (var folderWrapper = GenericComObjectWrapper.Create (folder))
        {
          UpdateFolder (folderWrapper.Inner);
        }
      }

      _faultFinder.FixTimeRangeUsage(_folderType);

      if (_folderType == OlItemType.olContactItem)
      {
        MessageBox.Show (
            "The contact synchronization is still in development and currently only beta quality!",
            "CalDav Synchronizer",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
      }
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

    private void _advancedSettingsButton_Click (object sender, EventArgs e)
    {
      using (AdvancedOptionsForm advancedOptionsForm = new AdvancedOptionsForm (CoreceMappingConfiguration))
      {
        advancedOptionsForm.Options = _advancedOptions;

        if (advancedOptionsForm.ShowDialog() == DialogResult.OK)
        {
          _advancedOptions = advancedOptionsForm.Options;
        }
      }
    }

    private MappingConfigurationBase CoreceMappingConfiguration (MappingConfigurationBase mappingConfiguration)
    {
      switch (_folderType)
      {
        case OlItemType.olAppointmentItem:
          if (mappingConfiguration == null || mappingConfiguration.GetType() != typeof (EventMappingConfiguration))
            return new EventMappingConfiguration();
          break;
      }

      return mappingConfiguration;
    }

    private void _browseToProfileCacheDirectoryToolStripMenuItem_Click (object sender, EventArgs e)
    {
      try
      {
        var profileDataDirectory = _profileDataDirectoryFactory (_optionsId);

        if (Directory.Exists (profileDataDirectory))
          System.Diagnostics.Process.Start (profileDataDirectory);
        else
          MessageBox.Show ("The directory does not exist.");
      }
      catch (Exception x)
      {
        ExceptionHandler.Instance.HandleException (x, s_logger);
      }
    }
    
    public bool CloseConnectionAfterEachRequest
    {
      get { return _advancedOptions.CloseConnectionAfterEachRequest; }
    }

    public ProxyOptions ProxyOptions
    {
      get { return _advancedOptions.ProxyOptions; }
    }

    public OlItemType? OutlookFolderType
    {
      get { return _folderType; }
    }

    public bool SelectedSynchronizationModeRequiresWriteableServerResource
    {
      get { return _syncSettingsControl.SelectedModeRequiresWriteableServerResource; }
    }

    public string SelectedSynchronizationModeDisplayName
    {
      get { return _syncSettingsControl.SelectedModeDisplayName; }
    }
  }
}