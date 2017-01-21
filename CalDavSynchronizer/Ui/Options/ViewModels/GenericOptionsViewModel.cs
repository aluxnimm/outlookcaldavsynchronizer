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
using System.Net;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class GenericOptionsViewModel : OptionsViewModelBase, IOptionsSection
  {
    private readonly ObservableCollection<ISubOptionsViewModel> _subOptions = new ObservableCollection<ISubOptionsViewModel>();
    private readonly NetworkSettingsViewModel _networkSettingsViewModel;
    private readonly OutlookFolderViewModel _outlookFolderViewModel;
    private readonly IOptionsSection _serverSettingsViewModel;
    private readonly SyncSettingsViewModel _syncSettingsViewModel;
    private readonly TimeRangeViewModel _timeRangeViewModel;
    private ISubOptionsViewModel _mappingConfigurationViewModel;
    private readonly IReadOnlyList<string> _availableCategories;

    public GenericOptionsViewModel (
      IOptionsViewModelParent parent,
      IOptionsSection serverSettingsViewModel,
      IOptionTasks optionTasks,
      OptionsModel model,
      IReadOnlyList<string> availableCategories,
      IViewOptions viewOptions)
        : base (viewOptions, model)
    {
      if (parent == null) throw new ArgumentNullException(nameof(parent));
      if (serverSettingsViewModel == null) throw new ArgumentNullException(nameof(serverSettingsViewModel));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (availableCategories == null) throw new ArgumentNullException(nameof(availableCategories));


      Model = model;
      _availableCategories = availableCategories;

      _syncSettingsViewModel = new SyncSettingsViewModel(model, viewOptions);
      _networkSettingsViewModel = new NetworkSettingsViewModel(model);

      _serverSettingsViewModel = serverSettingsViewModel;
      _outlookFolderViewModel = new OutlookFolderViewModel (model, optionTasks, viewOptions);
      _timeRangeViewModel = new TimeRangeViewModel(model, viewOptions);

      RegisterPropertyChangeHandler(model, nameof(model.MappingConfigurationModelOrNull), UpdateMappingConfigurationViewModel);

      RegisterPropertyChangePropagation(model, nameof(model.SelectedFolderOrNull), nameof(OutlookFolderType));

    }
    

    private ISubOptionsViewModel MappingConfigurationViewModel
    {
      get { return _mappingConfigurationViewModel; }
      set
      {
        if (!ReferenceEquals (value, _mappingConfigurationViewModel))
        {
          _subOptions.Remove (_mappingConfigurationViewModel);
          if (value != null)
            _subOptions.Add (value);
          _mappingConfigurationViewModel = value;
        }
      }
    }
  
    protected override IEnumerable<ISubOptionsViewModel> CreateSubOptions ()
    {
      _subOptions.Add (_networkSettingsViewModel);
      UpdateMappingConfigurationViewModel();
      return _subOptions;
    }

    protected override IEnumerable<IOptionsSection> CreateSections ()
    {
      return new IOptionsSection[] { _outlookFolderViewModel, _serverSettingsViewModel, _syncSettingsViewModel, _timeRangeViewModel };
    }

    private void UpdateMappingConfigurationViewModel()
    {
      if (Model.MappingConfigurationModelOrNull == null)
        MappingConfigurationViewModel = null;
      else if (Model.MappingConfigurationModelOrNull is EventMappingConfigurationModel && !(MappingConfigurationViewModel is EventMappingConfigurationViewModel))
        MappingConfigurationViewModel = new EventMappingConfigurationViewModel(_availableCategories, (EventMappingConfigurationModel)Model.MappingConfigurationModelOrNull, Model);
      else if (Model.MappingConfigurationModelOrNull is ContactMappingConfigurationModel && !(MappingConfigurationViewModel is ContactMappingConfigurationViewModel))
        MappingConfigurationViewModel = new ContactMappingConfigurationViewModel((ContactMappingConfigurationModel)Model.MappingConfigurationModelOrNull);
      else if (Model.MappingConfigurationModelOrNull is TaskMappingConfigurationModel && !(MappingConfigurationViewModel is TaskMappingConfigurationViewModel))
        MappingConfigurationViewModel = new TaskMappingConfigurationViewModel(_availableCategories, (TaskMappingConfigurationModel)Model.MappingConfigurationModelOrNull, ViewOptions);
    }

    public override OptionsModel Model { get; }

    public override bool Validate(StringBuilder errorMessageBuilder)
    {
      return Model.Validate(errorMessageBuilder);
    }
   
    public static OlItemType olAppointmentItem { get; } = OlItemType.olAppointmentItem;
    public static OlItemType olTaskItem { get; } = OlItemType.olTaskItem;

    public override OlItemType? OutlookFolderType => Model.SelectedFolderOrNull?.DefaultItemType;

    public static GenericOptionsViewModel DesignInstance => new GenericOptionsViewModel(
      new DesignOptionsViewModelParent(),
      ViewModels.ServerSettingsViewModel.DesignInstance,
      NullOptionTasks.Instance,
      OptionsModel.DesignInstance,
      new[] {"Cat1", "Cat2"},
      OptionsCollectionViewModel.DesignViewOptions)
    {
      IsActive = true,
      Name = "Test Profile",
    };

    public OutlookFolderViewModel OutlookFolderViewModel
    {
      get { return _outlookFolderViewModel; }
    }

    public IOptionsSection ServerSettingsViewModel
    {
      get { return _serverSettingsViewModel; }
    }

    public SyncSettingsViewModel SyncSettingsViewModel
    {
      get { return _syncSettingsViewModel; }
    }

    public TimeRangeViewModel TimeRangeViewModel
    {
      get { return _timeRangeViewModel; }
    }
  }
}