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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class TaskMappingConfigurationModel : MappingConfigurationModel, ICustomPropertiesMappingConfigurationModel
  {
    private readonly ObservableCollection<ISubOptionsViewModel> _subOptions = new ObservableCollection<ISubOptionsViewModel>();
    private bool _mapBody;
    private bool _mapPriority;
    private bool _mapRecurringTasks;
    private bool _mapStartAndDueAsFloating;
    private ReminderMapping _mapReminder;
    private bool _mapReminderAsDateTime;
    private string _taskCategory;
    private bool _includeEmptyTaskCategoryFilter;
    private bool _invertTaskCategoryFilter;
    private bool _mapCustomProperties;
    private bool _isCategoryFilterSticky;

    public TaskMappingConfigurationModel (TaskMappingConfiguration data)
    {
      if (data == null) throw new ArgumentNullException(nameof(data));

      InitializeData(data);
    }

    public ObservableCollection<PropertyMappingModel> Mappings { get; } = new ObservableCollection<PropertyMappingModel>();

    public bool MapBody
    {
      get { return _mapBody; }
      set
      {
        CheckedPropertyChange (ref _mapBody, value);
      }
    }

    public bool MapPriority
    {
      get { return _mapPriority; }
      set
      {
        CheckedPropertyChange (ref _mapPriority, value);
      }
    }

    public bool MapRecurringTasks
    {
      get { return _mapRecurringTasks; }
      set
      {
        CheckedPropertyChange (ref _mapRecurringTasks, value);
      }
    }
    public bool MapStartAndDueAsFloating
    {
      get { return _mapStartAndDueAsFloating; }
      set
      {
        CheckedPropertyChange (ref _mapStartAndDueAsFloating, value);
      }
    }
    public ReminderMapping MapReminder
    {
      get { return _mapReminder; }
      set
      {
        CheckedPropertyChange (ref _mapReminder, value);
      }
    }
    public bool MapReminderAsDateTime
    {
      get { return _mapReminderAsDateTime; }
      set
      {
        CheckedPropertyChange(ref _mapReminderAsDateTime, value);
      }
    }

    public string TaskCategory
    {
      get { return _taskCategory; }
      set
      {
        CheckedPropertyChange(ref _taskCategory, value);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(nameof(UseTaskCategoryAsFilter));
      }
    }

    public bool IsCategoryFilterSticky
    {
      get { return _isCategoryFilterSticky; }
      set
      {
        CheckedPropertyChange (ref _isCategoryFilterSticky, value);
      }
    }

    public bool UseTaskCategoryAsFilter => !String.IsNullOrEmpty(_taskCategory);

    public bool IncludeEmptyTaskCategoryFilter
    {
      get { return _includeEmptyTaskCategoryFilter; }
      set
      {
        if (value)
        {
          InvertTaskCategoryFilter = false;
        }
        CheckedPropertyChange (ref _includeEmptyTaskCategoryFilter, value);
      }
    }
    public bool InvertTaskCategoryFilter
    {
      get { return _invertTaskCategoryFilter; }
      set
      {
        if (value)
        {
          IncludeEmptyTaskCategoryFilter = false;
        }
        CheckedPropertyChange (ref _invertTaskCategoryFilter, value);
      }
    }

    public bool MapCustomProperties
    {
      get { return _mapCustomProperties; }
      set
      {
        CheckedPropertyChange(ref _mapCustomProperties, value);
      }
    }


    private void InitializeData (TaskMappingConfiguration mappingConfiguration)
    {
      MapBody = mappingConfiguration.MapBody;
      MapPriority = mappingConfiguration.MapPriority;
      MapRecurringTasks = mappingConfiguration.MapRecurringTasks;
      MapStartAndDueAsFloating = mappingConfiguration.MapStartAndDueAsFloating;
      MapReminder = mappingConfiguration.MapReminder;
      MapReminderAsDateTime = mappingConfiguration.MapReminderAsDateTime;
      TaskCategory = mappingConfiguration.TaskCategory;
      IncludeEmptyTaskCategoryFilter = mappingConfiguration.IncludeEmptyTaskCategoryFilter;
      InvertTaskCategoryFilter = mappingConfiguration.InvertTaskCategoryFilter;
      _mapCustomProperties = mappingConfiguration.MapCustomProperties;
      _isCategoryFilterSticky = mappingConfiguration.IsCategoryFilterSticky;

      if (mappingConfiguration.UserDefinedCustomPropertyMappings != null)
        Array.ForEach(mappingConfiguration.UserDefinedCustomPropertyMappings, m => Mappings.Add(new PropertyMappingModel(m)));
    }

    public override MappingConfigurationBase GetData()
    {
      return new TaskMappingConfiguration
      {
        MapBody = _mapBody,
        MapPriority = _mapPriority,
        MapRecurringTasks = _mapRecurringTasks,
        MapStartAndDueAsFloating = _mapStartAndDueAsFloating,
        MapReminder = _mapReminder,
        MapReminderAsDateTime = _mapReminderAsDateTime,
        TaskCategory = _taskCategory,
        IncludeEmptyTaskCategoryFilter = _includeEmptyTaskCategoryFilter,
        InvertTaskCategoryFilter = _invertTaskCategoryFilter,
        MapCustomProperties = _mapCustomProperties,
        UserDefinedCustomPropertyMappings = Mappings.Select(m => m.GetData()).ToArray(),
        IsCategoryFilterSticky = _isCategoryFilterSticky
      };
    }

    public override void AddOneTimeTasks(Action<OneTimeChangeCategoryTask> add)
    {
    
    }

    public override bool Validate (StringBuilder errorMessageBuilder)
    {
      return EventMappingConfigurationModel.ValidatePropertyMappings(errorMessageBuilder, Mappings);
    }
    
  }
}