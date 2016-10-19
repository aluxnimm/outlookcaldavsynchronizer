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
using CalDavSynchronizer.Ui.Options.ResourceSelection.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class MultipleOptionsTemplateViewModel : ViewModelBase, IOptionsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private readonly NetworkSettingsViewModel _networkSettingsViewModel;
    private readonly IServerSettingsTemplateViewModel _serverSettingsViewModel;
    private readonly ProfileType _profileType;
    private readonly GeneralOptions _generalOptions;
    private readonly DelegateCommandWithoutCanExecuteDelegation _discoverResourcesCommand;
    private readonly DelegateCommandWithoutCanExecuteDelegation _getAccountSettingsCommand;

    private string _name;
    private bool _isSelected;
    private readonly IOptionsViewModelParent _parent;
    private readonly IOptionTasks _optionTasks;

    public MultipleOptionsTemplateViewModel (
        IOptionsViewModelParent parent,
        GeneralOptions generalOptions,
        IServerSettingsTemplateViewModel serverSettingsViewModel,
        ProfileType profileType,
        IOptionTasks optionTasks)

    {
      _parent = parent;
      if (parent == null)
        throw new ArgumentNullException (nameof (parent));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));

      _discoverResourcesCommand = new DelegateCommandWithoutCanExecuteDelegation (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        DiscoverResourcesAsync();
      });

      _getAccountSettingsCommand = new DelegateCommandWithoutCanExecuteDelegation(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        GetAccountSettings();
      });

      _networkSettingsViewModel = new NetworkSettingsViewModel();

      SubOptions = new[] { _networkSettingsViewModel };

      _serverSettingsViewModel = serverSettingsViewModel;
      _profileType = profileType;
      _optionTasks = optionTasks;
      _generalOptions = generalOptions;
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

    private async void DiscoverResourcesAsync ()
    {
      _discoverResourcesCommand.SetCanExecute (false);
      try
      {
        var serverResources = await _serverSettingsViewModel.GetServerResources (_networkSettingsViewModel, _generalOptions);

        var calendars = serverResources.Calendars.Select (c => new CalendarDataViewModel (c)).ToArray();
        var addressBooks = serverResources.AddressBooks.Select (a => new AddressBookDataViewModel (a)).ToArray();
        var taskLists = serverResources.TaskLists.Select (d => new TaskListDataViewModel (d)).ToArray();

        using (var selectResourcesForm =  SelectResourceForm.CreateForFolderAssignment(_optionTasks, ConnectionTests.ResourceType.Calendar, calendars, addressBooks, taskLists))
        {
          if (selectResourcesForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
          {
            List<Contracts.Options> optionList = new List<Contracts.Options>();

            foreach (var resource in calendars.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions(resource);
              _serverSettingsViewModel.FillOptions (options, resource.Model);
              optionList.Add (options);
            }

            foreach (var resource in addressBooks.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.FillOptions (options, resource.Model);
              optionList.Add (options);
            }

            foreach (var resource in taskLists.Where (c => c.SelectedFolder != null))
            {
              var options = CreateOptions (resource);
              _serverSettingsViewModel.FillOptions (options, resource.Model);
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

    private Contracts.Options CreateOptions (ResourceDataViewModelBase resource)
    {
      var options = Contracts.Options.CreateDefault (_profileType);
      options.Name = $"{Name} ({resource.Name})";
      options.OutlookFolderEntryId = resource.SelectedFolder.FolderId;
      options.OutlookFolderStoreId = resource.SelectedFolder.StoreId;
      _networkSettingsViewModel.FillOptions (options);
      return options;
    }

    public IServerSettingsTemplateViewModel ServerSettingsViewModel => _serverSettingsViewModel;

    public ICommand DiscoverResourcesCommand => _discoverResourcesCommand;
    public ICommand GetAccountSettingsCommand => _getAccountSettingsCommand;

    public bool IsActive { get; set; }
    public bool SupportsIsActive { get; } = false;
    public Guid Id { get; private set; }
    public IEnumerable<ISubOptionsViewModel> SubOptions { get; }

    public bool? IsMultipleOptionsTemplateViewModel { get; } = true;
    public OlItemType? OutlookFolderType { get; } = null;

    public bool IsSelected
    {
      get { return _isSelected; }
      set { CheckedPropertyChange (ref _isSelected, value); }
    }

    public string Name
    {
      get { return _name; }
      set { CheckedPropertyChange (ref _name, value); }
    }

    public void SetOptions (Contracts.Options options)
    {
      _networkSettingsViewModel.SetOptions (options);
      _serverSettingsViewModel.SetOptions (options);
      Name = _profileType.ToString();
      Id = options.Id;
    }

    public Contracts.Options GetOptionsOrNull () => null;
    public bool Validate (StringBuilder errorMessageBuilder) => true;
    
  }
}