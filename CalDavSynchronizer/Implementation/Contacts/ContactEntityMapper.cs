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
      return target;
    }

    public GenericComObjectWrapper<ContactItem> Map2To1 (vCard source, GenericComObjectWrapper<ContactItem> target)
    {
      target.Inner.FirstName = source.GivenName;
      target.Inner.LastName = source.FamilyName;
      target.Inner.Title = source.NamePrefix;
      target.Inner.Suffix = source.NameSuffix;
      target.Inner.MiddleName = source.AdditionalNames;

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

      for (int i = 0; i < source.DeliveryAddresses.Count;i++)
      {
        if (source.DeliveryAddresses[i].IsHome)
        {
          target.Inner.HomeAddressCity = source.DeliveryAddresses[i].City;
          target.Inner.HomeAddressCountry = source.DeliveryAddresses[i].Country;
          target.Inner.HomeAddressPostalCode= source.DeliveryAddresses[i].PostalCode;
          target.Inner.HomeAddressState = source.DeliveryAddresses[i].Region;
          target.Inner.HomeAddressStreet = source.DeliveryAddresses[i].Street;
          if (source.DeliveryAddresses[i].IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olHome;
          }
        }
        else if (source.DeliveryAddresses[i].IsWork)
        {
          target.Inner.BusinessAddressCity = source.DeliveryAddresses[i].City;
          target.Inner.BusinessAddressCountry = source.DeliveryAddresses[i].Country;
          target.Inner.BusinessAddressPostalCode = source.DeliveryAddresses[i].PostalCode;
          target.Inner.BusinessAddressState = source.DeliveryAddresses[i].Region;
          target.Inner.BusinessAddressStreet = source.DeliveryAddresses[i].Street;
          if (source.DeliveryAddresses[i].IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olBusiness;
          }
        }
        else
        {
          target.Inner.OtherAddressCity = source.DeliveryAddresses[i].City;
          target.Inner.OtherAddressCountry = source.DeliveryAddresses[i].Country;
          target.Inner.OtherAddressPostalCode = source.DeliveryAddresses[i].PostalCode;
          target.Inner.OtherAddressState = source.DeliveryAddresses[i].Region;
          target.Inner.OtherAddressStreet = source.DeliveryAddresses[i].Street;
          if (source.DeliveryAddresses[i].IsPreferred)
          {
            target.Inner.SelectedMailingAddress = OlMailingAddress.olOther;
          }
        }
      }
      
      return target;
    }
  }
}