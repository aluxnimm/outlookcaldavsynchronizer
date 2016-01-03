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
using System.Globalization;
using System.Runtime.InteropServices;
using CalDavSynchronizer.Implementation.ComWrappers;
using CalDavSynchronizer.Contracts;
using DDay.iCal;
using GenSync.EntityMapping;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;
using log4net;
using System.Reflection;
using System.IO;


namespace CalDavSynchronizer.Implementation.Contacts
{
  public class ContactEntityMapper : IEntityMapper<ContactItemWrapper, vCard>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";
    private const string PR_EMAIL2ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8094001F";
    private const string PR_EMAIL3ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/80a4001F";
    private const string PR_USER_X509_CERTIFICATE = "http://schemas.microsoft.com/mapi/proptag/0x3A701102";
    private const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";

    private readonly ContactMappingConfiguration _configuration;

    public ContactEntityMapper (ContactMappingConfiguration configuration)
    {
      _configuration = configuration;
    }

    public vCard Map1To2 (ContactItemWrapper source, vCard target, IEntityMappingLogger logger)
    {
      target.GivenName = source.Inner.FirstName;
      target.FamilyName = source.Inner.LastName;
      target.NamePrefix = source.Inner.Title;
      target.NameSuffix = source.Inner.Suffix;
      target.AdditionalNames = source.Inner.MiddleName;
      target.Gender = MapGender2To1 (source.Inner.Gender);

      if (!string.IsNullOrEmpty (source.Inner.FileAs))
      {
        target.FormattedName = source.Inner.FileAs;
      }
      else if (!string.IsNullOrEmpty (source.Inner.CompanyAndFullName))
      {
        target.FormattedName = source.Inner.CompanyAndFullName;
      }
      else
      {
        target.FormattedName = "<Empty>";
      }

      if (!string.IsNullOrEmpty (source.Inner.NickName))
      {
        Array.ForEach (
            source.Inner.NickName.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Nicknames.Add (c)
            );
      }

      target.AccessClassification = MapPrivacy1To2 (source.Inner.Sensitivity);

      if (!string.IsNullOrEmpty (source.Inner.Categories))
      {
        Array.ForEach (
            source.Inner.Categories.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add (c.Trim())
            );
      }

      if (!string.IsNullOrEmpty (source.Inner.IMAddress))
      {
        target.IMs.Add (new vCardIMPP (source.Inner.IMAddress, IMServiceType.AIM, ItemType.HOME));
      }

      MapEmailAddresses1To2 (source.Inner, target);

      if (!string.IsNullOrEmpty (source.Inner.HomeAddress))
      {
        vCardDeliveryAddress homeAddress = new vCardDeliveryAddress();
        homeAddress.AddressType.Add (vCardDeliveryAddressTypes.Home);
        homeAddress.City = source.Inner.HomeAddressCity;
        homeAddress.Country = source.Inner.HomeAddressCountry;
        homeAddress.PostalCode = source.Inner.HomeAddressPostalCode;
        homeAddress.Region = source.Inner.HomeAddressState;
        homeAddress.Street = source.Inner.HomeAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olHome)
        {
          homeAddress.AddressType.Add (vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add (homeAddress);
      }

      if (!string.IsNullOrEmpty (source.Inner.BusinessAddress))
      {
        vCardDeliveryAddress businessAddress = new vCardDeliveryAddress();
        businessAddress.AddressType.Add (vCardDeliveryAddressTypes.Work);
        businessAddress.City = source.Inner.BusinessAddressCity;
        businessAddress.Country = source.Inner.BusinessAddressCountry;
        businessAddress.PostalCode = source.Inner.BusinessAddressPostalCode;
        businessAddress.Region = source.Inner.BusinessAddressState;
        businessAddress.Street = source.Inner.BusinessAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olBusiness)
        {
          businessAddress.AddressType.Add (vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add (businessAddress);
      }

      if (!string.IsNullOrEmpty (source.Inner.OtherAddress))
      {
        vCardDeliveryAddress otherAddress = new vCardDeliveryAddress();
        otherAddress.City = source.Inner.OtherAddressCity;
        otherAddress.Country = source.Inner.OtherAddressCountry;
        otherAddress.PostalCode = source.Inner.OtherAddressPostalCode;
        otherAddress.Region = source.Inner.OtherAddressState;
        otherAddress.Street = source.Inner.OtherAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olOther)
        {
          otherAddress.AddressType.Add (vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add (otherAddress);
      }

      MapPhoneNumbers1To2 (source.Inner, target);

      if (_configuration.MapBirthday && !source.Inner.Birthday.Equals (new DateTime (4501, 1, 1, 0, 0, 0)))
      {
        target.BirthDate = source.Inner.Birthday.Date;
      }
      
      target.Organization = source.Inner.CompanyName;
      target.Department = source.Inner.Department;

      target.Title = source.Inner.JobTitle;
      target.Office = source.Inner.OfficeLocation;

      if (!string.IsNullOrEmpty (source.Inner.WebPage))
      {
        target.Websites.Add (new vCardWebsite (source.Inner.WebPage));
      }
      if (!string.IsNullOrEmpty (source.Inner.PersonalHomePage))
      {
        target.Websites.Add (new vCardWebsite (source.Inner.PersonalHomePage, vCardWebsiteTypes.Personal));
      }
      if (!string.IsNullOrEmpty (source.Inner.BusinessHomePage))
      {
        target.Websites.Add (new vCardWebsite (source.Inner.BusinessHomePage, vCardWebsiteTypes.Work));
      }

      MapCertificate1To2 (source.Inner, target);

      if (_configuration.MapContactPhoto) MapPhoto1To2 (source.Inner, target);

      if (!string.IsNullOrEmpty (source.Inner.Body))
      {
        target.Notes.Add (new vCardNote (source.Inner.Body));
      }

      return target;
    }

    public ContactItemWrapper Map2To1 (vCard source, ContactItemWrapper target, IEntityMappingLogger logger)
    {
      target.Inner.FirstName = source.GivenName;
      target.Inner.LastName = source.FamilyName;
      target.Inner.Title = source.NamePrefix;
      target.Inner.Suffix = source.NameSuffix;
      target.Inner.MiddleName = source.AdditionalNames;
      target.Inner.Gender = MapGender1To2 (source.Gender);
      target.Inner.FileAs = source.FormattedName;

      if (source.Nicknames.Count > 0)
      {
        string[] nickNames = new string[source.Nicknames.Count];
        source.Nicknames.CopyTo (nickNames, 0);
        target.Inner.NickName = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, nickNames);
      }

      target.Inner.Sensitivity = MapPrivacy2To1 (source.AccessClassification);

      if (source.Categories.Count > 0)
      {
        string[] categories = new string[source.Categories.Count];
        source.Categories.CopyTo (categories, 0);
        target.Inner.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, categories);
      }

      if (source.IMs.Count > 0)
      {
        target.Inner.IMAddress = source.IMs[0].Handle;
      }

      if (source.EmailAddresses.Count >= 1)
      {
        target.Inner.Email1Address = source.EmailAddresses[0].Address;

        if (source.EmailAddresses.Count >= 2)
        {
          target.Inner.Email2Address = source.EmailAddresses[1].Address;

          if (source.EmailAddresses.Count >= 3)
          {
            target.Inner.Email3Address = source.EmailAddresses[2].Address;
          }
        }
      }

      foreach (var sourceAddress in source.DeliveryAddresses)
      {
        if (sourceAddress.IsHome)
        {
          target.Inner.HomeAddressCity = sourceAddress.City;
          target.Inner.HomeAddressCountry = sourceAddress.Country;
          target.Inner.HomeAddressPostalCode = sourceAddress.PostalCode;
          target.Inner.HomeAddressState = sourceAddress.Region;
          target.Inner.HomeAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olHome;
          }
        }
        else if (sourceAddress.IsWork)
        {
          target.Inner.BusinessAddressCity = sourceAddress.City;
          target.Inner.BusinessAddressCountry = sourceAddress.Country;
          target.Inner.BusinessAddressPostalCode = sourceAddress.PostalCode;
          target.Inner.BusinessAddressState = sourceAddress.Region;
          target.Inner.BusinessAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olBusiness;
          }
        }
        else
        {
          target.Inner.OtherAddressCity = sourceAddress.City;
          target.Inner.OtherAddressCountry = sourceAddress.Country;
          target.Inner.OtherAddressPostalCode = sourceAddress.PostalCode;
          target.Inner.OtherAddressState = sourceAddress.Region;
          target.Inner.OtherAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olOther;
          }
        }
      }

      MapTelephoneNumber2To1 (source, target.Inner);

      if (_configuration.MapBirthday)
      {
        target.Inner.Birthday = source.BirthDate ?? new DateTime(4501, 1, 1);
      }

      if (!string.IsNullOrEmpty (source.Organization))
      {
        string[] organizationAndDepartments = source.Organization.Split (new[] { ';' }, 2);
        target.Inner.CompanyName = organizationAndDepartments[0];
        target.Inner.Department = (organizationAndDepartments.Length > 1) ? organizationAndDepartments[1]: null;
      }
      else
      {
        target.Inner.CompanyName = target.Inner.Department = null;
      }
      
      target.Inner.JobTitle = source.Title;
      target.Inner.OfficeLocation = source.Office;

      vCardWebsite sourceWebSite;

      if ((sourceWebSite = source.Websites.GetFirstChoice (vCardWebsiteTypes.Default)) != null)
      {
        target.Inner.WebPage = sourceWebSite.Url;
      }
      vCardWebsite sourceHomePage;

      if ((sourceHomePage = source.Websites.GetFirstChoice (vCardWebsiteTypes.Personal)) != null)
      {
        target.Inner.PersonalHomePage = sourceHomePage.Url;
      }
      vCardWebsite sourceBusinessHomePage;

      if ((sourceBusinessHomePage = source.Websites.GetFirstChoice (vCardWebsiteTypes.Work)) != null)
      {
        target.Inner.BusinessHomePage = sourceBusinessHomePage.Url;
      }

      MapCertificate2To1 (source, target.Inner);

      if (_configuration.MapContactPhoto) MapPhoto2To1 (source, target.Inner);

      if (source.Notes.Count > 0)
      {
        target.Inner.Body = source.Notes[0].Text;
      }

      return target;
    }

    private static OlGender MapGender1To2 (vCardGender sourceGender)
    {
      switch (sourceGender)
      {
        case vCardGender.Female:
          return OlGender.olFemale;
        case vCardGender.Male:
          return OlGender.olMale;
        case vCardGender.Unknown:
          return OlGender.olUnspecified;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", sourceGender));
    }

    private static vCardGender MapGender2To1 (OlGender sourceGender)
    {
      switch (sourceGender)
      {
        case OlGender.olFemale:
          return vCardGender.Female;
        case OlGender.olMale:
          return vCardGender.Male;
        case OlGender.olUnspecified:
          return vCardGender.Unknown;
      }

      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", sourceGender));
    }

    private vCardAccessClassification MapPrivacy1To2 (OlSensitivity value)
    {
      switch (value)
      {
        case OlSensitivity.olNormal:
          return vCardAccessClassification.Public;
        case OlSensitivity.olPersonal:
          return vCardAccessClassification.Private;
        case OlSensitivity.olPrivate:
          return vCardAccessClassification.Private;
        case OlSensitivity.olConfidential:
          return vCardAccessClassification.Confidential;
      }
      throw new NotImplementedException (string.Format ("Mapping for value '{0}' not implemented.", value));
    }

    private OlSensitivity MapPrivacy2To1 (vCardAccessClassification value)
    {
      switch (value)
      {
        case vCardAccessClassification.Public:
          return OlSensitivity.olNormal;
        case vCardAccessClassification.Private:
          return OlSensitivity.olPrivate;
        case vCardAccessClassification.Confidential:
          return OlSensitivity.olConfidential;
      }
      return OlSensitivity.olNormal;
    }

    private static void MapEmailAddresses1To2 (ContactItem source, vCard target)
    {
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
            s_logger.Error ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
          }
        }
        else
        {
          email1Address = source.Email1Address;
        }
        if (!string.IsNullOrEmpty (email1Address))
          target.EmailAddresses.Add (new vCardEmailAddress (email1Address));
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
            s_logger.Error ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
          }
        }
        else
        {
          email2Address = source.Email2Address;
        }
        if (!string.IsNullOrEmpty (email2Address))
          target.EmailAddresses.Add (new vCardEmailAddress (email2Address));
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
            s_logger.Error ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
          }
        }
        else
        {
          email3Address = source.Email3Address;
        }
        if (!string.IsNullOrEmpty (email3Address))
          target.EmailAddresses.Add (new vCardEmailAddress (email3Address));
      }
    }

    private static byte[] GetRawCert (byte[] certWrapper)
    {
      // strip header and footer for PidTagUserX509Certificate according to
      // http://msdn.microsoft.com/en-us/library/hh745506%28v=exchg.80%29.aspx

      using (MemoryStream ms = new MemoryStream())
      {
        ms.Write (certWrapper, 12, certWrapper.Length - 20);
        byte[] o = ms.ToArray();
        return o;
      }
    }

    private static object[] BuildCertProperty (byte[] rawData)
    {
      // add header and footer for PidTagUserX509Certificate according to
      // http://msdn.microsoft.com/en-us/library/hh745506%28v=exchg.80%29.aspx

      using (MemoryStream ms = new MemoryStream())
      {
        byte[] headerWithoutLength = { 0x01, 0x00, 0x08, 0x00, 0x01, 0x00, 0x00, 0x00, 0x03, 0x00 };
        ms.Write (headerWithoutLength, 0, headerWithoutLength.Length);
        byte[] lengthBytes = BitConverter.GetBytes ((short) (rawData.Length + 4));
        ms.WriteByte (lengthBytes[0]);
        ms.WriteByte (lengthBytes[1]);
        ms.Write (rawData, 0, rawData.Length);
        byte[] footer =
        {
            0x06, 0x00, 0x08, 0x00, 0x01, 0x00, 0x00, 0x00, 0x20, 0x00, 0x08, 0x00,
            0x07, 0x00, 0x00, 0x00, 0x02, 0x00, 0x04, 0x00
        };
        ms.Write (footer, 0, footer.Length);

        object[] o = new object[] { ms.ToArray() };
        return o;
      }
    }

    private static void MapCertificate1To2 (ContactItem source, vCard target)
    {
      try
      {
        object[] certWrapper = source.GetPropertySafe (PR_USER_X509_CERTIFICATE);

        if (certWrapper.Length > 0)
        {
          byte[] rawCert = GetRawCert ((byte[]) certWrapper[0]);
          target.Certificates.Add (new vCardCertificate ("X509", rawCert));
        }
      }
      catch (COMException ex)
      {
        s_logger.Error ("Could not get property PR_USER_X509_CERTIFICATE for contact.", ex);
      }
    }

    private static void MapCertificate2To1 (vCard source, ContactItem target)
    {
      if (source.Certificates.Count > 0)
      {
        object[] certWrapper = BuildCertProperty (source.Certificates[0].Data);
        using (var oPa = GenericComObjectWrapper.Create (target.PropertyAccessor))
        {
          try
          {
            oPa.Inner.SetProperty (PR_USER_X509_CERTIFICATE, certWrapper);
          }
          catch (COMException ex)
          {
            s_logger.Error ("Could not set property PR_USER_X509_CERTIFICATE for contact.", ex);
          }
          catch (System.UnauthorizedAccessException ex)
          {
            s_logger.Error ("Could not access PR_USER_X509_CERTIFICATE for contact.", ex);
          }
        }
      }
    }

    private static void MapPhoto1To2 (ContactItem source, vCard target)
    {
      if (source.HasPicture)
      {
        foreach (var att in source.Attachments.ToSafeEnumerable<Microsoft.Office.Interop.Outlook.Attachment>())
        {
          if (att.DisplayName == "ContactPicture.jpg")
          {
            using (var oPa = GenericComObjectWrapper.Create (att.PropertyAccessor))
            {
              try
              {
                byte[] rawAttachmentData = (byte[]) oPa.Inner.GetProperty (PR_ATTACH_DATA_BIN);
                target.Photos.Add (new vCardPhoto (rawAttachmentData));
              }
              catch (COMException ex)
              {
                s_logger.Error ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
              catch (System.UnauthorizedAccessException ex)
              {
                s_logger.Error ("Could not access PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
            }
          }
        }
      }
    }

    private static void MapPhoto2To1 (vCard source, ContactItem target)
    {
      if (source.Photos.Count > 0)
      {
        vCardPhoto contactPhoto = source.Photos[0];

        if (contactPhoto.IsLoaded)
        {
          string picturePath = Path.GetTempPath() + @"\Contact_" + target.EntryID + ".jpg";
          using (FileStream fs = new FileStream (picturePath, FileMode.Create))
          {
            fs.Write (contactPhoto.GetBytes(), 0, contactPhoto.GetBytes().Length);
            fs.Flush();
          }
          target.AddPicture (picturePath);
          File.Delete (picturePath);
        }
      }
    }

    private static void MapPhoneNumbers1To2 (ContactItem source, vCard target)
    {
      if (!string.IsNullOrEmpty (source.PrimaryTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.PrimaryTelephoneNumber, vCardPhoneTypes.Main);
        phoneNumber.IsPreferred = true;
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.MobileTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.MobileTelephoneNumber, vCardPhoneTypes.Cellular);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.HomeTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.HomeTelephoneNumber, vCardPhoneTypes.Home);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.Home2TelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.Home2TelephoneNumber, vCardPhoneTypes.HomeVoice);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.HomeFaxNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.HomeFaxNumber, vCardPhoneTypes.Fax | vCardPhoneTypes.Home);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.BusinessTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.BusinessTelephoneNumber, vCardPhoneTypes.Work);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.Business2TelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.Business2TelephoneNumber, vCardPhoneTypes.WorkVoice);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.BusinessFaxNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.BusinessFaxNumber, vCardPhoneTypes.WorkFax);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.PagerNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.PagerNumber, vCardPhoneTypes.Pager);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.CarTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.CarTelephoneNumber, vCardPhoneTypes.Car);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.ISDNNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.ISDNNumber, vCardPhoneTypes.ISDN);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add (phoneNumber);
      }
    }

    private static void MapTelephoneNumber2To1 (vCard source, ContactItem target)
    {
      target.HomeTelephoneNumber = string.Empty;
      target.BusinessTelephoneNumber = string.Empty;

      foreach (var phoneNumber in source.Phones)
      {
        if (phoneNumber.IsMain)
        {
          target.PrimaryTelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.IsCellular)
        {
          target.MobileTelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.IsHome && !phoneNumber.IsFax)
        {
          if (string.IsNullOrEmpty (target.HomeTelephoneNumber))
          {
            target.HomeTelephoneNumber = phoneNumber.FullNumber;
          }
          else
          {
            target.Home2TelephoneNumber = phoneNumber.FullNumber;
          }
        }
        else if (phoneNumber.IsWork && !phoneNumber.IsFax)
        {
          if (string.IsNullOrEmpty (target.BusinessTelephoneNumber))
          {
            target.BusinessTelephoneNumber = phoneNumber.FullNumber;
          }
          else
          {
            target.Business2TelephoneNumber = phoneNumber.FullNumber;
          }
        }
        else if (phoneNumber.IsFax)
        {
          if (phoneNumber.IsHome)
          {
            target.HomeFaxNumber = phoneNumber.FullNumber;
          }
          else
          {
            target.BusinessFaxNumber = phoneNumber.FullNumber;
          }
        }
        else if (phoneNumber.IsPager)
        {
          target.PagerNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.IsCar)
        {
          target.CarTelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.IsISDN)
        {
          target.ISDNNumber = phoneNumber.FullNumber;
        }
        else
        {
          if (phoneNumber.IsPreferred && string.IsNullOrEmpty (target.HomeTelephoneNumber))
          {
            target.HomeTelephoneNumber = phoneNumber.FullNumber;
          }
          else
          {
            target.OtherTelephoneNumber = phoneNumber.FullNumber;
          }
        }
      }
    }
  }
}