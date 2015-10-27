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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.EntityMapping;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = Microsoft.Office.Interop.Outlook.Exception;
using RecurrencePattern = DDay.iCal.RecurrencePattern;

namespace CalDavSynchronizer.Implementation.Events
{
  public class EventEntityMapper : IEntityMapper<AppointmentItemWrapper, IICalendar>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";
    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";
    private const string PR_SENDER_NAME = "http://schemas.microsoft.com/mapi/proptag/0x0C1A001E";
    private const string PR_SENDER_EMAIL_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x0C1F001E";
    private const string PR_SENT_REPRESENTING_NAME = "http://schemas.microsoft.com/mapi/proptag/0x0042001E";
    private const string PR_SENT_REPRESENTING_EMAIL_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x0065001E";
    private const string PR_SENT_REPRESENTING_ADDRTYPE = "http://schemas.microsoft.com/mapi/proptag/0x0064001E";
    private const string PR_SENDER_ADDRTYPE = "http://schemas.microsoft.com/mapi/proptag/0x0C1E001E";
    private const string PR_SENT_REPRESENTING_ENTRYID = "http://schemas.microsoft.com/mapi/proptag/0x00410102";
    private const string PR_SENDER_ENTRYID = "http://schemas.microsoft.com/mapi/proptag/0x0C190102";

    private readonly int _outlookMajorVersion;

    private readonly string _outlookEmailAddress;
    private readonly string _serverEmailUri;
    private readonly TimeZoneInfo _localTimeZoneInfo;

    public EventEntityMapper (string outlookEmailAddress, Uri serverEmailAddress, string localTimeZoneId, string outlookApplicationVersion)
    {
      _outlookEmailAddress = outlookEmailAddress;
      _serverEmailUri = serverEmailAddress.ToString();
      _localTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (localTimeZoneId);

      string outlookMajorVersionString = outlookApplicationVersion.Split (new char[] { '.' })[0];
      _outlookMajorVersion = Convert.ToInt32 (outlookMajorVersionString);
    }

    public IICalendar Map1To2 (AppointmentItemWrapper sourceWrapper, IICalendar existingTargetCalender)
    {
      var newTargetCalender = new iCalendar();
      var startTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (sourceWrapper.Inner.StartTimeZone.ID);
      var startIcalTimeZone = iCalTimeZone.FromSystemTimeZone (startTimeZoneInfo, new DateTime (1970, 1, 1), false);

      DDayICalWorkaround.CalendarDataPreprocessor.FixTimeZoneDSTRRules (startTimeZoneInfo, startIcalTimeZone);
      newTargetCalender.TimeZones.Add (startIcalTimeZone);

      iCalTimeZone endIcalTimeZone;
      if (!sourceWrapper.Inner.EndTimeZone.ID.Equals (sourceWrapper.Inner.StartTimeZone.ID))
      {
        var endTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById (sourceWrapper.Inner.EndTimeZone.ID);

        endIcalTimeZone = iCalTimeZone.FromSystemTimeZone (endTimeZoneInfo, new DateTime(1970, 1, 1), false);
        DDayICalWorkaround.CalendarDataPreprocessor.FixTimeZoneDSTRRules (endTimeZoneInfo, endIcalTimeZone);
        newTargetCalender.TimeZones.Add (endIcalTimeZone);
      }
      else
      {
        endIcalTimeZone = startIcalTimeZone;
      }

      var existingTargetEvent = existingTargetCalender.Events.FirstOrDefault (e => e.RecurrenceID == null);

      var newTargetEvent = new Event();

      if (existingTargetEvent != null)
        newTargetEvent.UID = existingTargetEvent.UID;

      newTargetCalender.Events.Add (newTargetEvent);

      Map1To2 (sourceWrapper.Inner, newTargetEvent, false, startIcalTimeZone, endIcalTimeZone);

      for (int i = 0, newSequenceNumber = existingTargetCalender.Events.Count > 0 ? existingTargetCalender.Events.Max (e => e.Sequence) + 1 : 0;
          i < newTargetCalender.Events.Count;
          i++, newSequenceNumber++)
      {
        newTargetCalender.Events[i].Sequence = newSequenceNumber;
      }

      return newTargetCalender;
    }

    private void Map1To2 (AppointmentItem source, IEvent target, bool isRecurrenceException, iCalTimeZone startIcalTimeZone, iCalTimeZone endIcalTimeZone)
    {
      if (source.AllDayEvent)
      {
        // Outlook's AllDayEvent relates to Start and not not StartUtc!!!
        target.Start = new iCalDateTime (source.Start);
        target.Start.HasTime = false;
        target.End = new iCalDateTime (source.End);
        target.End.HasTime = false;
        target.IsAllDay = true;
      }
      else
      {
        target.Start = new iCalDateTime (source.StartInStartTimeZone);
        target.Start.SetTimeZone (startIcalTimeZone);
        target.DTEnd = new iCalDateTime (source.EndInEndTimeZone);
        target.End.SetTimeZone (endIcalTimeZone);
        target.IsAllDay = false;
      }

      target.Summary = source.Subject;
      target.Location = source.Location;
      target.Description = source.Body;

      target.Priority = MapPriority1To2 (source.Importance);

      bool organizerSet;
      MapAttendees1To2 (source, target, out organizerSet);

      if (!isRecurrenceException)
        MapRecurrance1To2 (source, target, startIcalTimeZone, endIcalTimeZone);

      if (!organizerSet)
        MapOrganizer1To2 (source, target);

      target.Class = MapPrivacy1To2 (source.Sensitivity);
      MapReminder1To2 (source, target);

      MapCategories1To2 (source, target);

      target.Transparency = MapTransparency1To2 (source.BusyStatus);
    }

    private TransparencyType MapTransparency1To2 (OlBusyStatus value)
    {
      switch (value)
      {
        case OlBusyStatus.olBusy:
        case OlBusyStatus.olOutOfOffice:
        case OlBusyStatus.olWorkingElsewhere:
          return TransparencyType.Opaque;
        case OlBusyStatus.olTentative:
        case OlBusyStatus.olFree:
          return TransparencyType.Transparent;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }


    private OlBusyStatus MapTransparency2To1 (TransparencyType value)
    {
      switch (value)
      {
        case TransparencyType.Opaque:
          return OlBusyStatus.olBusy;
        case TransparencyType.Transparent:
          return OlBusyStatus.olFree;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }


    private static void MapCategories1To2 (AppointmentItem source, IEvent target)
    {
      if (!string.IsNullOrEmpty (source.Categories))
      {
        Array.ForEach (
            source.Categories.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add (c)
            );
      }
    }

    private void MapReminder1To2 (AppointmentItem source, IEvent target)
    {
      if (source.ReminderSet)
      {
        var trigger = new Trigger (TimeSpan.FromMinutes (-source.ReminderMinutesBeforeStart));

        target.Alarms.Add (
            new Alarm()
            {
                Trigger = trigger,
                Description = "This is an event reminder"
            }
            );

        // Fix for google, since Google wants ACTION property DISPLAY in uppercase
        var actionProperty = new CalendarProperty ("ACTION", "DISPLAY");
        target.Alarms[0].Properties.Add (actionProperty);
      }
    }

    private void MapReminder2To1 (IEvent source, AppointmentItem target)
    {
      if (source.Alarms.Count == 0)
      {
        target.ReminderSet = false;
        return;
      }

      if (source.Alarms.Count > 1)
        s_logger.WarnFormat ("Event '{0}' contains multiple alarms. Ignoring all except first.", source.Url);

      var alarm = source.Alarms[0];

      if (alarm.Trigger == null)
      {
        s_logger.WarnFormat ("Event '{0}' contains non RFC-conform alarm. Ignoring alarm.", source.Url);
        target.ReminderSet = false;
        return;
      }

      if (!(alarm.Trigger.IsRelative
            && alarm.Trigger.Related == TriggerRelation.Start
            && alarm.Trigger.Duration.HasValue
            && alarm.Trigger.Duration < TimeSpan.Zero))
      {
        s_logger.WarnFormat ("Event '{0}' alarm is not relative before event start. Ignoring.", source.Url);
        target.ReminderSet = false;
        return;
      }

      target.ReminderSet = true;
      target.ReminderMinutesBeforeStart = -(int) alarm.Trigger.Duration.Value.TotalMinutes;
    }

    private string MapPrivacy1To2 (OlSensitivity value)
    {
      switch (value)
      {
        case OlSensitivity.olNormal:
          return "PUBLIC";
        case OlSensitivity.olPersonal:
          return "PRIVATE"; // not sure
        case OlSensitivity.olPrivate:
          return "PRIVATE";
        case OlSensitivity.olConfidential:
          return "CONFIDENTIAL";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private OlSensitivity MapPrivacy2To1 (string value)
    {
      switch (value)
      {
        case "PUBLIC":
          return OlSensitivity.olNormal;
        case "PRIVATE":
          return OlSensitivity.olPrivate;
        case "CONFIDENTIAL":
          return OlSensitivity.olConfidential;
      }
      return OlSensitivity.olNormal;
    }

    private string MapParticipation1To2 (OlResponseStatus value)
    {
      switch (value)
      {
        case OlResponseStatus.olResponseAccepted:
          return "ACCEPTED";
        case OlResponseStatus.olResponseDeclined:
          return "DECLINED";
        case OlResponseStatus.olResponseNone:
          return null;
        case OlResponseStatus.olResponseNotResponded:
          return "NEEDS-ACTION";
        case OlResponseStatus.olResponseOrganized:
          return "ACCEPTED";
        case OlResponseStatus.olResponseTentative:
          return "TENTATIVE";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private OlResponseStatus MapParticipation2To1 (string value)
    {
      switch (value)
      {
        case "NEEDS-ACTION":
          return OlResponseStatus.olResponseNotResponded;
        case "ACCEPTED":
          return OlResponseStatus.olResponseAccepted;
        case "DECLINED":
          return OlResponseStatus.olResponseDeclined;
        case "TENTATIVE":
          return OlResponseStatus.olResponseTentative;
        case "DELEGATED":
          return OlResponseStatus.olResponseNotResponded;
        case null:
          return OlResponseStatus.olResponseNone;
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }


    private OlMeetingResponse? MapParticipation2ToMeetingResponse (string value)
    {
      switch (value)
      {
        case "ACCEPTED":
          return OlMeetingResponse.olMeetingAccepted;
        case "DECLINED":
          return OlMeetingResponse.olMeetingDeclined;
        case "TENTATIVE":
          return OlMeetingResponse.olMeetingTentative;
        case "NEEDS-ACTION":
        case "DELEGATED":
        case null:
          return null;
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private AddressEntry GetEventOrganizer (AppointmentItem source)
    {
      if (_outlookMajorVersion < 14)
      {
        // Microsoft recommends this way for Outlook 2007. May still work with Outlook 2010+
        using (var propertyAccessor = GenericComObjectWrapper.Create (source.PropertyAccessor))
        {
          string organizerEntryID = propertyAccessor.Inner.BinaryToString (propertyAccessor.Inner.GetProperty (PR_SENT_REPRESENTING_ENTRYID));
          return Globals.ThisAddIn.Application.Session.GetAddressEntryFromID (organizerEntryID);
        }
      }
      else
      {
        // NB this works with Outlook 2010 but crashes with Outlook 2007
        return source.GetOrganizer();
      }
    }

    private void MapOrganizer1To2 (AppointmentItem source, IEvent target)
    {
      using (var organizerWrapper = GenericComObjectWrapper.Create (GetEventOrganizer (source)))
      {
        if (organizerWrapper.Inner != null && source.MeetingStatus != OlMeetingStatus.olNonMeeting)
        {
          if (StringComparer.InvariantCultureIgnoreCase.Compare (organizerWrapper.Inner.Name, source.Organizer) == 0)
          {
            SetOrganizer (target, organizerWrapper.Inner);
          }
          else
          {
            SetOrganizer (target, source.Organizer, source.GetPropertySafe (PR_SENDER_EMAIL_ADDRESS));
          }
        }
      }
    }

    private void SetOrganizer (IEvent target, AddressEntry organizer)
    {
      var targetOrganizer = new Organizer (GetMailUrl (organizer));
      targetOrganizer.CommonName = organizer.Name;
      target.Organizer = targetOrganizer;
      target.Organizer.Parameters.Add ("SCHEDULE-AGENT", "CLIENT");
    }

    private void SetOrganizer (IEvent target, string organizerCN, string organizerEmail)
    {
      var targetOrganizer = new Organizer (string.Format ("MAILTO:{0}", organizerEmail));
      targetOrganizer.CommonName = organizerCN;
      target.Organizer = targetOrganizer;
    }

    private string GetMailUrl (AddressEntry addressEntry)
    {
      string emailAddress = string.Empty;

      if (addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeUserAddressEntry
          || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
          || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeAgentAddressEntry
          || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeOrganizationAddressEntry
          || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangePublicFolderAddressEntry)
      {
        using (var exchUser = GenericComObjectWrapper.Create (addressEntry.GetExchangeUser()))
        {
          if (exchUser != null)
          {
            emailAddress = exchUser.Inner.PrimarySmtpAddress;
          }
        }
      }
      else if (addressEntry.AddressEntryUserType == OlAddressEntryUserType.olExchangeDistributionListAddressEntry
               || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olOutlookDistributionListAddressEntry)
      {
        using (var exchDL = GenericComObjectWrapper.Create (addressEntry.GetExchangeDistributionList()))
        {
          if (exchDL != null)
          {
            emailAddress = exchDL.Inner.PrimarySmtpAddress;
          }
        }
      }
      else if (addressEntry.AddressEntryUserType == OlAddressEntryUserType.olSmtpAddressEntry
               || addressEntry.AddressEntryUserType == OlAddressEntryUserType.olLdapAddressEntry)
      {
        emailAddress = addressEntry.Address;
      }
      else if (addressEntry.AddressEntryUserType == OlAddressEntryUserType.olOutlookContactAddressEntry)
      {
        if (addressEntry.Type == "EX")
        {
          using (var exchContact = GenericComObjectWrapper.Create (addressEntry.GetContact()))
          {
            if (exchContact != null)
            {
              if (exchContact.Inner.Email1AddressType == "EX")
              {
                try
                {
                  emailAddress = exchContact.Inner.GetPropertySafe (PR_EMAIL1ADDRESS);
                }
                catch (COMException ex)
                {
                  s_logger.Error ("Could not get property PR_EMAIL1ADDRESS for adressEntry", ex);
                }
              }
              else
              {
                emailAddress = exchContact.Inner.Email1Address;
              }
            }
          }
        }
        else
        {
          emailAddress = addressEntry.Address;
        }
      }
      else
      {
        try
        {
          emailAddress = addressEntry.GetPropertySafe (PR_SMTP_ADDRESS);
        }
        catch (COMException ex)
        {
          s_logger.Error ("Could not get property PR_SMTP_ADDRESS for adressEntry", ex);
        }
      }

      emailAddress = string.Format ("MAILTO:{0}", emailAddress);
      if (!Uri.IsWellFormedUriString (emailAddress, UriKind.Absolute))
      {
        s_logger.ErrorFormat ("Invalid email address URI {0} for addressEntry", emailAddress);
        s_logger.DebugFormat ("emailaddress: {0} AdressEntryType: {1} AdressEntryUserType: {2} addressEntry.Address: {3}", emailAddress, addressEntry.Type, addressEntry.AddressEntryUserType, addressEntry.Address);
        emailAddress = string.Empty;
      }
      return emailAddress;
    }

    private void MapRecurrance1To2 (AppointmentItem source, IEvent target, iCalTimeZone startIcalTimeZone, iCalTimeZone endIcalTimeZone)
    {
      if (source.IsRecurring)
      {
        using (var sourceRecurrencePatternWrapper = GenericComObjectWrapper.Create (source.GetRecurrencePattern()))
        {
          var sourceRecurrencePattern = sourceRecurrencePatternWrapper.Inner;
          IRecurrencePattern targetRecurrencePattern = new RecurrencePattern();
          if (!sourceRecurrencePattern.NoEndDate)
          {
            targetRecurrencePattern.Count = sourceRecurrencePattern.Occurrences;
            //Until must not be set if count is set, since outlook always sets Occurrences
            //but sogo wants it as utc end time of the last event not only the enddate at 0000
            //targetRecurrencePattern.Until = sourceRecurrencePattern.PatternEndDate.Add(sourceRecurrencePattern.EndTime.TimeOfDay).ToUniversalTime();
          }
          targetRecurrencePattern.Interval = (sourceRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                              sourceRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth) ? sourceRecurrencePattern.Interval / 12 : sourceRecurrencePattern.Interval;

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

              if (sourceRecurrencePattern.Instance == 5)
              {
                targetRecurrencePattern.BySetPosition.Add (-1);
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.Instance > 0)
              {
                targetRecurrencePattern.BySetPosition.Add (sourceRecurrencePattern.Instance);
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else
              {
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              break;
            case OlRecurrenceType.olRecursYearly:
              targetRecurrencePattern.Frequency = FrequencyType.Yearly;
              targetRecurrencePattern.ByMonthDay.Add (sourceRecurrencePattern.DayOfMonth);
              targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
              break;
            case OlRecurrenceType.olRecursYearNth:
              targetRecurrencePattern.Frequency = FrequencyType.Yearly;
              if (sourceRecurrencePattern.Instance == 5)
              {
                targetRecurrencePattern.BySetPosition.Add (-1);
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else if (sourceRecurrencePattern.Instance > 0)
              {
                targetRecurrencePattern.BySetPosition.Add (sourceRecurrencePattern.Instance);
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              else
              {
                MapDayOfWeek1To2 (sourceRecurrencePattern.DayOfWeekMask, targetRecurrencePattern.ByDay);
              }
              targetRecurrencePattern.ByMonth.Add (sourceRecurrencePattern.MonthOfYear);
              break;
          }

          target.RecurrenceRules.Add (targetRecurrencePattern);

          Dictionary<DateTime, PeriodList> targetExceptionDatesByOriginalOutlookDate = new Dictionary<DateTime, PeriodList>();
          HashSet<DateTime> originalOutlookDatesWithExceptions = new HashSet<DateTime>();

          foreach (var sourceException in sourceRecurrencePattern.Exceptions.ToSafeEnumerable<Exception>())
          {
            if (!sourceException.Deleted)
            {
              targetExceptionDatesByOriginalOutlookDate.Remove (sourceException.OriginalDate);
              originalOutlookDatesWithExceptions.Add (sourceException.OriginalDate);

              var targetException = new Event();
              target.Calendar.Events.Add (targetException);
              targetException.UID = target.UID;
              using (var wrapper = new AppointmentItemWrapper (sourceException.AppointmentItem, _ => { throw new InvalidOperationException ("Cannot reload exception AppointmentITem!"); }))
              {
                Map1To2 (wrapper.Inner, targetException, true, startIcalTimeZone, endIcalTimeZone);

                // check if new exception is already present in target
                // if it is found and not already present as exdate then add a new exdate to avoid 2 events
                var targetContainsExceptionList = target.GetOccurrences (wrapper.Inner.Start.Date, wrapper.Inner.End.Date.AddDays (1));
                if (targetContainsExceptionList.Count > 0)
                {
                  if (!originalOutlookDatesWithExceptions.Contains (wrapper.Inner.Start) && wrapper.Inner.Start.Date != sourceException.OriginalDate.Date)
                  {
                    PeriodList targetExList = new PeriodList();

                    if (wrapper.Inner.AllDayEvent)
                    {
                      iCalDateTime exDate = new iCalDateTime (wrapper.Inner.Start);
                      exDate.HasTime = false;
                      targetExList.Add (exDate);
                      targetExList.Parameters.Add ("VALUE", "DATE");
                    }
                    else
                    {
                      var timeZone = TimeZoneInfo.FindSystemTimeZoneById (wrapper.Inner.StartTimeZone.ID);
                      var originalDateUtc = TimeZoneInfo.ConvertTimeToUtc (wrapper.Inner.StartInStartTimeZone.Date, timeZone);
                      iCalDateTime exDate = new iCalDateTime (originalDateUtc.Add (source.StartInStartTimeZone.TimeOfDay)) { IsUniversalTime = true };

                      targetExList.Add (exDate);
                    }
                    targetExceptionDatesByOriginalOutlookDate.Add (wrapper.Inner.Start.Date.Add (source.StartInStartTimeZone.TimeOfDay), targetExList);
                  }
                }
              }

              if (source.AllDayEvent)
              {
                // Outlook's AllDayEvent relates to Start and not not StartUtc!!!
                targetException.RecurrenceID = new iCalDateTime (sourceException.OriginalDate);
                targetException.RecurrenceID.HasTime = false;
              }
              else
              {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById (source.StartTimeZone.ID);
                var originalDateUtc = TimeZoneInfo.ConvertTimeToUtc (sourceException.OriginalDate, timeZone);
                targetException.RecurrenceID = new iCalDateTime (originalDateUtc) { IsUniversalTime = true };
              }
            }
            else
            {
              if (!originalOutlookDatesWithExceptions.Contains (sourceException.OriginalDate))
              {
                PeriodList targetExList = new PeriodList();

                if (source.AllDayEvent)
                {
                  iCalDateTime exDate = new iCalDateTime (sourceException.OriginalDate);
                  exDate.HasTime = false;
                  targetExList.Add (exDate);
                  targetExList.Parameters.Add ("VALUE", "DATE");
                }
                else
                {
                  var timeZone = TimeZoneInfo.FindSystemTimeZoneById (source.StartTimeZone.ID);
                  var originalDateUtc = TimeZoneInfo.ConvertTimeToUtc (sourceException.OriginalDate, timeZone);
                  iCalDateTime exDate = new iCalDateTime (originalDateUtc.Add (source.StartInStartTimeZone.TimeOfDay)) { IsUniversalTime = true };

                  targetExList.Add (exDate);
                }
                targetExceptionDatesByOriginalOutlookDate.Add (sourceException.OriginalDate, targetExList);
              }
            }
          }
          target.ExceptionDates.AddRange (targetExceptionDatesByOriginalOutlookDate.Values);
        }
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


    private void MapRecurrance2To1 (IEvent source, IReadOnlyCollection<IEvent> exceptions, AppointmentItemWrapper targetWrapper)
    {
      if (source.RecurrenceRules.Count > 0)
      {
        using (var targetRecurrencePatternWrapper = GenericComObjectWrapper.Create (targetWrapper.Inner.GetRecurrencePattern()))
        {
          var targetRecurrencePattern = targetRecurrencePatternWrapper.Inner;
          if (source.RecurrenceRules.Count > 1)
          {
            s_logger.WarnFormat ("Event '{0}' contains more than one recurrence rule. Since outlook supports only one rule, all except the first one will be ignored.", source.Url);
          }
          var sourceRecurrencePattern = source.RecurrenceRules[0];

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
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursWeekly;
              }
              break;
            case FrequencyType.Monthly:
              if (sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthNth;
                if (sourceRecurrencePattern.ByWeekNo.Count > 1)
                {
                  s_logger.WarnFormat ("Event '{0}' contains more than one week in a monthly recurrence rule. Since outlook supports only one week, all except the first one will be ignored.", source.Url);
                }
                else if (sourceRecurrencePattern.ByWeekNo.Count > 0)
                {
                  targetRecurrencePattern.Instance = sourceRecurrencePattern.ByWeekNo[0];
                }
                else
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.ByDay[0].Offset >= 0) ? sourceRecurrencePattern.ByDay[0].Offset : 5;
                }
                if (sourceRecurrencePattern.BySetPosition.Count > 0)
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.BySetPosition[0] >= 0) ? sourceRecurrencePattern.BySetPosition[0] : 5;
                }
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
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursMonthly;
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
              else if (sourceRecurrencePattern.ByMonth.Count > 0 && sourceRecurrencePattern.ByDay.Count > 0)
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearNth;
                if (sourceRecurrencePattern.ByMonth.Count > 1)
                {
                  s_logger.WarnFormat ("Event '{0}' contains more than one months in a yearly recurrence rule. Since outlook supports only one month, all except the first one will be ignored.", source.Url);
                }
                targetRecurrencePattern.MonthOfYear = sourceRecurrencePattern.ByMonth[0];

                targetRecurrencePattern.Instance = (sourceRecurrencePattern.ByDay[0].Offset >= 0) ? sourceRecurrencePattern.ByDay[0].Offset : 5;
                if (sourceRecurrencePattern.BySetPosition.Count > 0)
                {
                  targetRecurrencePattern.Instance = (sourceRecurrencePattern.BySetPosition[0] >= 0) ? sourceRecurrencePattern.BySetPosition[0] : 5;
                }
                targetRecurrencePattern.DayOfWeekMask = MapDayOfWeek2To1 (sourceRecurrencePattern.ByDay);
              }
              else
              {
                targetRecurrencePattern.RecurrenceType = OlRecurrenceType.olRecursYearly;
              }
              break;
            default:
              s_logger.WarnFormat ("Recurring event '{0}' contains the Frequency '{1}', which is not supported by outlook. Ignoring recurrence rule.", source.Url, sourceRecurrencePattern.Frequency);
              targetWrapper.Inner.ClearRecurrencePattern();
              break;
          }

          targetRecurrencePattern.Interval = (targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearly ||
                                              targetRecurrencePattern.RecurrenceType == OlRecurrenceType.olRecursYearNth) ? sourceRecurrencePattern.Interval * 12 : sourceRecurrencePattern.Interval;

          if (sourceRecurrencePattern.Count >= 0)
            targetRecurrencePattern.Occurrences = sourceRecurrencePattern.Count;

          if (sourceRecurrencePattern.Until != default(DateTime))
            targetRecurrencePattern.PatternEndDate = sourceRecurrencePattern.Until;
        }
        // Due to limitations out outlook, the Appointment has to be saved here. Otherwise 'targetRecurrencePattern.GetOccurrence ()'
        // will throw an exception

        targetWrapper.SaveAndReload();

        using (var targetRecurrencePatternWrapper = GenericComObjectWrapper.Create (targetWrapper.Inner.GetRecurrencePattern()))
        {
          var targetRecurrencePattern = targetRecurrencePatternWrapper.Inner;

          if (source.ExceptionDates != null)
          {
            foreach (IPeriodList exdateList in source.ExceptionDates)
            {
              foreach (IPeriod exdate in exdateList)
              {
                try
                {
                  NodaTime.DateTimeZone startZone = NodaTime.DateTimeZoneProviders.Bcl[targetWrapper.Inner.StartTimeZone.ID];
                  DateTime originalStart;

                  if (exdate.StartTime.IsUniversalTime)
                  {
                    originalStart = NodaTime.Instant.FromDateTimeUtc (exdate.StartTime.Value).InZone (startZone).ToDateTimeUnspecified().Date;
                  }
                  else
                  {
                    originalStart = exdate.StartTime.Date;
                  }
                  var originalExDate =  NodaTime.LocalDateTime.FromDateTime (originalStart.Add (targetWrapper.Inner.StartInStartTimeZone.TimeOfDay));
                  
                  NodaTime.ZonedDateTime zonedExDate = originalExDate.InZoneLeniently (startZone);
                  NodaTime.ZonedDateTime localExDate = zonedExDate.WithZone (NodaTime.DateTimeZoneProviders.Bcl.GetSystemDefault());

                  using (var wrapper = GenericComObjectWrapper.Create (targetRecurrencePattern.GetOccurrence (localExDate.ToDateTimeUnspecified())))
                  {
                    wrapper.Inner.Delete();
                  }
                }
                catch (COMException ex)
                {
                  s_logger.Error ("Can't find occurence of exception", ex);
                }
              }
            }
          }
          // to prevent skipping of occurences while moving (outlook throws exception when skipping occurences), moving has to be done in two steps
          // first move all exceptions which are preponed from earliest to latest
          MapRecurrenceExceptions2To1 (exceptions.Where (e => e.Start.UTC < e.RecurrenceID.Date).OrderBy (e => e.Start.UTC), targetWrapper, targetRecurrencePattern);
          // then move all exceptions which are postponed or are not moved from last to first
          MapRecurrenceExceptions2To1 (exceptions.Where (e => e.Start.UTC >= e.RecurrenceID.Date).OrderByDescending (e => e.Start.UTC), targetWrapper, targetRecurrencePattern);
          // HINT: this algorith will only prevent skipping while moving. If the final state contains skipped occurences, outlook will throw an exception anyway
        }
      }
    }

     private void MapRecurrenceExceptions2To1 (IEnumerable<IEvent> exceptions, AppointmentItemWrapper targetWrapper, Microsoft.Office.Interop.Outlook.RecurrencePattern targetRecurrencePattern)
    {
      foreach (var recurranceException in exceptions)
      {
        try
        {
          NodaTime.DateTimeZone startZone = NodaTime.DateTimeZoneProviders.Bcl[targetWrapper.Inner.StartTimeZone.ID];
          DateTime originalStart;

          if (recurranceException.RecurrenceID.IsUniversalTime)
          {
            originalStart = NodaTime.Instant.FromDateTimeUtc (recurranceException.RecurrenceID.Value).InZone (startZone).ToDateTimeUnspecified().Date;
          }
          else
          {
            originalStart = recurranceException.RecurrenceID.Date;
          }

          var originalExDate =  NodaTime.LocalDateTime.FromDateTime (originalStart.Add (targetWrapper.Inner.StartInStartTimeZone.TimeOfDay));
          NodaTime.ZonedDateTime zonedExDate = originalExDate.InZoneLeniently(startZone);
          NodaTime.ZonedDateTime localExDate = zonedExDate.WithZone (NodaTime.DateTimeZoneProviders.Bcl.GetSystemDefault());

          var targetException = targetRecurrencePattern.GetOccurrence (localExDate.ToDateTimeUnspecified());

          using (var exceptionWrapper = new AppointmentItemWrapper (targetException, _ => { throw new InvalidOperationException ("cannot reload exception item"); }))

          {
            Map2To1 (recurranceException, new IEvent[] { }, exceptionWrapper, true);
            exceptionWrapper.Inner.Save();
          }
        }
        catch (COMException ex)
        {
          s_logger.Error ("Can't find occurence of exception or exception can't be saved", ex);
        }
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


    private void MapAttendees1To2 (AppointmentItem source, IEvent target, out bool organizerSet)
    {
      organizerSet = false;
      bool ownAttendeeSet = false;

      foreach (var recipient in source.Recipients.ToSafeEnumerable<Recipient>())
      {
        if (!IsOwnIdentity (recipient))
        {
          Attendee attendee;

          if (!string.IsNullOrEmpty (recipient.Address))
          {
            using (var entryWrapper = GenericComObjectWrapper.Create (recipient.AddressEntry))
            {
              attendee = new Attendee (GetMailUrl (entryWrapper.Inner));
            }
          }
          else
          {
            attendee = new Attendee();
          }

          attendee.ParticipationStatus = MapParticipation1To2 (recipient.MeetingResponseStatus);
          attendee.CommonName = recipient.Name;
          attendee.Role = MapAttendeeType1To2 ((OlMeetingRecipientType) recipient.Type);
          target.Attendees.Add (attendee);
        }
        else
        {
          if ((source.MeetingStatus == OlMeetingStatus.olMeetingReceived) && (!ownAttendeeSet))
          {
            Attendee ownAttendee;

            if (!string.IsNullOrEmpty (recipient.Address))
            {
              using (var entryWrapper = GenericComObjectWrapper.Create (recipient.AddressEntry))
              {
                ownAttendee = new Attendee (GetMailUrl (entryWrapper.Inner));
              }
            }
            else
            {
              ownAttendee = new Attendee();
            }
            ownAttendee.CommonName = recipient.Name;
            ownAttendee.ParticipationStatus = MapParticipation1To2 (source.ResponseStatus);
            ownAttendee.Role = MapAttendeeType1To2 ((OlMeetingRecipientType) recipient.Type);
            target.Attendees.Add (ownAttendee);
            ownAttendeeSet = true;
          }
        }
        if (((OlMeetingRecipientType) recipient.Type) == OlMeetingRecipientType.olOrganizer)
        {
          if (!string.IsNullOrEmpty (recipient.Address))
          {
            using (var entryWrapper = GenericComObjectWrapper.Create (recipient.AddressEntry))
            {
              SetOrganizer (target, entryWrapper.Inner);
            }
          }
          else
          {
            SetOrganizer (target, recipient.Name, string.Empty);
          }

          organizerSet = true;
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
        case null:
        case "NON-PARTICIPANT":
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

    public AppointmentItemWrapper Map2To1 (IICalendar sourceCalendar, AppointmentItemWrapper target)
    {
      IEvent sourceMasterEvent = null;
      IReadOnlyCollection<IEvent> sourceExceptionEvents;

      var sourceEvents = sourceCalendar.Events;

      if (sourceEvents.Count == 1)
      {
        sourceMasterEvent = sourceEvents[0];
        sourceExceptionEvents = new IEvent[] { };
      }
      else
      {
        var sourceExceptionEventsList = new List<IEvent>();
        sourceExceptionEvents = sourceExceptionEventsList;

        foreach (var sourceEvent in sourceEvents)
        {
          if (sourceEvent.RecurrenceID == null)
            sourceMasterEvent = sourceEvent;
          else
            sourceExceptionEventsList.Add (sourceEvent);
        }

        // TODO
        // Maybe it is a goo idea to sort the exception events here by RecurrenceId
      }

      if (sourceMasterEvent == null)
        throw new System.Exception ("CalDav Ressources with contains only exceptions are NOT supported!");

      return Map2To1 (sourceMasterEvent, sourceExceptionEvents, target, false);
    }


    private AppointmentItemWrapper Map2To1 (IEvent source, IReadOnlyCollection<IEvent> recurrenceExceptionsOrNull, AppointmentItemWrapper targetWrapper, bool isRecurrenceException)
    {
      if (!isRecurrenceException && targetWrapper.Inner.IsRecurring)
      {
        targetWrapper.Inner.ClearRecurrencePattern();
        targetWrapper.SaveAndReload();
      }

      if (source.IsAllDay)
      {
        targetWrapper.Inner.Start = source.Start.Value;
        targetWrapper.Inner.End = source.End.Value;
        targetWrapper.Inner.AllDayEvent = true;
      }
      else
      {
        targetWrapper.Inner.AllDayEvent = false;
 
        if (!string.IsNullOrEmpty (source.Start.TZID))
        {
          try
          { 
            var tzi = TimeZoneInfo.FindSystemTimeZoneById (source.Start.TZID);
            targetWrapper.Inner.StartTimeZone = targetWrapper.Inner.Application.TimeZones[source.Start.TZID];
          }
          catch (COMException ex)
          {
            s_logger.Error("Can't set StartTimeZone of appointment.", ex);
          }
          catch (TimeZoneNotFoundException)
          {
            targetWrapper.Inner.StartTimeZone = targetWrapper.Inner.Application.TimeZones[TimeZoneMapper.IanaToWindows (source.Start.TZID) ?? _localTimeZoneInfo.Id];
          }
        }

        if (source.Start.IsUniversalTime)
        {
          targetWrapper.Inner.StartUTC = source.Start.Value;
        }
        else
        {
          targetWrapper.Inner.StartInStartTimeZone = source.Start.Value;
        }

        if (source.DTEnd != null)
        {
          if (!string.IsNullOrEmpty (source.DTEnd.TZID))
          {
            try
            {
              var tzi = TimeZoneInfo.FindSystemTimeZoneById(source.DTEnd.TZID);
              targetWrapper.Inner.EndTimeZone = targetWrapper.Inner.Application.TimeZones[source.DTEnd.TZID];
            }
            catch (COMException ex)
            {
              s_logger.Error("Can't set EndTimeZone of appointment.", ex);
            }
            catch (TimeZoneNotFoundException)
            {
              targetWrapper.Inner.EndTimeZone = targetWrapper.Inner.Application.TimeZones[TimeZoneMapper.IanaToWindows(source.DTEnd.TZID) ?? _localTimeZoneInfo.Id];
            }
          }

          if (source.DTEnd.IsUniversalTime)
          {
            targetWrapper.Inner.EndUTC = source.DTEnd.Value;
          }
          else
          {
            targetWrapper.Inner.EndInEndTimeZone = source.DTEnd.Value;
          }
        }
        else if (source.Start.HasTime)
        {
          targetWrapper.Inner.EndTimeZone = targetWrapper.Inner.StartTimeZone;
          targetWrapper.Inner.End = targetWrapper.Inner.Start;
        }
        else
        {
          targetWrapper.Inner.EndUTC = source.Start.AddDays (1).UTC;
        }
      }


      targetWrapper.Inner.Subject = source.Summary;
      targetWrapper.Inner.Location = source.Location;
      targetWrapper.Inner.Body = source.Description;

      targetWrapper.Inner.Importance = MapPriority2To1 (source.Priority);

      MapAttendeesAndOrganizer2To1 (source, targetWrapper.Inner);

      if (!isRecurrenceException)
        MapRecurrance2To1 (source, recurrenceExceptionsOrNull, targetWrapper);

      if (!isRecurrenceException)
        targetWrapper.Inner.Sensitivity = MapPrivacy2To1 (source.Class);

      MapReminder2To1 (source, targetWrapper.Inner);

      if (!isRecurrenceException)
        MapCategories2To1 (source, targetWrapper.Inner);

      targetWrapper.Inner.BusyStatus = MapTransparency2To1 (source.Transparency);

      if (source.Organizer != null)
      {
        var ownSourceAttendee = source.Attendees.FirstOrDefault (a => StringComparer.InvariantCultureIgnoreCase.Compare (a.Value != null ? a.Value.ToString() : null, _serverEmailUri) == 0);
        if (ownSourceAttendee != null)
        {
          var response = MapParticipation2ToMeetingResponse (ownSourceAttendee.ParticipationStatus);
          if ((response != null) && (MapParticipation2To1 (ownSourceAttendee.ParticipationStatus) != targetWrapper.Inner.ResponseStatus))
          {
            using (var newMeetingItem = GenericComObjectWrapper.Create (targetWrapper.Inner.Respond (response.Value)))
            {
              var newAppointment = newMeetingItem.Inner.GetAssociatedAppointment (false);
              targetWrapper.Replace (newAppointment);
            }
          }
        }
      }

      return targetWrapper;
    }

    private static void MapCategories2To1 (IEvent source, AppointmentItem target)
    {
      target.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, source.Categories);
    }


    private void MapAttendeesAndOrganizer2To1 (IEvent source, AppointmentItem target)
    {
      var recipientsToDispose = new HashSet<Recipient>();
      try
      {
        var targetRecipientsWhichShouldRemain = new HashSet<Recipient>();
        var indexByEmailAddresses = GetOutlookRecipientsByEmailAddressesOrName (target, recipientsToDispose);

        foreach (var attendee in source.Attendees)
        {
          Recipient targetRecipient = null;

          string attendeeEmail = string.Empty;
          if (attendee.Value != null)
          {
            try
            {
              attendeeEmail = attendee.Value.ToString();
            }
            catch (UriFormatException ex)
            {
              s_logger.Error ("Ignoring invalid Uri in attendee email.", ex);
            }
          }
          if (attendeeEmail.Length >= s_mailtoSchemaLength && !string.IsNullOrEmpty (attendeeEmail.Substring (s_mailtoSchemaLength)))
          {
            if (!indexByEmailAddresses.TryGetValue (attendeeEmail, out targetRecipient))
            {
              if (!string.IsNullOrEmpty (attendee.CommonName))
              {
                targetRecipient = target.Recipients.Add (attendee.CommonName + "<" + attendeeEmail.Substring (s_mailtoSchemaLength) + ">");
              }
              else
              {
                targetRecipient = target.Recipients.Add (attendeeEmail.Substring (s_mailtoSchemaLength));
              }
            }
          }
          else
          {
            if (!string.IsNullOrEmpty (attendee.CommonName))
              targetRecipient = target.Recipients.Add (attendee.CommonName);
          }

          if (targetRecipient != null)
          {
            recipientsToDispose.Add (targetRecipient);
            targetRecipientsWhichShouldRemain.Add (targetRecipient);
            targetRecipient.Type = (int) MapAttendeeType2To1 (attendee.Role);
            targetRecipient.Resolve();
          }
        }

        if (source.Organizer != null)
        {
          string sourceOrganizerEmail = string.Empty;

          try
          {
            sourceOrganizerEmail = source.Organizer.Value.ToString().Substring (s_mailtoSchemaLength);
          }
          catch (UriFormatException ex)
          {
            s_logger.Error ("Ignoring invalid Uri in organizer email.", ex);
          }

          if (StringComparer.InvariantCultureIgnoreCase.Compare (sourceOrganizerEmail, _outlookEmailAddress) != 0)
          {
            Recipient targetRecipient = null;

            target.MeetingStatus = OlMeetingStatus.olMeetingReceived;

            targetRecipient = target.Recipients.Add (source.Organizer.CommonName + "<" + sourceOrganizerEmail + ">");
            recipientsToDispose.Add (targetRecipient);
            targetRecipientsWhichShouldRemain.Add (targetRecipient);
            targetRecipient.Type = (int) OlMeetingRecipientType.olOrganizer;

            using (var oPa = GenericComObjectWrapper.Create (target.PropertyAccessor))
            {
              string organizerID = null;

              if (targetRecipient.Resolve())
              {
                using (var organizerAddressEntry = GenericComObjectWrapper.Create (targetRecipient.AddressEntry))
                {
                  organizerID = organizerAddressEntry.Inner.ID;
                }
              }

              var propertyTagsSentRepresenting = new object[] { PR_SENT_REPRESENTING_NAME, PR_SENT_REPRESENTING_EMAIL_ADDRESS, PR_SENT_REPRESENTING_ADDRTYPE, PR_SENT_REPRESENTING_ENTRYID };
              var propertyTagsSender = new object[] { PR_SENDER_NAME, PR_SENDER_EMAIL_ADDRESS, PR_SENT_REPRESENTING_ADDRTYPE, PR_SENDER_ENTRYID };
              object[] propertyValues;

              if (source.Organizer.CommonName != null)
              {
                propertyValues = new object[] { source.Organizer.CommonName, sourceOrganizerEmail, "SMTP", oPa.Inner.StringToBinary (organizerID) };
              }
              else
              {
                propertyValues = new object[] { sourceOrganizerEmail, sourceOrganizerEmail, "SMTP", oPa.Inner.StringToBinary (organizerID) };
              }

              try
              {
                oPa.Inner.SetProperties (propertyTagsSentRepresenting, propertyValues);

                if (_outlookMajorVersion >= 15)
                {
                  oPa.Inner.SetProperties (propertyTagsSender, propertyValues);
                }
              }
              catch (COMException ex)
              {
                s_logger.Error ("Could not set property PR_SENDER_* for organizer", ex);
              }
            }
          }
          else if (target.Recipients.Count > 0)
          {
            target.MeetingStatus = OlMeetingStatus.olMeeting;
          }
          else
          {
            target.MeetingStatus = OlMeetingStatus.olNonMeeting;
          }
        }
        else
        {
          target.MeetingStatus = OlMeetingStatus.olNonMeeting;
        }

        for (int i = target.Recipients.Count; i > 0; i--)
        {
          var recipient = target.Recipients[i];
          recipientsToDispose.Add (recipient);
          if (!IsOwnIdentity (recipient))
          {
            if (!targetRecipientsWhichShouldRemain.Contains (recipient))
              target.Recipients.Remove (i);
          }
        }
      }
      finally
      {
        recipientsToDispose.ToSafeEnumerable().ToArray();
      }
    }

    private Dictionary<string, Recipient> GetOutlookRecipientsByEmailAddressesOrName (AppointmentItem appointment, HashSet<Recipient> disposeList)
    {
      Dictionary<string, Recipient> indexByEmailAddresses = new Dictionary<string, Recipient> (StringComparer.InvariantCultureIgnoreCase);

      foreach (Recipient recipient in appointment.Recipients)
      {
        disposeList.Add (recipient);
        if (! string.IsNullOrEmpty (recipient.Address))
        {
          using (var entryWrapper = GenericComObjectWrapper.Create (recipient.AddressEntry))
          {
            indexByEmailAddresses[GetMailUrl (entryWrapper.Inner)] = recipient;
          }
        }
        else
        {
          indexByEmailAddresses[recipient.Name] = recipient;
        }
      }

      return indexByEmailAddresses;
    }
  }
}