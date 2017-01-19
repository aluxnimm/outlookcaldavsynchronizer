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
using System.Linq;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  public class OptionsViewModelFactory : IOptionsViewModelFactory
  {
    private readonly IOptionsViewModelParent _optionsViewModelParent;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly IReadOnlyList<string> _availableCategories;
    private readonly IOptionTasks _optionTasks;
    private readonly ISettingsFaultFinder _settingsFaultFinder;
    private readonly GeneralOptions _generalOptions;
    private readonly IViewOptions _viewOptions;


    public OptionsViewModelFactory (
      IOptionsViewModelParent optionsViewModelParent, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      IReadOnlyList<string> availableCategories, 
      IOptionTasks optionTasks,
      ISettingsFaultFinder settingsFaultFinder, 
      GeneralOptions generalOptions, 
      IViewOptions viewOptions)
    {
      if (optionsViewModelParent == null)
        throw new ArgumentNullException (nameof (optionsViewModelParent));
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (availableCategories == null)
        throw new ArgumentNullException (nameof (availableCategories));
      if (optionTasks == null) throw new ArgumentNullException(nameof(optionTasks));
      if (settingsFaultFinder == null) throw new ArgumentNullException(nameof(settingsFaultFinder));
      if (generalOptions == null) throw new ArgumentNullException(nameof(generalOptions));
      if (viewOptions == null) throw new ArgumentNullException(nameof(viewOptions));

      _optionsViewModelParent = optionsViewModelParent;
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _availableCategories = availableCategories;
      _optionTasks = optionTasks;
      _settingsFaultFinder = settingsFaultFinder;
      _generalOptions = generalOptions;
      _viewOptions = viewOptions;
    }

    public List<IOptionsViewModel> Create(IReadOnlyCollection<Contracts.Options> options)
    {
      if (options == null)
        throw new ArgumentNullException(nameof(options));
    
      return options
        .Select(o => Create(new OptionsModel(_settingsFaultFinder, _optionTasks, _outlookAccountPasswordProvider, o, _generalOptions)))
        .ToList();
    }

    public List<IOptionsViewModel> Create (IReadOnlyCollection<OptionsModel> options)
    {
      if (options == null)
        throw new ArgumentNullException (nameof (options));
  
      return options.Select (o => Create (o)).ToList();
    }

    public IOptionsViewModel CreateTemplate (Contracts.Options prototype)
    {
      var prototypeModel = new OptionsModel(_settingsFaultFinder, _optionTasks, _outlookAccountPasswordProvider, prototype, _generalOptions);
      var optionsViewModel = new MultipleOptionsTemplateViewModel(
        _optionsViewModelParent,
        prototypeModel.IsGoogle
          ? (IServerSettingsTemplateViewModel)new GoogleServerSettingsTemplateViewModel(_outlookAccountPasswordProvider, prototypeModel)
          : new ServerSettingsTemplateViewModel(_outlookAccountPasswordProvider, prototypeModel),
        _optionTasks,
        prototypeModel,
        _viewOptions);

      return optionsViewModel;
    }

    private IOptionsViewModel Create (OptionsModel model)
    {
      var optionsViewModel = new GenericOptionsViewModel (
          _optionsViewModelParent,
          model.IsGoogle 
            ? new GoogleServerSettingsViewModel(model, _optionTasks, _viewOptions)
            : (IOptionsSection) new ServerSettingsViewModel(model, _optionTasks, _viewOptions),
          _optionTasks,
          model,
          _availableCategories,
          _viewOptions);

      return optionsViewModel;
    }

  }
}