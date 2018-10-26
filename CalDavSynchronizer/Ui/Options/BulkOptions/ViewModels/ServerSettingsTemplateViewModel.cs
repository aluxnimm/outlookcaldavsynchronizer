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
using CalDavSynchronizer.ProfileTypes;
using CalDavSynchronizer.Scheduling;
using CalDavSynchronizer.Ui.Options.Models;
using CalDavSynchronizer.Ui.Options.ViewModels;
using CalDavSynchronizer.Utilities;
using log4net;
using Microsoft.Office.Interop.Outlook;

namespace CalDavSynchronizer.Ui.Options.BulkOptions.ViewModels
{
  internal class ServerSettingsTemplateViewModel : ModelBase, IServerSettingsTemplateViewModel
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodBase.GetCurrentMethod().DeclaringType);

    private readonly IOutlookAccountPasswordProvider _outlookAccountPasswordProvider;
    private readonly OptionsModel _prototypeModel;


    public ServerSettingsTemplateViewModel (IOutlookAccountPasswordProvider outlookAccountPasswordProvider, OptionsModel prototypeModel, ProfileModelOptions modelOptions)
    {
      _outlookAccountPasswordProvider = outlookAccountPasswordProvider ?? throw new ArgumentNullException(nameof(outlookAccountPasswordProvider));
      _prototypeModel = prototypeModel ?? throw new ArgumentNullException(nameof(prototypeModel));
      ModelOptions = modelOptions ?? throw new ArgumentNullException(nameof(modelOptions));

      RegisterPropertyChangePropagation(prototypeModel, nameof(prototypeModel.CalenderUrl), nameof(CalenderUrl));
      RegisterPropertyChangePropagation(prototypeModel, nameof(prototypeModel.UserName), nameof(UserName));
      RegisterPropertyChangePropagation(prototypeModel, nameof(prototypeModel.UseAccountPassword), nameof(UseAccountPassword));
      RegisterPropertyChangePropagation(prototypeModel, nameof(prototypeModel.Password), nameof(Password));
      RegisterPropertyChangePropagation(prototypeModel, nameof(prototypeModel.EmailAddress), nameof(EmailAddress));
    }

    public ProfileModelOptions ModelOptions { get; }

    public string CalenderUrl
    {
      get { return _prototypeModel.CalenderUrl; }
      set { _prototypeModel.CalenderUrl = value; }
    }

    public string UserName
    {
      get { return _prototypeModel.UserName; }
      set { _prototypeModel.UserName = value; }
    }

    public bool UseAccountPassword
    {
      get { return _prototypeModel.UseAccountPassword; }
      set { _prototypeModel.UseAccountPassword = value; }
    }

    public SecureString Password
    {
      get { return _prototypeModel.Password; }
      set { _prototypeModel.Password = value; }
    }

    public string EmailAddress
    {
      get { return _prototypeModel.EmailAddress; }
      set { _prototypeModel.EmailAddress = value; }
    }

    public void SetResourceUrl (OptionsModel options, CalendarData resource)
    {
      options.CalenderUrl = resource.Uri.ToString ();
    }

    public void SetResourceUrl (OptionsModel options, AddressBookData resource)
    {
      options.CalenderUrl = resource.Uri.ToString ();
    }

    public void SetResourceUrl (OptionsModel options, TaskListData resource)
    {
      options.CalenderUrl = resource.Id;
    }

    public async Task<ServerResources> GetServerResources ()
    {
      string caldavUrlString ;
      string carddavUrlString;

      if (string.IsNullOrEmpty (CalenderUrl) && !string.IsNullOrEmpty (EmailAddress))
      {
        s_logger.Debug("Auto-detect CaldAV/CardDAV URL");
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

      var webDavClientCaldav = _prototypeModel.CreateWebDavClient(new Uri(trimmedCaldavUrl));
      var webDavClientCarddav = _prototypeModel.CreateWebDavClient(new Uri(trimmedCarddavUrl));
      var calDavDataAccess = new CalDavDataAccess (caldavUrl, webDavClientCaldav);
      var cardDavDataAccess = new CardDavDataAccess (carddavUrl, webDavClientCarddav, string.Empty, contentType => true);

      s_logger.Debug("Get User's resources");
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


    public static ServerSettingsTemplateViewModel DesignInstance = new ServerSettingsTemplateViewModel(NullOutlookAccountPasswordProvider.Instance, OptionsModel.DesignInstance, new ProfileModelOptions(false, false, false, false, "DAV Url", false))
    {
      CalenderUrl = "http://bulkurl",
      EmailAddress = "bulkemail",
      UseAccountPassword = true,
      Password = SecureStringUtility.ToSecureString("bulkpwd"),
      UserName = "username",
    };
  }
}