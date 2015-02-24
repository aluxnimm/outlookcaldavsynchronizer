// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using System.Reflection;
using DDay.iCal;
using log4net;
using Microsoft.Office.Interop.Outlook;
using ICalRecurrencePattern = DDay.iCal.RecurrencePattern;

namespace CalDavSynchronizer.EntityMapping
{
  internal class EntityMapper : IEntityMapper<AppointmentItem, IEvent>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";
    private string _outlookEmailAddress;
    private Uri _serverEmailAddress;

    public EntityMapper (string outlookEmailAddress, Uri serverEmailAddress)
    {
      _outlookEmailAddress = outlookEmailAddress;
      _serverEmailAddress = serverEmailAddress;
    }


    public IEvent Map1To2 (AppointmentItem source, IEvent target)
    {
      target.Start = new iCalDateTime (source.StartUTC);
      target.DTEnd = new iCalDateTime (source.EndUTC);
      target.Summary = source.Subject;
      target.Location = source.Location;
      target.Description = source.Body;
      target.IsAllDay = source.AllDayEvent;

      target.Priority = MapPriority1To2 (source.Importance);

      MapAttendees1To2 (source, target);
      MapRecurrance1To2 (source, target);
      return target;

    }


    private void MapRecurrance1To2 (AppointmentItem source, IEvent target)
    {
      if (source.IsRecurring)
      {
        var sourceRecurrencePattern = source.GetRecurrencePattern();

        IRecurrencePattern targetRecurrencePattern = new ICalRecurrencePattern();
        if (!sourceRecurrencePattern.NoEndDate)
        {
          targetRecurrencePattern.Count = sourceRecurrencePattern.Occurrences;
          targetRecurrencePattern.Until = sourceRecurrencePattern.PatternEndDate;
        }
        targetRecurrencePattern.Interval = sourceRecurrencePattern.Interval;

        switch (sourceRecurrencePattern.RecurrenceType)
        {
          case OlRecurrenceType.olRecursDaily:
            targetRecurrencePattern.Frequency = FrequencyType.Daily;
            break;
          case OlRecurrenceType.olRecursWeekly:
            targetRecurrencePattern.Frequency = FrequencyType.Weekly;
            MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
            break;
          case OlRecurrenceType.olRecursMonthly:
            targetRecurrencePattern.Frequency = FrequencyType.Monthly;
            targetRecurrencePattern.ByMonthDay.Add (sourceRecurrencePattern.DayOfMonth);
            break;
          case OlRecurrenceType.olRecursMonthNth:
            targetRecurrencePattern.Frequency = FrequencyType.Monthly;
            MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
            targetRecurrencePattern.ByWeekNo.Add (sourceRecurrencePattern.Instance);
            break;
          case OlRecurrenceType.olRecursYearly:
            targetRecurrencePattern.Frequency = FrequencyType.Yearly;
            targetRecurrencePattern.ByMonthDay.Add (sourceRecurrencePattern.DayOfMonth);
            targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
            break;
          case OlRecurrenceType.olRecursYearNth:
            targetRecurrencePattern.Frequency = FrequencyType.Yearly;
            MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
            targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
            targetRecurrencePattern.ByWeekNo.Add (sourceRecurrencePattern.Instance);
            break;
        }

        target.RecurrenceRules.Add (targetRecurrencePattern);
      }
    }

    private void MapDayOfWeek1To2 (OlDaysOfWeek source, IList<IWeekDay> target)
    {
      if ((source & OlDaysOfWeek.olMonday) > 0)
        target.Add (new WeekDay (DayOfWeek.Monday));
      if ((source & OlDaysOfWeek.olTuesday) > 0)
        target.Add (new WeekDay (DayOfWeek.Tuesday));
      if ((source & OlDaysOfWeek.olWednesday) > 0)
        target.Add (new WeekDay (DayOfWeek.Wednesday));
      if ((source & OlDaysOfWeek.olThursday) > 0)
        target.Add (new WeekDay (DayOfWeek.Thursday));
      if ((source & OlDaysOfWeek.olFriday) > 0)
        target.Add (new WeekDay (DayOfWeek.Friday));
      if ((source & OlDaysOfWeek.olSaturday) > 0)
        target.Add (new WeekDay (DayOfWeek.Saturday));
      if ((source & OlDaysOfWeek.olSunday) > 0)
        target.Add (new WeekDay (DayOfWeek.Sunday));
    }

    private OlDaysOfWeek MapDayOfWeek2To1 (IList<IWeekDay> source)
    {
      OlDaysOfWeek target = 0;

      foreach (var day in source)
      {
        switch (day.DayOfWeek)
        {
          case DayOfWeek.Monday:
            target |= OlDaysOfWeek.olMonday;
            break;
          case DayOfWeek.Tuesday:
            target |= OlDaysOfWeek.olTuesday;
            break;
          case DayOfWeek.Wednesday:
            target |= OlDaysOfWeek.olWednesday;
            break;
          case DayOfWeek.Thursday:
            target |= OlDaysOfWeek.olThursday;
            break;
          case DayOfWeek.Friday:
            target |= OlDaysOfWeek.olFriday;
            break;
          case DayOfWeek.Saturday:
            target |= OlDaysOfWeek.olSaturday;
            break;
          case DayOfWeek.Sunday:
            target |= OlDaysOfWeek.olSunday;
            break;
        }
      }
      return target;
    }


    private void MapRecurrance2To1 (IEvent source, AppointmentItem target)
    {
      if (source.RecurrenceRules.Count > 0)
      {
        var targetRecurrencePattern = target.GetRecurrencePattern();
        if (source.RecurrenceRules.Count > 1)
        {
          s_logger.WarnFormat ("Event '{0}' contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.", source.Url);
        }
        var sourceRecurrencePattern = source.RecurrenceRules[0];


        targetRecurrencePattern.Occurrences = sourceRecurrencePattern.Count;
        targetRecurrencePattern.PatternEndDate = sourceRecurrencePattern.Until;

        targetRecurrencePattern.Interval = sourceRecurrencePattern.Interval;

        switch (sourceRecurrencePattern.Frequency)
        {
          case FrequencyType.Daily:
            targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursDaily;
            break;
          case FrequencyType.Weekly:
            if (sourceRecurrencePattern.ByDay.Count > 0)
            {
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
              targetRecurrencePattern.DayOfWeekMask = MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
            }
            else
            {
              s_logger.WarnFormat ("Event '{0}' contains a weekly recurrence pattern, which is not supported by outlook. Ignoring recurrence rule.", source.Url);
              target.ClearRecurrencePattern ();
            }
            break;
          case FrequencyType.Monthly:
            if (sourceRecurrencePattern.ByWeekNo.Count > 0 && sourceRecurrencePattern.ByDay.Count > 0)
            {
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthNth;
              if (sourceRecurrencePattern.ByWeekNo.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];
              targetRecurrencePattern.DayOfWeekMask = MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
            }
            else if (sourceRecurrencePattern.ByMonthDay.Count > 0)
            {
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
              if (sourceRecurrencePattern.ByMonthDay.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
            }
            else
            {
              s_logger.WarnFormat ("Event '{0}' contains a monthly recurrence pattern, which is not supported by outlook. Ignoring recurrence rule.", source.Url);
              target.ClearRecurrencePattern();
            }
            break;
          case FrequencyType.Yearly:
            if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByWeekNo.Count > 0)
            {
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
              if (sourceRecurrencePattern.ByMonth.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

              if (sourceRecurrencePattern.ByWeekNo.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one week in a yearly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];

              targetRecurrencePattern.DayOfWeekMask = MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
            }
            else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByMonthDay.Count > 0)
            {
              targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
              if (sourceRecurrencePattern.ByMonth.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

              if (sourceRecurrencePattern.ByMonthDay.Count > 1)
              {
                s_logger.WarnFormat ("Event '{0}' contains more than one days in a monthly recurrence rule. Since outlook supports only one day, all except the first one will be ignored.", source.Url);
              }
              targetRecurrencePattern.DayOfMonth = sourceRecurrencePattern.ByMonthDay[0];
            }
            else
            {
              s_logger.WarnFormat ("Event '{0}' contains a yearly recurrence pattern, which is not supported by outlook. Ignoring recurrence rule.", source.Url);
              target.ClearRecurrencePattern ();
            }
            break;
          default:
            s_logger.WarnFormat ("Recurring event '{0}' contains the Frequency '{1}', which is not supported by outlook. Ignoring recurrence rule.", source.Url, sourceRecurrencePattern.Frequency);
            target.ClearRecurrencePattern();
            break;
        }
      }
      else
      {
        target.ClearRecurrencePattern();
      }
    }

    private int MapPriority1To2 (OlImportance value)
    {
      switch (value)
      {
        case OlImportance.olImportanceLow:
          return 9;
        case OlImportance.olImportanceNormal:
          return 5;
        case OlImportance.olImportanceHigh:
          return 1;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private OlImportance MapPriority2To1 (int value)
    {
      switch (value)
      {
        case 9:
          return OlImportance.olImportanceLow;
        case 0:
        case 5:
          return OlImportance.olImportanceNormal;
        case 1:
          return OlImportance.olImportanceHigh;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }


    private void MapAttendees1To2 (AppointmentItem source, IEvent target)
    {
      foreach (Recipient recipient in source.Recipients)
      {
        if (!IsOwnIdentity (recipient))
        {
          string emailAddress = recipient.PropertyAccessor.GetProperty (PR_SMTP_ADDRESS);
          var attendee = new Attendee (string.Format ("MAILTO:{0}", emailAddress));
          attendee.CommonName = recipient.Name;
          attendee.Role = MapAttendeeType1To2 ((OlMeetingRecipientType) recipient.Type);
          target.Attendees.Add (attendee);
        }
      }
    }

    private bool IsOwnIdentity (Recipient recipient)
    {
      return StringComparer.InvariantCultureIgnoreCase.Compare (recipient.Address, _outlookEmailAddress) == 0;
    }


    public string MapAttendeeType1To2 (OlMeetingRecipientType recipientType)
    {
      switch (recipientType)
      {
        case OlMeetingRecipientType.olOptional:
          return "OPT-PARTICIPANT";
        case OlMeetingRecipientType.olRequired:
          return "REQ-PARTICIPANT";
        case OlMeetingRecipientType.olResource:
          return "CHAIR";
        case OlMeetingRecipientType.olOrganizer:
          return "REQ-PARTICIPANT";
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", recipientType));
    }

    public OlMeetingRecipientType MapAttendeeType2To1 (string recipientType)
    {
      switch (recipientType)
      {
        case "OPT-PARTICIPANT":
          return OlMeetingRecipientType.olOptional;
        case "REQ-PARTICIPANT":
          return OlMeetingRecipientType.olRequired;
        case "CHAIR":
          return OlMeetingRecipientType.olResource;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", recipientType));
    }


    private const int s_mailtoSchemaLength = 7; // length of "mailto:"

    public AppointmentItem Map2To1 (IEvent source, AppointmentItem target)
    {
      target.StartUTC = source.Start.UTC;
      target.EndUTC = source.DTEnd.UTC;
      target.Subject = source.Summary;
      target.Location = source.Location;
      target.Body = source.Description;
      target.AllDayEvent = source.IsAllDay;

      target.Importance = MapPriority2To1 (source.Priority);

      MapAttendees2To1 (source, target);
      MapRecurrance2To1 (source, target);

      return target;
    }

    private void MapAttendees2To1 (IEvent source, AppointmentItem target)
    {
      var targetRecipientsWhichShouldRemain = new HashSet<Recipient>();
      var indexByEmailAddresses = GetOutlookRecipientsByEmailAddresses (target);

      foreach (var attendee in source.Attendees)
      {
        Recipient targetRecipient;
        if (!indexByEmailAddresses.TryGetValue (attendee.Value, out targetRecipient))
        {
          targetRecipient = target.Recipients.Add (attendee.Value.ToString().Substring (s_mailtoSchemaLength));
        }
        targetRecipientsWhichShouldRemain.Add (targetRecipient);

        targetRecipient.Type = (int) MapAttendeeType2To1 (attendee.Role);
      }

      for (int i = target.Recipients.Count; i > 0; i--)
      {
        var recipient = target.Recipients[i];
        if (!IsOwnIdentity (recipient))
        {
          if (!targetRecipientsWhichShouldRemain.Contains (recipient))
            target.Recipients.Remove (i);
        }
      }
    }

    private Dictionary<Uri, Recipient> GetOutlookRecipientsByEmailAddresses (AppointmentItem appointment)
    {
      Dictionary<Uri, Recipient> indexByEmailAddresses = new Dictionary<Uri, Recipient>();

      foreach (Recipient recipient in appointment.Recipients)
      {
        var emailAddress = new Uri ("mailto:" + recipient.PropertyAccessor.GetProperty (PR_SMTP_ADDRESS));
        indexByEmailAddresses.Add (emailAddress, recipient);
      }

      return indexByEmailAddresses;
    }
  }
}