using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Utilities
{
  public static class SecureStringUtility
  {
    public static string ToUnsecureString (SecureString secureString)
    {
      IntPtr unmanagedString = IntPtr.Zero;
      try
      {
        unmanagedString = Marshal.SecureStringToGlobalAllocUnicode (secureString);
        return Marshal.PtrToStringUni (unmanagedString);
      }
      finally
      {
        Marshal.ZeroFreeGlobalAllocUnicode (unmanagedString);
      }
    }

    public static SecureString ToSecureString (string value)
    {
      var result = new SecureString ();
      foreach (var c in value)
        result.AppendChar (c);
      result.MakeReadOnly ();
      return result;
    }
  }
}