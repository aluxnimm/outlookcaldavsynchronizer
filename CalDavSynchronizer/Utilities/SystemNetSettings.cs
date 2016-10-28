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
using System.Net.Configuration;
using System.Reflection;
using log4net;

namespace CalDavSynchronizer.Utilities
{
  public static class SystemNetSettings
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    // Code based on
    // https://social.msdn.microsoft.com/Forums/en-US/ff098248-551c-4da9-8ba5-358a9f8ccc57/how-do-i-enable-useunsafeheaderparsing-from-code-net-20?forum=netfxnetcom

    public static bool UseUnsafeHeaderParsing
    {
      get
      {
        try
        {
          Assembly assembly = Assembly.GetAssembly (typeof (SettingsSection));
          if (assembly != null)
          {
            //Use the assembly in order to get the internal type for the internal class
            Type settingsSectionType = assembly.GetType ("System.Net.Configuration.SettingsSectionInternal");
            if (settingsSectionType != null)
            {
              //Use the internal static property to get an instance of the internal settings class.
              //If the static instance isn't created already invoking the property will create it for us.
              object anInstance = settingsSectionType.InvokeMember ("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] {});
              if (anInstance != null)
              {
                //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField ("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                if (aUseUnsafeHeaderParsing != null)
                {
                  return (bool) aUseUnsafeHeaderParsing.GetValue (anInstance);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          s_logger.Warn ("Could not read useUnsafeHeaderParsing in System.Net.Configuration", ex);
        }
        return false;
      }
      set
      {
        try
        {
          //Get the assembly that contains the internal class
          Assembly assembly = Assembly.GetAssembly (typeof (SettingsSection));
          if (assembly != null)
          {
            //Use the assembly in order to get the internal type for the internal class
            Type settingsSectionType = assembly.GetType ("System.Net.Configuration.SettingsSectionInternal");
            if (settingsSectionType != null)
            {
              //Use the internal static property to get an instance of the internal settings class.
              //If the static instance isn't created already invoking the property will create it for us.
              object anInstance = settingsSectionType.InvokeMember ("Section", BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] {});
              if (anInstance != null)
              {
                //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField ("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                if (aUseUnsafeHeaderParsing != null)
                {
                  aUseUnsafeHeaderParsing.SetValue (anInstance, value);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          s_logger.Warn ("Could not set useUnsafeHeaderParsing in System.Net.Configuration", ex);
        }
      }
    }
  }
}
   

