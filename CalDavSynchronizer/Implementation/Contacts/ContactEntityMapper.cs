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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CalDavSynchronizer.Implementation.Common;
using Exception = System.Exception;


namespace CalDavSynchronizer.Implementation.Contacts
{
  public class ContactEntityMapper : IEntityMapper<IContactItemWrapper, vCard, ICardDavRepositoryLogger>
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private const string PR_EMAIL1ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8084001F";
    private const string PR_EMAIL2ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/8094001F";
    private const string PR_EMAIL3ADDRESS = "http://schemas.microsoft.com/mapi/id/{00062004-0000-0000-C000-000000000046}/80a4001F";
    private const string PR_USER_X509_CERTIFICATE = "http://schemas.microsoft.com/mapi/proptag/0x3A701102";
    private const string PR_ATTACH_DATA_BIN = "http://schemas.microsoft.com/mapi/proptag/0x37010102";

    private readonly ContactMappingConfiguration _configuration;
    private readonly Func<WebClient> _webClientFactory;

    public ContactEntityMapper (ContactMappingConfiguration configuration, Func<WebClient> webClientFactory)
    {
      if (configuration == null) throw new ArgumentNullException(nameof(configuration));
      if (webClientFactory == null) throw new ArgumentNullException(nameof(webClientFactory));

      _configuration = configuration;
      _webClientFactory = webClientFactory;
    }

    public Task<vCard> Map1To2 (IContactItemWrapper source, vCard target, IEntitySynchronizationLogger logger, ICardDavRepositoryLogger context)
    {
      target.RevisionDate = source.Inner.LastModificationTime.ToUniversalTime();
      
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

      target.Nicknames.Clear();
      if (!string.IsNullOrEmpty (source.Inner.NickName))
      {
        Array.ForEach (
            source.Inner.NickName.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Nicknames.Add (c)
            );
      }

      target.AccessClassification = CommonEntityMapper.MapPrivacy1To2 (source.Inner.Sensitivity);

      target.Categories.Clear();
      if (!string.IsNullOrEmpty (source.Inner.Categories))
      {
        Array.ForEach (
            source.Inner.Categories.Split (new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add (c.Trim())
            );
      }

      target.IMs.Clear();
      if (!string.IsNullOrEmpty (source.Inner.IMAddress))
      {
        //IMAddress are expected to be in form of ([Protocol]: [Address]; [Protocol]: [Address])
        var imsRaw = source.Inner.IMAddress.Split (new [] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var imRaw in imsRaw)
        {
          var imDetails = imRaw.Trim().Split (new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
          var im = new vCardIMPP();
          if (imDetails.Length == 1)
          {
            im.Handle = imDetails[0].Trim();
            // Set default ServiceType to the configured DefaultImServiceType (defaults to AIM)
            im.ServiceType = _configuration.DefaultImServicType;
          }
          else if (imDetails.Length > 1)
          {
            var serviceType = IMTypeUtils.GetIMServiceType(imDetails[0].Trim());
            if (serviceType == null)
            {
              im.ServiceType = _configuration.DefaultImServicType;
              s_logger.Warn ($"Unknown IM ServiceType '{imDetails[0]}' not implemented, defaulting to '{_configuration.DefaultImServicType}'");
              logger.LogWarning ($"Unknown IM ServiceType '{imDetails[0]}' not implemented, defaulting to '{_configuration.DefaultImServicType}'");
            }
            else
            {
              im.ServiceType = serviceType.Value;
            }
            im.Handle = imRaw.Substring (imRaw.IndexOf (":")+1).Trim();
          }

          //Only add the im Address if not empty
          if (!string.IsNullOrEmpty (im.Handle))
          {
            im.IsPreferred = target.IMs.Count == 0;
            im.ItemType = ItemType.HOME;
            target.IMs.Add (im);
          }
        }
      }

      target.DeliveryAddresses.Clear();
      if (!string.IsNullOrEmpty (source.Inner.HomeAddress))
      {
        vCardDeliveryAddress homeAddress = new vCardDeliveryAddress();
        homeAddress.AddressType.Add (vCardDeliveryAddressTypes.Home);
        homeAddress.City = source.Inner.HomeAddressCity;
        homeAddress.Country = source.Inner.HomeAddressCountry;
        homeAddress.PostalCode = source.Inner.HomeAddressPostalCode;
        homeAddress.Region = source.Inner.HomeAddressState;
        homeAddress.Street = source.Inner.HomeAddressStreet;
        homeAddress.PoBox = source.Inner.HomeAddressPostOfficeBox;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olHome)
        {
          homeAddress.AddressType.Add (vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add (homeAddress);
      }

      if (!string.IsNullOrEmpty (source.Inner.BusinessAddress) || !string.IsNullOrEmpty(source.Inner.OfficeLocation))
      {
        vCardDeliveryAddress businessAddress = new vCardDeliveryAddress();
        businessAddress.AddressType.Add (vCardDeliveryAddressTypes.Work);
        businessAddress.City = source.Inner.BusinessAddressCity;
        businessAddress.Country = source.Inner.BusinessAddressCountry;
        businessAddress.PostalCode = source.Inner.BusinessAddressPostalCode;
        businessAddress.Region = source.Inner.BusinessAddressState;
        businessAddress.Street = source.Inner.BusinessAddressStreet;
        businessAddress.PoBox = source.Inner.BusinessAddressPostOfficeBox;
        if (!string.IsNullOrEmpty(source.Inner.OfficeLocation))
        {
          businessAddress.ExtendedAddress = source.Inner.OfficeLocation;
        }
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
        otherAddress.PoBox = source.Inner.OtherAddressPostOfficeBox;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olOther)
        {
          otherAddress.AddressType.Add (vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add (otherAddress);
      }

      MapPhoneNumbers1To2 (source.Inner, target);

      if (_configuration.MapAnniversary)
      {
        target.Anniversary = source.Inner.Anniversary.Equals (OutlookUtility.OUTLOOK_DATE_NONE) ? default(DateTime?) : source.Inner.Anniversary.Date;
      }
      if (_configuration.MapBirthday)
      {
        target.BirthDate = source.Inner.Birthday.Equals (OutlookUtility.OUTLOOK_DATE_NONE) ? default(DateTime?) : source.Inner.Birthday.Date;
      }
      target.Organization = source.Inner.CompanyName;
      target.Department = source.Inner.Department;

      target.Title = source.Inner.JobTitle;
      target.Role = source.Inner.Profession;

      target.Websites.Clear();
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

      target.Notes.Clear();
      if (!string.IsNullOrEmpty (source.Inner.Body))
      {
        target.Notes.Add (new vCardNote (source.Inner.Body));
      }

      return Task.FromResult(target);
    }

    public async Task<IContactItemWrapper> Map2To1 (vCard source, IContactItemWrapper target, IEntitySynchronizationLogger logger, ICardDavRepositoryLogger context)
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

      target.Inner.Sensitivity = CommonEntityMapper.MapPrivacy2To1 (source.AccessClassification);

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

      MapIMs2To1 (source, target.Inner);

      target.Inner.Email1Address = string.Empty;
      target.Inner.Email1DisplayName = string.Empty;
      target.Inner.Email2Address = string.Empty;
      target.Inner.Email2DisplayName = string.Empty;
      target.Inner.Email3Address = string.Empty;
      target.Inner.Email3DisplayName = string.Empty;
      if (source.EmailAddresses.Count >= 1)
      {
        Func<vCardEmailAddress, bool> firstPredicate = e => _configuration.MapOutlookEmail1ToWork ? e.ItemType == ItemType.WORK :  e.ItemType == ItemType.HOME;

        var first = source.EmailAddresses.FirstOrDefault (firstPredicate) ?? source.EmailAddresses.First();
        target.Inner.Email1Address = first.Address;

        var second = source.EmailAddresses.FirstOrDefault (e => _configuration.MapOutlookEmail1ToWork ? e.ItemType == ItemType.HOME : e.ItemType == ItemType.WORK && e!= first) ??
                           source.EmailAddresses.FirstOrDefault (e => e != first);

        if (second != null)
        {
          target.Inner.Email2Address = second.Address;

          var other = source.EmailAddresses.FirstOrDefault (e => e != first && e != second);
          if (other != null)
          {
            target.Inner.Email3Address = other.Address;
          }
        }
      }

      MapPostalAdresses2To1 (source, target.Inner);

      MapTelephoneNumber2To1 (source, target.Inner);

      if (_configuration.MapAnniversary)
      {
        if (source.Anniversary.HasValue)
        {
          if (!source.Anniversary.Value.Date.Equals (target.Inner.Anniversary))
          {
            try
            {
              target.Inner.Anniversary = source.Anniversary.Value;
            }
            catch (COMException ex)
            {
              s_logger.Warn ("Could not update contact anniversary.", ex);
              logger.LogWarning ("Could not update contact anniversary.", ex);
            }
            catch (OverflowException ex)
            {
              s_logger.Warn ("Contact anniversary has invalid value.", ex);
              logger.LogWarning ("Contact anniversary has invalid value.", ex);
            }
          }
        }
        else
        {
          target.Inner.Anniversary = OutlookUtility.OUTLOOK_DATE_NONE;
        }
      }

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
              logger.LogWarning ("Could not update contact birthday.", ex);
            }
            catch (OverflowException ex)
            {
              s_logger.Warn ("Contact birthday has invalid value.", ex);
              logger.LogWarning ("Contact birthday has invalid value.", ex);
            }
          }
        }
        else
        {
          target.Inner.Birthday = OutlookUtility.OUTLOOK_DATE_NONE;
        }       
      }

      target.Inner.CompanyName = source.Organization;
      target.Inner.Department = source.Department;
      
      target.Inner.JobTitle = source.Title;
      target.Inner.Profession = source.Role;

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

    private void MapEmailAddresses1To2 (ContactItem source, vCard target, IEntitySynchronizationLogger logger)
    {
      target.EmailAddresses.Clear();
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
          target.EmailAddresses.Add (new vCardEmailAddress (email1Address, vCardEmailAddressType.Internet, _configuration.MapOutlookEmail1ToWork ? ItemType.WORK : ItemType.HOME));
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
          target.EmailAddresses.Add (new vCardEmailAddress (email2Address, vCardEmailAddressType.Internet, _configuration.MapOutlookEmail1ToWork ? ItemType.HOME : ItemType.WORK));
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

    private static void MapCertificate1To2 (ContactItem source, vCard target, IEntitySynchronizationLogger logger)
    {
      try
      {
        object[] certWrapper = source.GetPropertySafe (PR_USER_X509_CERTIFICATE);

        target.Certificates.Clear();
        if (certWrapper.Length > 0)
        {
          byte[] rawCert = GetRawCert ((byte[]) certWrapper[0]);
          target.Certificates.Add (new vCardCertificate ("X509", rawCert));
        }
      }
      catch (COMException ex)
      {
        s_logger.Warn ("Could not get property PR_USER_X509_CERTIFICATE for contact.", ex);
        logger.LogWarning ("Could not get property PR_USER_X509_CERTIFICATE for contact.", ex);
      }
    }

    private static void MapCertificate2To1 (vCard source, ContactItem target, IEntitySynchronizationLogger logger)
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
            logger.LogWarning ("Could not set property PR_USER_X509_CERTIFICATE for contact.", ex);
          }
          catch (System.UnauthorizedAccessException ex)
          {
            s_logger.Warn ("Could not access PR_USER_X509_CERTIFICATE for contact.", ex);
            logger.LogWarning ("Could not access PR_USER_X509_CERTIFICATE for contact.", ex);
          }
        }
      }
    }

    private static void MapPhoto1To2 (ContactItem source, vCard target, IEntitySynchronizationLogger logger)
    {
      target.Photos.Clear();
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

    private async Task MapPhoto2To1 (vCard source, ContactItem target, IEntitySynchronizationLogger logger)
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
            using (var client = _webClientFactory())
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
            logger.LogWarning ("Could not load picture for contact.");
            return;
          }
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
        catch (Exception ex)
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
      target.Phones.Clear();
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

      // if no PhoneTypes are set (e.g. Yandex drops the types) 
      // assume a default ordering of cell,work,home to avoid data loss of the first 3 numbers

      if (source.Phones.Count >= 1 && source.Phones.All (p => p.PhoneType == vCardPhoneTypes.Default))
      {
        var phoneNumber1 = source.Phones[0].FullNumber;
        target.MobileTelephoneNumber = _configuration.FixPhoneNumberFormat
          ? FixPhoneNumberFormat (phoneNumber1)
          : phoneNumber1;

        if (source.Phones.Count >= 2)
        {
          var phoneNumber2 = source.Phones[1].FullNumber;
          target.BusinessTelephoneNumber = _configuration.FixPhoneNumberFormat
             ? FixPhoneNumberFormat (phoneNumber2)
             : phoneNumber2;
          if (source.Phones.Count >= 3)
          {
            var phoneNumber3= source.Phones[2].FullNumber;
            target.HomeTelephoneNumber = _configuration.FixPhoneNumberFormat
               ? FixPhoneNumberFormat (phoneNumber3)
               : phoneNumber3;
          }
        }
        return;
      }

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
      target.OfficeLocation = string.Empty;

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
          if (!string.IsNullOrEmpty (sourceAddress.ExtendedAddress))
            target.HomeAddressStreet += "\r\n" + sourceAddress.ExtendedAddress;
          target.HomeAddressPostOfficeBox = sourceAddress.PoBox;
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
          if (!string.IsNullOrEmpty(sourceAddress.ExtendedAddress))
          {
            if (string.IsNullOrEmpty(target.OfficeLocation))
            {
              target.OfficeLocation = sourceAddress.ExtendedAddress;
            }
            else
            {
              target.BusinessAddressStreet += "\r\n" + sourceAddress.ExtendedAddress;
            }
          }
          target.BusinessAddressPostOfficeBox = sourceAddress.PoBox;
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
          if (!string.IsNullOrEmpty (sourceAddress.ExtendedAddress))
            target.OtherAddressStreet += "\r\n" + sourceAddress.ExtendedAddress;
          target.OtherAddressPostOfficeBox = sourceAddress.PoBox;
          if (sourceAddress.IsPreferred)
          {
            target.SelectedMailingAddress = OlMailingAddress.olOther;
          }
        }
      }
    }

    private void MapIMs2To1 (vCard source, ContactItem target)
    {
      var alreadyContainedImAddresses = new HashSet<string>();

      target.IMAddress = string.Empty;
      foreach (var im in source.IMs)
      {
        string imString;

        if (im.ServiceType != IMServiceType.Unspecified && im.ServiceType != _configuration.DefaultImServicType)
          imString = im.ServiceType + ": " + im.Handle;
        else
          imString = im.Handle;

        if (!string.IsNullOrEmpty (target.IMAddress))
        {
          // some servers like iCloud use IMPP and X-PROTOCOL together
          // don't add IM address twice in such case
          if (alreadyContainedImAddresses.Add (imString))
          {
            target.IMAddress += "; " + imString;
          }
        }
        else
        {
          target.IMAddress = imString;
        }
      }
    }
  }
}