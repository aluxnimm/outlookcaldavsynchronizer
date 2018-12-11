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
using CalDavSynchronizer.Globalization;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.ProfileTypes
{
  public abstract class ProfileModelFactoryBase : IProfileModelFactory
  {
    protected readonly IOptionsViewModelParent OptionsViewModelParent;
    protected readonly IOutlookAccountPasswordProvider OutlookAccountPasswordProvider;
    protected readonly IReadOnlyList<string> AvailableCategories;
    protected readonly IOptionTasks OptionTasks;
    protected readonly ISettingsFaultFinder SettingsFaultFinder;
    protected readonly GeneralOptions GeneralOptions;
    protected readonly IViewOptions ViewOptions;
    protected readonly OptionModelSessionData SessionData;

    protected ProfileModelFactoryBase(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
    {
      if (profileType == null) throw new ArgumentNullException(nameof(profileType));
      if (optionsViewModelParent == null) throw new ArgumentNullException(nameof(optionsViewModelParent));
      if (outlookAccountPasswordProvider == null) throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
      if (availableCategories == null) throw new ArgumentNullException(nameof(availableCategories));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (settingsFaultFinder == null) throw new ArgumentNullException(nameof(settingsFaultFinder));
      if (generalOptions == null) throw new ArgumentNullException(nameof(generalOptions));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));
      if (sessionData == null) throw new ArgumentNullException(nameof(sessionData));

      ProfileType = profileType;
      OptionsViewModelParent = optionsViewModelParent;
      OutlookAccountPasswordProvider = outlookAccountPasswordProvider;
      AvailableCategories = availableCategories;
      OptionTasks = optionTasks;
      SettingsFaultFinder = settingsFaultFinder;
      GeneralOptions = generalOptions;
      ViewOptions = viewOptions;
      SessionData = sessionData;
      ServerSettingsDetector = new Lazy<IServerSettingsDetector>(CreateServerSettingsDetector);
    }

    public IProfileType ProfileType { get; }

    protected Lazy<IServerSettingsDetector> ServerSettingsDetector { get; }
    protected virtual IServerSettingsDetector CreateServerSettingsDetector() => new ServerSettingsDetector(OutlookAccountPasswordProvider);

    public OptionsModel CreateModelFromData(Contracts.Options data)
    {
      return CreateModel(data);
    }

    protected virtual OptionsModel CreateModel(Contracts.Options data)
    {
      return new OptionsModel(SettingsFaultFinder, OptionTasks, OutlookAccountPasswordProvider, data, GeneralOptions, this, false, SessionData, ServerSettingsDetector.Value);
    }
    
    protected virtual OptionsModel CreatePrototypeModel(Contracts.Options data)
    {
      return new OptionsModel(SettingsFaultFinder, OptionTasks, OutlookAccountPasswordProvider, data, GeneralOptions, this, false, SessionData, ServerSettingsDetector.Value);
    }

    public virtual IOptionsViewModel CreateViewModel(OptionsModel model)
    {
      var optionsViewModel = new GenericOptionsViewModel(
        OptionsViewModelParent,
        CreateServerSettingsViewModel(model),
        OptionTasks,
        model,
        AvailableCategories,
        ViewOptions);

      return optionsViewModel;
    }

    protected virtual ServerSettingsViewModel CreateServerSettingsViewModel(OptionsModel model)
    {
      return new ServerSettingsViewModel(model, OptionTasks, ViewOptions);
    }

    public IOptionsViewModel CreateTemplateViewModel()
    {
      var data = ProfileType.CreateOptions();
      data.Name = ProfileType.Name;
      var prototypeModel = CreateModel(data);
      var optionsViewModel = CreateTemplateViewModel(prototypeModel);

      return optionsViewModel;
    }

    public virtual ProfileModelOptions ModelOptions { get; } = new ProfileModelOptions(true, true, true, true, Strings.Get($"DAV URL"), true, true, true);

    protected virtual IOptionsViewModel CreateTemplateViewModel(OptionsModel prototypeModel)
    {
      var optionsViewModel = new MultipleOptionsTemplateViewModel(
        OptionsViewModelParent,
        CreateServerSettingsTemplateViewModel(prototypeModel),
        OptionTasks,
        prototypeModel,
        ViewOptions);
      return optionsViewModel;
    }

    protected virtual IServerSettingsTemplateViewModel CreateServerSettingsTemplateViewModel(OptionsModel prototypeModel)
    {
      return new ServerSettingsTemplateViewModel(OutlookAccountPasswordProvider, prototypeModel, ModelOptions);
    }
  }
}