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
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using log4net;
using Microsoft.Win32;

namespace CalDavSynchronizer
{
  public class OutlookAccountPasswordProvider : IOutlookAccountPasswordProvider
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);
    private readonly string _profileRegistryKeyName;

    public OutlookAccountPasswordProvider (string outlookProfileName, string outlookVersion)
    {
      if (string.IsNullOrEmpty (outlookProfileName))
        throw new ArgumentException ("Argument is null or empty", nameof (outlookProfileName));
      if (string.IsNullOrEmpty (outlookVersion))
        throw new ArgumentException ("Argument is null or empty", nameof (outlookVersion));

      var outlookVersions = outlookVersion.Split ('.');
      var outlookVersionInRegistryFormat = outlookVersions[0] + "." + outlookVersions[1];
      var outlookMajorVersion = Convert.ToInt32 (outlookVersions[0]);

      if (outlookMajorVersion < 15)
      {
        _profileRegistryKeyName = @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles\" + outlookProfileName +
                                  @"\9375CFF0413111d3B88A00104B2A6676";
      }
      else
      {
        _profileRegistryKeyName = @"Software\Microsoft\Office\" + outlookVersionInRegistryFormat + @"\Outlook\Profiles\" + outlookProfileName +
                                  @"\9375CFF0413111d3B88A00104B2A6676";
      }
    }

    string ConvertRegistryValueToString (object value)
    {
      if (value == null)
        return null;
      
      var binaryValue =  value as byte[];
      if(binaryValue != null)
        return Encoding.Unicode.GetString (binaryValue).TrimEnd ('\0');

      return value as string;
    }

    public string GetPassword (string accountNameOrNull)
    {
      try
      {
        using (RegistryKey profileKey = Registry.CurrentUser.OpenSubKey (_profileRegistryKeyName))
        {
          foreach (string subKeyName in profileKey.GetSubKeyNames())
          {
            using (RegistryKey subKey = profileKey.OpenSubKey (subKeyName))
            {
              if (accountNameOrNull != null)
              {
                var registryAccountName = ConvertRegistryValueToString (subKey.GetValue ("Account Name"));
                if (registryAccountName != accountNameOrNull)
                  continue;
              }

              var passwordValue = (byte[]) subKey.GetValue ("IMAP Password") ?? (byte[]) subKey.GetValue ("POP3 Password");

              if (passwordValue == null)
                continue;
              var encPassword = passwordValue.Skip (1).ToArray();
              try
              {
                var clearPassword = ProtectedData.Unprotect (encPassword, null, DataProtectionScope.CurrentUser);
                string result = Encoding.Unicode.GetString (clearPassword).TrimEnd ('\0');
                return result;
              }
              catch (CryptographicException x)
              {
                s_logger.Error ("Error while decrypting account password. Using empty password", x);
                return string.Empty;
              }
            }
          }
          return string.Empty;
        }
      }
      catch (Exception ex)
      {
        s_logger.Error ("Error while fetching account password from registry. Using empty password", ex);
        return string.Empty;
      }
    }
  }
}