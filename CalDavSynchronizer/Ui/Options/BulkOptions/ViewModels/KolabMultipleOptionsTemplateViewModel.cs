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
using log4net;
using Microsoft.Office.Interop.Outlook;
using System.Text.RegularExpressions;
using Exception = System.Exception;

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

      _parent = parent;
      if (parent == null)
        throw new ArgumentNullException (nameof (parent));
     if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (prototypeModel == null) throw new ArgumentNullException(nameof(prototypeModel));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _prototypeModel = prototypeModel;
      ViewOptions = viewOptions;

      _getAccountSettingsCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        GetAccountSettings();
      });

      _discoverResourcesCommand = new DelegateCommandWithoutCanExecuteDelegation (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        DiscoverResourcesAsync();
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
      _optionTasks = optionTasks;

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
        MessageBox.Show(message, "Account settings");
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

    private async void DiscoverResourcesAsync ()
    {
      _discoverResourcesCommand.SetCanExecute (false);
      try
      {
        var serverResources = await _serverSettingsViewModel.GetServerResources ();

        var calendars = serverResources.Calendars.Select (c => new CalendarDataViewModel (c)).ToArray();
        var addressBooks = serverResources.AddressBooks.Select (a => new AddressBookDataViewModel (a)).ToArray();
        var taskLists = serverResources.TaskLists.Select (d => new TaskListDataViewModel (d)).ToArray();

        // --- Create Folders if requested and required
        if (AutoCreateOutlookFolders)
        {
          // Get Outlook's default calendar folder (this is where we create the Kolab folders)
          Folder defaultCalendarFolder = Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderCalendar) as Folder;
          // Find all Kolab calendars that are not yet synced to an outlook folder
          foreach (var resource in calendars.Where(c => c.SelectedFolder == null))
          {
            string newCalendarName = resource.Name + " (" + Name + ")";
            Folder newCalendarFolder = null;
            try
            {
              // Use existing folder if it does exist
              newCalendarFolder = defaultCalendarFolder.Folders[newCalendarName] as Folder;
            }
            catch
            {
              // Create missing folder
              newCalendarFolder = defaultCalendarFolder.Folders.Add(newCalendarName, OlDefaultFolders.olFolderCalendar) as Folder;
              // Make sure it has not been renamed to "name (this computer only)"
              newCalendarFolder.Name = newCalendarName;
            }
            // use the selected folder for syncing with kolab
            resource.SelectedFolder = new OutlookFolderDescriptor(newCalendarFolder);
            resource.SelectedFolderName = newCalendarFolder.Name;
          }

          // Create and assign all Kolab address books that are not yet synced to an outlook folder
          Folder defaultAddressBookFolder = Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderContacts) as Folder;
          foreach (var resource in addressBooks.Where(c => c.SelectedFolder == null))
          {
            string newAddressBookName = resource.Name + " (" + Name + ")";
            Folder newAddressBookFolder = null;
            try
            {
              newAddressBookFolder = defaultAddressBookFolder.Folders[newAddressBookName] as Folder;
            }
            catch
            {
              newAddressBookFolder = defaultAddressBookFolder.Folders.Add(newAddressBookName, OlDefaultFolders.olFolderContacts) as Folder;
              newAddressBookFolder.Name = newAddressBookName;
            }
            resource.SelectedFolder = new OutlookFolderDescriptor(newAddressBookFolder);
            resource.SelectedFolderName = newAddressBookFolder.Name;
          }

          // Create and assign all Kolab address books that are not yet synced to an outlook folder
          Folder defaultTaskListsFolder = Globals.ThisAddIn.Application.Session.GetDefaultFolder(OlDefaultFolders.olFolderTasks) as Folder;
          foreach (var resource in taskLists.Where(c => c.SelectedFolder == null))
          {
            string newTaskListName = resource.Name + " (" + Name + ")";
            Folder newTaskListFolder = null;
            try
            {
              newTaskListFolder = defaultTaskListsFolder.Folders[newTaskListName] as Folder;
            }
            catch
            {
              newTaskListFolder = defaultTaskListsFolder.Folders.Add(newTaskListName, OlDefaultFolders.olFolderTasks) as Folder;
              newTaskListFolder.Name = newTaskListName;
            }
            resource.SelectedFolder = new OutlookFolderDescriptor(newTaskListFolder);
            resource.SelectedFolderName = newTaskListFolder.Name;
          }
        }
        using (var selectResourcesForm = SelectResourceForm.CreateForFolderAssignment(_optionTasks, ConnectionTests.ResourceType.Calendar, calendars, addressBooks, taskLists))
        {
          if (selectResourcesForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            var optionList = new List<OptionsModel>();

            foreach (var resource in calendars.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptionsWithCategory (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              optionList.Add (options);
            }

            foreach (var resource in addressBooks.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
              optionList.Add (options);
            }

            foreach (var resource in taskLists.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.SetResourceUrl (options, resource.Model);
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
      eventMappingOptions.EventCategoryColor = ColorHelper.FindMatchingCategoryColor (resource.Color.GetValueOrDefault());
      eventMappingOptions.UseEventCategoryColorAndMapFromCalendarColor = true;
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
            "You need to choose a calendar folder to merge the Kolab resources!",
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