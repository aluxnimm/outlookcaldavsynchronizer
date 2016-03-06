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
    private IMappingConfigurationViewModelFactory _mappingConfigurationViewModelFactory;

    public OptionsViewModelFactory (
      NameSpace session, 
      IOptionsViewModelParent optionsViewModelParent, 
      IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
      IMappingConfigurationViewModelFactory mappingConfigurationViewModelFactory)
    {
      if (session == null)
        throw new ArgumentNullException (nameof (session));
      if (optionsViewModelParent == null)
        throw new ArgumentNullException (nameof (optionsViewModelParent));
      if (outlookAccountPasswordProvider == null)
        throw new ArgumentNullException (nameof (outlookAccountPasswordProvider));
      if (mappingConfigurationViewModelFactory == null)
        throw new ArgumentNullException (nameof (mappingConfigurationViewModelFactory));

      _optionsViewModelParent = optionsViewModelParent;
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _mappingConfigurationViewModelFactory = mappingConfigurationViewModelFactory;
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
          _mappingConfigurationViewModelFactory);

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
  }
}