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
using CalDavSynchronizer.DataAccess;
using GenSync.EntityMapping;
using GenSync.Logging;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;
using log4net;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exception = System.Exception;


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

    public Task<vCard> Map1To2 (ContactItemWrapper source, vCard target, IEntityMappingLogger logger)
    {
      target.GivenName = source.Inner.FirstName;
      target.FamilyName = source.Inner.LastName;
      target.NamePrefix = source.Inner.Title;
      target.NameSuffix = source.Inner.Suffix;
      target.AdditionalNames = source.Inner.MiddleName;
      target.Gender = MapGender2To1 (source.Inner.Gender);

      MapEmailAddresses1To2(source.Inner, target, logger);

      if (!string.IsNullOrEmpty (source.Inner.FileAs))
      {
        target.FormattedName = source.Inner.FileAs;
      }
      else if (!string.IsNullOrEmpty (source.Inner.CompanyAndFullName))
      {
        target.FormattedName = source.Inner.CompanyAndFullName;
      }
      else if (target.EmailAddresses.Count >= 1)
      {
        target.FormattedName = target.EmailAddresses[0].Address;
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

      if (!string.IsNullOrEmpty (source.Inner.PersonalHomePage))
      {
        target.Websites.Add (new vCardWebsite (source.Inner.PersonalHomePage, vCardWebsiteTypes.Personal));
      }
      if (!string.IsNullOrEmpty (source.Inner.BusinessHomePage))
      {
        target.Websites.Add (new vCardWebsite (source.Inner.BusinessHomePage, vCardWebsiteTypes.Work));
      }

      MapCertificate1To2 (source.Inner, target, logger);

      if (_configuration.MapContactPhoto) MapPhoto1To2 (source.Inner, target, logger);

      if (!string.IsNullOrEmpty (source.Inner.Body))
      {
        target.Notes.Add (new vCardNote (source.Inner.Body));
      }

      return Task.FromResult(target);
    }

    public async Task<ContactItemWrapper> Map2To1 (vCard source, ContactItemWrapper target, IEntityMappingLogger logger)
    {
      target.Inner.FirstName = source.GivenName;
      target.Inner.LastName = source.FamilyName;
      target.Inner.Title = source.NamePrefix;
      target.Inner.Suffix = source.NameSuffix;
      target.Inner.MiddleName = source.AdditionalNames;
      target.Inner.Gender = MapGender1To2 (source.Gender);
      if (string.IsNullOrEmpty (target.Inner.FullName))
        target.Inner.FullName = source.FormattedName;
      if (!_configuration.KeepOutlookFileAs)
        target.Inner.FileAs = source.FormattedName;

      if (source.Nicknames.Count > 0)
      {
        string[] nickNames = new string[source.Nicknames.Count];
        source.Nicknames.CopyTo (nickNames, 0);
        target.Inner.NickName = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, nickNames);
      }
      else
      {
        target.Inner.NickName = string.Empty;
      }

      target.Inner.Sensitivity = MapPrivacy2To1 (source.AccessClassification);

      if (source.Categories.Count > 0)
      {
        string[] categories = new string[source.Categories.Count];
        source.Categories.CopyTo (categories, 0);
        target.Inner.Categories = string.Join (CultureInfo.CurrentCulture.TextInfo.ListSeparator, categories);
      }
      else
      {
        target.Inner.Categories = string.Empty;
      }

      if (source.IMs.Count > 0)
      {
        target.Inner.IMAddress = source.IMs[0].Handle;
      }
      else
      {
        target.Inner.IMAddress = string.Empty;
      }

      target.Inner.Email1Address = string.Empty;
      target.Inner.Email1DisplayName = string.Empty;
      target.Inner.Email2Address = string.Empty;
      target.Inner.Email2DisplayName = string.Empty;
      target.Inner.Email3Address = string.Empty;
      target.Inner.Email3DisplayName = string.Empty;
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

      MapPostalAdresses2To1 (source, target.Inner);

      MapTelephoneNumber2To1 (source, target.Inner);

      if (_configuration.MapBirthday)
      {
        if (source.BirthDate.HasValue)
        {
          if (!source.BirthDate.Value.Date.Equals (target.Inner.Birthday))
          {
            try
            {
              target.Inner.Birthday = source.BirthDate.Value;
            }
            catch (COMException ex)
            {
              s_logger.Warn ("Could not update contact birthday.", ex);
              logger.LogMappingWarning ("Could not update contact birthday.", ex);
            }
          }
        }
        else
        {
          target.Inner.Birthday = new DateTime (4501, 1, 1);
        }       
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

      MapHomePage2To1 (source, target.Inner);

      MapCertificate2To1 (source, target.Inner, logger);

      if (_configuration.MapContactPhoto)
        await MapPhoto2To1 (source, target.Inner, logger);

      if (source.Notes.Count > 0)
      {
        target.Inner.Body = source.Notes[0].Text;
      }
      else
      {
        target.Inner.Body = string.Empty;
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

    private static void MapEmailAddresses1To2 (ContactItem source, vCard target, IEntityMappingLogger logger)
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
            s_logger.Warn ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL1ADDRESS for Email1Address", ex);
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
            s_logger.Warn ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL2ADDRESS for Email2Address", ex);
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
            s_logger.Warn ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
            logger.LogMappingWarning ("Could not get property PR_EMAIL3ADDRESS for Email3Address", ex);
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

    private static void MapCertificate1To2 (ContactItem source, vCard target, IEntityMappingLogger logger)
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
        s_logger.Warn ("Could not get property PR_USER_X509_CERTIFICATE for contact.", ex);
        logger.LogMappingWarning ("Could not get property PR_USER_X509_CERTIFICATE for contact.", ex);
      }
    }

    private static void MapCertificate2To1 (vCard source, ContactItem target, IEntityMappingLogger logger)
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
            s_logger.Warn ("Could not set property PR_USER_X509_CERTIFICATE for contact.", ex);
            logger.LogMappingWarning ("Could not set property PR_USER_X509_CERTIFICATE for contact.", ex);
          }
          catch (System.UnauthorizedAccessException ex)
          {
            s_logger.Warn ("Could not access PR_USER_X509_CERTIFICATE for contact.", ex);
            logger.LogMappingWarning ("Could not access PR_USER_X509_CERTIFICATE for contact.", ex);
          }
        }
      }
    }

    private static void MapPhoto1To2 (ContactItem source, vCard target, IEntityMappingLogger logger)
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
                s_logger.Warn ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
                logger.LogMappingWarning ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
              catch (System.UnauthorizedAccessException ex)
              {
                s_logger.Warn ("Could not access PR_ATTACH_DATA_BIN to export picture for contact.", ex);
                logger.LogMappingWarning ("Could not get property PR_ATTACH_DATA_BIN to export picture for contact.", ex);
              }
            }
          }
        }
      }
    }

    private async Task MapPhoto2To1 (vCard source, ContactItem target, IEntityMappingLogger logger)
    {
      if (source.Photos.Count > 0)
      {
        if (target.HasPicture && _configuration.KeepOutlookPhoto) return;

        vCardPhoto contactPhoto = source.Photos[0];

        string picturePath = Path.GetTempPath() + @"\Contact_" + target.EntryID + ".jpg";
        try
        {
          if (!contactPhoto.IsLoaded && contactPhoto.Url != null)
          {
            using (var client = HttpUtility.CreateWebClient())
            {
              await client.DownloadFileTaskAsync (contactPhoto.Url, picturePath);
            }
          }
          else if (contactPhoto.IsLoaded)
          {
            File.WriteAllBytes (picturePath, contactPhoto.GetBytes());
          }
          else
          {
            s_logger.Warn ("Could not load picture for contact.");
            logger.LogMappingWarning ("Could not load picture for contact.");
            return;
          }
          try
          {
            target.AddPicture (picturePath);
          }
          catch (COMException x)
          {
            s_logger.Warn ("Could not add picture for contact.", x);
            logger.LogMappingWarning ("Could not add picture for contact.", x);
          }
          File.Delete (picturePath);
        }
        catch (Exception ex)
        {
          s_logger.Warn ("Could not add picture for contact.", ex);
          logger.LogMappingWarning ("Could not add picture for contact.", ex);
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
            logger.LogMappingWarning ("Could not remove picture for contact.", x);
          }
        }
      }
    }

    private static void MapHomePage2To1 (vCard source, ContactItem target)
    {
      vCardWebsite sourceWebSite;

      target.WebPage = null;
      target.BusinessHomePage = null;
      target.PersonalHomePage = null;

      if ((sourceWebSite = source.Websites.GetFirstChoice (vCardWebsiteTypes.Default)) != null)
      {
        target.WebPage = sourceWebSite.Url;
      }
      vCardWebsite sourceHomePage;

      if ((sourceHomePage = source.Websites.GetFirstChoice (vCardWebsiteTypes.Personal)) != null)
      {
        target.PersonalHomePage = sourceHomePage.Url;
      }
      vCardWebsite sourceBusinessHomePage;

      if ((sourceBusinessHomePage = source.Websites.GetFirstChoice (vCardWebsiteTypes.Work)) != null)
      {
        target.BusinessHomePage = sourceBusinessHomePage.Url;
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
      if (!string.IsNullOrEmpty (source.OtherTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.OtherTelephoneNumber, vCardPhoneTypes.Voice);
        target.Phones.Add (phoneNumber);
      }
      if (!string.IsNullOrEmpty (source.OtherFaxNumber))
      {
        vCardPhone phoneNumber = new vCardPhone (source.OtherFaxNumber, vCardPhoneTypes.Fax);
        target.Phones.Add (phoneNumber);
      }
    }

    private void MapTelephoneNumber2To1 (vCard source, ContactItem target)
    {
      target.HomeTelephoneNumber = string.Empty;
      target.BusinessTelephoneNumber = string.Empty;
      target.BusinessFaxNumber = string.Empty;
      target.PrimaryTelephoneNumber = string.Empty;
      target.MobileTelephoneNumber = string.Empty;

      foreach (var phoneNumber in source.Phones)
      {
        string sourceNumber = _configuration.FixPhoneNumberFormat ? 
                              FixPhoneNumberFormat (phoneNumber.FullNumber) : phoneNumber.FullNumber;
        if (phoneNumber.IsMain)
        {
          target.PrimaryTelephoneNumber = sourceNumber;
        }
        else if (phoneNumber.IsCellular)
        {
          target.MobileTelephoneNumber = sourceNumber;
        }
        else if (phoneNumber.IsiPhone && string.IsNullOrEmpty (target.MobileTelephoneNumber))
        {
          target.MobileTelephoneNumber = sourceNumber;
        }
        else if (phoneNumber.IsHome && !phoneNumber.IsFax)
        {
          if (string.IsNullOrEmpty (target.HomeTelephoneNumber))
          {
            target.HomeTelephoneNumber = sourceNumber;
          }
          else
          {
            target.Home2TelephoneNumber = sourceNumber;
          }
        }
        else if (phoneNumber.IsWork && !phoneNumber.IsFax)
        {
          if (string.IsNullOrEmpty (target.BusinessTelephoneNumber))
          {
            target.BusinessTelephoneNumber = sourceNumber;
          }
          else
          {
            target.Business2TelephoneNumber = sourceNumber;
          }
        }
        else if (phoneNumber.IsFax)
        {
          if (phoneNumber.IsHome)
          {
            target.HomeFaxNumber = sourceNumber;
          }
          else
          {
            if (string.IsNullOrEmpty (target.BusinessFaxNumber))
            {
              target.BusinessFaxNumber = sourceNumber;
            }
            else
            {
              target.OtherFaxNumber = sourceNumber;
            }
          }
        }
        else if (phoneNumber.IsPager)
        {
          target.PagerNumber = sourceNumber;
        }
        else if (phoneNumber.IsCar)
        {
          target.CarTelephoneNumber = sourceNumber;
        }
        else if (phoneNumber.IsISDN)
        {
          target.ISDNNumber = sourceNumber;
        }
        else
        {
          if (phoneNumber.IsPreferred && string.IsNullOrEmpty (target.PrimaryTelephoneNumber))
          {
            target.PrimaryTelephoneNumber = sourceNumber;
          }
          else if (phoneNumber.IsPreferred && string.IsNullOrEmpty (target.HomeTelephoneNumber))
          {
            target.HomeTelephoneNumber = sourceNumber;
          }
          else
          {
            target.OtherTelephoneNumber = sourceNumber;
          }
        }
      }
    }

    private static string FixPhoneNumberFormat (string number)
    {
      // Reformat telephone numbers so that Outlook can split country/area code and extension
      var match = Regex.Match (number, @"(\+\d+) (\d+) (\d+)( \d+)?");
      if (match.Success)
      {
        string ext = string.IsNullOrEmpty (match.Groups[4].Value) ? string.Empty : " - " + match.Groups[4].Value;
          
        return match.Groups[1].Value + " ( " + match.Groups[2].Value + " ) " + match.Groups[3].Value + ext;
      }
      else
      {
        return number;
      }
    }

    private static void MapPostalAdresses2To1 (vCard source, ContactItem target)
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

      foreach (var sourceAddress in source.DeliveryAddresses)
      {
        if (sourceAddress.IsHome)
        {
          target.HomeAddressCity = sourceAddress.City;
          target.HomeAddressCountry = sourceAddress.Country;
          target.HomeAddressPostalCode = sourceAddress.PostalCode;
          target.HomeAddressState = sourceAddress.Region;
          target.HomeAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.SelectedMailingAddress = OlMailingAddress.olHome;
          }
        }
        else if (sourceAddress.IsWork)
        {
          target.BusinessAddressCity = sourceAddress.City;
          target.BusinessAddressCountry = sourceAddress.Country;
          target.BusinessAddressPostalCode = sourceAddress.PostalCode;
          target.BusinessAddressState = sourceAddress.Region;
          target.BusinessAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.SelectedMailingAddress = OlMailingAddress.olBusiness;
          }
        }
        else
        {
          target.OtherAddressCity = sourceAddress.City;
          target.OtherAddressCountry = sourceAddress.Country;
          target.OtherAddressPostalCode = sourceAddress.PostalCode;
          target.OtherAddressState = sourceAddress.Region;
          target.OtherAddressStreet = sourceAddress.Street;
          if (sourceAddress.IsPreferred)
          {
            target.SelectedMailingAddress = OlMailingAddress.olOther;
          }
        }
      }
    }
  }
}