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
using Microsoft.Office.Interop.Outlook;
using NodaTime.TimeZones;

namespace CalDavSynchronizer.Contracts
{
  public class EventMappingConfiguration : MappingConfigurationBase, IPropertyMappingConfiguration
  {
    public ReminderMapping MapReminder { get; set; }
    public bool MapSensitivityPrivateToClassConfidential { get; set; }
    public bool MapClassConfidentialToSensitivityPrivate { get; set; }
    public bool MapClassPublicToSensitivityPrivate { get; set; }
    public bool MapSensitivityPublicToDefault { get; set; }
    public bool MapAttendees { get; set; }
    public bool ScheduleAgentClient { get; set; }
    public bool SendNoAppointmentNotifications { get; set; }
    public bool OrganizerAsDelegate { get; set; }
    public bool MapBody { get; set; }
    public bool MapRtfBodyToXAltDesc { get; set; }
    public bool MapXAltDescToRtfBody { get; set; }
    public bool CreateEventsInUTC { get; set; }
    public bool UseIanaTz { get; set; }
    public string EventTz { get; set; }
    public bool IncludeHistoricalData { get; set; }
    public bool UseGlobalAppointmentID { get; set; }
    public string EventCategory { get; set; }
    public bool IncludeEmptyEventCategoryFilter { get; set; }
    public bool InvertEventCategoryFilter { get; set; }
    public bool IsCategoryFilterSticky { get; set; }
    public bool CleanupDuplicateEvents { get; set; }
    public bool MapCustomProperties { get; set; }
    public bool MapEventColorToCategory { get; set; }
    public ColorCategoryMapping[] EventColorToCategoryMappings { get; set; }
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
     
    }

    public override TResult Accept<TResult>(IMappingConfigurationBaseVisitor<TResult> visitor)
    {
      return visitor.Visit(this);
    }
  }
}