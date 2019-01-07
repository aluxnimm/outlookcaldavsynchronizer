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
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.OAuth.Google;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.ConnectionTests;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class GoogleServerSettingsTemplateViewModel : ModelBase, IServerSettingsTemplateViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly OptionsModel _prototypeModel;

    public GoogleServerSettingsTemplateViewModel(IOutlookAccountPasswordProvider outlookAccountPasswordProvider, OptionsModel prototypeModel)
    {
      if (outlookAccountPasswordProvider == null) throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
      if (prototypeModel == null) throw new ArgumentNullException(nameof(prototypeModel));

      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
      _prototypeModel = prototypeModel;

      RegisterPropertyChangePropagation(_prototypeModel, nameof(_prototypeModel.CalenderUrl), nameof(CalenderUrl));
      RegisterPropertyChangePropagation(_prototypeModel, nameof(_prototypeModel.EmailAddress), nameof(EmailAddress));
    }

    public string CalenderUrl
    {
      get { return _prototypeModel.CalenderUrl; }
      set { _prototypeModel.CalenderUrl = value; }
    }

    public string EmailAddress
    {
      get { return _prototypeModel.EmailAddress; }
      set
      {
        _prototypeModel.EmailAddress = value;
        _prototypeModel.UserName = value;
      }
    }
    
    public void SetResourceUrl (OptionsModel options, CalendarData resource)
    {
      options.CalenderUrl = resource.Uri.ToString();
    }

    public void SetResourceUrl (OptionsModel options, AddressBookData resource)
    {
      options.CalenderUrl = string.Empty;
      options.UseGoogleNativeApi = true;
    }

    public void SetResourceUrl (OptionsModel options, TaskListData resource)
    {
      options.CalenderUrl = resource.Id;
    }

    public void DiscoverAccountServerSettings()
    {
      var serverAccountSettings = _outlookAccountPasswordProvider.GetAccountServerSettings (null);
      EmailAddress = serverAccountSettings.EmailAddress;
    }

    public async Task<ServerResources> GetServerResources ()
    {
      var trimmedUrl = CalenderUrl.Trim();
      var url = new Uri (trimmedUrl.EndsWith ("/") ? trimmedUrl : trimmedUrl + "/");

      var webDavClient = _prototypeModel.CreateWebDavClient();
      var calDavDataAccess = new CalDavDataAccess (url, webDavClient);
      var foundResources = await calDavDataAccess.GetUserResourcesIncludingCalendarProxies (false);

      var foundAddressBooks = new[] { new AddressBookData (new Uri ("googleApi://defaultAddressBook"), "Default AddressBook", AccessPrivileges.All) };

      var service = await GoogleHttpClientFactory.LoginToGoogleTasksService (EmailAddress, SynchronizerFactory.CreateProxy (_prototypeModel.CreateProxyOptions()));
      var taskLists = await service.Tasklists.List().ExecuteAsync();
      var taskListsData = taskLists?.Items.Select (i => new TaskListData (i.Id, i.Title, AccessPrivileges.All)).ToArray() ?? new TaskListData[] { };

      return new ServerResources (foundResources.CalendarResources, foundAddressBooks, taskListsData);
    }

    public static GoogleServerSettingsTemplateViewModel DesignInstance => new GoogleServerSettingsTemplateViewModel(NullOutlookAccountPasswordProvider.Instance, OptionsModel.DesignInstance)
    {
      CalenderUrl = "http://BulkUrl.com",
      EmailAddress = "bla@bulk.com",
    };


  }
}