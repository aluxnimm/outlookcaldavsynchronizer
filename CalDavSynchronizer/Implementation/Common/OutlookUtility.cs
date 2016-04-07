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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.Logging;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.Common
{
  class OutlookUtility
  {
    private const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";
    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";

    public static string GetEmailAdressOrNull (AddressEntry addressEntry, IEntityMappingLogger logger, ILog generalLogger)
    {
      OlAddressEntryUserType type;

      if (addressEntry != null)
      {
        try
        {
          type = addressEntry.AddressEntryUserType;
        }
        catch (COMException ex)
        {
          generalLogger.Warn ("Could not get type from AddressEntry", ex);
          logger.LogMappingWarning ("Could not get type from AddressEntry", ex);
          return null;
        }
        if (type == OlAddressEntryUserType.olExchangeUserAddressEntry
            || type == OlAddressEntryUserType.olExchangeRemoteUserAddressEntry
            || type == OlAddressEntryUserType.olExchangeAgentAddressEntry
            || type == OlAddressEntryUserType.olExchangeOrganizationAddressEntry
            || type == OlAddressEntryUserType.olExchangePublicFolderAddressEntry)
        {
          try
          {
            using (var exchUser = GenericComObjectWrapper.Create (addressEntry.GetExchangeUser ()))
            {
              if (exchUser.Inner != null)
              {
                return exchUser.Inner.PrimarySmtpAddress;
              }
            }
          }
          catch (COMException ex)
          {
            generalLogger.Warn ("Could not get email address from adressEntry.GetExchangeUser()", ex);
            logger.LogMappingWarning ("Could not get email address from adressEntry.GetExchangeUser()", ex);
          }
        }
        else if (type == OlAddressEntryUserType.olExchangeDistributionListAddressEntry
                 || type == OlAddressEntryUserType.olOutlookDistributionListAddressEntry)
        {
          try
          {
            using (var exchDL = GenericComObjectWrapper.Create (addressEntry.GetExchangeDistributionList ()))
            {
              if (exchDL.Inner != null)
              {
                return exchDL.Inner.PrimarySmtpAddress;
              }
            }
          }
          catch (COMException ex)
          {
            generalLogger.Warn ("Could not get email address from adressEntry.GetExchangeDistributionList()", ex);
            logger.LogMappingWarning ("Could not get email address from adressEntry.GetExchangeDistributionList()", ex);
          }
        }
        else if (type == OlAddressEntryUserType.olSmtpAddressEntry
                 || type == OlAddressEntryUserType.olLdapAddressEntry)
        {
          return addressEntry.Address;
        }
        else if (type == OlAddressEntryUserType.olOutlookContactAddressEntry)
        {
          if (addressEntry.Type == "EX")
          {
            try
            {
              using (var exchContact = GenericComObjectWrapper.Create (addressEntry.GetContact ()))
              {
                if (exchContact.Inner != null)
                {
                  if (exchContact.Inner.Email1AddressType == "EX")
                  {
                    return exchContact.Inner.GetPropertySafe (PR_EMAIL1ADDRESS);
                  }
                  else
                  {
                    return exchContact.Inner.Email1Address;
                  }
                }
              }
            }
            catch (COMException ex)
            {
              generalLogger.Warn ("Could not get email address from adressEntry.GetContact()", ex);
              logger.LogMappingWarning ("Could not get email address from adressEntry.GetContact()", ex);
            }
          }
          else
          {
            return addressEntry.Address;
          }
        }
        else
        {
          try
          {
            return addressEntry.GetPropertySafe (PR_SMTP_ADDRESS);
          }
          catch (COMException ex)
          {
            generalLogger.Warn ("Could not get property PR_SMTP_ADDRESS for adressEntry", ex);
            logger.LogMappingWarning ("Could not get property PR_SMTP_ADDRESS for adressEntry", ex);
          }
        }
      }

      return null;
    }
  }
}
