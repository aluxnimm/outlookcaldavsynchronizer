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
using System.Collections;
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
using NodaTime.TimeZones;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class EventMappingConfigurationViewModel : ViewModelBase, ISubOptionsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private OlCategoryShortcutKey _categoryShortcutKey;
    private bool _createEventsInUtc;
    private bool _useIanaTz;
    private string _eventTz;
    private bool _includeHistoricalData;
    private bool _useGlobalAppointmentID;
    private string _eventCategory;
    private OlCategoryColor _eventCategoryColor;
    private bool _invertEventCategoryFilter;
    private bool _mapAttendees;
    private bool _mapBody;
    private bool _mapClassConfidentialToSensitivityPrivate;
    private bool _mapClassPublicToSensitivityPrivate;
    private ReminderMapping _mapReminder;
    private bool _mapSensitivityPrivateToClassConfidential;
    private bool _scheduleAgentClient;
    private bool _sendNoAppointmentNotifications;
    private bool _useEventCategoryColorAndMapFromCalendarColor;
    private readonly ICurrentOptions _currentOptions;
    private bool _cleanupDuplicateEvents;

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

    public IReadOnlyList<string> AvailableTimezones
    {
      get
      {
        var zones = TzdbDateTimeZoneSource.Default.CanonicalIdMap.Values.Distinct().Where (v => v.Contains ("/")).ToList();
        zones.Sort();
        return zones;
      }
    }

    public IList<Item<OlCategoryColor>> AvailableEventCategoryColors { get; } = 
      ColorHelper.CategoryColors.Select (kv => new Item<OlCategoryColor> (kv.Key, kv.Key.ToString().Substring (15))).ToList();

    public ICommand GetServerCalendarColorCommand { get; }
    public ICommand SetServerCalendarColorCommand { get; }

    public OlCategoryShortcutKey CategoryShortcutKey
    {
      get { return _categoryShortcutKey; }
      set
      {
        CheckedPropertyChange (ref _categoryShortcutKey, value);
      }
    }

    public bool CreateEventsInUtc
    {
      get { return _createEventsInUtc; }
      set
      {
        if (value)
        {
          UseIanaTz = false;
          IncludeHistoricalData = false;
        }
        CheckedPropertyChange (ref _createEventsInUtc, value);
      }
    }

    public bool UseIanaTz
    {
      get { return _useIanaTz; }
      set
      {
        if (value)
          CreateEventsInUtc = false;

        CheckedPropertyChange (ref _useIanaTz, value);
      }
    }
    public string EventTz
    {
      get { return _eventTz; }
      set
      {
        CheckedPropertyChange (ref _eventTz, value);
      }
    }
    public bool IncludeHistoricalData
    {
      get { return _includeHistoricalData; }
      set
      {
  
        CheckedPropertyChange (ref _includeHistoricalData, value);
      }
    }
    public bool UseGlobalAppointmendID
    {
      get { return _useGlobalAppointmentID; }
      set
      {
        CheckedPropertyChange (ref _useGlobalAppointmentID, value);
      }
    }

    public bool CleanupDuplicateEvents
    {
      get { return _cleanupDuplicateEvents; }
      set
      {
        CheckedPropertyChange (ref _cleanupDuplicateEvents, value);
      }
    }

    public string EventCategory
    {
      get { return _eventCategory; }
      set
      {
        CheckedPropertyChange (ref _eventCategory, value);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof(UseEventCategoryAsFilter));
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
        CheckedPropertyChange (ref _eventCategoryColor, value);
      }
    }

    public bool InvertEventCategoryFilter
    {
      get { return _invertEventCategoryFilter; }
      set
      {
        CheckedPropertyChange (ref _invertEventCategoryFilter, value);
      }
    }

    public bool MapAttendees
    {
      get { return _mapAttendees; }
      set
      {
        CheckedPropertyChange (ref _mapAttendees, value);
      }
    }

    public bool MapBody
    {
      get { return _mapBody; }
      set
      {
        CheckedPropertyChange (ref _mapBody, value);
      }
    }

    public bool MapClassConfidentialToSensitivityPrivate
    {
      get { return _mapClassConfidentialToSensitivityPrivate; }
      set
      {
        CheckedPropertyChange (ref _mapClassConfidentialToSensitivityPrivate, value);
      }
    }

    public bool MapClassPublicToSensitivityPrivate
    {
      get { return _mapClassPublicToSensitivityPrivate; }
      set
      {
        CheckedPropertyChange (ref _mapClassPublicToSensitivityPrivate, value);
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

    public bool MapSensitivityPrivateToClassConfidential
    {
      get { return _mapSensitivityPrivateToClassConfidential; }
      set
      {
        CheckedPropertyChange (ref _mapSensitivityPrivateToClassConfidential, value);
      }
    }

    public bool ScheduleAgentClient
    {
      get { return _scheduleAgentClient; }
      set
      {
        CheckedPropertyChange (ref _scheduleAgentClient, value);
      }
    }

    public bool SendNoAppointmentNotifications
    {
      get { return _sendNoAppointmentNotifications; }
      set
      {
        CheckedPropertyChange (ref _sendNoAppointmentNotifications, value);
      }
    }

    public bool UseEventCategoryColorAndMapFromCalendarColor
    {
      get { return _useEventCategoryColorAndMapFromCalendarColor; }
      set
      {
        CheckedPropertyChange (ref _useEventCategoryColorAndMapFromCalendarColor, value);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (UseEventCategoryAsFilterAndMapColor));
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

    public IReadOnlyList<string> AvailableCategories { get; }


    public void SetOptions (CalDavSynchronizer.Contracts.Options options)
    {
      SetOptions(options.MappingConfiguration as EventMappingConfiguration ?? new EventMappingConfiguration());
    }

    public void SetOptions (EventMappingConfiguration mappingConfiguration)
    {
      CategoryShortcutKey = mappingConfiguration.CategoryShortcutKey;
      CreateEventsInUtc = mappingConfiguration.CreateEventsInUTC;
      UseIanaTz = mappingConfiguration.UseIanaTz;
      EventTz = mappingConfiguration.EventTz;
      IncludeHistoricalData = mappingConfiguration.IncludeHistoricalData;
      UseGlobalAppointmendID = mappingConfiguration.UseGlobalAppointmentID;
      EventCategory = mappingConfiguration.EventCategory;
      EventCategoryColor = mappingConfiguration.EventCategoryColor;
      InvertEventCategoryFilter = mappingConfiguration.InvertEventCategoryFilter;
      MapAttendees = mappingConfiguration.MapAttendees;
      MapBody = mappingConfiguration.MapBody;
      MapClassConfidentialToSensitivityPrivate = mappingConfiguration.MapClassConfidentialToSensitivityPrivate;
      MapClassPublicToSensitivityPrivate = mappingConfiguration.MapClassPublicToSensitivityPrivate;
      MapReminder = mappingConfiguration.MapReminder;
      MapSensitivityPrivateToClassConfidential = mappingConfiguration.MapSensitivityPrivateToClassConfidential;
      ScheduleAgentClient = mappingConfiguration.ScheduleAgentClient;
      SendNoAppointmentNotifications = mappingConfiguration.SendNoAppointmentNotifications;
      UseEventCategoryColorAndMapFromCalendarColor = mappingConfiguration.UseEventCategoryColorAndMapFromCalendarColor;
      CleanupDuplicateEvents = mappingConfiguration.CleanupDuplicateEvents;
    }

    public void FillOptions (CalDavSynchronizer.Contracts.Options options)
    {
      options.MappingConfiguration = new EventMappingConfiguration
                                     {
                                         CategoryShortcutKey = _categoryShortcutKey,
                                         CreateEventsInUTC = _createEventsInUtc,
                                         UseIanaTz = _useIanaTz,
                                         EventTz = _eventTz,
                                         IncludeHistoricalData = _includeHistoricalData,
                                         UseGlobalAppointmentID = _useGlobalAppointmentID,
                                         EventCategory = _eventCategory,
                                         EventCategoryColor = _eventCategoryColor,
                                         InvertEventCategoryFilter = _invertEventCategoryFilter,
                                         MapAttendees = _mapAttendees,
                                         MapBody = _mapBody,
                                         MapClassConfidentialToSensitivityPrivate = _mapClassConfidentialToSensitivityPrivate,
                                         MapReminder = _mapReminder,
                                         MapSensitivityPrivateToClassConfidential = _mapSensitivityPrivateToClassConfidential,
                                         MapClassPublicToSensitivityPrivate = _mapClassPublicToSensitivityPrivate,
                                         ScheduleAgentClient = _scheduleAgentClient,
                                         SendNoAppointmentNotifications = _sendNoAppointmentNotifications,
                                         UseEventCategoryColorAndMapFromCalendarColor = _useEventCategoryColorAndMapFromCalendarColor,
                                         CleanupDuplicateEvents = _cleanupDuplicateEvents
      };
    }

    public string Name => "Event mapping configuration";

    public bool Validate (StringBuilder errorMessageBuilder)
    {
      return true;
    }


    public IEnumerable<ISubOptionsViewModel> SubOptions => new ISubOptionsViewModel[] { };

    public static EventMappingConfigurationViewModel DesignInstance = new EventMappingConfigurationViewModel(new[] {"Cat1","Cat2"}, new DesignCurrentOptions())
                                                                      {
                                                                          CategoryShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF4,
                                                                          CreateEventsInUtc = true,
                                                                          EventTz = "TheTimeZoneID",
                                                                          IncludeHistoricalData = true,
                                                                          UseGlobalAppointmendID = true,
                                                                          EventCategory = "TheCategory",
                                                                          EventCategoryColor = OlCategoryColor.olCategoryColorDarkMaroon,
                                                                          InvertEventCategoryFilter = true,
                                                                          MapAttendees = true,
                                                                          MapBody = true,
                                                                          MapClassConfidentialToSensitivityPrivate = true,
                                                                          MapClassPublicToSensitivityPrivate = true,
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
      SetServerCalendarColorCommand = new DelegateCommand (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        SetServerCalendarColorAsync();
      });
      GetServerCalendarColorCommand = new DelegateCommand (_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        GetServerCalendarColorAsync();
      });
    }

    private async void GetServerCalendarColorAsync ()
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

    private async void SetServerCalendarColorAsync ()
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