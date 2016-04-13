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
using System.Windows;
using System.Windows.Input;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;
using log4net;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class ServerSettingsTemplateViewModel : ViewModelBase, IServerSettingsTemplateViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private string _calenderUrl;
    private string _emailAddress;
    private SecureString _password;
    private string _userName;

    public ServerSettingsTemplateViewModel ()
    {
    }

    public string CalenderUrl
    {
      get { return _calenderUrl; }
      set { CheckedPropertyChange (ref _calenderUrl, value); }
    }

    public string UserName
    {
      get { return _userName; }
      set { CheckedPropertyChange (ref _userName, value); }
    }

    public SecureString Password
    {
      get { return _password; }
      set { CheckedPropertyChange (ref _password, value); }
    }

    public string EmailAddress
    {
      get { return _emailAddress; }
      set { CheckedPropertyChange (ref _emailAddress, value); }
    }

    public void SetOptions (Contracts.Options options)
    {
      CalenderUrl = options.CalenderUrl;
      UserName = options.UserName;
      Password = options.Password;
      EmailAddress = options.EmailAddress;
    }


    public void FillOptions (Contracts.Options options, CalendarData resource)
    {
      options.CalenderUrl = resource.Uri.ToString ();
      FillOptions (options);
    }

    public void FillOptions (Contracts.Options options, AddressBookData resource)
    {
      options.CalenderUrl = resource.Uri.ToString ();
      FillOptions (options);
    }

    public void FillOptions (Contracts.Options options, TaskListData resource)
    {
      options.CalenderUrl = resource.Id;
      FillOptions (options);
    }

    public void FillOptions (Contracts.Options options)
    {
      options.UserName = _userName;
      options.Password = _password;
      options.EmailAddress = _emailAddress;
      options.UseAccountPassword = false;
      options.ServerAdapterType = ServerAdapterType.WebDavHttpClientBased;
    }

    public async Task<ServerResources> GetServerResources (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions)
    {
      var trimmedUrl = CalenderUrl.Trim();
      var url = new Uri (trimmedUrl.EndsWith ("/") ? trimmedUrl : trimmedUrl + "/");

      var webDavClient = CreateWebDavClient (networkSettings, generalOptions);
      var calDavDataAccess = new CalDavDataAccess (url, webDavClient);
      var foundCaldendars = await calDavDataAccess.GetUserCalendarsNoThrow (true);
      var cardDavDataAccess = new CardDavDataAccess (url, webDavClient);
      var foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (true);
      var tasks = foundCaldendars.Select (c => new TaskListData (c.Uri.ToString(), c.Name)).ToArray();

      return new ServerResources (foundCaldendars, foundAddressBooks, tasks);
    }
    
    public static ServerSettingsTemplateViewModel DesignInstance = new ServerSettingsTemplateViewModel()
                                                                   {
                                                                       CalenderUrl = "http://bulkurl",
                                                                       EmailAddress = "bulkemail",
                                                                       Password = SecureStringUtility.ToSecureString ("bulkpwd"),
                                                                       UserName = "username",
                                                                   };

    private IWebDavClient CreateWebDavClient (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions)
    {
      return SynchronizerFactory.CreateWebDavClient (
          UserName,
          Password,
          CalenderUrl,
          TimeSpan.Parse (ConfigurationManager.AppSettings["calDavConnectTimeout"]),
           ServerAdapterType.WebDavHttpClientBased,
          networkSettings.CloseConnectionAfterEachRequest,
          networkSettings.PreemptiveAuthentication,
          networkSettings.ForceBasicAuthentication,
          networkSettings.CreateProxyOptions (),
          generalOptions.AcceptInvalidCharsInServerResponse);
    }
  }
}