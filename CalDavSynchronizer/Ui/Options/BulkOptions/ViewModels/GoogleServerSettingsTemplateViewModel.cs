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
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;
using Google.Apis.Tasks.v1.Data;
using log4net;
using Microsoft.Office.Interop.Outlook;
using Exception = System.Exception;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class GoogleServerSettingsTemplateViewModel : ViewModelBase, IServerSettingsTemplateViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private string _emailAddress;
    private string _calenderUrl;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;

    public GoogleServerSettingsTemplateViewModel (IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
    }


    public string CalenderUrl
    {
      get { return _calenderUrl; }
      set { CheckedPropertyChange (ref _calenderUrl, value); }
    }

    public string EmailAddress
    {
      get { return _emailAddress; }
      set
      {
        if (!Equals (_emailAddress, value))
        {
          _emailAddress = value;
          // ReSharper disable once ExplicitCallerInfoArgument
          OnPropertyChanged (nameof (EmailAddress));
        }
      }
    }
   
    public void SetOptions (Contracts.Options options)
    {
      EmailAddress = options.EmailAddress;
      if (!string.IsNullOrEmpty (options.CalenderUrl))
        CalenderUrl = options.CalenderUrl;
      else
        CalenderUrl = OptionTasks.GoogleDavBaseUrl;
    }

    public void FillOptions (Contracts.Options options, CalendarData resource)
    {
      options.CalenderUrl = resource.Uri.ToString();
      options.ServerAdapterType = ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth;
      FillOptions (options);
    }

    public void FillOptions (Contracts.Options options, AddressBookData resource)
    {
      options.CalenderUrl = string.Empty;
      options.ServerAdapterType = ServerAdapterType.GoogleContactApi;
      FillOptions (options);
    }

    public void FillOptions (Contracts.Options options, TaskListData resource)
    {
      options.CalenderUrl = resource.Id;
      options.ServerAdapterType = ServerAdapterType.GoogleTaskApi;
      FillOptions (options);
    }

    private void FillOptions (Contracts.Options options)
    {
      options.UserName = EmailAddress;
      options.Password = new SecureString ();
      options.EmailAddress = EmailAddress;
      options.UseAccountPassword = false;
    }

    public void DiscoverAccountServerSettings()
    {
      var serverAccountSettings = _outlookAccountPasswordProvider.GetAccountServerSettings (null);
      EmailAddress = serverAccountSettings.EmailAddress;
    }

    public async Task<ServerResources> GetServerResources (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions)
    {
      var trimmedUrl = CalenderUrl.Trim();
      var url = new Uri (trimmedUrl.EndsWith ("/") ? trimmedUrl : trimmedUrl + "/");

      var webDavClient = CreateWebDavClient (networkSettings, generalOptions);
      var calDavDataAccess = new CalDavDataAccess (url, webDavClient);
      var foundResources = await calDavDataAccess.GetUserResourcesNoThrow (false);

      var foundAddressBooks = new[] { new AddressBookData (new Uri ("googleApi://defaultAddressBook"), "Default AddressBook") };

      var service = await GoogleHttpClientFactory.LoginToGoogleTasksService (EmailAddress, SynchronizerFactory.CreateProxy (networkSettings.CreateProxyOptions()));
      var taskLists = await service.Tasklists.List().ExecuteAsync();
      var taskListsData = taskLists?.Items.Select (i => new TaskListData (i.Id, i.Title)).ToArray() ?? new TaskListData[] { };

      return new ServerResources (foundResources.CalendarResources, foundAddressBooks, taskListsData);
    }


    private IWebDavClient CreateWebDavClient (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions)
    {
      return SynchronizerFactory.CreateWebDavClient (
          EmailAddress,
          SecureStringUtility.ToSecureString (string.Empty),
          CalenderUrl,
          generalOptions.CalDavConnectTimeout,
          ServerAdapterType.WebDavHttpClientBasedWithGoogleOAuth,
          networkSettings.CloseConnectionAfterEachRequest,
          networkSettings.PreemptiveAuthentication,
          networkSettings.ForceBasicAuthentication,
          networkSettings.CreateProxyOptions(),
          generalOptions.AcceptInvalidCharsInServerResponse);
    }

    public static GoogleServerSettingsTemplateViewModel DesignInstance => new GoogleServerSettingsTemplateViewModel (NullOutlookAccountPasswordProvider.Instance )
                                                                          {
                                                                              CalenderUrl = "http://BulkUrl.com",
                                                                              EmailAddress = "bla@bulk.com",
                                                                          };


  }
}