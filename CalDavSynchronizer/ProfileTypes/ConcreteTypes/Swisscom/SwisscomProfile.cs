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

using System.Collections.Generic;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Swisscom
{
  class SwisscomProfile : ProfileTypeBase
  {
    public override string Name => "Swisscom";
    public override string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_swisscom.png";

    public override IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
    {
      return new ProfileModelFactory(this, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData);
    }

    public override Options CreateOptions()
    {
      var data = base.CreateOptions();
      data.PreemptiveAuthentication = false;
      data.SynchronizationMode = Implementation.SynchronizationMode.MergeInBothDirections;
      data.SynchronizationIntervalInMinutes = 30;
      data.IsChunkedSynchronizationEnabled = true;
      data.ChunkSize = 100;
      data.ForceBasicAuthentication = true;
      data.CloseAfterEachRequest = true;
      data.PreemptiveAuthentication = true;
      data.EnableChangeTriggeredSynchronization = true;
      data.ServerAdapterType = ServerAdapterType.WebDavHttpClientBased;

      return data;
    }

    class ProfileModelFactory : ProfileModelFactoryBase
    {
      public ProfileModelFactory(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
        : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData)
      {
      }

      protected override ServerSettingsViewModel CreateServerSettingsViewModel(OptionsModel model)
      {
        return new SwisscomServerSettingsViewModel(model, OptionTasks, ViewOptions);
      }

      protected override IServerSettingsDetector CreateServerSettingsDetector()
      {
        return new SwisscomServerSettingsDetector();
      }

      public override ProfileModelOptions ModelOptions { get; } = new ProfileModelOptions(false, false, false, false, Strings.Get($"Detected URL"), false, false, false);
    }
  }
}