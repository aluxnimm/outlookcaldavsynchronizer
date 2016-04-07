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
