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
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using NodaTime.TimeZones;
using Thought.vCards;

namespace CalDavSynchronizer.ProfileTypes
{
  public abstract class ProfileTypeBase : IProfileType
  {
    public virtual Contracts.Options CreateOptions()
    {
      return new Contracts.Options
      {
        ConflictResolution = ConflictResolution.Automatic,
        DaysToSynchronizeInTheFuture = 365,
        DaysToSynchronizeInThePast = 60,
        SynchronizationIntervalInMinutes = 30,
        SynchronizationMode = SynchronizationMode.MergeInBothDirections,
        Name = Strings.Get($"<New Profile>"),
        Id = Guid.NewGuid(),
        Inactive = false,
        PreemptiveAuthentication = true,
        ForceBasicAuthentication = false,
        ProxyOptions = new ProxyOptions() { ProxyUseDefault = true },
        IsChunkedSynchronizationEnabled = true,
        ChunkSize = 100,
        ServerAdapterType = ServerAdapterType.WebDavHttpClientBased,
        ProfileTypeOrNull = ProfileTypeRegistry.GetProfileTypeName(this)
      };
    }

    public virtual EventMappingConfiguration CreateEventMappingConfiguration()
    {
      var data = new EventMappingConfiguration();
      data.MapReminder = ReminderMapping.JustUpcoming;
      data.MapSensitivityPrivateToClassConfidential = false;
      data.MapClassConfidentialToSensitivityPrivate = false;
      data.MapClassPublicToSensitivityPrivate = false;
      data.MapSensitivityPublicToDefault = false;
      data.MapAttendees = true;
      data.ScheduleAgentClient = true;
      data.SendNoAppointmentNotifications = false;
      data.MapBody = true;
      data.MapRtfBodyToXAltDesc = false;
      data.MapXAltDescToRtfBody = false;
      data.CreateEventsInUTC = false;
      data.UseIanaTz = false;

      try
      {
        data.EventTz = NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault()?.Id;
      }
      catch (DateTimeZoneNotFoundException)
      {
        // Default to GMT if Windows Zone can't be mapped to IANA zone.
        data.EventTz = "Etc/GMT";
      }
      data.IncludeHistoricalData = false;
      data.UseGlobalAppointmentID = false;
      data.IncludeEmptyEventCategoryFilter = false;
      data.InvertEventCategoryFilter = false;
      data.CleanupDuplicateEvents = false;
      data.MapCustomProperties = false;
      return data;
    }

    public virtual ContactMappingConfiguration CreateContactMappingConfiguration()
    {
      var data = new ContactMappingConfiguration();
      data.MapAnniversary = true;
      data.MapBirthday = true;
      data.MapContactPhoto = true;
      data.KeepOutlookPhoto = false;
      data.KeepOutlookFileAs = true;
      data.FixPhoneNumberFormat = false;
      data.MapOutlookEmail1ToWork = false;
      data.WriteImAsImpp = false;
      data.DefaultImServicType = IMServiceType.AIM;
      data.MapDistributionLists = false;
      return data;
    }

    public virtual TaskMappingConfiguration CreateTaskMappingConfiguration()
    {
      var data = new TaskMappingConfiguration();
      data.MapReminder = ReminderMapping.JustUpcoming;
      data.MapPriority = true;
      data.MapBody = true;
      data.MapRecurringTasks = true;
      data.MapStartAndDueAsFloating = false;
      data.IncludeEmptyTaskCategoryFilter = false;
      data.InvertTaskCategoryFilter = false;
      data.MapCustomProperties = false;
      return data;
    }


    public abstract string Name { get; }
    public abstract string ImageUrl { get; }
    public abstract IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData);
  }

}