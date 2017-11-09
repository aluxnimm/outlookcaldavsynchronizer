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
using CalDavSynchronizer.DataAccess;

namespace CalDavSynchronizer.Ui.Options.ProfileTypes
{
  class KolabProfile : ProfileBase
  {
    public KolabProfile(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions) : base(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions)
    {
    }

    public override string Name => "Kolab";
    public override string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_kolab.png";

    protected override void InitializeData(Contracts.Options data)
    {
      GeneralOptions.TriggerSyncAfterSendReceive = true;  // Synchronize items when syncing IMAP or sending mail
      new GeneralOptionsDataAccess().SaveOptions(GeneralOptions);

      data.CalenderUrl = "https://kolab.coreboso.de/iRony/";
      data.EnableChangeTriggeredSynchronization = true;   // Synchronize items immediately after change
      data.DaysToSynchronizeInThePast = 31;               // Start syncing one month ago
      data.DaysToSynchronizeInTheFuture = 365;            // Sync up to one year.
    }

    protected override void InitializePrototypeData(Contracts.Options data)
    {
      InitializeData(data);
    }

    protected override IOptionsViewModel CreateTemplateViewModel(OptionsModel prototypeModel)
    {
      return new KolabMultipleOptionsTemplateViewModel(
       OptionsViewModelParent,
       new KolabServerSettingsTemplateViewModel(OutlookAccountPasswordProvider, prototypeModel),
       OptionTasks,
       prototypeModel,
       ViewOptions);
    }
  }
}