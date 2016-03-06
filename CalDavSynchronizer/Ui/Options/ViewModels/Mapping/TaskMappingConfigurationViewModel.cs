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
    private ReminderMapping _mapReminder;

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

    public ReminderMapping MapReminder
    {
      get { return _mapReminder; }
      set
      {
        _mapReminder = value;
        OnPropertyChanged();
      }
    }

    public static TaskMappingConfigurationViewModel DesignInstance => new TaskMappingConfigurationViewModel
                                                                         {
                                                                              MapBody = true,
                                                                              MapPriority = true,
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
      MapReminder = mappingConfiguration.MapReminder;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new TaskMappingConfiguration
                                     {
                                         MapBody = _mapBody,
                                         MapPriority = _mapPriority,
                                         MapReminder = _mapReminder
                                     };
    }

    public string Name => "Task mapping configuration";

    public bool Validate (StringBuilder errorBuilder)
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