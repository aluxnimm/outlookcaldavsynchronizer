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
using System.Collections.Generic;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.Ui.Options.ProfileTypes
{
  public class ProfileTypeRegistry : IProfileTypeRegistry
  {
    private readonly GenericProfile _genericProfile;
    private readonly GoogleProfile _googleProfile;

    private ProfileTypeRegistry(IReadOnlyList<IProfileType> allTypes, GenericProfile genericProfile, GoogleProfile googleProfile)
    {
      _genericProfile = genericProfile;
      _googleProfile = googleProfile;
      AllTypes = allTypes;
    }

    public static IProfileTypeRegistry Create(
      IOptionsViewModelParent optionsViewModelParent,
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      IReadOnlyList<string> availableCategories,
      IOptionTasks optionTasks,
      ISettingsFaultFinder settingsFaultFinder,
      GeneralOptions generalOptions,
      IViewOptions viewOptions)
    {
      if (optionsViewModelParent == null) throw new ArgumentNullException(nameof(optionsViewModelParent));
      if (outlookAccountPasswordProvider == null) throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
      if (availableCategories == null) throw new ArgumentNullException(nameof(availableCategories));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (settingsFaultFinder == null) throw new ArgumentNullException(nameof(settingsFaultFinder));
      if (generalOptions == null) throw new ArgumentNullException(nameof(generalOptions));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));


      var generic = new GenericProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions);
      var google = new GoogleProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions);
      var all = new List<IProfileType> {generic, google};
      all.Add(new FruuxProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new PosteoProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new YandexProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new GmxCalendarProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new SarenetProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new LandmarksProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new SogoProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new CozyProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new NextcloudProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new MailboxOrgProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
      all.Add(new EasyProjectProfile(optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions));
    
      return new ProfileTypeRegistry(all, generic, google);
    }

    public IReadOnlyList<IProfileType> AllTypes { get; }

    public IProfileType DetermineType(Contracts.Options data)
    {
      if (GoogleProfile.IsGoogleProfile(data))
        return _googleProfile;
      else
        return _genericProfile;
    }
  }
}