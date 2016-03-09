using System;
using System.Collections.Generic;
using System.Linq;
using CalDavSynchronizer.Contracts;
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

    public List<OptionsViewModelBase> Create (ICollection<CalDavSynchronizer.Contracts.Options> options, bool fixInvalidSettings)
    {
      return options.Select (o => Create (o, fixInvalidSettings)).ToList();
    }

    public OptionsViewModelBase Create (CalDavSynchronizer.Contracts.Options options, bool fixInvalidSettings)
    {
      var optionsViewModel = new GenericOptionsViewModel (
          _session,
          _optionsViewModelParent,
          fixInvalidSettings,
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
      return options.DisplayType == OptionsDisplayType.Google
             || options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth
             || options.ServerAdapterType == ServerAdapterType.GoogleTaskApi;
    }

    IServerSettingsViewModel CreateGoogleServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      return new GoogleServerSettingsViewModel (settingsFaultFinder, currentOptions);
    }

    IServerSettingsViewModel CreateServerSettingsViewModel (ISettingsFaultFinder settingsFaultFinder, ICurrentOptions currentOptions)
    {
      return new ServerSettingsViewModel (settingsFaultFinder, currentOptions);
    }

    IMappingConfigurationViewModelFactory CreateMappingConfigurationViewModelFactory (ICurrentOptions currentOptions)
    {
      return new MappingConfigurationViewModelFactory(_availableCategories,currentOptions);
    }
  }
}