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
using System.Xml.Serialization;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Contracts
{
  public class EventMappingConfiguration : MappingConfigurationBase, IPropertyMappingConfiguration
  {
    public ReminderMapping MapReminder { get; set; }
    public bool MapSensitivityPrivateToClassConfidential { get; set; }
    public bool MapClassConfidentialToSensitivityPrivate { get; set; }
    public bool MapClassPublicToSensitivityPrivate { get; set; }
    public bool MapAttendees { get; set; }
    public bool ScheduleAgentClient { get; set; }
    public bool SendNoAppointmentNotifications { get; set; }
    public bool MapBody { get; set; }
    public bool CreateEventsInUTC { get; set; }
    public bool UseIanaTz { get; set; }
    public string EventTz { get; set; }
    public bool IncludeHistoricalData { get; set; }
    public bool UseGlobalAppointmentID { get; set; }
    public string EventCategory { get; set; }
    public bool InvertEventCategoryFilter { get; set; }
    public bool UseEventCategoryColorAndMapFromCalendarColor { get; set; }
    public OlCategoryColor EventCategoryColor { get; set; }
    public OlCategoryShortcutKey CategoryShortcutKey { get; set; }
    public bool CleanupDuplicateEvents { get; set; }
    public bool MapCustomProperties { get; set; }
    private PropertyMapping[] _userDefinedCustomPropertyMappings;
    public PropertyMapping[] UserDefinedCustomPropertyMappings
    {
      get { return _userDefinedCustomPropertyMappings ?? new PropertyMapping[0]; }
      set { _userDefinedCustomPropertyMappings = value ?? new PropertyMapping[0]; }
    }


    [XmlIgnore]
    public bool UseEventCategoryAsFilter
    {
      get { return !string.IsNullOrEmpty (EventCategory); }
    }

    public EventMappingConfiguration ()
    {
      MapReminder = ReminderMapping.JustUpcoming;
      MapSensitivityPrivateToClassConfidential = false;
      MapClassConfidentialToSensitivityPrivate = false;
      MapClassPublicToSensitivityPrivate = false;
      MapAttendees = true;
      ScheduleAgentClient = true;
      SendNoAppointmentNotifications = false;
      MapBody = true;
      CreateEventsInUTC = false;
      UseIanaTz = false;
      EventTz = NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault()?.Id;
      IncludeHistoricalData = false;
      UseGlobalAppointmentID = false;
      InvertEventCategoryFilter = false;
      UseEventCategoryColorAndMapFromCalendarColor = false;
      EventCategoryColor = OlCategoryColor.olCategoryColorNone;
      CategoryShortcutKey = OlCategoryShortcutKey.olCategoryShortcutKeyNone;
      CleanupDuplicateEvents = false;
      MapCustomProperties = false;
    }

    public override ISubOptionsViewModel CreateConfigurationViewModel (IMappingConfigurationViewModelFactory factory)
    {
      return factory.Create (this);
    }
  }
}