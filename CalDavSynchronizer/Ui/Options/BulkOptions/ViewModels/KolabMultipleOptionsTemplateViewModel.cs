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
using Exception = System.Exception;
using Microsoft.Win32;

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
        serverResources = await _serverSettingsViewModel.GetServerResources ();

        var calendars = serverResources.Calendars.Select (c => new CalendarDataViewModel (c)).ToArray();
        var addressBooks = serverResources.AddressBooks.Select (a => new AddressBookDataViewModel (a)).ToArray();
        var taskLists = serverResources.TaskLists.Select (d => new TaskListDataViewModel (d)).ToArray();
        if (OnlyAddNewUrls)
        {
          // Exclude all resourcres that have already been configured
          var options = (_parent as OptionsCollectionViewModel).Options;
          var configuredUrls = new HashSet<String> (options.Select(o => o.Model.CalenderUrl));
          calendars = calendars.Where(c => !configuredUrls.Contains(c.Uri.ToString())).ToArray();
          addressBooks = addressBooks.Where(c => !configuredUrls.Contains(c.Uri.ToString())).ToArray();
          taskLists = taskLists.Where(c => !configuredUrls.Contains(c.Id)).ToArray();
        }
        // --- Create folders if requested and required
        if (AutoCreateOutlookFolders)
        {
          // https://docs.microsoft.com/en-us/visualstudio/vsto/how-to-programmatically-create-a-custom-calendar
          // https://msdn.microsoft.com/en-us/library/office/ff184655.aspx
          // Get Outlook's default calendar folder (this is where we create the Kolab folders)
          GenericComObjectWrapper<Folder> defaultCalendarFolder = new GenericComObjectWrapper<Folder> (Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar) as Folder);
          // Find all Kolab calendars that are not yet synced to an outlook folder
          foreach (var resource in calendars.Where(c => c.SelectedFolder == null))
          {
            string newCalendarName = resource.Name + " (" + Name + ")";
            GenericComObjectWrapper<Folder> newCalendarFolder = null;
            try
            {
              // Use existing folder if it does exist
              newCalendarFolder = new GenericComObjectWrapper<Folder> (defaultCalendarFolder.Inner.Folders[newCalendarName] as Folder);
            }
            catch
            {
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
          if (AutoConfigure || selectResourcesForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            var optionList = new List<OptionsModel>();

            foreach (var resource in calendars.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              if (resource.Model.ReadOnly)
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
              optionList.Add (options);
            }

            foreach (var resource in addressBooks.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              if (resource.Model.ReadOnly)
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
              optionList.Add (options);
            }

            foreach (var resource in taskLists.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              if (resource.Model.ReadOnly)
                options.SynchronizationMode = SynchronizationMode.ReplicateServerIntoOutlook;
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
}