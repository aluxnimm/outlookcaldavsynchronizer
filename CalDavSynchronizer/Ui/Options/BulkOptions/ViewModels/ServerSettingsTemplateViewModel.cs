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
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using CalDavSynchronizer.Contracts;
using CalDavSynchronizer.DataAccess;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class ServerSettingsTemplateViewModel : ViewModelBase, IServerSettingsTemplateViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private string _calenderUrl;
    private string _emailAddress;
    private bool _useAccountPassword;
    private SecureString _password;
    private string _userName;
    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;

    public ServerSettingsTemplateViewModel (IOutlookAccountPasswordProvider outlookAccountPasswordProvider)
    {
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider;
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
    public bool UseAccountPassword
    {
      get { return _useAccountPassword; }
      set
      {
        CheckedPropertyChange (ref _useAccountPassword, value);
      }
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
      UseAccountPassword = options.UseAccountPassword;
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
      options.UseAccountPassword = _useAccountPassword;
      options.ServerAdapterType = ServerAdapterType.WebDavHttpClientBased;
    }

    public async Task<ServerResources> GetServerResources (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions)
    {
      string caldavUrlString ;
      string carddavUrlString;

      if (string.IsNullOrEmpty (CalenderUrl) && !string.IsNullOrEmpty (EmailAddress))
      {
        bool success;
        caldavUrlString = OptionTasks.DoSrvLookup (EmailAddress, OlItemType.olAppointmentItem, out success);
        carddavUrlString = OptionTasks.DoSrvLookup (EmailAddress, OlItemType.olContactItem, out success);
      }
      else
      {
        caldavUrlString = CalenderUrl;
        carddavUrlString = CalenderUrl;
      }

      var trimmedCaldavUrl = caldavUrlString.Trim();
      var caldavUrl = new Uri (trimmedCaldavUrl.EndsWith ("/") ? trimmedCaldavUrl : trimmedCaldavUrl + "/");

      var trimmedCarddavUrl = carddavUrlString.Trim();
      var carddavUrl = new Uri (trimmedCarddavUrl.EndsWith("/") ? trimmedCarddavUrl : trimmedCarddavUrl + "/");

      var webDavClientCaldav = CreateWebDavClient (networkSettings, generalOptions, trimmedCaldavUrl);
      var webDavClientCarddav = CreateWebDavClient (networkSettings, generalOptions, trimmedCarddavUrl);
      var calDavDataAccess = new CalDavDataAccess (caldavUrl, webDavClientCaldav);
      var cardDavDataAccess = new CardDavDataAccess (carddavUrl, webDavClientCarddav);

      return await GetUserResources (calDavDataAccess, cardDavDataAccess);
    }

    public void DiscoverAccountServerSettings()
    {
      var serverAccountSettings = _outlookAccountPasswordProvider.GetAccountServerSettings (null);
      EmailAddress = serverAccountSettings.EmailAddress;
      string path = !string.IsNullOrEmpty (CalenderUrl) ? new Uri (CalenderUrl).AbsolutePath : string.Empty; 
  
      bool success;
      var dnsDiscoveredUrl = OptionTasks.DoSrvLookup (EmailAddress, OlItemType.olAppointmentItem, out success);
      CalenderUrl = success ? dnsDiscoveredUrl : "https://" + serverAccountSettings.ServerString+path;
      UserName = serverAccountSettings.UserName;
      UseAccountPassword = true;
    }

    private static async Task<ServerResources> GetUserResources (CalDavDataAccess calDavDataAccess, CardDavDataAccess cardDavDataAccess)
    {
      var calDavResources = await calDavDataAccess.GetUserResourcesNoThrow (true);
      if (calDavResources.CalendarResources.Count == 0 && calDavResources.TaskListResources.Count == 0)
        calDavResources = await calDavDataAccess.GetUserResourcesNoThrow (false);
      var foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (true);
      if (foundAddressBooks.Count == 0)
        foundAddressBooks = await cardDavDataAccess.GetUserAddressBooksNoThrow (false);
      return new ServerResources (calDavResources.CalendarResources, foundAddressBooks, calDavResources.TaskListResources);
    }


    public static ServerSettingsTemplateViewModel DesignInstance = new ServerSettingsTemplateViewModel (NullOutlookAccountPasswordProvider.Instance)
                                                                   {
                                                                       CalenderUrl = "http://bulkurl",
                                                                       EmailAddress = "bulkemail",
                                                                       UseAccountPassword = true,
                                                                       Password = SecureStringUtility.ToSecureString ("bulkpwd"),
                                                                       UserName = "username",
                                                                   };

    private IWebDavClient CreateWebDavClient (NetworkSettingsViewModel networkSettings, GeneralOptions generalOptions, string davUrl)
    {
      return SynchronizerFactory.CreateWebDavClient (
          UserName,
          UseAccountPassword ? _outlookAccountPasswordProvider.GetPassword (null) : Password,
          davUrl,
          generalOptions.CalDavConnectTimeout,
          ServerAdapterType.WebDavHttpClientBased,
          networkSettings.CloseConnectionAfterEachRequest,
          networkSettings.PreemptiveAuthentication,
          networkSettings.ForceBasicAuthentication,
          networkSettings.CreateProxyOptions (),
          generalOptions.EnableClientCertificate,
          generalOptions.AcceptInvalidCharsInServerResponse);
    }
  }
}