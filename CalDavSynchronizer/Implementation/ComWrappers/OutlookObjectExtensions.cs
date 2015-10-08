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