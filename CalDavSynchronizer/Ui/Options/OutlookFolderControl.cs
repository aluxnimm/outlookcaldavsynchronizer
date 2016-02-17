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
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options
{
  public partial class OutlookFolderControl : UserControl
  {
    public const string ConnectionTestCaption = "Test settings";
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private OlItemType? _folderType;
    private string _folderEntryId;
    private string _folderStoreId;
    private NameSpace _session;
    private ISettingsFaultFinder _faultFinder;

    public event EventHandler FolderChanged;


    public void Initialize (
        NameSpace session,
        ISettingsFaultFinder faultFinder)
    {
      InitializeComponent();

      _session = session;
      _faultFinder = faultFinder;
      _selectOutlookFolderButton.Click += _selectOutlookFolderButton_Click;
    }

    protected virtual void OnFolderChanged ()
    {
      EventHandler handler = FolderChanged;
      if (handler != null)
        handler (this, EventArgs.Empty);
    }


    private void _selectOutlookFolderButton_Click (object sender, EventArgs e)
    {
      SelectFolder();
    }

    public void SetOptions (Contracts.Options value)
    {
      _synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Checked = value.EnableChangeTriggeredSynchronization;

      UpdateFolder (value.OutlookFolderEntryId, value.OutlookFolderStoreId);
    }

    public void FillOptions (Contracts.Options optionsToFill)
    {
      optionsToFill.OutlookFolderEntryId = _folderEntryId;
      optionsToFill.OutlookFolderStoreId = _folderStoreId;
      optionsToFill.EnableChangeTriggeredSynchronization = _synchronizeImmediatelyAfterOutlookItemChangeCheckBox.Checked;
    }

    private void UpdateFolder (MAPIFolder folder)
    {
      if (folder.DefaultItemType != OlItemType.olAppointmentItem && folder.DefaultItemType != OlItemType.olTaskItem && folder.DefaultItemType != OlItemType.olContactItem)
      {
        string wrongFolderMessage = string.Format ("Wrong ItemType in folder '{0}'. It should be a calendar, task or contact folder.", folder.Name);
        MessageBox.Show (wrongFolderMessage, "Configuration Error");
        return;
      }

      _folderEntryId = folder.EntryID;
      _folderStoreId = folder.StoreID;
      _outoookFolderNameTextBox.Text = folder.Name;
      _folderType = folder.DefaultItemType;
      OnFolderChanged();
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

      _faultFinder.FixTimeRangeUsage (_folderType);

      if (_folderType == OlItemType.olContactItem)
      {
        MessageBox.Show (
            "The contact synchronization is still in development and doesn't support contact groups/distribution lists at the moment!",
            ComponentContainer.MessageBoxTitle,
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
      }
    }

    public OlItemType? OutlookFolderType
    {
      get { return _folderType; }
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (string.IsNullOrWhiteSpace (_folderStoreId) || string.IsNullOrWhiteSpace (_folderEntryId))
      {
        errorMessageBuilder.AppendLine ("- There is no Outlook Folder selected.");
        result = false;
      }

      return result;
    }
  }
}