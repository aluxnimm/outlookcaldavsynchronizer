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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options.Models;
using log4net;
using NodaTime.TimeZones;

namespace CalDavSynchronizer.Ui.Options.ViewModels.Mapping
{
  public class EventMappingConfigurationViewModel : ModelBase, ISubOptionsViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private readonly EventMappingConfigurationModel _model;
    private readonly OptionsModel _optionsModel;

    private bool _isSelected;
    private bool _isExpanded;

    public EventMappingConfigurationViewModel(IReadOnlyList<string> availableCategories, EventMappingConfigurationModel model, OptionsModel optionsModel)
    {
      if (availableCategories == null)
        throw new ArgumentNullException(nameof(availableCategories));
      if (model == null) throw new ArgumentNullException(nameof(model));
      if (optionsModel == null) throw new ArgumentNullException(nameof(optionsModel));

      AvailableCategories = availableCategories;
      _model = model;
      _optionsModel = optionsModel;
      SetServerCalendarColorCommand = new DelegateCommand(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        SetServerCalendarColorAsync();
      });
      GetServerCalendarColorCommand = new DelegateCommand(_ =>
      {
        ComponentContainer.EnsureSynchronizationContext();
        GetServerCalendarColorAsync();
      });

      Items = new[] { new CustomPropertyMappingViewModel(model) };

      RegisterPropertyChangePropagation(_model, nameof(_model.OneTimeSetCategoryShortcutKey), nameof(OneTimeSetCategoryShortcutKey));
      RegisterPropertyChangePropagation(_model, nameof(_model.CreateEventsInUtc), nameof(CreateEventsInUtc));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseIanaTz), nameof(UseIanaTz));
      RegisterPropertyChangePropagation(_model, nameof(_model.EventTz), nameof(EventTz));
      RegisterPropertyChangePropagation(_model, nameof(_model.IncludeHistoricalData), nameof(IncludeHistoricalData));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseGlobalAppointmentId), nameof(UseGlobalAppointmentId));
      RegisterPropertyChangePropagation(_model, nameof(_model.EventCategory), nameof(EventCategory));
      RegisterPropertyChangePropagation(_model, nameof(_model.OneTimeSetEventCategoryColor), nameof(OneTimeSetEventCategoryColor));
      RegisterPropertyChangePropagation(_model, nameof(_model.IncludeEmptyEventCategoryFilter), nameof(IncludeEmptyEventCategoryFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.InvertEventCategoryFilter), nameof(InvertEventCategoryFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapAttendees), nameof(MapAttendees));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapAttendees), nameof(MapAttendees));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapBody), nameof(MapBody));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapRtfBodyToXAltDesc), nameof(MapRtfBodyToXAltDesc));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapXAltDescToRtfBody), nameof(MapXAltDescToRtfBody));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapClassConfidentialToSensitivityPrivate), nameof(MapClassConfidentialToSensitivityPrivate));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapClassConfidentialToSensitivityPrivate), nameof(MapClassConfidentialToSensitivityPrivate));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapReminder), nameof(MapReminder));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapSensitivityPrivateToClassConfidential), nameof(MapSensitivityPrivateToClassConfidential));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapSensitivityPublicToDefault), nameof(MapSensitivityPublicToDefault));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapClassPublicToSensitivityPrivate), nameof(MapClassPublicToSensitivityPrivate));
      RegisterPropertyChangePropagation(_model, nameof(_model.ScheduleAgentClient), nameof(ScheduleAgentClient));
      RegisterPropertyChangePropagation(_model, nameof(_model.SendNoAppointmentNotifications), nameof(SendNoAppointmentNotifications));
      RegisterPropertyChangePropagation(_model, nameof(_model.OrganizerAsDelegate), nameof(OrganizerAsDelegate));
      RegisterPropertyChangePropagation(_model, nameof(_model.DoOneTimeSetCategoryColor), nameof(DoOneTimeSetCategoryColor));
      RegisterPropertyChangePropagation(_model, nameof(_model.CleanupDuplicateEvents), nameof(CleanupDuplicateEvents));
      RegisterPropertyChangePropagation(_model, nameof(_model.MapEventColorToCategory), nameof(MapEventColorToCategory));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseEventCategoryAsFilter), nameof(UseEventCategoryAsFilter));
      RegisterPropertyChangePropagation(_model, nameof(_model.UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor), nameof(UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor));
      RegisterPropertyChangePropagation(_model, nameof(_model.IsCategoryFilterSticky), nameof(IsCategoryFilterSticky));

    }

    public IList<Item<ReminderMapping>> AvailableReminderMappings { get; } = new List<Item<ReminderMapping>>
                                                                             {
                                                                                 new Item<ReminderMapping> (ReminderMapping.@true,  Strings.Get($"Yes")),
                                                                                 new Item<ReminderMapping> (ReminderMapping.@false,  Strings.Get($"No")),
                                                                                 new Item<ReminderMapping> (ReminderMapping.JustUpcoming,  Strings.Get($"Just upcoming reminders"))
                                                                             };

    public IList<Item<OlCategoryShortcutKey>> AvailableShortcutKeys { get; } = new List<Item<OlCategoryShortcutKey>>
                                                                               {
                                                                                   new Item<OlCategoryShortcutKey> (OlCategoryShortcutKey.olCategoryShortcutKeyNone,  Strings.Get($"None")),
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
        var zones = TzdbDateTimeZoneSource.Default.CanonicalIdMap.Values.Distinct().Where(v => v.Contains("/")).ToList();
        zones.Sort();
        return zones;
      }
    }

    public IList<Item<OlCategoryColor>> AvailableEventCategoryColors { get; } =
      ColorHelper.ArgbColorByCategoryColor.Select(kv => new Item<OlCategoryColor>(kv.Key, kv.Key.ToString().Substring(15))).ToList();

    public ICommand GetServerCalendarColorCommand { get; }
    public ICommand SetServerCalendarColorCommand { get; }

    public OlCategoryShortcutKey OneTimeSetCategoryShortcutKey
    {
      get { return _model.OneTimeSetCategoryShortcutKey; }
      set { _model.OneTimeSetCategoryShortcutKey = value; }
    }

    public bool IsCategoryFilterSticky
    {
      get { return _model.IsCategoryFilterSticky; }
      set { _model.IsCategoryFilterSticky = value; }
    }

    public bool CreateEventsInUtc
    {
      get { return _model.CreateEventsInUtc; }
      set { _model.CreateEventsInUtc = value; }
    }

    public bool UseIanaTz
    {
      get { return _model.UseIanaTz; }
      set { _model.UseIanaTz = value; }
    }

    public string EventTz
    {
      get { return _model.EventTz; }
      set { _model.EventTz = value; }
    }

    public bool IncludeHistoricalData
    {
      get { return _model.IncludeHistoricalData; }
      set { _model.IncludeHistoricalData = value; }
    }

    public bool UseGlobalAppointmentId
    {
      get { return _model.UseGlobalAppointmentId; }
      set { _model.UseGlobalAppointmentId = value; }
    }


    public bool CleanupDuplicateEvents
    {
      get { return _model.CleanupDuplicateEvents; }
      set { _model.CleanupDuplicateEvents = value; }
    }

    public bool MapEventColorToCategory
    {
      get { return _model.MapEventColorToCategory; }
      set { _model.MapEventColorToCategory = value; }
    }

    public string EventCategory
    {
      get { return _model.EventCategory; }
      set { _model.EventCategory = value; }
    }

    public bool UseEventCategoryAsFilter => _model.UseEventCategoryAsFilter;
    public bool UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor => _model.UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor;


    public OlCategoryColor OneTimeSetEventCategoryColor
    {
      get { return _model.OneTimeSetEventCategoryColor; }
      set { _model.OneTimeSetEventCategoryColor = value; }
    }

    public bool IncludeEmptyEventCategoryFilter
    {
      get { return _model.IncludeEmptyEventCategoryFilter; }
      set { _model.IncludeEmptyEventCategoryFilter = value; }
    }

    public bool InvertEventCategoryFilter
    {
      get { return _model.InvertEventCategoryFilter; }
      set { _model.InvertEventCategoryFilter = value; }
    }

    public bool MapAttendees
    {
      get { return _model.MapAttendees; }
      set { _model.MapAttendees = value; }
    }

    public bool MapBody
    {
      get { return _model.MapBody; }
      set { _model.MapBody = value; }
    }

    public bool MapRtfBodyToXAltDesc
    {
      get { return _model.MapRtfBodyToXAltDesc; }
      set { _model.MapRtfBodyToXAltDesc = value; }
    }

    public bool MapXAltDescToRtfBody
    {
      get { return _model.MapXAltDescToRtfBody; }
      set { _model.MapXAltDescToRtfBody = value; }
    }

    public bool MapClassConfidentialToSensitivityPrivate
    {
      get { return _model.MapClassConfidentialToSensitivityPrivate; }
      set { _model.MapClassConfidentialToSensitivityPrivate = value; }
    }

    public bool MapClassPublicToSensitivityPrivate
    {
      get { return _model.MapClassPublicToSensitivityPrivate; }
      set { _model.MapClassPublicToSensitivityPrivate = value; }
    }

    public ReminderMapping MapReminder
    {
      get { return _model.MapReminder; }
      set { _model.MapReminder = value; }
    }

    public bool MapSensitivityPrivateToClassConfidential
    {
      get { return _model.MapSensitivityPrivateToClassConfidential; }
      set { _model.MapSensitivityPrivateToClassConfidential = value; }
    }

    public bool MapSensitivityPublicToDefault
    {
      get { return _model.MapSensitivityPublicToDefault; }
      set { _model.MapSensitivityPublicToDefault = value; }
    }

    public bool ScheduleAgentClient
    {
      get { return _model.ScheduleAgentClient; }
      set { _model.ScheduleAgentClient = value; }
    }

    public bool SendNoAppointmentNotifications
    {
      get { return _model.SendNoAppointmentNotifications; }
      set { _model.SendNoAppointmentNotifications = value; }
    }

    public bool OrganizerAsDelegate
    {
      get { return _model.OrganizerAsDelegate; }
      set { _model.OrganizerAsDelegate = value; }
    }

    public bool DoOneTimeSetCategoryColor
    {
      get { return _model.DoOneTimeSetCategoryColor; }
      set { _model.DoOneTimeSetCategoryColor = value; }
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

    public IReadOnlyList<string> AvailableCategories { get; }



    public string Name => Strings.Get($"Event Mapping Configuration");

    public IEnumerable<ITreeNodeViewModel> Items { get; }

    public static EventMappingConfigurationViewModel DesignInstance = new EventMappingConfigurationViewModel(new[] { "Cat1", "Cat2" }, new EventMappingConfigurationModel(new EventMappingConfiguration(), new OptionModelSessionData(new Dictionary<string,OutlookCategory>())), OptionsModel.DesignInstance)
    {
      OneTimeSetCategoryShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyCtrlF4,
      CreateEventsInUtc = true,
      EventTz = "TheTimeZoneID",
      IncludeHistoricalData = true,
      UseGlobalAppointmentId = true,
      EventCategory = "TheCategory",
      OneTimeSetEventCategoryColor = OlCategoryColor.olCategoryColorDarkMaroon,
      IncludeEmptyEventCategoryFilter = false,
      InvertEventCategoryFilter = true,
      MapAttendees = true,
      MapBody = true,
      MapRtfBodyToXAltDesc = true,
      MapXAltDescToRtfBody = true,
      MapClassConfidentialToSensitivityPrivate = true,
      MapClassPublicToSensitivityPrivate = true,
      MapReminder = ReminderMapping.JustUpcoming,
      MapSensitivityPrivateToClassConfidential = true,
      MapSensitivityPublicToDefault = true,
      ScheduleAgentClient = true,
      SendNoAppointmentNotifications = true,
      DoOneTimeSetCategoryColor = true,
      CleanupDuplicateEvents = true,
      MapEventColorToCategory = true
    };

   
    private async void GetServerCalendarColorAsync()
    {
      try
      {
        var serverColor = await _optionsModel.CreateCalDavDataAccess().GetCalendarColorNoThrow();

        if (serverColor != null)
        {
          OneTimeSetEventCategoryColor = ColorHelper.FindMatchingCategoryColor(serverColor.Value);
        }
      }
      catch (System.Exception x)
      {
        ExceptionHandler.Instance.DisplayException(x, s_logger);
      }
    }

    private async void SetServerCalendarColorAsync()
    {
      try
      {
        if (OneTimeSetEventCategoryColor != OlCategoryColor.olCategoryColorNone)
        {
          if (await _optionsModel.CreateCalDavDataAccess().SetCalendarColorNoThrow(ColorHelper.ArgbColorByCategoryColor[OneTimeSetEventCategoryColor]))
          {
            System.Windows.MessageBox.Show(Strings.Get($"Successfully updated the server calendar color!"));
          }
          else
          {
            System.Windows.MessageBox.Show(Strings.Get($"Error updating the server calendar color!"));
          }
        }
        else
        {
          System.Windows.MessageBox.Show(Strings.Get($"No color set for updating the server calendar color!"));
        }
      }
      catch (System.Exception x)
      {
        ExceptionHandler.Instance.DisplayException(x, s_logger);
      }
    }
  }
}