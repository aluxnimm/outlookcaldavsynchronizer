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
using System.Text.RegularExpressions;
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
    private const string PR_SENDER_EMAIL_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x0C1F001E";
    private const string PR_SENT_REPRESENTING_ENTRYID = "http://schemas.microsoft.com/mapi/proptag/0x00410102";

    public static readonly DateTime OUTLOOK_DATE_NONE = new DateTime (4501, 1, 1, 0 ,0, 0);
    
    public static string GetEmailAdressOrNull (AddressEntry addressEntry, IEntityMappingLogger logger, ILog generalLogger)
    {
      OlAddressEntryUserType type;

      if (addressEntry != null)
      {
        try
        {
          type = addressEntry.AddressEntryUserType;
        }
        catch (System.Exception ex)
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
          catch (System.Exception ex)
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
          catch (System.Exception ex)
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
            catch (System.Exception ex)
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
          catch (System.Exception ex)
          {
            generalLogger.Warn ("Could not get property PR_SMTP_ADDRESS for adressEntry", ex);
            logger.LogMappingWarning ("Could not get property PR_SMTP_ADDRESS for adressEntry", ex);
          }
        }
      }

      return null;
    }

    public static string GetSenderEmailAddressOrNull (AppointmentItem source, IEntityMappingLogger logger, ILog generalLogger)
    {
      try
      {
        return source.GetPropertySafe (PR_SENDER_EMAIL_ADDRESS);
      }
      catch (System.Exception ex)
      {
        generalLogger.Warn ("Can't access property PR_SENDER_EMAIL_ADDRESS of appointment", ex);
        logger.LogMappingWarning ("Can't access property PR_SENDER_EMAIL_ADDRESS of appointment", ex);
        return null;
      }
    }

    public static AddressEntry GetEventOrganizerOrNull (AppointmentItem source, IEntityMappingLogger logger, ILog generalLogger, int outlookMajorVersion)
    {
      try
      {
        if (outlookMajorVersion < 14)
        {
          // Microsoft recommends this way for Outlook 2007. May still work with Outlook 2010+
          using (var propertyAccessor = GenericComObjectWrapper.Create (source.PropertyAccessor))
          {
            string organizerEntryID = propertyAccessor.Inner.BinaryToString (propertyAccessor.Inner.GetProperty(PR_SENT_REPRESENTING_ENTRYID));
            return Globals.ThisAddIn.Application.Session.GetAddressEntryFromID (organizerEntryID);
          }
        }
        else
        {
          // NB this works with Outlook 2010 but crashes with Outlook 2007
          return source.GetOrganizer();
        }
      }
      catch (System.Exception ex)
      {
        generalLogger.Warn ("Can't get organizer of appointment", ex);
        logger.LogMappingWarning ("Can't get organizer of appointment", ex);
        return null;
      }
    }

    public static byte[] MapUidToGlobalId (string uid)
    {
      byte[] prefix = { 0x04, 0x00, 0x00, 0x00, 0x82, 0x00, 0xE0, 0x00, 0x74, 0xC5, 0xB7, 0x10, 0x1A, 0x82, 0xE0, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
      byte[] uidPrefix = { 0x76, 0x43, 0x61, 0x6C, 0x2D, 0x55, 0x69, 0x64, 0x01, 0x00, 0x00, 0x00 };
      byte[] uidBytes = Encoding.Default.GetBytes (uid + "\0");
      int len = uid.Length + uidPrefix.Length + 1;
      byte[] size = BitConverter.GetBytes (len);

      byte[] globalId = new byte[prefix.Length + size.Length + uidPrefix.Length + uidBytes.Length];
      Buffer.BlockCopy (prefix, 0, globalId, 0, prefix.Length);
      Buffer.BlockCopy (size, 0, globalId, prefix.Length, size.Length);
      Buffer.BlockCopy (uidPrefix, 0, globalId, prefix.Length + size.Length, uidPrefix.Length);
      Buffer.BlockCopy (uidBytes, 0, globalId, prefix.Length + size.Length + uidPrefix.Length, uidBytes.Length);

      return globalId;
    }

    public static byte[] MapUidToGlobalExceptionId (string uid, DateTime originalStart)
    {
      byte[] globalId = MapUidToGlobalId (uid);

      // Update Bytes 17-20 (YH, YL, M, D) for recurrence exception according to
      // https://msdn.microsoft.com/en-us/library/ee157690(v=exchg.80).aspx

      byte[] yearsBytes = BitConverter.GetBytes (originalStart.Year);
      globalId[16] = yearsBytes[1];
      globalId[17] = yearsBytes[0];
      globalId[18] = (byte) originalStart.Month;
      globalId[19] = (byte) originalStart.Day;

      return globalId;
    }

    public static string RemoveEmailFromName (Recipient recipient)
    {
      return Regex.Replace (recipient.Name, " \\([^()]*\\)$", string.Empty);
    }
  }
}
