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
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using CalDavSynchronizer.Utilities;
using CalDavSynchronizer.Implementation.ComWrappers;
using log4net;
using Microsoft.Office.Interop.Outlook;
using System.Text.RegularExpressions;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Properties;
using Exception = System.Exception;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class KolabMultipleOptionsTemplateViewModel : ModelBase, IOptionsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private readonly NetworkSettingsViewModel _networkSettingsViewModel;
    private readonly IServerSettingsTemplateViewModel _serverSettingsViewModel;
    private readonly DelegateCommandWithoutCanExecuteDelegation _getAccountSettingsCommand;
    private readonly DelegateCommandWithoutCanExecuteDelegation _discoverResourcesCommand;
    private readonly DelegateCommandWithoutCanExecuteDelegation _mergeResourcesCommand;

    private const string DEFAULT_CALENDAR = "Meetings";

    private bool _isSelected;
    private readonly IOptionsViewModelParent _parent;
    private readonly IOptionTasks _optionTasks;
    private bool _isExpanded;
    private readonly OptionsModel _prototypeModel;

    private string _selectedFolderName;
    private OutlookFolderDescriptor _selectedFolder;

    public KolabMultipleOptionsTemplateViewModel (
        IOptionsViewModelParent parent,
        IServerSettingsTemplateViewModel serverSettingsViewModel,
        IOptionTasks optionTasks,
        OptionsModel prototypeModel, 
        IViewOptions viewOptions)
    {

      _parent = parent ?? throw new ArgumentNullException (nameof (parent));
      _optionTasks = optionTasks ?? throw new ArgumentNullException(nameof(optionTasks));
      _prototypeModel = prototypeModel ?? throw new ArgumentNullException(nameof(prototypeModel));
      ViewOptions = viewOptions ?? throw new ArgumentNullException(nameof(viewOptions));

      _getAccountSettingsCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        GetAccountSettings();
      });

      _discoverResourcesCommand = new DelegateCommandWithoutCanExecuteDelegation (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        DiscoverResourcesCommandAsync();
      });

      _mergeResourcesCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        MergeResourcesAsync();
      });

      SelectFolderCommand = new DelegateCommand(_ => SelectFolder());

      _networkSettingsViewModel = new NetworkSettingsViewModel(_prototypeModel);
      Items = new[] { _networkSettingsViewModel };

      _serverSettingsViewModel = serverSettingsViewModel;

      var folder = _optionTasks.GetDefaultCalendarFolderOrNull();
      if (folder != null)
      {
        _selectedFolder = folder;
        SelectedFolderName = folder.Name;
      }

      RegisterPropertyChangePropagation(_prototypeModel, nameof(_prototypeModel.Name), nameof(Name));
    }

    private void GetAccountSettings()
    {
      _getAccountSettingsCommand.SetCanExecute(false);
      try
      {
        _serverSettingsViewModel.DiscoverAccountServerSettings();
      }
      catch (Exception x)
      {
        s_logger.Error("Exception while getting account settings.", x);
        string message = null;
        for (Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show(message, Strings.Get($"Account settings"));
      }
      finally
      {
        _getAccountSettingsCommand.SetCanExecute(true);
      }
    }

    private async void MergeResourcesAsync()
    {
      _mergeResourcesCommand.SetCanExecute (false);
      try
      {
        var serverResources = await _serverSettingsViewModel.GetServerResources ();

        var calendars = serverResources.Calendars.Select (c => new CalendarDataViewModel(c)).ToArray();

        var optionList = new List<OptionsModel>();

        foreach (var resource in calendars)
        {
          resource.SelectedFolder = _selectedFolder;
          var options = CreateOptionsWithCategory (resource);

          _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
          optionList.Add(options);
        }

        _parent.RequestAdd (optionList);
        _parent.RequestRemoval (this);

      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while MergeResourcesAsync.", x);
        string message = null;
        for (Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show(message, OptionTasks.ConnectionTestCaption);
      }
      finally
      {
        _mergeResourcesCommand.SetCanExecute (true);
      }
    }

    private async void DiscoverResourcesCommandAsync()
    {
      await DiscoverResourcesAsync();
    }

    public async Task<ServerResources> DiscoverResourcesAsync()
    {
      _discoverResourcesCommand.SetCanExecute (false);
      ServerResources serverResources = new ServerResources();
      try
      {
        s_logger.Debug("Get CalDAV/CardDAV data from Server");
        serverResources = await _serverSettingsViewModel.GetServerResources ();

        var calendars = serverResources.Calendars.Select (c => new CalendarDataViewModel (c)).ToArray();
        var addressBooks = serverResources.AddressBooks.Select (a => new AddressBookDataViewModel (a)).ToArray();
        var taskLists = serverResources.TaskLists.Select (d => new TaskListDataViewModel (d)).ToArray();
        var existingOptions = (_parent as OptionsCollectionViewModel).Options;
        if (OnlyAddNewUrls)
        {
          s_logger.Debug("Exclude all server resources that have already been configured");
          // Exclude all resourcres that have already been configured
          var configuredUrls = new HashSet<String> (existingOptions.Select(o => o.Model.CalenderUrl));
          calendars = calendars.Where(c => !configuredUrls.Contains(c.Uri.ToString())).ToArray();
          addressBooks = addressBooks.Where(c => !configuredUrls.Contains(c.Uri.ToString())).ToArray();
          taskLists = taskLists.Where(c => !configuredUrls.Contains(c.Id)).ToArray();
        }
        // --- Create folders if requested and required
        if (AutoCreateOutlookFolders)
        {
          s_logger.Debug("Auto-create outlook folders");
          // https://docs.microsoft.com/en-us/visualstudio/vsto/how-to-programmatically-create-a-custom-calendar
          // https://msdn.microsoft.com/en-us/library/office/ff184655.aspx
          // Get Outlook's default calendar folder (this is where we create the Kolab folders)
          GenericComObjectWrapper<Folder> defaultCalendarFolder = new GenericComObjectWrapper<Folder> (Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar) as Folder);
          // Get sync option that syncs the default calendar (if any)
          bool defaultFolderIsSynced = existingOptions.Any(o => o.Model.SelectedFolderOrNull?.EntryId == defaultCalendarFolder.Inner.EntryID);
          // Find all Kolab calendars that are not yet synced to an outlook folder
          foreach (var resource in calendars.Where(c => c.SelectedFolder == null))
          {
            s_logger.Debug($"Find folder for calendar '{Name}'");
            string newCalendarName = resource.Name + " (" + Name + ")";
            GenericComObjectWrapper<Folder> newCalendarFolder;
            try
            {
              // Sync CalDAV default calendar with Outlook default calendar folder.
              // Only do so if there are no sync settings yet for the Outlook default calendar
              if (resource.Model.IsDefault && !defaultFolderIsSynced)
              {
                s_logger.Debug($"Sync Calendar '{Name}' with default outlook calendar");
                newCalendarFolder = defaultCalendarFolder;
              }
              // Use existing folder if it does exist
              else
              {
                s_logger.Debug($"Try to use an existing folder for calendar '{Name}'");
                newCalendarFolder = new GenericComObjectWrapper<Folder>(defaultCalendarFolder.Inner.Folders[newCalendarName] as Folder);
              }
            }
            catch
            {
              s_logger.Debug($"Create new folder for calendar '{Name}'");
              // Create missing folder
              newCalendarFolder = new GenericComObjectWrapper<Folder> (defaultCalendarFolder.Inner.Folders.Add(newCalendarName, OlDefaultFolders.olFolderCalendar) as Folder);
              // Make sure it has not been renamed to "name (this computer only)"
              newCalendarFolder.Inner.Name = newCalendarName;
            }
            // use the selected folder for syncing with kolab
            resource.SelectedFolder = new OutlookFolderDescriptor (newCalendarFolder.Inner.EntryID, newCalendarFolder.Inner.StoreID, newCalendarFolder.Inner.DefaultItemType, newCalendarFolder.Inner.Name, 0);
          }

          // Create and assign all Kolab address books that are not yet synced to an outlook folder
          GenericComObjectWrapper<Folder> defaultAddressBookFolder = new GenericComObjectWrapper<Folder> (Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts) as Folder);
          foreach (var resource in addressBooks.Where(c => c.SelectedFolder == null))
          {
            s_logger.Debug($"Find folder for address book '{Name}'");
            string newAddressBookName = resource.Name + " (" + Name + ")";
            GenericComObjectWrapper<Folder> newAddressBookFolder = null;
            try
            {
              newAddressBookFolder = new GenericComObjectWrapper<Folder>(defaultAddressBookFolder.Inner.Folders[newAddressBookName] as Folder);
            }
            catch
            {
              newAddressBookFolder = new GenericComObjectWrapper<Folder> (defaultAddressBookFolder.Inner.Folders.Add(newAddressBookName, OlDefaultFolders.olFolderContacts) as Folder);
              newAddressBookFolder.Inner.Name = newAddressBookName;
              newAddressBookFolder.Inner.ShowAsOutlookAB = true;
            }
            // Special handling for GAL delivered by CardDAV: set as default address list
            if (resource.Uri.Segments.Last() == "ldap-directory/")
            {
              var _session = Globals.ThisAddIn.Application.Session;
              foreach (AddressList al in _session.AddressLists)
              {
                if (al.Name == newAddressBookName)
                {
                  // We need to set it in the registry, as there does not seem to exist an appropriate API
                  string regPath =
                    @"Software\Microsoft\Office\" + Globals.ThisAddIn.Application.Version.Split(new char[] { '.' })[0] + @".0" +
                    @"\Outlook\Profiles\" + _session.CurrentProfileName +
                    @"\9207f3e0a3b11019908b08002b2a56c2";
                  var key = Registry.CurrentUser.OpenSubKey(regPath, true);
                  if (key != null)
                  {
                    // Turn ID into byte array
                    byte[] bytes = new byte[al.ID.Length / 2];
                    for (int i = 0; i < al.ID.Length; i += 2)
                      bytes[i / 2] = Convert.ToByte(al.ID.Substring(i, 2), 16);
                    key.SetValue("01023d06", bytes);
                  }
                }
              }

            }
            resource.SelectedFolder = new OutlookFolderDescriptor (newAddressBookFolder.Inner.EntryID, newAddressBookFolder.Inner.StoreID, newAddressBookFolder.Inner.DefaultItemType, newAddressBookFolder.Inner.Name, 0);
          }

          // Create and assign all Kolab task lists that are not yet synced to an outlook folder
          GenericComObjectWrapper<Folder> defaultTaskListsFolder = new GenericComObjectWrapper<Folder> (Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks) as Folder);
          foreach (var resource in taskLists.Where(c => c.SelectedFolder == null))
          {
            s_logger.Debug($"Find folder for task list '{Name}'");
            string newTaskListName = resource.Name + " (" + Name + ")";
            GenericComObjectWrapper<Folder> newTaskListFolder = null;
            try
            {
              newTaskListFolder = new GenericComObjectWrapper<Folder> (defaultTaskListsFolder.Inner.Folders[newTaskListName] as Folder);
            }
            catch
            {
              newTaskListFolder = new GenericComObjectWrapper<Folder> (defaultTaskListsFolder.Inner.Folders.Add(newTaskListName, OlDefaultFolders.olFolderTasks) as Folder);
              newTaskListFolder.Inner.Name = newTaskListName;
            }
            resource.SelectedFolder = new OutlookFolderDescriptor(newTaskListFolder.Inner.EntryID, newTaskListFolder.Inner.StoreID, newTaskListFolder.Inner.DefaultItemType, newTaskListFolder.Inner.Name, 0);
          }
        }
        using (var selectResourcesForm = SelectResourceForm.CreateForFolderAssignment(_optionTasks, ConnectionTests.ResourceType.Calendar, calendars, addressBooks, taskLists))
        {
          // Create and add new sync profiles
          s_logger.Debug("Create and add all new sync profiles");
          if (AutoConfigure || selectResourcesForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            var optionList = new List<OptionsModel>();

            foreach (var resource in calendars.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              GenericComObjectWrapper<Folder> folder = new GenericComObjectWrapper<Folder>(
                Globals.ThisAddIn.Application.Session.GetFolderFromID(resource.SelectedFolder.EntryId) as Folder);
              if (resource.Model.ReadOnly)
              {
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
                folder.Inner.Description = Strings.Get($"Read-only calendar") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook are discarded and replaced by data from the server.");
                try
                {
                  // Setting the icon might fail if we sync with the default Outlook calendar folder
                  folder.Inner.SetCustomIcon(
                    PictureDispConverter.ToIPictureDisp(Resources.CalendarReadOnly) as stdole.StdPicture);
                }
                catch { }
              }
              else
              {
                options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
                folder.Inner.Description = Strings.Get($"Read-write calendar") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook and remote changes from the server are merged.");
                try
                {
                  // Setting the icon might fail if we sync with the default Outlook calendar folder
                  folder.Inner.SetCustomIcon(
                    PictureDispConverter.ToIPictureDisp(Resources.CalendarReadWrite) as stdole.StdPicture);
                }
                catch { }
              }
              optionList.Add (options);
            }

            foreach (var resource in addressBooks.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              GenericComObjectWrapper<Folder> folder = new GenericComObjectWrapper<Folder>(
                Globals.ThisAddIn.Application.Session.GetFolderFromID(resource.SelectedFolder.EntryId) as Folder);
              if (resource.Model.ReadOnly)
              {
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
                folder.Inner.Description = Strings.Get($"Read-only address book") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook are discarded and replaced by data from the server.");
                folder.Inner.SetCustomIcon(
                  PictureDispConverter.ToIPictureDisp(Resources.AddressbookReadOnly) as stdole.StdPicture);
              }
              else
              {
                options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
                folder.Inner.Description = Strings.Get($"Read-write address book") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook and remote changes from the server are merged.");
                folder.Inner.SetCustomIcon(
                  PictureDispConverter.ToIPictureDisp(Resources.AddressbookReadWrite) as stdole.StdPicture);
              }
              optionList.Add (options);
            }

            foreach (var resource in taskLists.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              GenericComObjectWrapper<Folder> folder = new GenericComObjectWrapper<Folder>(
                Globals.ThisAddIn.Application.Session.GetFolderFromID(resource.SelectedFolder.EntryId) as Folder);
              if (resource.Model.ReadOnly)
              {
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
                folder.Inner.Description = Strings.Get($"Read-only task list") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook are discarded and replaced by data from the server.");
                folder.Inner.SetCustomIcon(
                  PictureDispConverter.ToIPictureDisp(Resources.TasklistReadOnly) as stdole.StdPicture);
              }
              else
              {
                options.SynchronizationMode = SynchronizationMode.MergeInBothDirections;
                folder.Inner.Description = Strings.Get($"Read-write task list") + $" »{options.Name}«:\n" + Strings.Get($"local changes made in Outlook and remote changes from the server are merged.");
                folder.Inner.SetCustomIcon(
                  PictureDispConverter.ToIPictureDisp(Resources.TasklistReadWrite) as stdole.StdPicture);
              }
              optionList.Add (options);
            }

            _parent.RequestAdd (optionList);
            _parent.RequestRemoval (this);
          }
        }
      }
      catch (Exception x)
      {
        s_logger.Error ("Exception while DiscoverResourcesAsync.", x);
        string message = null;
        for (Exception ex = x; ex != null; ex = ex.InnerException)
          message += ex.Message + Environment.NewLine;
        MessageBox.Show (message, OptionTasks.ConnectionTestCaption);
      }
      finally
      {
        _discoverResourcesCommand.SetCanExecute (true);
      }
      return serverResources;
    }

    private OptionsModel CreateOptions (ResourceDataViewModelBase resource)
    {
      var options = _prototypeModel.Clone();
      options.Name = $"{_prototypeModel.Name} ({resource.Name})";
      options.SetFolder(resource.SelectedFolder);
      return options;
    }

    private OptionsModel CreateOptionsWithCategory (CalendarDataViewModel resource)
    {
      var options = CreateOptions (resource);
      var eventMappingOptions = (EventMappingConfigurationModel) options.MappingConfigurationModelOrNull;
      eventMappingOptions.EventCategory = resource.Name;
      // set Meeting calendar to default calendar where all appointments without category are synced to and change time range for meetings
      if (resource.Name == DEFAULT_CALENDAR)
      {
        eventMappingOptions.IncludeEmptyEventCategoryFilter = true;
        options.DaysToSynchronizeInTheFuture = 365;
        options.DaysToSynchronizeInThePast = 60;
      }
      eventMappingOptions.OneTimeSetEventCategoryColor = ColorHelper.FindMatchingCategoryColor(resource.Color.GetValueOrDefault());
      eventMappingOptions.DoOneTimeSetCategoryColor = true;
      return options;
    }

private void SelectFolder()
    {
      var folder = _optionTasks.PickFolderOrNull();
      if (folder != null && folder.DefaultItemType == OlItemType.olAppointmentItem)
      {
        _selectedFolder = folder;
        SelectedFolderName = folder.Name;

      }
      else
      {
        MessageBox.Show(
            Strings.Get($"You need to choose a calendar folder to merge the Kolab resources!"),
            ComponentContainer.MessageBoxTitle,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
      }
    }

    public string SelectedFolderName
    {
      get { return _selectedFolderName; }
      private set
      {
        CheckedPropertyChange (ref _selectedFolderName, value);
      }
    }

    public IServerSettingsTemplateViewModel ServerSettingsViewModel => _serverSettingsViewModel;

    public ICommand GetAccountSettingsCommand => _getAccountSettingsCommand;
    public ICommand DiscoverResourcesCommand => _discoverResourcesCommand;
    public ICommand MergeResourcesCommand => _mergeResourcesCommand;

    public ICommand SelectFolderCommand { get; }

    public bool IsActive { get; set; }
    public bool SupportsIsActive { get; } = false;
    public bool AutoCreateOutlookFolders { get; set; } = false;
    public bool AutoConfigure { get; set; } = false;
    public bool OnlyAddNewUrls { get; set; } = false;
    public IEnumerable<ISubOptionsViewModel> Items { get; }
    IEnumerable<ITreeNodeViewModel> ITreeNodeViewModel.Items => Items;

    public bool IsMultipleOptionsTemplateViewModel { get; } = true;
    public OlItemType? OutlookFolderType { get; } = null;

    public bool IsSelected
    {
      get { return _isSelected; }
      set { CheckedPropertyChange (ref _isSelected, value); }
    }

    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        CheckedPropertyChange (ref _isExpanded, value);
      }
    }

    public string Name
    {
      get { return _prototypeModel.Name; }
      set { _prototypeModel.Name = value; }
    }

    public Contracts.Options GetOptionsOrNull () => null;
    public bool Validate (StringBuilder errorMessageBuilder) => true;
    public IViewOptions ViewOptions { get; }
    public OptionsModel Model => _prototypeModel;
  }

  // See https://msdn.microsoft.com/en-us/VBA/Outlook-VBA/articles/folder-setcustomicon-method-outlook
  public static class PictureDispConverter
  {
    // IPictureDisp GUID. 
    public static Guid iPictureDispGuid = typeof(stdole.IPictureDisp).GUID;

    // Converts an icon into an IPictureDisp. 
    public static stdole.IPictureDisp ToIPictureDisp(Icon icon)
    {
      PICTDESC.Icon pictIcon = new PICTDESC.Icon(icon);
      return PictureDispConverter.OleCreatePictureIndirect(pictIcon, ref iPictureDispGuid, true);
    }

    // Converts an image into an IPictureDisp. 
    public static stdole.IPictureDisp ToIPictureDisp(Image image)
    {
      Bitmap bitmap = (image is Bitmap) ? (Bitmap)image : new Bitmap(image);
      PICTDESC.Bitmap pictBit = new PICTDESC.Bitmap(bitmap);
      return PictureDispConverter.OleCreatePictureIndirect(pictBit, ref iPictureDispGuid, true);
    }

    [DllImport("OleAut32.dll", EntryPoint = "OleCreatePictureIndirect", ExactSpelling = true,
    PreserveSig = false)]
    private static extern stdole.IPictureDisp OleCreatePictureIndirect(
    [MarshalAs(UnmanagedType.AsAny)] object picdesc, ref Guid iid, bool fOwn);

    private readonly static HandleCollector handleCollector =
    new HandleCollector("Icon handles", 1000);

    // WINFORMS COMMENT: 
    // PICTDESC is a union in native, so we'll just 
    // define different ones for the different types 
    // the "unused" fields are there to make it the right 
    // size, since the struct in native is as big as the biggest 
    // union. 
    private static class PICTDESC
    {
      // Picture Types 
      public const short PICTYPE_UNINITIALIZED = -1;
      public const short PICTYPE_NONE = 0;
      public const short PICTYPE_BITMAP = 1;
      public const short PICTYPE_METAFILE = 2;
      public const short PICTYPE_ICON = 3;
      public const short PICTYPE_ENHMETAFILE = 4;

      [StructLayout(LayoutKind.Sequential)]
      public class Icon
      {
        internal int cbSizeOfStruct = Marshal.SizeOf(typeof(PICTDESC.Icon));
        internal int picType = PICTDESC.PICTYPE_ICON;
        internal IntPtr hicon = IntPtr.Zero;
        internal int unused1 = 0;
        internal int unused2 = 0;

        internal Icon(System.Drawing.Icon icon)
        {
          this.hicon = icon.ToBitmap().GetHicon();
        }
      }

      [StructLayout(LayoutKind.Sequential)]
      public class Bitmap
      {
        internal int cbSizeOfStruct = Marshal.SizeOf(typeof(PICTDESC.Bitmap));
        internal int picType = PICTDESC.PICTYPE_BITMAP;
        internal IntPtr hbitmap = IntPtr.Zero;
        internal IntPtr hpal = IntPtr.Zero;
        internal int unused = 0;
        internal Bitmap(System.Drawing.Bitmap bitmap)
        {
          this.hbitmap = bitmap.GetHbitmap();
        }
      }
    }
  }
}