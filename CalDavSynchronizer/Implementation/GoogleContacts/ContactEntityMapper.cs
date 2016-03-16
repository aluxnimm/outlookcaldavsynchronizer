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
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xaml;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using Google.Contacts;
using Google.GData.Contacts;
using Google.GData.Extensions;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactEntityMapper : IEntityMapper<ContactItemWrapper, Contact>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";
    private const string PR_EMAIL2ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8094001F";
    private const string PR_EMAIL3ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/80a4001F";
    private const string PR_USER_X509_CERTIFICATE = "http://schemas.microsoft.com/mapi/proptag/0x3A701102";
    private const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";

    private readonly ContactMappingConfiguration _configuration;

    public GoogleContactEntityMapper (ContactMappingConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Contact Map1To2 (ContactItemWrapper source, Contact target, IEntityMappingLogger logger)
    {
      target.Name = new Name()
      {
        GivenName = source.Inner.FirstName,
        FamilyName = source.Inner.LastName,
        FullName = source.Inner.FullName,
        AdditionalName = source.Inner.MiddleName,
        NamePrefix = source.Inner.Title,
        NameSuffix = source.Inner.Suffix,
      };

      MapEmailAddresses1To2(source.Inner, target, logger);

      if (!string.IsNullOrEmpty(source.Inner.FileAs))
      {
        target.Title = source.Inner.FileAs;
      }
      else if (!string.IsNullOrEmpty(source.Inner.CompanyAndFullName))
      {
        target.Title = source.Inner.CompanyAndFullName;
      }
      else if (target.Emails.Count >= 1)
      {
        target.Title = target.Emails[0].Address;
      }

      MapPostalAddresses1To2 (source.Inner, target);

      MapPhoneNumbers1To2 (source.Inner, target);

      target.ContactEntry.Nickname = source.Inner.NickName;
      target.ContactEntry.Initials = source.Inner.Initials;

      target.Organizations.Clear();
      if (!string.IsNullOrEmpty(source.Inner.CompanyName) || !string.IsNullOrEmpty(source.Inner.Department) ||
          !string.IsNullOrEmpty(source.Inner.JobTitle))
      {
        target.Organizations.Add (new Organization()
        {
          Name = source.Inner.CompanyName,
          Department = source.Inner.Department,
          Title = source.Inner.JobTitle,
          Rel = ContactsRelationships.IsWork,
          Primary = true,
        });
      }

      if (!string.IsNullOrEmpty (source.Inner.PersonalHomePage))
      {
        target.ContactEntry.Websites.Add (new Website()
        {
          Href = source.Inner.PersonalHomePage,
          Rel = ContactsRelationships.IsHome,
          Primary = true,
        });
      }
      if (!string.IsNullOrEmpty (source.Inner.BusinessHomePage))
      {
        target.ContactEntry.Websites.Add(new Website()
        {
          Href = source.Inner.BusinessHomePage,
          Rel = ContactsRelationships.IsWork,
          Primary = target.ContactEntry.Websites.Count == 0,
        });
      }

      if (_configuration.MapBirthday && !source.Inner.Birthday.Equals (new DateTime (4501, 1, 1, 0, 0, 0)))
      {
        target.ContactEntry.Birthday = source.Inner.Birthday.ToString ("yyyy-MM-dd");
      }
      else
      {
        target.ContactEntry.Birthday = null;
      }

      target.Content = !string.IsNullOrEmpty(source.Inner.Body) ? 
                        System.Security.SecurityElement.Escape (source.Inner.Body) : null;

      return target;
    }

    private static void MapEmailAddresses1To2 (ContactItem source, Contact target, IEntityMappingLogger logger)
    {
      target.Emails.Clear();

      if (!string.IsNullOrEmpty (source.Email1Address))
      {
        string email1Address = string.Empty;

        if (source.Email1AddressType == "EX")
        {
          try
          {
            email1Address = source.GetPropertySafe (PR_EMAIL1ADDRESS);
          }
          catch (COMException ex)
          {
            s_logger.Warn ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
          }
        }
        else
        {
          email1Address = source.Email1Address;
        }
        if (!string.IsNullOrEmpty (email1Address))
        {
          target.Emails.Add (new EMail()
          {
            Primary = true,
            Address = email1Address,
            Rel = ContactsRelationships.IsWork,
          });
        }
      }

      if (!string.IsNullOrEmpty (source.Email2Address))
      {
        string email2Address = string.Empty;

        if (source.Email2AddressType == "EX")
        {
          try
          {
            email2Address = source.GetPropertySafe (PR_EMAIL2ADDRESS);
          }
          catch (COMException ex)
          {
            s_logger.Warn ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
          }
        }
        else
        {
          email2Address = source.Email2Address;
        }
        if (!string.IsNullOrEmpty (email2Address))
        {
          target.Emails.Add(new EMail()
          {
            Primary = (target.Emails.Count == 0),
            Address = email2Address,
            Rel = ContactsRelationships.IsHome,
          });
        }
      }

      if (!string.IsNullOrEmpty (source.Email3Address))
      {
        string email3Address = string.Empty;

        if (source.Email3AddressType == "EX")
        {
          try
          {
            email3Address = source.GetPropertySafe (PR_EMAIL3ADDRESS);
          }
          catch (COMException ex)
          {
            s_logger.Warn ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
          }
        }
        else
        {
          email3Address = source.Email3Address;
        }
        if (!string.IsNullOrEmpty (email3Address))
        {
          target.Emails.Add (new EMail()
          {
            Primary = (target.Emails.Count == 0),
            Address = email3Address,
            Rel = ContactsRelationships.IsOther,
          });
        }
      }
    }

    private static void MapPostalAddresses1To2 (ContactItem source, Contact target)
    {
      target.PostalAddresses.Clear();

      if (!string.IsNullOrEmpty (source.HomeAddress))
      {
        target.PostalAddresses.Add(new StructuredPostalAddress()
        {
          City = source.HomeAddressCity,
          Country = source.HomeAddressCity,
          Postcode = source.HomeAddressPostalCode,
          Pobox = source.HomeAddressPostOfficeBox,
          Region = source.HomeAddressState,
          Street = source.HomeAddressStreet,
          Rel = ContactsRelationships.IsHome,
          Primary = (source.SelectedMailingAddress == OlMailingAddress.olHome),
        });
      }
      if (!string.IsNullOrEmpty (source.BusinessAddress))
      {
        target.PostalAddresses.Add (new StructuredPostalAddress()
        {
          City = source.BusinessAddressCity,
          Country = source.BusinessAddressCity,
          Postcode = source.HomeAddressPostalCode,
          Pobox = source.BusinessAddressPostOfficeBox,
          Region = source.BusinessAddressState,
          Street = source.BusinessAddressStreet,
          Rel = ContactsRelationships.IsWork,
          Primary = (source.SelectedMailingAddress == OlMailingAddress.olBusiness),
        });
      }
      if (!string.IsNullOrEmpty (source.OtherAddress))
      {
        target.PostalAddresses.Add (new StructuredPostalAddress()
        {
          City = source.OtherAddressCity,
          Country = source.OtherAddressCity,
          Postcode = source.OtherAddressPostalCode,
          Pobox = source.OtherAddressPostOfficeBox,
          Region = source.OtherAddressState,
          Street = source.OtherAddressStreet,
          Rel = ContactsRelationships.IsOther,
          Primary = (source.SelectedMailingAddress == OlMailingAddress.olOther),
        });
      }
    }

    private static void MapPhoneNumbers1To2 (ContactItem source, Contact target)
    {
      if (!string.IsNullOrEmpty (source.PrimaryTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.PrimaryTelephoneNumber,
          Rel = ContactsRelationships.IsMain,
          Primary = true,
        });
      }
      if (!string.IsNullOrEmpty (source.MobileTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.MobileTelephoneNumber,
          Rel = ContactsRelationships.IsMobile,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.HomeTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.HomeTelephoneNumber,
          Rel = ContactsRelationships.IsHome,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.Home2TelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.Home2TelephoneNumber,
          Rel = ContactsRelationships.IsHome,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.HomeFaxNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.HomeFaxNumber,
          Rel = ContactsRelationships.IsHomeFax,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.BusinessTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.BusinessTelephoneNumber,
          Rel = ContactsRelationships.IsWork,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.Business2TelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.Business2TelephoneNumber,
          Rel = ContactsRelationships.IsWork,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.BusinessFaxNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.BusinessFaxNumber,
          Rel = ContactsRelationships.IsWorkFax,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.PagerNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.PagerNumber,
          Rel = ContactsRelationships.IsPager,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.CarTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.CarTelephoneNumber,
          Rel = ContactsRelationships.IsCar,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.ISDNNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.ISDNNumber,
          Rel = ContactsRelationships.IsISDN,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.OtherTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.OtherTelephoneNumber,
          Rel = ContactsRelationships.IsOther,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.OtherFaxNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.OtherFaxNumber,
          Rel = ContactsRelationships.IsOtherFax,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty (source.AssistantTelephoneNumber))
      {
        target.Phonenumbers.Add (new PhoneNumber()
        {
          Value = source.AssistantTelephoneNumber,
          Rel = ContactsRelationships.IsAssistant,
          Primary = target.Phonenumbers.Count == 0,
        });
      }
    }

    public ContactItemWrapper Map2To1 (Contact source, ContactItemWrapper target, IEntityMappingLogger logger)
    {
      target.Inner.FirstName = source.Name.GivenName;
      target.Inner.LastName = source.Name.FamilyName;

      return target;
    }
  }
}