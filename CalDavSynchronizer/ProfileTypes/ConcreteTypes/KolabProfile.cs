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
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes
{
  class KolabProfile : ProfileTypeBase
  {
    public override string Name => "Kolab";
    public override string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_kolab.png";
    private GeneralOptions _generalOptions;

    public override IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
    {
      _generalOptions = generalOptions;
      return new ProfileModelFactory(this, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData);
    }

    public override Contracts.Options CreateOptions()
    {
      _generalOptions.TriggerSyncAfterSendReceive = true;  // Synchronize items when syncing IMAP or sending mail
      new GeneralOptionsDataAccess().SaveOptions(_generalOptions);

      var data = base.CreateOptions();
      data.CalenderUrl = "https://kolab.coreboso.de/iRony/";
      data.EnableChangeTriggeredSynchronization = true;   // Synchronize items immediately after change
      data.DaysToSynchronizeInThePast = 31;               // Start syncing one month ago
      data.DaysToSynchronizeInTheFuture = 365;            // Sync up to one year.
      return data;
    }

    public override EventMappingConfiguration CreateEventMappingConfiguration()
    {
      var data = base.CreateEventMappingConfiguration();
      // data.UseGlobalAppointmentID = true;
      // data.UseIanaTz = true;
      data.MapXAltDescToRtfBody = true;
      data.MapRtfBodyToXAltDesc = true;
      return data;
    }

    class ProfileModelFactory : ProfileModelFactoryBase
    {
      public ProfileModelFactory(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
        : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData)
      {
      }

      protected override IOptionsViewModel CreateTemplateViewModel(OptionsModel prototypeModel)
      {
        return new KolabMultipleOptionsTemplateViewModel(
          OptionsViewModelParent,
          new ServerSettingsTemplateViewModel(OutlookAccountPasswordProvider, prototypeModel, ModelOptions),
          OptionTasks,
          prototypeModel,
          ViewOptions);
      }

      public override ProfileModelOptions ModelOptions { get; } = new ProfileModelOptions(true, true, true, true, Strings.Get($"Kolab URL"), true, true, true);
    }
  }
}