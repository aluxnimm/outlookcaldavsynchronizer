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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xaml;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Implementation.Common;
using CalDavSynchronizer.Implementation.ComWrappers;
using GenSync.EntityMapping;
using GenSync.Logging;
using Google.Contacts;
using Google.GData.Contacts;
using Google.GData.Extensions;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;
using Exception = System.Exception;

namespace CalDavSynchronizer.Implementation.GoogleContacts
{
  public class GoogleContactEntityMapper : IEntityMapper<IContactItemWrapper, GoogleContactWrapper, IGoogleContactContext>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";
    private const string PR_EMAIL2ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8094001F";
    private const string PR_EMAIL3ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/80a4001F";
    private const string PR_USER_X509_CERTIFICATE = "http://schemas.microsoft.com/mapi/proptag/0x3A701102";
    private const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";

    private const string REL_SPOUSE = "spouse";
    private const string REL_CHILD = "child";
    private const string REL_MANAGER = "manager";
    private const string REL_ASSISTANT = "assistant";
    private const string REL_ANNIVERSARY = "anniversary";
    private const string REL_HOMEPAGE = "home-page";
    private const string REL_WORK = "work";
    private const string REL_HOME = "home";
    private const string REL_BLOG = "blog";
    private const string REL_FTP = "ftp";

    private readonly ContactMappingConfiguration _configuration;

    public GoogleContactEntityMapper (ContactMappingConfiguration configuration)
    {
      _configuration = configuration;
    }

    public Task<GoogleContactWrapper> Map1To2 (IContactItemWrapper source, GoogleContactWrapper targetWrapper, IEntitySynchronizationLogger logger, IGoogleContactContext context)
    {
      var target = targetWrapper.Contact;

      MapFileAs1To2(source, target);

      MapName1To2(source, target);

      MapEmailAddresses1To2(source.Inner, target, logger);         

      MapPostalAddresses1To2(source.Inner, target);

      MapPhoneNumbers1To2 (source.Inner, target);

      target.ContactEntry.Nickname = source.Inner.NickName;
      target.ContactEntry.Initials = source.Inner.Initials;

      #region company
      target.Organizations.Clear();
      if (!string.IsNullOrEmpty(source.Inner.Companies))
      {
        //Companies are expected to be in form of "[Company]; [Company]".
        string[] companiesRaw = source.Inner.Companies.Split(';');
        foreach (string companyRaw in companiesRaw)
        {
          Organization company = new Organization();
          company.Name = (target.Organizations.Count == 0) ? source.Inner.CompanyName : companyRaw;
          company.Title = (target.Organizations.Count == 0) ? source.Inner.JobTitle : null;
          company.Department = (target.Organizations.Count == 0) ? source.Inner.Department : null;
          company.Primary = target.Organizations.Count == 0;
          company.Rel = ContactsRelationships.IsWork;
          target.Organizations.Add(company);
        }
      }

      if (target.Organizations.Count == 0 && (!string.IsNullOrEmpty(source.Inner.CompanyName) || !string.IsNullOrEmpty(source.Inner.Department) ||
          !string.IsNullOrEmpty(source.Inner.JobTitle)))
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
      #endregion company

      target.ContactEntry.Occupation = source.Inner.Profession;

      target.Location = source.Inner.OfficeLocation;

      target.ContactEntry.Websites.Clear();
      if (!string.IsNullOrEmpty (source.Inner.WebPage))
      {
        target.ContactEntry.Websites.Add (new Website()
        {
          Href = source.Inner.WebPage,
          Rel = REL_HOMEPAGE,
          Primary = true,
        });
      }
      if (!string.IsNullOrEmpty (source.Inner.BusinessHomePage))
      {
        target.ContactEntry.Websites.Add(new Website()
        {
          Href = source.Inner.BusinessHomePage,
          Rel = REL_WORK,
          Primary = target.ContactEntry.Websites.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty(source.Inner.PersonalHomePage))
      {
        target.ContactEntry.Websites.Add(new Website()
        {
          Href = source.Inner.PersonalHomePage,
          Rel = REL_HOME,
          Primary = target.ContactEntry.Websites.Count == 0,
        });
      }
      if (!string.IsNullOrEmpty(source.Inner.FTPSite))
      {
        target.ContactEntry.Websites.Add(new Website()
        {
          Href = source.Inner.FTPSite,
          Rel = REL_FTP,
          Primary = target.ContactEntry.Websites.Count == 0,
        });
      }

      MapBirthday1To2(source, target);
      
      MapAnniversary1To2(source, target, logger);
     
      MapRelations1To2(source, target);

      MapImAddresses1To2(source, target);

      target.Content = !string.IsNullOrEmpty(source.Inner.Body) ? 
                        System.Security.SecurityElement.Escape (source.Inner.Body) : null;

      target.ContactEntry.Sensitivity = MapPrivacy1To2 (source.Inner.Sensitivity);

      target.Languages.Clear();
      if (!string.IsNullOrEmpty (source.Inner.Language))
      {
        foreach (var lang in source.Inner.Language.Split (';'))
        {
          target.Languages.Add (new Language () { Label = lang});
        }
      }

      target.ContactEntry.Hobbies.Clear();
      if (!string.IsNullOrEmpty (source.Inner.Hobby))
      {
        foreach (var hobby in source.Inner.Hobby.Split (';'))
        {
          target.ContactEntry.Hobbies.Add (new Hobby (hobby));
        }
      }

      targetWrapper.Groups.Clear ();
      targetWrapper.Groups.AddRange (CommonEntityMapper.SplitCategoryString (source.Inner.Categories));

      if (_configuration.MapContactPhoto)
        MapPhoto1To2 (source.Inner, targetWrapper, logger);

      return Task.FromResult(targetWrapper);
    }

    private static void MapImAddresses1To2(IContactItemWrapper source, Contact target)
    {
      target.IMs.Clear();
      if (!string.IsNullOrEmpty(source.Inner.IMAddress))
      {
        //IMAddress are expected to be in form of ([Protocol]: [Address]; [Protocol]: [Address])
        var imAddresses = source.Inner.IMAddress.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var imAddress in imAddresses)
        {
          var imAddressParts = imAddress.Trim().Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
          var im = new IMAddress();
          if (imAddressParts.Length == 1)
          {
            im.Address = imAddressParts[0].Trim();
          }
          else if (imAddressParts.Length > 1)
          {
            im.Protocol = imAddressParts[0].Trim();
            im.Address = imAddressParts[1].Trim();
          }

          //Only add the im Address if not empty (to avoid Google exception "address" empty)
          if (!string.IsNullOrEmpty(im.Address))
          {
            im.Primary = target.IMs.Count == 0;
            im.Rel = ContactsRelationships.IsHome;
            target.IMs.Add(im);
          }
        }
      }
    }

    private static void MapName1To2(IContactItemWrapper source, Contact target)
    {
      Name name = new Name()
      {
        GivenName = source.Inner.FirstName,
        FamilyName = source.Inner.LastName,
        AdditionalName = source.Inner.MiddleName,
        NamePrefix = source.Inner.Title,
        NameSuffix = source.Inner.Suffix,
      };

      //Use the Google's full name to save a unique identifier. When saving the FullName, it always overwrites the Google Title
      if (!string.IsNullOrEmpty(source.Inner.FullName)) //Only if source.FullName has a value, i.e. not only a company or email contact
      {
        name.FullName = source.Inner.FileAs;
      }

      target.Name = name;
    }

    private static void MapFileAs1To2(IContactItemWrapper source, Contact target)
    {
      if (!string.IsNullOrEmpty(source.Inner.FileAs))
        target.Title = source.Inner.FileAs;
      else if (!string.IsNullOrEmpty(source.Inner.CompanyAndFullName))
        target.Title = source.Inner.CompanyAndFullName;
      else if (!string.IsNullOrEmpty(source.Inner.FullName))
        target.Title = source.Inner.FullName;
      else if (!string.IsNullOrEmpty(source.Inner.CompanyName))
        target.Title = source.Inner.CompanyName;
      else if (!string.IsNullOrEmpty(source.Inner.Email1Address))
        target.Title = source.Inner.Email1Address;
    }

    private static void MapRelations1To2(IContactItemWrapper source, Contact target)
    {

      foreach (var googleRel in target.ContactEntry.Relations.ToList())
      {
        if (googleRel.Rel == REL_SPOUSE || googleRel.Rel == REL_CHILD || googleRel.Rel == REL_MANAGER || googleRel.Rel == REL_ASSISTANT)
          target.ContactEntry.Relations.Remove(googleRel);
      }

      if (!string.IsNullOrEmpty(source.Inner.Spouse))
      {
        var rel = new Relation()
        {
          Rel = REL_SPOUSE,
          Value = source.Inner.Spouse
        };
        target.ContactEntry.Relations.Add(rel);
      }

      if (!string.IsNullOrEmpty(source.Inner.Children))
      {
        var rel = new Relation()
        {
          Rel = REL_CHILD,
          Value = source.Inner.Children
        };
        target.ContactEntry.Relations.Add(rel);
      }

      if (!string.IsNullOrEmpty(source.Inner.ManagerName))
      {
        var rel = new Relation()
        {
          Rel = REL_MANAGER,
          Value = source.Inner.ManagerName
        };
        target.ContactEntry.Relations.Add(rel);
      }

      if (!string.IsNullOrEmpty(source.Inner.AssistantName))
      {
        var rel = new Relation()
        {
          Rel = REL_ASSISTANT,
          Value = source.Inner.AssistantName
        };
        target.ContactEntry.Relations.Add(rel);
      }
    }

    private void MapAnniversary1To2(IContactItemWrapper source, Contact target, IEntitySynchronizationLogger logger)
    {
      if (!_configuration.MapAnniversary)
        return;

      var googleAnniversary = target.ContactEntry.Events.FirstOrDefault(e => e.Relation != null && e.Relation.Equals(REL_ANNIVERSARY));

      if (googleAnniversary != null)
        target.ContactEntry.Events.Remove(googleAnniversary);

      try
      {
        if (!source.Inner.Anniversary.Equals(OutlookUtility.OUTLOOK_DATE_NONE))
        {
          target.ContactEntry.Events.Add(
            new Event
            {
              Relation = REL_ANNIVERSARY,
              When = new When
              {
                AllDay = true,
                StartTime = source.Inner.Anniversary.Date
              }
            });
        }
      }
      catch (Exception ex)
      {
        s_logger.Warn("Anniversary couldn't be updated from Outlook to Google for '" + source.Inner.FileAs + "': " + ex.Message, ex);
        logger.LogWarning("Anniversary couldn't be updated from Outlook to Google for '" + source.Inner.FileAs + "': " + ex.Message, ex);
      }
    }

    private void MapBirthday1To2(IContactItemWrapper source, Contact target)
    {
      if (_configuration.MapBirthday && !source.Inner.Birthday.Equals(OutlookUtility.OUTLOOK_DATE_NONE))
        target.ContactEntry.Birthday = source.Inner.Birthday.ToString("yyyy-MM-dd");
      else
        target.ContactEntry.Birthday = null;
    }

    private void MapPhoto1To2 (ContactItem source, GoogleContactWrapper target, IEntitySynchronizationLogger logger)
    {
      target.PhotoOrNull = null;

      if (source.HasPicture)
      {
        foreach (var att in source.Attachments.ToSafeEnumerable<Attachment>())
        {
          if (att.DisplayName == "ContactPicture.jpg")
          {
            using (var oPa = GenericComObjectWrapper.Create (att.PropertyAccessor))
            {
              try
              {
                target.PhotoOrNull = (byte[]) oPa.Inner.GetProperty (PR_ATTACH_DATA_BIN);
              }
              catch (COMException ex)
              {
                s_logger.Warn ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
                logger.LogWarning ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
              catch (System.UnauthorizedAccessException ex)
              {
                s_logger.Warn ("Could not access PR_ATTACH_DATA_BIN to export picture for contact.", ex);
                logger.LogWarning ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
            }
          }
        }
      }
    }

    private void MapEmailAddresses1To2 (ContactItem source, Contact target, IEntitySynchronizationLogger logger)
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
            logger.LogWarning ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
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
            Rel = _configuration.MapOutlookEmail1ToWork ? ContactsRelationships.IsWork : ContactsRelationships.IsHome,
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
            logger.LogWarning ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
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
            Rel = _configuration.MapOutlookEmail1ToWork ? ContactsRelationships.IsHome : ContactsRelationships.IsWork,
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
            logger.LogWarning ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
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
          Country = source.HomeAddressCountry,
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
          Country = source.BusinessAddressCountry,
          Postcode = source.BusinessAddressPostalCode,
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
          Country = source.OtherAddressCountry,
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
      target.Phonenumbers.Clear();

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

    public static string MapPrivacy1To2 (OlSensitivity value)
    {
      switch (value)
      {
        case OlSensitivity.olNormal:
          return "normal";
        case OlSensitivity.olPersonal:
          return "personal"; 
        case OlSensitivity.olPrivate:
          return "private";
        case OlSensitivity.olConfidential:
          return "confidential";
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    public Task<IContactItemWrapper> Map2To1 (GoogleContactWrapper sourceWrapper, IContactItemWrapper target, IEntitySynchronizationLogger logger, IGoogleContactContext context)
    {
      var source = sourceWrapper.Contact;

      if (source.Name != null)
      {
        target.Inner.FirstName = source.Name.GivenName;
        target.Inner.LastName = source.Name.FamilyName;
        target.Inner.Title = source.Name.NamePrefix;
        target.Inner.Suffix = source.Name.NameSuffix;
        target.Inner.MiddleName = source.Name.AdditionalName;
        if (string.IsNullOrEmpty(target.Inner.FullName)) //The Outlook fullName is automatically set, so don't assign it from Google, unless the structured properties were empty
          target.Inner.FullName = source.Name.FullName;
      }

      if (string.IsNullOrEmpty (target.Inner.FileAs) || !_configuration.KeepOutlookFileAs)
      {
        if (!string.IsNullOrEmpty (source.Title))
        {
          target.Inner.FileAs = source.Title.Replace("\r\n", "\n").Replace("\n", "\r\n"); //Replace twice to not replace a \r\n by \r\r\n. This is necessary because \r\n are saved as \n only to google and \r\n is saved on Outlook side to separate the single parts of the FullName
        }
        else if (!string.IsNullOrEmpty (source.Name?.FullName))
        {
          target.Inner.FileAs = source.Name.FullName.Replace("\r\n", "\n").Replace("\n", "\r\n"); //Replace twice to not replace a \r\n by \r\r\n. This is necessary because \r\n are saved as \n only to google and \r\n is saved on Outlook side to separate the single parts of the FullName
        }
        else if (source.Organizations.Count > 0 && !string.IsNullOrEmpty (source.Organizations[0].Name))
        {
          target.Inner.FileAs = source.Organizations[0].Name;
        }
        else if (source.Emails.Count > 0 && !string.IsNullOrEmpty (source.Emails[0].Address))
        {
          target.Inner.FileAs = source.Emails[0].Address;
        }
      }

      MapPostalAddresses2To1 (source, target.Inner);

      MapPhoneNumbers2To1 (source, target.Inner);

      target.Inner.Email1Address = string.Empty;
      target.Inner.Email1DisplayName = string.Empty;
      target.Inner.Email2Address = string.Empty;
      target.Inner.Email2DisplayName = string.Empty;
      target.Inner.Email3Address = string.Empty;
      target.Inner.Email3DisplayName = string.Empty;
      if (source.Emails.Count >= 1)
      {
        Func<EMail, bool> firstPredicate = e => _configuration.MapOutlookEmail1ToWork
              ? e.Rel == ContactsRelationships.IsWork
              : e.Rel == ContactsRelationships.IsHome;

        var first = source.Emails.FirstOrDefault (firstPredicate) ?? source.Emails.First();
        target.Inner.Email1Address = first.Address;
        if (!string.IsNullOrEmpty (first.Label)) target.Inner.Email1DisplayName = first.Label;

        var second = source.Emails.FirstOrDefault (e => _configuration.MapOutlookEmail1ToWork ? e.Rel == ContactsRelationships.IsHome : e.Rel == ContactsRelationships.IsWork && e != first) ??
                           source.Emails.FirstOrDefault (e => e != first);

        if (second != null)
        {
          target.Inner.Email2Address = second.Address;
          if (!string.IsNullOrEmpty (second.Label)) target.Inner.Email2DisplayName = second.Label;

          var other = source.Emails.FirstOrDefault (e => e != first && e != second);
          if (other != null)
          {
            target.Inner.Email3Address = other.Address;
            if (!string.IsNullOrEmpty (other.Label)) target.Inner.Email3DisplayName = other.Label;
          }
        }
      }

      target.Inner.NickName = source.ContactEntry.Nickname;
      target.Inner.Initials = source.ContactEntry.Initials;

      #region companies
      target.Inner.Companies = string.Empty;
      target.Inner.CompanyName = string.Empty;
      target.Inner.JobTitle = string.Empty;
      target.Inner.Department = string.Empty;
      foreach (Organization company in source.Organizations)
      {
        if (string.IsNullOrEmpty(company.Name) && string.IsNullOrEmpty(company.Title) && string.IsNullOrEmpty(company.Department))
          continue;

        if (company.Primary || company.Equals(source.Organizations[0]))
        {//Per default copy the first company, but if there is a primary existing, use the primary
          target.Inner.CompanyName = company.Name;
          target.Inner.JobTitle = company.Title;
          target.Inner.Department = company.Department;
        }
        if (!string.IsNullOrEmpty(target.Inner.Companies))
          target.Inner.Companies += "; ";
        target.Inner.Companies += company.Name;
      }
      #endregion companies

      target.Inner.Profession = source.ContactEntry.Occupation;

      target.Inner.OfficeLocation = source.Location;

      target.Inner.BusinessHomePage = string.Empty;
      target.Inner.WebPage = string.Empty;
      
      if (source.ContactEntry.Websites.Count == 1)
        target.Inner.WebPage = source.ContactEntry.Websites[0].Href;
      else
      {
        foreach (var site in source.ContactEntry.Websites)
        {
          if (site.Primary || site.Rel == REL_HOMEPAGE)
            target.Inner.WebPage = site.Href;
          else if (site.Rel == REL_WORK)
            target.Inner.BusinessHomePage = site.Href;
          else if (site.Rel == REL_HOME || site.Rel == REL_BLOG)
            target.Inner.PersonalHomePage = site.Href;
          else if (site.Rel == REL_FTP)
            target.Inner.FTPSite = site.Href;
        }
      }

      MapBirthday2To1(target, source, logger);

      MapAnniversary2To1(target, source, logger);
      
      MapRelations2To1(target, source);
      
      MapImAddresses2To1(target, source);

      target.Inner.Body = source.Content;

      target.Inner.Sensitivity = MapPrivacy2To1 (source.ContactEntry.Sensitivity);

      target.Inner.Language = string.Empty;
      if (source.Languages.Count >0)
        target.Inner.Language = string.Join (";", source.Languages.Select (l => l.Label));

      target.Inner.Hobby = string.Empty;
      if (source.ContactEntry.Hobbies.Count >0)
        target.Inner.Hobby = string.Join (";", source.ContactEntry.Hobbies.Select (h => h.Value));

      target.Inner.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, sourceWrapper.Groups);

      if (_configuration.MapContactPhoto)
        MapPhoto2To1 (sourceWrapper, target.Inner, logger);

      return Task.FromResult(target);
    }

    private static void MapImAddresses2To1(IContactItemWrapper target, Contact source)
    {
      target.Inner.IMAddress = string.Empty;
      foreach (var im in source.IMs)
      {
        if (!string.IsNullOrEmpty(target.Inner.IMAddress))
          target.Inner.IMAddress += "; ";
        if (!string.IsNullOrEmpty(im.Protocol) && !im.Protocol.Equals("None", StringComparison.InvariantCultureIgnoreCase))
          target.Inner.IMAddress += im.Protocol + ": " + im.Address;
        else
          target.Inner.IMAddress += im.Address;
      }
    }

    private static void MapRelations2To1(IContactItemWrapper target, Contact source)
    {
      target.Inner.Children = string.Empty;
      target.Inner.Spouse = string.Empty;
      target.Inner.ManagerName = string.Empty;
      target.Inner.AssistantName = string.Empty;

      foreach (var googleRel in source.ContactEntry.Relations)
      {
        switch (googleRel.Rel)
        {
          case REL_CHILD:
            target.Inner.Children = googleRel.Value;
            break;
          case REL_SPOUSE:
            target.Inner.Spouse = googleRel.Value;
            break;
          case REL_MANAGER:
            target.Inner.ManagerName = googleRel.Value;
            break;
          case REL_ASSISTANT:
            target.Inner.AssistantName = googleRel.Value;
            break;
        }
      }
    }

    private void MapAnniversary2To1(IContactItemWrapper target, Contact source, IEntitySynchronizationLogger logger)
    {
      if (!_configuration.MapAnniversary)
        return;

      try
      {
        var googleAnniversary = source.ContactEntry.Events.FirstOrDefault(e => e.Relation != null && e.Relation.Equals(REL_ANNIVERSARY));

        if (googleAnniversary != null)
        {
          if (googleAnniversary.When.StartTime.Date != target.Inner.Anniversary.Date)
            target.Inner.Anniversary = googleAnniversary.When.StartTime.Date;
        }
        else
        {
          target.Inner.Anniversary = OutlookUtility.OUTLOOK_DATE_NONE;
        }
      }
      catch (System.Exception ex)
      {
        s_logger.Warn("Anniversary couldn't be updated from Google to Outlook for '" + target.Inner.FileAs + "': " + ex.Message, ex);
        logger.LogWarning("Anniversary couldn't be updated from Google to Outlook for '" + target.Inner.FileAs + "': " + ex.Message, ex);
      }
    }

    private void MapBirthday2To1(IContactItemWrapper target, Contact source, IEntitySynchronizationLogger logger)
    {
      if (!_configuration.MapBirthday)
        return;

      if (DateTime.TryParse(source.ContactEntry.Birthday, out DateTime sourceBirthday))
      {
        if (!sourceBirthday.Date.Equals(target.Inner.Birthday))
        {
          try
          {
            target.Inner.Birthday = sourceBirthday;
          }
          catch (COMException ex)
          {
            s_logger.Warn("Could not update contact birthday.", ex);
            logger.LogWarning("Could not update contact birthday.", ex);
          }
          catch (OverflowException ex)
          {
            s_logger.Warn("Contact birthday has invalid value.", ex);
            logger.LogWarning("Contact birthday has invalid value.", ex);
          }
        }
      }
      else
      {
        target.Inner.Birthday = OutlookUtility.OUTLOOK_DATE_NONE;
      }
    }

    private void MapPhoto2To1 (GoogleContactWrapper source, ContactItem target, IEntitySynchronizationLogger logger)
    {
      if (source.PhotoOrNull != null)
      {
        if (target.HasPicture && _configuration.KeepOutlookPhoto) return;

        try
        {
          string picturePath = Path.GetTempPath() + @"\Contact_" + target.EntryID + ".jpg";
          File.WriteAllBytes (picturePath, source.PhotoOrNull);
          try
          {
            target.AddPicture (picturePath);
          }
          catch (COMException x)
          {
            s_logger.Warn ("Could not add picture for contact.", x);
            logger.LogWarning ("Could not add picture for contact.", x);
          }
          File.Delete (picturePath);
        }
        catch (System.Exception ex)
        {
          s_logger.Warn ("Could not add picture for contact.", ex);
          logger.LogWarning ("Could not add picture for contact.", ex);
        }
      }
      else
      {
        if (target.HasPicture)
        {
          try
          {
            target.RemovePicture();
          }
          catch (COMException x)
          {
            s_logger.Warn ("Could not remove picture for contact.", x);
            logger.LogWarning ("Could not remove picture for contact.", x);
          }
        }
      }
    }

    private void MapPhoneNumbers2To1 (Contact source, ContactItem target)
    {
      target.HomeTelephoneNumber = string.Empty;
      target.BusinessTelephoneNumber = string.Empty;
      target.BusinessFaxNumber = string.Empty;
      target.PrimaryTelephoneNumber = string.Empty;
      target.MobileTelephoneNumber = string.Empty;

      foreach (var phoneNumber in source.Phonenumbers)
      {
        string sourceNumber = _configuration.FixPhoneNumberFormat ?
                              FixPhoneNumberFormat (phoneNumber.Value) : phoneNumber.Value;

        switch (phoneNumber.Rel)
        {
          case ContactsRelationships.IsMain:
            target.PrimaryTelephoneNumber = sourceNumber;
            break;
          case ContactsRelationships.IsMobile:
            target.MobileTelephoneNumber = sourceNumber;
            break;
          case ContactsRelationships.IsHome:
            if (string.IsNullOrEmpty (target.HomeTelephoneNumber))
            {
              target.HomeTelephoneNumber = sourceNumber;
            }
            else
            {
              target.Home2TelephoneNumber = sourceNumber;
            }
            break;
          case ContactsRelationships.IsWork:
            if (string.IsNullOrEmpty (target.BusinessTelephoneNumber))
            {
              target.BusinessTelephoneNumber = sourceNumber;
            }
            else
            {
              target.Business2TelephoneNumber = sourceNumber;
            }
            break;
          case ContactsRelationships.IsHomeFax:
            target.HomeFaxNumber = sourceNumber;
            break;
          case ContactsRelationships.IsWorkFax:
            target.BusinessFaxNumber = sourceNumber;
            break;
          case ContactsRelationships.IsOtherFax:
            target.OtherFaxNumber = sourceNumber;
            break;
          case ContactsRelationships.IsPager:
            target.PagerNumber = sourceNumber;
            break;
          case ContactsRelationships.IsCar:
            target.CarTelephoneNumber = sourceNumber;
            break;
          case ContactsRelationships.IsISDN:
            target.ISDNNumber = sourceNumber;
            break;
          case ContactsRelationships.IsAssistant:
            target.AssistantTelephoneNumber = sourceNumber;
            break;
          default:
            if (phoneNumber.Primary && string.IsNullOrEmpty (target.PrimaryTelephoneNumber))
            {
              target.PrimaryTelephoneNumber = sourceNumber;
            }
            else if (phoneNumber.Primary && string.IsNullOrEmpty (target.HomeTelephoneNumber))
            {
              target.HomeTelephoneNumber = sourceNumber;
            }
            else
            {
              target.OtherTelephoneNumber = sourceNumber;
            }
            break;
        }
      }
    }

    private static string FixPhoneNumberFormat(string number)
    {
      // Reformat telephone numbers so that Outlook can split country/area code and extension
      var match = Regex.Match(number, @"(\+\d+) (\d+) (\d+)( \d+)?");
      if (match.Success)
      {
        string ext = string.IsNullOrEmpty(match.Groups[4].Value) ? string.Empty : " - " + match.Groups[4].Value;

        return match.Groups[1].Value + " ( " + match.Groups[2].Value + " ) " + match.Groups[3].Value + ext;
      }
      else
      {
        return number;
      }
    }

    private static void MapPostalAddresses2To1 (Contact source, ContactItem target)
    {
      target.HomeAddress = string.Empty;
      target.HomeAddressStreet = string.Empty;
      target.HomeAddressCity = string.Empty;
      target.HomeAddressPostalCode = string.Empty;
      target.HomeAddressCountry = string.Empty;
      target.HomeAddressState = string.Empty;
      target.HomeAddressPostOfficeBox = string.Empty;

      target.BusinessAddress = string.Empty;
      target.BusinessAddressStreet = string.Empty;
      target.BusinessAddressCity = string.Empty;
      target.BusinessAddressPostalCode = string.Empty;
      target.BusinessAddressCountry = string.Empty;
      target.BusinessAddressState = string.Empty;
      target.BusinessAddressPostOfficeBox = string.Empty;

      target.OtherAddress = string.Empty;
      target.OtherAddressStreet = string.Empty;
      target.OtherAddressCity = string.Empty;
      target.OtherAddressPostalCode = string.Empty;
      target.OtherAddressCountry = string.Empty;
      target.OtherAddressState = string.Empty;
      target.OtherAddressPostOfficeBox = string.Empty;

      target.SelectedMailingAddress = OlMailingAddress.olNone;

      foreach (var sourceAddress in source.PostalAddresses)
      {
        if (sourceAddress.Rel == ContactsRelationships.IsHome)
        {
          target.HomeAddressCity = sourceAddress.City;
          target.HomeAddressCountry = sourceAddress.Country;
          target.HomeAddressPostalCode = sourceAddress.Postcode;
          target.HomeAddressState = sourceAddress.Region;
          target.HomeAddressStreet = sourceAddress.Street;
          target.HomeAddressPostOfficeBox = sourceAddress.Pobox;
          if (string.IsNullOrEmpty(target.HomeAddress))
            target.HomeAddress = sourceAddress.FormattedAddress;
          if (sourceAddress.Primary)
          {
            target.SelectedMailingAddress = OlMailingAddress.olHome;
          }
        }
        else if (sourceAddress.Rel == ContactsRelationships.IsWork)
        {
          target.BusinessAddressCity = sourceAddress.City;
          target.BusinessAddressCountry = sourceAddress.Country;
          target.BusinessAddressPostalCode = sourceAddress.Postcode;
          target.BusinessAddressState = sourceAddress.Region;
          target.BusinessAddressStreet = sourceAddress.Street;
          target.BusinessAddressPostOfficeBox = sourceAddress.Pobox;
          if (string.IsNullOrEmpty(target.BusinessAddress))
            target.BusinessAddress = sourceAddress.FormattedAddress;
          if (sourceAddress.Primary)
          {
            target.SelectedMailingAddress = OlMailingAddress.olBusiness;
          }
        }
        else
        {
          target.OtherAddressCity = sourceAddress.City;
          target.OtherAddressCountry = sourceAddress.Country;
          target.OtherAddressPostalCode = sourceAddress.Postcode;
          target.OtherAddressState = sourceAddress.Region;
          target.OtherAddressStreet = sourceAddress.Street;
          target.OtherAddressPostOfficeBox = sourceAddress.Pobox;
          if (string.IsNullOrEmpty(target.OtherAddress))
            target.OtherAddress = sourceAddress.FormattedAddress;
          if (sourceAddress.Primary)
          {
            target.SelectedMailingAddress = OlMailingAddress.olOther;
          }
        }
      }
    }
    public static OlSensitivity MapPrivacy2To1(string value)
    {
      switch (value)
      {
        case "public":
          return OlSensitivity.olNormal;
        case "personal":
          return OlSensitivity.olPersonal;
        case "private":
          return OlSensitivity.olPrivate;
        case "confidential":
          return OlSensitivity.olConfidential;
      }
      return OlSensitivity.olNormal;
    }
  }
}