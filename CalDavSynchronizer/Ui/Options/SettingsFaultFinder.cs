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
using System.Windows.Forms;
using CalDavSynchronizer.Implementation;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Ui.Options.ViewModels;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options
{
  public class SettingsFaultFinder : ISettingsFaultFinder
  {
    private readonly ISyncSettingsControl _syncSettingsControl;

    public SettingsFaultFinder (ISyncSettingsControl syncSettingsControl)
    {
      if (syncSettingsControl == null)
        throw new ArgumentNullException (nameof (syncSettingsControl));

      _syncSettingsControl = syncSettingsControl;
    }


    public void FixSynchronizationMode (TestResult result)
    {
      const SynchronizationMode readOnlyDefaultMode = SynchronizationMode.ReplicateServerIntoOutlook;
      if (result.ResourceType.HasFlag (ResourceType.Calendar))
      {
        if (!result.CalendarProperties.HasFlag (CalendarProperties.IsWriteable)
            && OptionTasks.DoesModeRequireWriteableServerResource(_syncSettingsControl.SynchronizationMode))
        {
          _syncSettingsControl.SynchronizationMode = readOnlyDefaultMode;
          MessageBox.Show (
              string.Format (
                  "The specified Url is a read-only calendar. Synchronization mode set to '{0}'.",
                  _syncSettingsControl.AvailableSynchronizationModes.Single (m => m.Value == readOnlyDefaultMode).Name),
              OptionTasks.ConnectionTestCaption);
        }
      }

      if (result.ResourceType.HasFlag (ResourceType.AddressBook))
      {
        if (!result.AddressBookProperties.HasFlag (AddressBookProperties.IsWriteable)
            && OptionTasks.DoesModeRequireWriteableServerResource(_syncSettingsControl.SynchronizationMode))
        {
          _syncSettingsControl.SynchronizationMode = readOnlyDefaultMode;
          MessageBox.Show (
              string.Format (
                  "The specified Url is a read-only addressbook. Synchronization mode set to '{0}'.",
                  _syncSettingsControl.AvailableSynchronizationModes.Single (m => m.Value == readOnlyDefaultMode).Name),
              OptionTasks.ConnectionTestCaption);
        }
      }
    }

   
    public void FixTimeRangeUsage (OlItemType? folderType)
    {
      _syncSettingsControl.UseSynchronizationTimeRange = folderType != OlItemType.olContactItem;
    }
  }


}