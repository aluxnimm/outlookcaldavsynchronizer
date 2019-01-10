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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options.Models;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class TaskMappingConfigurationViewModel : ModelBase, ISubOptionsViewModel
  {
    private readonly TaskMappingConfigurationModel _model;
    private readonly CustomPropertyMappingViewModel _customPropertyMappingViewModel;

    private bool _isSelected;
    private bool _isExpanded;


    public TaskMappingConfigurationViewModel(IReadOnlyList<string> availableCategories, TaskMappingConfigurationModel model, IViewOptions viewOptions)
    {
      if (availableCategories == null)
        throw new ArgumentNullException(nameof(availableCategories));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      AvailableCategories = availableCategories;
      _model = model;
      ViewOptions = viewOptions;

      _customPropertyMappingViewModel = new CustomPropertyMappingViewModel(model);
      Items = new[] { _customPropertyMappingViewModel };


      RegisterPropertyChangePropagation(_model, nameof(_model.MapBody), nameof(MapBody));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapPriority), nameof(MapPriority));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapRecurringTasks), nameof(MapRecurringTasks));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapStartAndDueAsFloating), nameof(MapStartAndDueAsFloating));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapReminder), nameof(MapReminder));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapReminderAsDateTime), nameof(MapReminderAsDateTime));
      RegisterPropertyChangePropagation(_model, nameof(_model.TaskCategory), nameof(TaskCategory));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseTaskCategoryAsFilter), nameof(UseTaskCategoryAsFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.IncludeEmptyTaskCategoryFilter), nameof(IncludeEmptyTaskCategoryFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.InvertTaskCategoryFilter), nameof(InvertTaskCategoryFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.IsCategoryFilterSticky), nameof(IsCategoryFilterSticky));
    }

    public IList<Item<ReminderMapping>> AvailableReminderMappings => new List<Item<ReminderMapping>>
                                                                     {
                                                                         new Item<ReminderMapping> (ReminderMapping.@true,  Strings.Get($"Yes")),
                                                                         new Item<ReminderMapping> (ReminderMapping.@false,  Strings.Get($"No")),
                                                                         new Item<ReminderMapping> (ReminderMapping.JustUpcoming,  Strings.Get($"Just upcoming reminders"))
                                                                     };

    public IReadOnlyList<string> AvailableCategories { get; }

    public bool IsCategoryFilterSticky
    {
      get { return _model.IsCategoryFilterSticky; }
      set { _model.IsCategoryFilterSticky = value; }
    }

    public bool MapBody
    {
      get { return _model.MapBody; }
      set { _model.MapBody = value; }
    }

    public bool MapPriority
    {
      get { return _model.MapPriority; }
      set { _model.MapPriority = value; }
    }

    public bool MapRecurringTasks
    {
      get { return _model.MapRecurringTasks; }
      set { _model.MapRecurringTasks = value; }
    }

    public bool MapStartAndDueAsFloating
    {
      get { return _model.MapStartAndDueAsFloating; }
      set { _model.MapStartAndDueAsFloating = value; }
    }

    public ReminderMapping MapReminder
    {
      get { return _model.MapReminder; }
      set { _model.MapReminder = value; }
    }

    public bool MapReminderAsDateTime
    {
      get { return _model.MapReminderAsDateTime; }
      set { _model.MapReminderAsDateTime = value; }
    }

    public string TaskCategory
    {
      get { return _model.TaskCategory; }
      set { _model.TaskCategory = value; }
    }

    public bool UseTaskCategoryAsFilter => !String.IsNullOrEmpty(_model.TaskCategory);

    public bool IncludeEmptyTaskCategoryFilter
    {
      get { return _model.IncludeEmptyTaskCategoryFilter; }
      set { _model.IncludeEmptyTaskCategoryFilter = value; }
    }

    public bool InvertTaskCategoryFilter
    {
      get { return _model.InvertTaskCategoryFilter; }
      set { _model.InvertTaskCategoryFilter = value; }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        CheckedPropertyChange(ref _isSelected, value);
      }
    }

    public bool IsExpanded
    {
      get { return _isExpanded; }
      set
      {
        CheckedPropertyChange(ref _isExpanded, value);
      }
    }

    public static TaskMappingConfigurationViewModel DesignInstance => new TaskMappingConfigurationViewModel(new[] { "Cat1", "Cat2" }, new TaskMappingConfigurationModel(new TaskMappingConfiguration()), OptionsCollectionViewModel.DesignViewOptions)
    {
      MapBody = true,
      MapPriority = true,
      MapRecurringTasks = true,
      MapStartAndDueAsFloating = false,
      MapReminder = ReminderMapping.JustUpcoming,
      TaskCategory = "TheCategory",
      IncludeEmptyTaskCategoryFilter = false,
      InvertTaskCategoryFilter = true,
    };


    public string Name => Strings.Get($"Task Mapping Configuration");


    public IEnumerable<ITreeNodeViewModel> Items { get; }
    public IViewOptions ViewOptions { get; }
  }
}