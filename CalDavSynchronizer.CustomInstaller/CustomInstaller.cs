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
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CalDavSynchronizer.CustomInstaller
{
    [RunInstaller(true)]
    public partial class CustomInstaller : System.Configuration.Install.Installer
    {
        public CustomInstaller()
        {
            InitializeComponent();
        }

        private bool IsAllUsersInstall()
        {
            // An ALLUSERS property value of 1 specifies the per-machine installation context.
            // An ALLUSERS property value of an empty string ("") specifies the per-user installation context.

            // In the custom action data, we have mapped the parameter 'AllUsers' to ALLUSERS.
            string s = base.Context.Parameters["AllUsers"];
            if (s == null)
                return true;
            else if (s == string.Empty)
                return false;
            else
                return true;
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            if (IsAllUsersInstall() && Environment.Is64BitOperatingSystem)
            {
                const string addinKeyPath = "SOFTWARE\\Microsoft\\Office\\Outlook\\Addins\\CalDavSynchronizer.1";

                try
                {
                    RegistryKey localKey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    RegistryKey localKey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    RegistryKey key32 = localKey32.OpenSubKey(addinKeyPath);
                    if (key32 != null)
                    {
                        RegistryKey key64 = localKey64.OpenSubKey(addinKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                        if (key64 == null)
                        {
                            localKey64.CreateSubKey(addinKeyPath);
                            key64 = localKey64.OpenSubKey(addinKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                        }

                        if (key64 != null)
                        {
                            foreach (var name in key32.GetValueNames())
                            {
                                key64.SetValue(name, key32.GetValue(name), key32.GetValueKind(name));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InstallException("Can't add registry key in HKLM", ex);
                }
            }
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Commit(IDictionary savedState)
        {
            base.Commit(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand)]
        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);


            if (IsAllUsersInstall() && Environment.Is64BitOperatingSystem)
            {
                const string addinKeyPath = "SOFTWARE\\Microsoft\\Office\\Outlook\\Addins";
                const string addinKeyName = "CalDavSynchronizer.1";

                try
                {
                    RegistryKey localKey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    RegistryKey key64 = localKey64.OpenSubKey(addinKeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    if (key64 != null)
                    {
                        if (key64.OpenSubKey(addinKeyName) != null)
                            key64.DeleteSubKey(addinKeyName);
                    }
                }
                catch (Exception ex)
                {
                    throw new InstallException("Can't remove registry key in HKLM", ex);
                }
            }
        }
    }
}