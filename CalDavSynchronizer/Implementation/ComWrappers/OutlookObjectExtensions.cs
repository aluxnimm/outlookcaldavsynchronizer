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
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Implementation.ComWrappers
{
  internal static class OutlookObjectExtensions
  {
    public static dynamic GetPropertySafe (this AppointmentItem a, string propertyName)
    {
      return a.PropertyAccessor.GetPropertySafe (propertyName);
    }

    public static dynamic GetPropertySafe (this AddressEntry a, string propertyName)
    {
      return a.PropertyAccessor.GetPropertySafe (propertyName);
    }

    public static dynamic GetPropertySafe (this ContactItem a, string propertyName)
    {
      return a.PropertyAccessor.GetPropertySafe (propertyName);
    }

    private static dynamic GetPropertySafe (this PropertyAccessor accessor, string propertyName)
    {
      using (var wrapper = GenericComObjectWrapper.Create (accessor))
      {
        return wrapper.Inner.GetProperty (propertyName);
      }
    }
  }
}