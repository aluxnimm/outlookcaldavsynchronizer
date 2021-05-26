using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.ViewModels;
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using System.Collections.Generic;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler
{
    public class DaimlerProfile : ProfileTypeBase
    {
        private const string name = "Daimler";
        public override string Name => name;

        public override string ImageUrl => "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/daimler.png";

        public override IProfileModelFactory CreateModelFactory(
            IOptionsViewModelParent optionsViewModelParent,
            IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
            IReadOnlyList<string> availableCategories,
            IOptionTasks optionTasks,
            GeneralOptions generalOptions,
            IViewOptions viewOptions,
            OptionModelSessionData sessionData)
        {
            return new ProfileModelFactory(
                this,
                optionsViewModelParent,
                outlookAccountPasswordProvider,
                availableCategories,
                optionTasks,
                generalOptions,
                viewOptions,
                sessionData);
        }

        public override Options CreateOptions()
        {
            var data = base.CreateOptions();
            data.EnableChangeTriggeredSynchronization = true;
            data.ServerAdapterType = ServerAdapterType.DaimlerOAuth;

            return data;
        }

        public override ContactMappingConfiguration CreateContactMappingConfiguration()
        {
            var data = base.CreateContactMappingConfiguration();
            data.MapDistributionLists = true;
            data.DistributionListType = DistributionListType.VCardGroupWithUid;

            return data;
        }

        public override EventMappingConfiguration CreateEventMappingConfiguration()
        {
            var data = base.CreateEventMappingConfiguration();
            data.CleanupDuplicateEvents = true;
            data.UseGlobalAppointmentID = true;

            return data;
        }

        class ProfileModelFactory : ProfileModelFactoryBase
        {
            public ProfileModelFactory(
                IProfileType profileType,
                IOptionsViewModelParent optionsViewModelParent,
                IOutlookAccountPasswordProvider outlookAccountPasswordProvider,
                IReadOnlyList<string> availableCategories,
                IOptionTasks optionTasks,
                GeneralOptions generalOptions,
                IViewOptions viewOptions,
                OptionModelSessionData sessionData)
              : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, generalOptions, viewOptions, sessionData)
            {

            }

            protected override OptionsModel CreateModel(Options data)
            {
                return new OptionsModel(
                    optionTasks: OptionTasks,
                    outlookAccountPasswordProvider: OutlookAccountPasswordProvider,
                    data: data,
                    //data: CreateOptions(),
                    generalOptions: GeneralOptions,
                    profileModelFactory: this,
                    isGoogle: false,
                    sessionData: SessionData,
                    serverSettingsDetector: ServerSettingsDetector.Value);
            }

            protected override ServerSettingsViewModel CreateServerSettingsViewModel(OptionsModel model) =>
                new DaimlerServerSettingsViewModel(model, GeneralOptions, OptionTasks, ViewOptions);

            public override ProfileModelOptions ModelOptions { get; } =
                new ProfileModelOptions(
                    areAdvancedNetWorkSettingsEnabled: false,
                    isEnableChangeTriggeredSynchronizationEnabled: false,
                    isTaskMappingConfigurationEnabled: false,
                    isContactMappingConfigurationEnabled: false,
                    davUrlLabelText: "CalDAV Server URL",
                    areSyncSettingsEnabled: true,
                    areSyncSettingsVisible: true,
                    isEnableChangeTriggeredSynchronizationVisible: false);
        }
    }
}
