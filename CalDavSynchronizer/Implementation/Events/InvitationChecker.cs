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
using System.Reflection;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Implementation.Events
{
  class InvitationChecker : IInvitationChecker
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly int _outlookMajorVersion;

    private readonly string _serverEmailAddress;

    public InvitationChecker (string serverEmailAddress, string outlookApplicationVersion)

    {
      _serverEmailAddress = serverEmailAddress;

      string outlookMajorVersionString = outlookApplicationVersion.Split(new char[] { '.' })[0];
      _outlookMajorVersion = Convert.ToInt32(outlookMajorVersionString);
    }

    public bool IsMeetingInvitationFromServerIdentity (AppointmentItem appointment)
    {
      return
          IsMeetingReceived (appointment) &&
          IsServerIdentityOrganizer (appointment);
    }

    private bool IsMeetingReceived (AppointmentItem appointment)
    {
      return  appointment.MeetingStatus == OlMeetingStatus.olMeetingReceived ||
              appointment.MeetingStatus == OlMeetingStatus.olMeetingReceivedAndCanceled;
    }

    private bool IsServerIdentityOrganizer (AppointmentItem appointment)
    {
      using (var organizerWrapper = GenericComObjectWrapper.Create (
              OutlookUtility.GetEventOrganizerOrNull (appointment, NullEntitySynchronizationLogger.Instance, s_logger,_outlookMajorVersion)
            ))
      {
        var organizerEmail =
            OutlookUtility.GetEmailAdressOrNull (organizerWrapper.Inner, NullEntitySynchronizationLogger.Instance, s_logger) ??
            OutlookUtility.GetSenderEmailAddressOrNull (appointment, NullEntitySynchronizationLogger.Instance, s_logger);

        return string.Compare (organizerEmail, _serverEmailAddress, StringComparison.OrdinalIgnoreCase) == 0;
      }
    }

  
  }
}