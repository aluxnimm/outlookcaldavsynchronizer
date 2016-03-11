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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Utilities;
using Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Windows.Input;
using log4net;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class EventMappingConfigurationViewModel : ViewModelBase, IOptionsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private OlCategoryShortcutKey _categoryShortcutKey;
    private bool _createEventsInUtc;
    private string _eventCategory;
    private OlCategoryColor _eventCategoryColor;
    private bool _invertEventCategoryFilter;
    private bool _mapAttendees;
    private bool _mapBody;
    private bool _mapClassConfidentialToSensitivityPrivate;
    private ReminderMapping _mapReminder;
    private bool _mapSensitivityPrivateToClassConfidential;
    private bool _scheduleAgentClient;
    private bool _sendNoAppointmentNotifications;
    private bool _useEventCategoryColorAndMapFromCalendarColor;
    private readonly ICurrentOptions _currentOptions;

    public IList<Item<ReminderMapping>> AvailableReminderMappings { get; } = new List<Item<ReminderMapping>>
                                                                             {
                                                                                 new Item<ReminderMapping> (ReminderMapping.@true, "Yes"),
                                                                                 new Item<ReminderMapping> (ReminderMapping.@false, "No"),
                                                                                 new Item<ReminderMapping> (ReminderMapping.JustUpcoming, "Just upcoming reminders")
                                                                             };

    public IList<Item<OlCategoryShortcutKey>> AvailableShortcutKeys { get; } = new List<Item<OlCategoryShortcutKey>>
                                                                               {
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyNone, "None"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF2, "Ctrl+F2"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF3, "Ctrl+F3"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF4, "Ctrl+F4"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF5, "Ctrl+F5"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF6, "Ctrl+F6"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF7, "Ctrl+F7"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF8, "Ctrl+F8"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF9, "Ctrl+F9"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF10, "Ctrl+F10"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF11, "Ctrl+F11"),
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF12, "Ctrl+F12")
                                                                               };

    public IList<Item<OlCategoryColor>> AvailableEventCategoryColors { get; } = 
      ColorHelper.CategoryColors.Select (kv => new Item<OlCategoryColor> (kv.Key, kv.Key.ToString().Substring (15))).ToList();

    public ICommand GetServerCalendarColorCommand { get; }
    public ICommand SetServerCalendarColorCommand { get; }

    public OlCategoryShortcutKey CategoryShortcutKey
    {
      get { return _categoryShortcutKey; }
      set
      {
        _categoryShortcutKey = value;
        OnPropertyChanged();
      }
    }

    public bool CreateEventsInUtc
    {
      get { return _createEventsInUtc; }
      set
      {
        _createEventsInUtc = value;
        OnPropertyChanged();
      }
    }

    public string EventCategory
    {
      get { return _eventCategory; }
      set
      {
        _eventCategory = value;
        OnPropertyChanged();
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged(nameof(UseEventCategoryAsFilter));
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof(UseEventCategoryAsFilterAndMapColor));
      }
    }

    public bool UseEventCategoryAsFilter => !String.IsNullOrEmpty (_eventCategory);
    public bool UseEventCategoryAsFilterAndMapColor => !String.IsNullOrEmpty (_eventCategory) && _useEventCategoryColorAndMapFromCalendarColor;


    public OlCategoryColor EventCategoryColor
    {
      get { return _eventCategoryColor; }
      set
      {
        _eventCategoryColor = value;
        OnPropertyChanged();
      }
    }

    public bool InvertEventCategoryFilter
    {
      get { return _invertEventCategoryFilter; }
      set
      {
        _invertEventCategoryFilter = value;
        OnPropertyChanged();
      }
    }

    public bool MapAttendees
    {
      get { return _mapAttendees; }
      set
      {
        _mapAttendees = value;
        OnPropertyChanged();
      }
    }

    public bool MapBody
    {
      get { return _mapBody; }
      set
      {
        _mapBody = value;
        OnPropertyChanged();
      }
    }

    public bool MapClassConfidentialToSensitivityPrivate
    {
      get { return _mapClassConfidentialToSensitivityPrivate; }
      set
      {
        _mapClassConfidentialToSensitivityPrivate = value;
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

    public bool MapSensitivityPrivateToClassConfidential
    {
      get { return _mapSensitivityPrivateToClassConfidential; }
      set
      {
        _mapSensitivityPrivateToClassConfidential = value;
        OnPropertyChanged();
      }
    }

    public bool ScheduleAgentClient
    {
      get { return _scheduleAgentClient; }
      set
      {
        _scheduleAgentClient = value;
        OnPropertyChanged();
      }
    }

    public bool SendNoAppointmentNotifications
    {
      get { return _sendNoAppointmentNotifications; }
      set
      {
        _sendNoAppointmentNotifications = value;
        OnPropertyChanged();
      }
    }

    public bool UseEventCategoryColorAndMapFromCalendarColor
    {
      get { return _useEventCategoryColorAndMapFromCalendarColor; }
      set
      {
        _useEventCategoryColorAndMapFromCalendarColor = value;
        OnPropertyChanged();
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (UseEventCategoryAsFilterAndMapColor));
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

    public IReadOnlyList<string> AvailableCategories { get; }


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SetOptions(options.MappingConfiguration as EventMappingConfiguration ?? new EventMappingConfiguration());
    }

    public void SetOptions (EventMappingConfiguration mappingConfiguration)
    {
      CategoryShortcutKey = mappingConfiguration.CategoryShortcutKey;
      CreateEventsInUtc = mappingConfiguration.CreateEventsInUTC;
      EventCategory = mappingConfiguration.EventCategory;
      EventCategoryColor = mappingConfiguration.EventCategoryColor;
      InvertEventCategoryFilter = mappingConfiguration.InvertEventCategoryFilter;
      MapAttendees = mappingConfiguration.MapAttendees;
      MapBody = mappingConfiguration.MapBody;
      MapClassConfidentialToSensitivityPrivate = mappingConfiguration.MapClassConfidentialToSensitivityPrivate;
      MapReminder = mappingConfiguration.MapReminder;
      MapSensitivityPrivateToClassConfidential = mappingConfiguration.MapSensitivityPrivateToClassConfidential;
      ScheduleAgentClient = mappingConfiguration.ScheduleAgentClient;
      SendNoAppointmentNotifications = mappingConfiguration.SendNoAppointmentNotifications;
      UseEventCategoryColorAndMapFromCalendarColor = mappingConfiguration.UseEventCategoryColorAndMapFromCalendarColor;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new EventMappingConfiguration
                                     {
                                         CategoryShortcutKey = _categoryShortcutKey,
                                         CreateEventsInUTC = _createEventsInUtc,
                                         EventCategory = _eventCategory,
                                         EventCategoryColor = _eventCategoryColor,
                                         InvertEventCategoryFilter = _invertEventCategoryFilter,
                                         MapAttendees = _mapAttendees,
                                         MapBody = _mapBody,
                                         MapClassConfidentialToSensitivityPrivate = _mapClassConfidentialToSensitivityPrivate,
                                         MapReminder = _mapReminder,
                                         MapSensitivityPrivateToClassConfidential = _mapSensitivityPrivateToClassConfidential,
                                         ScheduleAgentClient = _scheduleAgentClient,
                                         SendNoAppointmentNotifications = _sendNoAppointmentNotifications,
                                         UseEventCategoryColorAndMapFromCalendarColor = _useEventCategoryColorAndMapFromCalendarColor
                                     };
    }

    public string Name => "Event mapping configuration";

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }


    public IEnumerable<IOptionsViewModel> SubOptions => new IOptionsViewModel[] { };

    public static EventMappingConfigurationViewModel DesignInstance = new EventMappingConfigurationViewModel(new[] {"Cat1","Cat2"}, new DesignCurrentOptions())
                                                                      {
                                                                          CategoryShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF4,
                                                                          CreateEventsInUtc = true,
                                                                          EventCategory = "TheCategory",
                                                                          EventCategoryColor = OlCategoryColor.olCategoryColorDarkMaroon,
                                                                          InvertEventCategoryFilter = true,
                                                                          MapAttendees = true,
                                                                          MapBody = true,
                                                                          MapClassConfidentialToSensitivityPrivate = true,
                                                                          MapReminder = ReminderMapping.JustUpcoming,
                                                                          MapSensitivityPrivateToClassConfidential = true,
                                                                          ScheduleAgentClient = true,
                                                                          SendNoAppointmentNotifications = true,
                                                                          UseEventCategoryColorAndMapFromCalendarColor = true
                                                                      };

    private bool _isSelected;

    public EventMappingConfigurationViewModel (IReadOnlyList<string> availableCategories, ICurrentOptions currentOptions)
    {
      if (availableCategories == null)
        throw new ArgumentNullException (nameof (availableCategories));
      if (currentOptions == null)
        throw new ArgumentNullException (nameof (currentOptions));

      AvailableCategories = availableCategories;
      _currentOptions = currentOptions;
      SetServerCalendarColorCommand = new DelegateCommand (_ => SetServerCalendarColor());
      GetServerCalendarColorCommand = new DelegateCommand (_ => GetServerCalendarColor());
    }

    private async void GetServerCalendarColor ()
    {
      try
      {
        var serverColor = await _currentOptions.CreateCalDavDataAccess().GetCalendarColorNoThrow();

        if (serverColor != null)
        {
          EventCategoryColor = ColorHelper.FindMatchingCategoryColor (serverColor.Value);
        }
      }
      catch (System.Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }

    private async void SetServerCalendarColor ()
    {
      try
      {
        if (EventCategoryColor != OlCategoryColor.olCategoryColorNone)
        {
          if (await _currentOptions.CreateCalDavDataAccess ().SetCalendarColorNoThrow (ColorHelper.CategoryColors[EventCategoryColor]))
          {
            System.Windows.MessageBox.Show ("Successfully updated the server calendar color!");
          }
          else
          {
            System.Windows.MessageBox.Show ("Error updating the server calendar color!");
          }
        }
        else
        {
          System.Windows.MessageBox.Show ("No color set for updating the server calendar color!");
        }
      }
      catch (System.Exception x)
      {
        ExceptionHandler.Instance.DisplayException (x, s_logger);
      }
    }
  }
}