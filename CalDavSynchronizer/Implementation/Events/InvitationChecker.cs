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
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Events
{
  class InvitationChecker : IInvitationChecker
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod ().DeclaringType);

    private readonly string _serverEmailAddress;
    private readonly string _outlookEmailAddress;
    private readonly bool _isServerIdentityDifferentThanOutlookIdentity;

    public InvitationChecker (string serverEmailAddress, string outlookEmailAddress)

    {
      _serverEmailAddress = serverEmailAddress;
      _outlookEmailAddress = outlookEmailAddress;
      _isServerIdentityDifferentThanOutlookIdentity = string.Compare (_outlookEmailAddress, _serverEmailAddress, StringComparison.OrdinalIgnoreCase) != 0;
    }

    public bool IsInvitationFromServerIdentityToOutlookIdentity (AppointmentItem appointment)
    {
      return
          _isServerIdentityDifferentThanOutlookIdentity &&
          IsServerIdentityOrganizer (appointment) &&
          IsOutlookIdentityInvited (appointment);
    }

    private bool IsServerIdentityOrganizer (AppointmentItem appointment)
    {
      using (var organizerWrapper = GenericComObjectWrapper.Create (appointment.GetOrganizer ()))
      {
        var organizerEmail =
            OutlookUtility.GetEmailAdressOrNull (organizerWrapper.Inner, NullEntitySynchronizationLogger.Instance, s_logger) ??
            OutlookUtility.GetSenderEmailAddressOrNull (appointment, NullEntitySynchronizationLogger.Instance, s_logger);

        return string.Compare (organizerEmail, _serverEmailAddress, StringComparison.OrdinalIgnoreCase) == 0;
      }
    }

    private bool IsOutlookIdentityInvited (AppointmentItem appointment)
    {
      foreach (var recipient in appointment.Recipients.ToSafeEnumerable<Recipient> ())
      {
        if (recipient.Resolve ())
        {
          using (var entryWrapper = GenericComObjectWrapper.Create (recipient.AddressEntry))
          {
            var recipientMailAddressOrNull = OutlookUtility.GetEmailAdressOrNull (entryWrapper.Inner, NullEntitySynchronizationLogger.Instance, s_logger);
            if (!string.IsNullOrEmpty (recipientMailAddressOrNull))
            {
              if (string.Compare (recipientMailAddressOrNull, _outlookEmailAddress, StringComparison.OrdinalIgnoreCase) == 0)
              {
                return true;
              }
            }
          }
        }
      }

      return false;
    }
  }
}