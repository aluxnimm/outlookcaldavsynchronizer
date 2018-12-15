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
using System.Linq;
using System.Text;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.Models
{
  public class EventMappingConfigurationModel : MappingConfigurationModel, ICustomPropertiesMappingConfigurationModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (System.Reflection.MethodBase.GetCurrentMethod ().DeclaringType);

    private OlCategoryShortcutKey _oneTimeSetCategoryShortcutKey;
    private bool _createEventsInUtc;
    private bool _useIanaTz;
    private string _eventTz;
    private bool _includeHistoricalData;
    private bool _useGlobalAppointmentId;
    private string _eventCategory;
    private OlCategoryColor _oneTimeSetEventCategoryColor;
    private bool _includeEmptyEventCategoryFilter;
    private bool _invertEventCategoryFilter;
    private bool _mapAttendees;
    private bool _mapBody;
    private bool _mapRtfBodyToXAltDesc;
    private bool _mapXAltDescToRtfBody;
    private bool _mapClassConfidentialToSensitivityPrivate;
    private bool _mapClassPublicToSensitivityPrivate;
    private ReminderMapping _mapReminder;
    private bool _mapSensitivityPrivateToClassConfidential;
    private bool _mapSensitivityPublicToDefault;
    private bool _scheduleAgentClient;
    private bool _sendNoAppointmentNotifications;
    private bool _organizerAsDelegate;
    private bool _doOneTimeSetCategoryColor;
    private bool _cleanupDuplicateEvents;
    private bool _mapEventColorToCategory;
    private bool _mapCustomProperties;
    private bool _isCategoryFilterSticky;
    private ColorCategoryMapping[] _eventColorToCategoryMappings;
    private readonly OptionModelSessionData _sessionData;

    public EventMappingConfigurationModel(EventMappingConfiguration data, OptionModelSessionData sessionData)
    {
      if (data == null) throw new ArgumentNullException(nameof(data));
      _sessionData = sessionData ?? throw new ArgumentNullException(nameof(sessionData));

      InitializeData(data);
    }

    public ObservableCollection<PropertyMappingModel> Mappings { get; } = new ObservableCollection<PropertyMappingModel>();

    public OlCategoryShortcutKey OneTimeSetCategoryShortcutKey
    {
      get { return _oneTimeSetCategoryShortcutKey; }
      set
      {
        CheckedPropertyChange (ref _oneTimeSetCategoryShortcutKey, value);
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
    public bool UseGlobalAppointmentId
    {
      get { return _useGlobalAppointmentId; }
      set
      {
        CheckedPropertyChange (ref _useGlobalAppointmentId, value);
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

    public bool MapEventColorToCategory
    {
      get { return _mapEventColorToCategory; }
      set
      {
        CheckedPropertyChange(ref _mapEventColorToCategory, value);
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
        OnPropertyChanged (nameof(UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor));
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

    public bool UseEventCategoryAsFilter => !String.IsNullOrEmpty (_eventCategory);
    public bool UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor => !String.IsNullOrEmpty (_eventCategory) && _doOneTimeSetCategoryColor;


    public OlCategoryColor OneTimeSetEventCategoryColor
    {
      get { return _oneTimeSetEventCategoryColor; }
      set
      {
        CheckedPropertyChange (ref _oneTimeSetEventCategoryColor, value);
      }
    }

    public bool IncludeEmptyEventCategoryFilter
    {
      get { return _includeEmptyEventCategoryFilter; }
      set
      {
        if (value)
        {
          InvertEventCategoryFilter = false;
        }
        CheckedPropertyChange (ref _includeEmptyEventCategoryFilter, value);
      }
    }

    public bool InvertEventCategoryFilter
    {
      get { return _invertEventCategoryFilter; }
      set
      {
        if (value)
        {
          IncludeEmptyEventCategoryFilter = false;
        }
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

    public bool MapRtfBodyToXAltDesc
    {
      get { return _mapRtfBodyToXAltDesc; }
      set
      {
        CheckedPropertyChange (ref _mapRtfBodyToXAltDesc, value);
      }
    }

    public bool MapXAltDescToRtfBody
    {
      get { return _mapXAltDescToRtfBody; }
      set
      {
        CheckedPropertyChange (ref _mapXAltDescToRtfBody, value);
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

    public bool MapSensitivityPublicToDefault
    {
      get { return _mapSensitivityPublicToDefault; }
      set
      {
        CheckedPropertyChange(ref _mapSensitivityPublicToDefault, value);
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

    public bool OrganizerAsDelegate
    {
      get { return _organizerAsDelegate; }
      set
      {
        CheckedPropertyChange (ref _organizerAsDelegate, value);
      }
    }

    public bool DoOneTimeSetCategoryColor
    {
      get { return _doOneTimeSetCategoryColor; }
      set
      {
        CheckedPropertyChange (ref _doOneTimeSetCategoryColor, value);
        // ReSharper disable once ExplicitCallerInfoArgument
        OnPropertyChanged (nameof (UseEventCategoryAsFilterAndDoOneTimeSetCategoryColor));
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

    /// <remarks>
    /// InitializeData has to set fields instead of properties, since properties can interfer with each other!
    /// </remarks>
    private void InitializeData (EventMappingConfiguration mappingConfiguration)
    {
     

      _createEventsInUtc = mappingConfiguration.CreateEventsInUTC;
      _useIanaTz = mappingConfiguration.UseIanaTz;
      _eventTz = mappingConfiguration.EventTz;
      _includeHistoricalData = mappingConfiguration.IncludeHistoricalData;
      _useGlobalAppointmentId = mappingConfiguration.UseGlobalAppointmentID;
      _eventCategory = mappingConfiguration.EventCategory;
      _includeEmptyEventCategoryFilter = mappingConfiguration.IncludeEmptyEventCategoryFilter;
      _invertEventCategoryFilter = mappingConfiguration.InvertEventCategoryFilter;
      _mapAttendees = mappingConfiguration.MapAttendees;
      _mapBody = mappingConfiguration.MapBody;
      _mapRtfBodyToXAltDesc = mappingConfiguration.MapRtfBodyToXAltDesc;
      _mapXAltDescToRtfBody = mappingConfiguration.MapXAltDescToRtfBody;
      _mapClassConfidentialToSensitivityPrivate = mappingConfiguration.MapClassConfidentialToSensitivityPrivate;
      _mapClassPublicToSensitivityPrivate = mappingConfiguration.MapClassPublicToSensitivityPrivate;
      _mapReminder = mappingConfiguration.MapReminder;
      _mapSensitivityPrivateToClassConfidential = mappingConfiguration.MapSensitivityPrivateToClassConfidential;
      _mapSensitivityPublicToDefault = mappingConfiguration.MapSensitivityPublicToDefault;
      _scheduleAgentClient = mappingConfiguration.ScheduleAgentClient;
      _sendNoAppointmentNotifications = mappingConfiguration.SendNoAppointmentNotifications;
      _organizerAsDelegate = mappingConfiguration.OrganizerAsDelegate;
      _cleanupDuplicateEvents = mappingConfiguration.CleanupDuplicateEvents;
      _mapEventColorToCategory = mappingConfiguration.MapEventColorToCategory;
      _mapCustomProperties = mappingConfiguration.MapCustomProperties;
      _isCategoryFilterSticky = mappingConfiguration.IsCategoryFilterSticky;
      _eventColorToCategoryMappings = mappingConfiguration.EventColorToCategoryMappings;

      if (mappingConfiguration.UserDefinedCustomPropertyMappings != null)
        Array.ForEach(mappingConfiguration.UserDefinedCustomPropertyMappings, m => Mappings.Add(new PropertyMappingModel(m)));

      if (!string.IsNullOrEmpty(_eventCategory))
      {
        if (_sessionData.CategoriesById.TryGetValue(_eventCategory, out var category))
        {
          _oneTimeSetCategoryShortcutKey = category.ShortcutKey;
          _oneTimeSetEventCategoryColor = category.Color;
        }
      }

    }

    public override MappingConfigurationBase GetData()
    {
      return new EventMappingConfiguration
      {
        CreateEventsInUTC = _createEventsInUtc,
        UseIanaTz = _useIanaTz,
        EventTz = _eventTz,
        IncludeHistoricalData = _includeHistoricalData,
        UseGlobalAppointmentID = _useGlobalAppointmentId,
        EventCategory = _eventCategory,
        IncludeEmptyEventCategoryFilter = _includeEmptyEventCategoryFilter,
        InvertEventCategoryFilter = _invertEventCategoryFilter,
        MapAttendees = _mapAttendees,
        MapBody = _mapBody,
        MapRtfBodyToXAltDesc = _mapRtfBodyToXAltDesc,
        MapXAltDescToRtfBody = _mapXAltDescToRtfBody,
        MapClassConfidentialToSensitivityPrivate = _mapClassConfidentialToSensitivityPrivate,
        MapReminder = _mapReminder,
        MapSensitivityPrivateToClassConfidential = _mapSensitivityPrivateToClassConfidential,
        MapSensitivityPublicToDefault = _mapSensitivityPublicToDefault,
        MapClassPublicToSensitivityPrivate = _mapClassPublicToSensitivityPrivate,
        ScheduleAgentClient = _scheduleAgentClient,
        SendNoAppointmentNotifications = _sendNoAppointmentNotifications,
        OrganizerAsDelegate = _organizerAsDelegate,
        CleanupDuplicateEvents = _cleanupDuplicateEvents,
        MapEventColorToCategory = _mapEventColorToCategory,
        MapCustomProperties = _mapCustomProperties,
        UserDefinedCustomPropertyMappings = Mappings.Select(m => m.GetData()).ToArray(),
        IsCategoryFilterSticky = _isCategoryFilterSticky,
        EventColorToCategoryMappings = _eventColorToCategoryMappings
      };
    }

    public override void AddOneTimeTasks(Action<OneTimeChangeCategoryTask> add)
    {
      if (_doOneTimeSetCategoryColor || _oneTimeSetCategoryShortcutKey != OlCategoryShortcutKey.olCategoryShortcutKeyNone)
      {
        add(new OneTimeChangeCategoryTask(
          EventCategory,
          _doOneTimeSetCategoryColor ? _oneTimeSetEventCategoryColor : (OlCategoryColor?) null,
          _oneTimeSetCategoryShortcutKey != OlCategoryShortcutKey.olCategoryShortcutKeyNone ? _oneTimeSetCategoryShortcutKey : (OlCategoryShortcutKey?) null));
      }
    }

    public override bool Validate (StringBuilder errorMessageBuilder)
    {
      return ValidatePropertyMappings(errorMessageBuilder, Mappings);
    }

    public static bool ValidatePropertyMappings(StringBuilder errorMessageBuilder, IReadOnlyList<PropertyMappingModel> mappings)
    {
      var isValid = true;
      if (mappings.Any(m => string.IsNullOrEmpty(m.OutlookProperty) || string.IsNullOrEmpty(m.DavProperty)))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- Custom properties must not be empty."));
        isValid = false;
      }
      if (mappings.Any(m => !string.IsNullOrEmpty(m.DavProperty) && !m.DavProperty.StartsWith("X-")))
      {
        errorMessageBuilder.AppendLine(Strings.Get($"- DAV X-Attributes for manual mapped properties have to start with 'X-'"));
        isValid = false;
      }

      return isValid;
    }
  }
}