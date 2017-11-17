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
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.ProfileTypes
{
  class EasyProjectProfile : IProfileType
  {
    public string Name => "EasyProject";
    public string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_easyproject.png";

    public IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
    {
      return new ProfileModelFactory(this, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData);
    }

    class ProfileModelFactory : ProfileModelFactoryBase
    {
      public ProfileModelFactory(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
        : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData)
      {
      }

      protected override void InitializeData(Contracts.Options data)
      {
        data.CalenderUrl = "https://demo.easyredmine.com/caldav/";
        data.EnableChangeTriggeredSynchronization = true;
        data.DaysToSynchronizeInThePast = 7;
        data.DaysToSynchronizeInTheFuture = 180;
        data.MappingConfiguration = new EventMappingConfiguration
        {
          UseGlobalAppointmentID = true,
          UseIanaTz = true,
          MapXAltDescToRtfBody = true,
          MapRtfBodyToXAltDesc = true
        };
      }

      protected override void InitializePrototypeData(Contracts.Options data)
      {
        InitializeData(data);
      }

      protected override IOptionsViewModel CreateTemplateViewModel(OptionsModel prototypeModel)
      {
        return new EasyProjectMultipleOptionsTemplateViewModel(
          OptionsViewModelParent,
          new EasyProjectServerSettingsTemplateViewModel(OutlookAccountPasswordProvider, prototypeModel),
          OptionTasks,
          prototypeModel,
          ViewOptions);
      }
    }
  }
}