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
  public class TaskMappingConfigurationViewModel : INotifyPropertyChanged, IOptionsViewModel
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
        _mapBody = value;
        OnPropertyChanged();
      }
    }

    public bool MapPriority
    {
      get { return _mapPriority; }
      set
      {
        _mapPriority = value;
        OnPropertyChanged();
      }
    }

    public bool MapRecurringTasks
    {
      get { return _mapRecurringTasks; }
      set
      {
        _mapRecurringTasks = value;
        OnPropertyChanged();
      }
    }
    public ReminderMapping MapReminder
    {
      get { return _mapReminder; }
      set
      {
        _mapReminder = value;
        OnPropertyChanged();
      }
    }

    public bool IsSelected
    {
      get { return _isSelected; }
      set
      {
        _isSelected = value;
        OnPropertyChanged ();
      }
    }

    public static TaskMappingConfigurationViewModel DesignInstance => new TaskMappingConfigurationViewModel
                                                                         {
                                                                              MapBody = true,
                                                                              MapPriority = true,
                                                                              MapRecurringTasks = true,
                                                                              MapReminder = ReminderMapping.JustUpcoming
                                                                         };


    public event PropertyChangedEventHandler PropertyChanged;

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

    protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
    {
      PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
    }
  }
}