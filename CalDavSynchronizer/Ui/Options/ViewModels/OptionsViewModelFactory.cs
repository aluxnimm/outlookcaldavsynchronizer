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
using CalDavSynchronizer.Ui.Options.ViewModels.Mapping;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.ViewModels
{
  internal class OptionsViewModelFactory : IOptionsViewModelFactory
  {
    private readonly IOptionsViewModelParent _optionsViewModelParent;
    private readonly NameSpace _session;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly IReadOnlyList<string> _availableCategories;

    public OptionsViewModelFactory (
      NameSpace session, 
      IOptionsViewModelParent optionsViewModelParent, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      IReadOnlyList<string> availableCategories)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (optionsViewModelParent == null)
        throw new ArgumentNullException (nameof (optionsViewModelParent));
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (availableCategories == null)
        throw new ArgumentNullException (nameof (availableCategories));

      _optionsViewModelParent = optionsViewModelParent;
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _availableCategories = availableCategories;
      _session = session;
    }

    public List<IOptionsViewModel> Create (IReadOnlyCollection<Contracts.Options> options, GeneralOptions generalOptions)
    {
      if (options == null)
        throw new ArgumentNullException (nameof (options));
      if (generalOptions == null)
        throw new ArgumentNullException (nameof (generalOptions));

      return options.Select (o => Create (o, generalOptions)).ToList();
    }

    public IOptionsViewModel CreateTemplate (Contracts.Options options, GeneralOptions generalOptions, ProfileType type)
    {
      var optionsViewModel = new MultipleOptionsTemplateViewModel (
         _session,
         _optionsViewModelParent,
         generalOptions,
         IsGoogleProfile (options)
             ? (IServerSettingsTemplateViewModel) new GoogleServerSettingsTemplateViewModel (_outlookAccountPasswordProvider)
             : new ServerSettingsTemplateViewModel (_outlookAccountPasswordProvider),
         type);

      optionsViewModel.SetOptions (options);
      return optionsViewModel;
    }

    private IOptionsViewModel Create (CalDavSynchronizer.Contracts.Options options, GeneralOptions generalOptions)
    {

      var optionsViewModel = new GenericOptionsViewModel (
          _session,
          _optionsViewModelParent,
          generalOptions,
          _outlookAccountPasswordProvider,
          IsGoogleProfile (options)
              ? CreateGoogleServerSettingsViewModel
              : new Func<ISettingsFaultFinder, ICurrentOptions, IServerSettingsViewModel> (CreateServerSettingsViewModel),
          CreateMappingConfigurationViewModelFactory);

      optionsViewModel.SetOptions (options);
      return optionsViewModel;
    }

    private static bool IsGoogleProfile (Contracts.Options options)
    {
      return options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth ||
             options.ServerAdapterType == ServerAdapterType.GoogleTaskApi ||
             options.ServerAdapterType == ServerAdapterType.GoogleContactApi;
    }

    IServerSettingsViewModel CreateGoogleServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      return new GoogleServerSettingsViewModel (settingsFaultFinder, currentOptions);
    }

    IServerSettingsViewModel CreateServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      return new ServerSettingsViewModel (settingsFaultFinder, currentOptions, _outlookAccountPasswordProvider);
    }

    IMappingConfigurationViewModelFactory CreateMappingConfigurationViewModelFactory (ICurrentOptions currentOptions)
    {
      return new MappingConfigurationViewModelFactory(_availableCategories,currentOptions);
    }
  }
}