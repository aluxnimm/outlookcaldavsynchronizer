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
using Microsoft.Office.Interop.Outlook;
using System.Threading.Tasks;

namespace CalDavSynchronizer.Ui.Options.Models
{
    public partial class ServerSettingsDetector : IServerSettingsDetector
    {
        private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;

        public ServerSettingsDetector(IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
        {
            if (outlookAccountPasswordProvider == null) throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
            _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
        }

        public Task AutoFillServerSettingsAsync(OptionsModel optionsModel)
        {
            var serverAccountSettings = _outlookAccountPasswordProvider.GetAccountServerSettings(optionsModel.FolderAccountName);
            optionsModel.EmailAddress = serverAccountSettings.EmailAddress;
            string path = !string.IsNullOrEmpty(optionsModel.CalenderUrl) ? new Uri(optionsModel.CalenderUrl).AbsolutePath : string.Empty;
            bool success;
            var dnsDiscoveredUrl = OptionTasks.DoSrvLookup(optionsModel.EmailAddress, OlItemType.olAppointmentItem, out success);
            optionsModel.CalenderUrl = success ? dnsDiscoveredUrl : "https://" + serverAccountSettings.ServerString + path;
            optionsModel.UserName = serverAccountSettings.UserName;
            optionsModel.UseAccountPassword = true;
            return Task.CompletedTask;
        }
    }
}