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

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class TaskMappingConfigurationViewModel : ViewModelBase, IOptionsViewModel
  {
    private readonly ObservableCollection<IOptionsViewModel> _subOptions = new ObservableCollection<IOptionsViewModel>();
    private bool _mapBody;
    private bool _mapPriority;
    private bool _mapRecurringTasks;
    private ReminderMapping _mapReminder;
    private bool _isSelected;

    public IList<Item<ReminderMapping>> AvailableReminderMappings => new List<Item<ReminderMapping>>
                                                                     {
                                                                         new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                         new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                         new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders")
                                                                     };

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
    public ReminderMapping MapReminder
    {
      get { return _mapReminder; }
      set
      {
        CheckedPropertyChange (ref _mapReminder, value);
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        CheckedPropertyChange (ref _isSelected, value);
      }
    }

    public static TaskMappingConfigurationViewModel DesignInstance => new TaskMappingConfigurationViewModel
                                                                         {
                                                                              MapBody = true,
                                                                              MapPriority = true,
                                                                              MapRecurringTasks = true,
                                                                              MapReminder = ReminderMapping.JustUpcoming
                                                                         };


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SetOptions(options.MappingConfiguration as TaskMappingConfiguration ?? new TaskMappingConfiguration());
    }

    public void SetOptions (TaskMappingConfiguration mappingConfiguration)
    {
      MapBody = mappingConfiguration.MapBody;
      MapPriority = mappingConfiguration.MapPriority;
      MapRecurringTasks = mappingConfiguration.MapRecurringTasks;
      MapReminder = mappingConfiguration.MapReminder;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new TaskMappingConfiguration
                                     {
                                         MapBody = _mapBody,
                                         MapPriority = _mapPriority,
                                         MapRecurringTasks = _mapRecurringTasks,
                                         MapReminder = _mapReminder
                                     };
    }

    public string Name => "Task mapping configuration";

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }

    public IEnumerable<IOptionsViewModel> SubOptions => _subOptions;
  }
}