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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class OutlookFolderViewModel : ViewModelBase, IOptionsSection
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private bool _enableChangeTriggeredSynchronization;
    private string _selectedFolderName;
    private OlItemType? _selectedFolderType;
    private FolderDescriptor _selectedFolder;
    private NameSpace _session;
    private ISettingsFaultFinder _faultFinder;

    class FolderDescriptor
    {
      public string FolderId { get; }
      public string StoreId { get; }

      public FolderDescriptor (string folderId, string storeId)
      {
        FolderId = folderId;
        StoreId = storeId;
      }
    }

    public OutlookFolderViewModel (NameSpace session, ISettingsFaultFinder faultFinder)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (faultFinder == null)
        throw new ArgumentNullException (nameof (faultFinder));

      _session = session;
      _faultFinder = faultFinder;
      SelectFolderCommand = new DelegateCommand (_ => SelectFolder());
    }

    public string FolderAccountName { get; private set; }

    public bool EnableChangeTriggeredSynchronization
    {
      get { return _enableChangeTriggeredSynchronization; }
      set
      {
        _enableChangeTriggeredSynchronization = value;
        OnPropertyChanged();
      }
    }

    public string SelectedFolderName
    {
      get { return _selectedFolderName; }
      private set
      {
        _selectedFolderName = value;
        OnPropertyChanged();
      }
    }

    public ICommand SelectFolderCommand { get; }

    public static OutlookFolderViewModel DesignInstance => new OutlookFolderViewModel (new DesignOutlookSession(), NullSettingsFaultFinder.Instance)
                                                           {
                                                               EnableChangeTriggeredSynchronization = true,
                                                               SelectedFolderName = "The outlook folder",
                                                               OutlookFolderType = OlItemType.olAppointmentItem
                                                           };

    public OlItemType? OutlookFolderType
    {
      get { return _selectedFolderType; }
      set
      {
        _selectedFolderType = value;
        OnPropertyChanged();
      }
    }

    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      EnableChangeTriggeredSynchronization = options.EnableChangeTriggeredSynchronization;

      if (!string.IsNullOrEmpty (options.OutlookFolderEntryId) && !string.IsNullOrEmpty (options.OutlookFolderStoreId))
        _selectedFolder = new FolderDescriptor (options.OutlookFolderEntryId, options.OutlookFolderStoreId);
      else
        _selectedFolder = null;

      UpdateFolder();

      FolderAccountName = options.OutlookFolderAccountName;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.EnableChangeTriggeredSynchronization = _enableChangeTriggeredSynchronization;
      options.OutlookFolderEntryId = _selectedFolder?.FolderId;
      options.OutlookFolderStoreId = _selectedFolder?.StoreId;
      options.OutlookFolderAccountName = FolderAccountName;
    }

    private void UpdateFolder (MAPIFolder folder)
    {
      if (folder.DefaultItemType != OlItemType.olAppointmentItem && folder.DefaultItemType != OlItemType.olTaskItem && folder.DefaultItemType != OlItemType.olContactItem)
      {
        string wrongFolderMessage = string.Format ("Wrong ItemType in folder '{0}'. It should be a calendar, task or contact folder.", folder.Name);
        MessageBox.Show (wrongFolderMessage, "Configuration Error");
        return;
      }

      _selectedFolder = new FolderDescriptor (folder.EntryID, folder.StoreID);
      SelectedFolderName = folder.Name;
      OutlookFolderType = folder.DefaultItemType;
    }

    private void UpdateFolder ()
    {
      if (_selectedFolder != null)
      {
        try
        {
          using (var folderWrapper = GenericComObjectWrapper.Create (_session.GetFolderFromID (_selectedFolder.FolderId, _selectedFolder.StoreId)))
          {
            UpdateFolder (folderWrapper.Inner);
          }
        }
        catch (System.Exception x)
        {
          s_logger.Error (null, x);
          SelectedFolderName = "<ERROR>";
          OutlookFolderType = null;
        }
      }
      else
      {
        SelectedFolderName = "<MISSING>";
        OutlookFolderType = null;
      }
    }

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      bool result = true;

      if (_selectedFolder == null)
      {
        errorMessageBuilder.AppendLine ("- There is no Outlook Folder selected.");
        result = false;
      }

      return result;
    }

    private void SelectFolder ()
    {
      var folder = _session.PickFolder();
      if (folder != null)
      {
        using (var folderWrapper = GenericComObjectWrapper.Create (folder))
        {
          UpdateFolder (folderWrapper.Inner);
          UpdateFolderAccountName();
        }
      }

      _faultFinder.FixTimeRangeUsage (OutlookFolderType);

      if (OutlookFolderType == OlItemType.olContactItem)
      {
        MessageBox.Show (
            "The contact synchronization is still in development and doesn't support contact groups/distribution lists at the moment!",
            ComponentContainer.MessageBoxTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
      }
    }

    public void UpdateFolderAccountName ()
    {
      FolderAccountName = _selectedFolder != null
          ? OptionTasks.GetFolderAccountNameOrNull (_session, _selectedFolder.StoreId)
          : null;
    }
  }
}