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

namespace CalDavSynchronizer.ProfileTypes
{
  public class ProfileModelOptions
  {
    public ProfileModelOptions(bool areAdvancedNetWorkSettingsEnabled, bool isEnableChangeTriggeredSynchronizationEnabled, bool isTaskMappingConfigurationEnabled, bool isContactMappingConfigurationEnabled, string davUrlLabelText, bool areSyncSettingsEnabled, bool areSyncSettingsVisible, bool isEnableChangeTriggeredSynchronizationVisible)
    {
      AreAdvancedNetWorkSettingsEnabled = areAdvancedNetWorkSettingsEnabled;
      IsEnableChangeTriggeredSynchronizationEnabled = isEnableChangeTriggeredSynchronizationEnabled;
      IsTaskMappingConfigurationEnabled = isTaskMappingConfigurationEnabled;
      IsContactMappingConfigurationEnabled = isContactMappingConfigurationEnabled;
      DavUrlLabelText = davUrlLabelText;
      AreSyncSettingsEnabled = areSyncSettingsEnabled;
      AreSyncSettingsVisible = areSyncSettingsVisible;
      IsEnableChangeTriggeredSynchronizationVisible = isEnableChangeTriggeredSynchronizationVisible;
    }

    public bool AreSyncSettingsVisible { get; }
    public bool AreSyncSettingsEnabled { get; }
    public bool AreAdvancedNetWorkSettingsEnabled { get; }
    public bool IsEnableChangeTriggeredSynchronizationVisible { get; }
    public bool IsEnableChangeTriggeredSynchronizationEnabled { get; }
    public bool IsContactMappingConfigurationEnabled { get; }
    public bool IsTaskMappingConfigurationEnabled { get; }
    public String DavUrlLabelText { get; }
  }
}