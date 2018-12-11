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
using CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes
{
  public class GoogleProfile : ProfileTypeBase
  {
    public const int MaximumWriteBatchSize = 100;
    public override string Name { get; } = "Google";
    public override string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_google.png";

    public bool IsGoogleProfile(Contracts.Options options)
    {
      return options.ServerAdapterType == ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth ||
             options.ServerAdapterType == ServerAdapterType.GoogleTaskApi ||
             options.ServerAdapterType == ServerAdapterType.GoogleContactApi;
    }

    public override IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
    {
      return new ProfileModelFactory(this, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData);
    }

    public override Contracts.Options CreateOptions()
    {
      var data = base.CreateOptions();
      data.CalenderUrl = Ui.Options.OptionTasks.GoogleDavBaseUrl;
      data.ServerAdapterType = ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
      data.IsChunkedSynchronizationEnabled = true;
      data.ChunkSize = 100;
      return data;
    }

    public override EventMappingConfiguration CreateEventMappingConfiguration()
    {
      var data = base.CreateEventMappingConfiguration();
      data.MapAttendees = false;
      data.MapSensitivityPublicToDefault = true;
      return data;
    }

    class ProfileModelFactory : ProfileModelFactoryBase
    {
      public ProfileModelFactory(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, ISettingsFaultFinder settingsFaultFinder, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
        : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, settingsFaultFinder, generalOptions, viewOptions, sessionData)
      {
      }
      
      protected override OptionsModel CreateModel(Contracts.Options data)
      {
        return new OptionsModel(SettingsFaultFinder, OptionTasks, OutlookAccountPasswordProvider, data, GeneralOptions, this, true, SessionData, ServerSettingsDetector.Value);
      }

      protected override IOptionsViewModel CreateTemplateViewModel(OptionsModel prototypeModel)
      {
        return new MultipleOptionsTemplateViewModel(
          OptionsViewModelParent,
          new GoogleServerSettingsTemplateViewModel(OutlookAccountPasswordProvider, prototypeModel),
          OptionTasks,
          prototypeModel,
          ViewOptions);
      }

      public override ProfileModelOptions ModelOptions { get; } = new ProfileModelOptions(true, true, false, true, Strings.Get($"DAV URL"), true, true, true);

      public override IOptionsViewModel CreateViewModel(OptionsModel model)
      {
        return new GenericOptionsViewModel(
          OptionsViewModelParent,
          new GoogleServerSettingsViewModel(model, OptionTasks, ViewOptions),
          OptionTasks,
          model,
          AvailableCategories,
          ViewOptions);
      }
    }
  }
}