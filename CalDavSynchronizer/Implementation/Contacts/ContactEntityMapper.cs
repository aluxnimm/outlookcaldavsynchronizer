// This file is Part of CalDavSynchronizer (http://outlookcaldavsynchronizer.sourceforge.net/)
// Copyright (c) 2015 Gerhard Zehetbauer 
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
using CalDavSynchronizer.Implementation.ComWrappers;
using DDay.iCal;
using GenSync.EntityMapping;
using Microsoft.Office.Interop.Outlook;
using Thought.vCards;

namespace CalDavSynchronizer.Implementation.Contacts
{
  public class ContactEntityMapper : IEntityMapper<GenericComObjectWrapper<ContactItem>, vCard>
  {
    public vCard Map1To2 (GenericComObjectWrapper<ContactItem> source, vCard target)
    {
      target.GivenName = source.Inner.FirstName;
      target.FamilyName = source.Inner.LastName;
      target.NamePrefix = source.Inner.Title;
      target.NameSuffix = source.Inner.Suffix;
      target.AdditionalNames = source.Inner.MiddleName;
      target.Gender = MapGender2To1 (source.Inner.Gender);

      if (!string.IsNullOrEmpty(source.Inner.NickName))
      {
        Array.ForEach(
            source.Inner.NickName.Split(new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Nicknames.Add(c)
            );
      }

      if (!string.IsNullOrEmpty(source.Inner.Categories))
      {
        Array.ForEach(
            source.Inner.Categories.Split(new[] { CultureInfo.CurrentCulture.TextInfo.ListSeparator }, StringSplitOptions.RemoveEmptyEntries),
            c => target.Categories.Add(c)
            );
      }

      if (!string.IsNullOrEmpty(source.Inner.Email1Address))
      {
        target.EmailAddresses.Add(new vCardEmailAddress(source.Inner.Email1Address));
      }
      if (!string.IsNullOrEmpty(source.Inner.Email2Address))
      {
        target.EmailAddresses.Add(new vCardEmailAddress(source.Inner.Email2Address));
      }
      if (!string.IsNullOrEmpty(source.Inner.Email3Address))
      {
        target.EmailAddresses.Add(new vCardEmailAddress(source.Inner.Email3Address));
      }

      if (!string.IsNullOrEmpty(source.Inner.HomeAddress))
      {
        vCardDeliveryAddress homeAddress = new vCardDeliveryAddress();
        homeAddress.AddressType.Add(vCardDeliveryAddressTypes.Home);
        homeAddress.City = source.Inner.HomeAddressCity;
        homeAddress.Country = source.Inner.HomeAddressCountry;
        homeAddress.PostalCode = source.Inner.HomeAddressPostalCode;
        homeAddress.Region = source.Inner.HomeAddressState;
        homeAddress.Street = source.Inner.HomeAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olHome)
        {
          homeAddress.AddressType.Add(vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add(homeAddress);
      }

      if (!string.IsNullOrEmpty(source.Inner.BusinessAddress))
      {
        vCardDeliveryAddress businessAddress = new vCardDeliveryAddress();
        businessAddress.AddressType.Add(vCardDeliveryAddressTypes.Work);
        businessAddress.City = source.Inner.BusinessAddressCity;
        businessAddress.Country = source.Inner.BusinessAddressCountry;
        businessAddress.PostalCode = source.Inner.BusinessAddressPostalCode;
        businessAddress.Region = source.Inner.BusinessAddressState;
        businessAddress.Street = source.Inner.BusinessAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olBusiness)
        {
          businessAddress.AddressType.Add(vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add(businessAddress);
      }

      if (!string.IsNullOrEmpty(source.Inner.OtherAddress))
      {
        vCardDeliveryAddress otherAddress = new vCardDeliveryAddress();
        otherAddress.AddressType.Add(vCardDeliveryAddressTypes.Postal);
        otherAddress.City = source.Inner.OtherAddressCity;
        otherAddress.Country = source.Inner.OtherAddressCountry;
        otherAddress.PostalCode = source.Inner.OtherAddressPostalCode;
        otherAddress.Region = source.Inner.OtherAddressState;
        otherAddress.Street = source.Inner.OtherAddressStreet;
        if (source.Inner.SelectedMailingAddress == OlMailingAddress.olOther)
        {
          otherAddress.AddressType.Add(vCardDeliveryAddressTypes.Preferred);
        }
        target.DeliveryAddresses.Add(otherAddress);
      }

      MapPhoneNumbers1to2 (source.Inner, target);

      if (!source.Inner.Birthday.Equals(new DateTime(4501, 1, 1, 0, 0, 0)))
      {
        // TODO: Workaround for serializer needed, BirthDate must be YYYY-MM-DD
        target.BirthDate = source.Inner.Birthday.Date;
      }

      target.Department = source.Inner.Department;
      target.Organization = source.Inner.CompanyName;
      target.Title = source.Inner.JobTitle;
      target.Office = source.Inner.OfficeLocation;

      if (!string.IsNullOrEmpty(source.Inner.WebPage))
      {
        target.Websites.Add(new vCardWebsite (source.Inner.WebPage));
      }
      if (!string.IsNullOrEmpty(source.Inner.PersonalHomePage))
      {
        target.Websites.Add(new vCardWebsite (source.Inner.PersonalHomePage, vCardWebsiteTypes.Personal));
      }
      if (!string.IsNullOrEmpty(source.Inner.BusinessHomePage))
      {
        target.Websites.Add(new vCardWebsite (source.Inner.BusinessHomePage, vCardWebsiteTypes.Work));
      }

      return target;
    }

    public GenericComObjectWrapper<ContactItem> Map2To1 (vCard source, GenericComObjectWrapper<ContactItem> target)
    {
      target.Inner.FirstName = source.GivenName;
      target.Inner.LastName = source.FamilyName;
      target.Inner.Title = source.NamePrefix;
      target.Inner.Suffix = source.NameSuffix;
      target.Inner.MiddleName = source.AdditionalNames;
      target.Inner.Gender = MapGender1To2 (source.Gender);

      if (source.Nicknames.Count >0)
      {
        string[] nickNames = new string[source.Nicknames.Count];
        source.Nicknames.CopyTo(nickNames,0);
        target.Inner.NickName = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, nickNames);
      }

      if (source.Categories.Count > 0)
      {
        string[] categories = new string[source.Categories.Count];
        source.Categories.CopyTo(categories, 0);
        target.Inner.Categories = string.Join(CultureInfo.CurrentCulture.TextInfo.ListSeparator, categories);
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

      MapTelephoneNumber2to1 (source, target.Inner);

      target.Inner.Birthday = source.BirthDate ?? new DateTime(4501, 1, 1);

      target.Inner.Department = source.Department;
      target.Inner.CompanyName = source.Organization;
      target.Inner.JobTitle = source.Title;
      target.Inner.OfficeLocation = source.Office;

      vCardWebsite sourceWebSite;

      if ((sourceWebSite = source.Websites.GetFirstChoice(vCardWebsiteTypes.Default)) != null)
      {
        target.Inner.WebPage = sourceWebSite.Url;
      }
      vCardWebsite sourceHomePage;

      if ((sourceHomePage = source.Websites.GetFirstChoice(vCardWebsiteTypes.Personal)) != null)
      {
        target.Inner.PersonalHomePage = sourceHomePage.Url;
      }
      vCardWebsite sourceBusinessHomePage;

      if ((sourceBusinessHomePage = source.Websites.GetFirstChoice(vCardWebsiteTypes.Work)) != null)
      {
        target.Inner.BusinessHomePage = sourceBusinessHomePage.Url;
      }

      return target;
    }

    private static OlGender MapGender1To2(vCardGender sourceGender)
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

      throw new NotImplementedException(string.Format("Mapping for value '{0}' not implemented.", sourceGender));
    }

    private static vCardGender MapGender2To1(OlGender sourceGender)
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

      throw new NotImplementedException(string.Format("Mapping for value '{0}' not implemented.", sourceGender));
    }

    private static void MapPhoneNumbers1to2 (ContactItem source, vCard target)
    {
      if (!string.IsNullOrEmpty(source.PrimaryTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.PrimaryTelephoneNumber, vCardPhoneTypes.Main);
        phoneNumber.IsPreferred = true;
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.MobileTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.MobileTelephoneNumber, vCardPhoneTypes.Cellular);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.HomeTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.HomeTelephoneNumber, vCardPhoneTypes.Home);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.Home2TelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.Home2TelephoneNumber, vCardPhoneTypes.HomeVoice);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.HomeFaxNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.HomeFaxNumber, vCardPhoneTypes.Fax);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.BusinessTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.BusinessTelephoneNumber, vCardPhoneTypes.Work);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.Business2TelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.Business2TelephoneNumber, vCardPhoneTypes.WorkVoice);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.BusinessFaxNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.BusinessFaxNumber, vCardPhoneTypes.WorkFax);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.PagerNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.PagerNumber, vCardPhoneTypes.Pager);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.CarTelephoneNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.CarTelephoneNumber, vCardPhoneTypes.Car);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
      if (!string.IsNullOrEmpty(source.ISDNNumber))
      {
        vCardPhone phoneNumber = new vCardPhone(source.ISDNNumber, vCardPhoneTypes.ISDN);
        phoneNumber.IsPreferred = (target.Phones.Count == 0);
        target.Phones.Add(phoneNumber);
      }
    }

    private static void MapTelephoneNumber2to1 (vCard source, ContactItem target)
    {
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
        else if (phoneNumber.IsHome)
        {
          target.HomeTelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.PhoneType == vCardPhoneTypes.HomeVoice)
        {
          target.Home2TelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.PhoneType == vCardPhoneTypes.Work)
        {
          target.BusinessTelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.PhoneType == vCardPhoneTypes.WorkVoice)
        {
          target.Business2TelephoneNumber = phoneNumber.FullNumber;
        }
        else if (phoneNumber.PhoneType == vCardPhoneTypes.WorkFax)
        {
          target.BusinessFaxNumber = phoneNumber.FullNumber;
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
      }

    }
  }
}