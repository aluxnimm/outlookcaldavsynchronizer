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
using CalDavSynchronizer.Ui.Options;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes
{
    class untermStrichProfile : ProfileTypeBase
    {
        public override string Name { get; } = "untermStrich";
        public override string ImageUrl { get; } = "pack://application:,,,/CalDavSynchronizer;component/Resources/ProfileLogos/logo_untermStrich.png";

        public override IProfileModelFactory CreateModelFactory(IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
        {
            return new ProfileModelFactory(this, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, generalOptions, viewOptions, sessionData);
        }

        public override Contracts.Options CreateOptions()
        {
            var data = base.CreateOptions();
            data.MappingConfiguration = CreateEventMappingConfiguration();
            data.CalenderUrl = "https://ihredomain.com/beliebigessubdir/dav/calendars/username/default/";
            data.IgnoreSynchronizationTimeRange = true;
            data.UseWebDavCollectionSync = true;
            data.EnableChangeTriggeredSynchronization = true;
            return data;
        }

        public override EventMappingConfiguration CreateEventMappingConfiguration()
        {
            var data = base.CreateEventMappingConfiguration();
            data.UseGlobalAppointmentID = false;
            return data;
        }

        public override ContactMappingConfiguration CreateContactMappingConfiguration()
        {
            var data = base.CreateContactMappingConfiguration();
            data.MapDistributionLists = false;
            data.DistributionListType = DistributionListType.VCardGroupWithUid;
            data.WriteImAsImpp = false;
            return data;
        }

        class ProfileModelFactory : ProfileModelFactoryBase
        {
            public ProfileModelFactory(IProfileType profileType, IOptionsViewModelParent optionsViewModelParent, IOutlookAccountPasswordProvider outlookAccountPasswordProvider, IReadOnlyList<string> availableCategories, IOptionTasks optionTasks, GeneralOptions generalOptions, IViewOptions viewOptions, OptionModelSessionData sessionData)
                : base(profileType, optionsViewModelParent, outlookAccountPasswordProvider, availableCategories, optionTasks, generalOptions, viewOptions, sessionData)
            {
            }
        }
    }
}