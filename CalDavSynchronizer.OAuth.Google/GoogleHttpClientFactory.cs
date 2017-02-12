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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Contacts;
using Google.GData.Client;
using Google.GData.Contacts;
using log4net;

namespace CalDavSynchronizer.OAuth.Google
{
  public static class GoogleHttpClientFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    private static ClientSecrets CreateClientSecrets () => new ClientSecrets
                                                           {
                                                               ClientId = "13856942399-rp437ddbn6406hfokpe5rqnosgnejodc.apps.googleusercontent.com",
                                                               ClientSecret = "WG276vw5WCcc2H4SSaYJ03VO"
                                                           };

    public static async Task<HttpClient> CreateHttpClient (string user, string userAgentHeader, IWebProxy proxy)
    {
      var userCredential = await LoginToGoogle (user, proxy);

      var client = new ProxySupportedHttpClientFactory (proxy).CreateHttpClient (new CreateHttpClientArgs() { ApplicationName = userAgentHeader });
      userCredential.Initialize (client);

      return client;
    }

    private static async Task<UserCredential> LoginToGoogle (string user, IWebProxy proxyOrNull)
    {
      GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer();

      initializer.ClientSecrets = CreateClientSecrets();
      initializer.HttpClientFactory = new ProxySupportedHttpClientFactory (proxyOrNull);

      UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (
          initializer,
          new[]
          {
              "https://www.googleapis.com/auth/calendar",
              "https://www.googleapis.com/auth/carddav",
              "https://www.googleapis.com/auth/tasks",
              "https://www.google.com/m8/feeds/" // => contacts
          },
          user,
          CancellationToken.None);

      return credential;
    }


    /// <remarks>
    /// This has to be done here, since UserCredential cannot be used in CalDavSynchronizer,
    /// since it leads to a 'The "FindRibbons" task failed unexpectedly' Error
    /// ( see https://connect.microsoft.com/VisualStudio/feedback/details/651634/the-findribbons-task-failed-unexpectedly)
    /// </remarks>
    public static async Task<TasksService> LoginToGoogleTasksService (string user, IWebProxy proxyOrNull)
    {
      var credential = await LoginToGoogle (user, proxyOrNull);
      var service = CreateTaskService (credential, proxyOrNull);

      try
      {
        await service.Tasklists.List().ExecuteAsync();
        return service;
      }
      catch (GoogleApiException x)
      {
        s_logger.Error ("Trying to access google task service failed. Revoking  token and reauthorizing.", x);

        await credential.RevokeTokenAsync (CancellationToken.None);
        await GoogleWebAuthorizationBroker.ReauthorizeAsync (credential, CancellationToken.None);
        return CreateTaskService (credential, proxyOrNull);
      }
    }

    private static TasksService CreateTaskService (UserCredential credential, IWebProxy proxyOrNull)
    {
      return new TasksService (new BaseClientService.Initializer
                               {
                                   HttpClientFactory = new ProxySupportedHttpClientFactory (proxyOrNull),
                                   HttpClientInitializer = credential,
                                   ApplicationName = "Outlook CalDav Synchronizer",
                               });
    }

    private static OAuth2Parameters CreateOAuth2Parameters (ClientSecrets clientSecrets, UserCredential credential)
    {
      return new OAuth2Parameters
      {
        ClientId = clientSecrets.ClientId,
        ClientSecret = clientSecrets.ClientSecret,
        AccessToken = credential.Token.AccessToken,
        RefreshToken = credential.Token.RefreshToken
      };
    }
    public static async Task<ContactsRequest> LoginToContactsService (string user, IWebProxy proxyOrNull)
    {
      var clientSecrets = CreateClientSecrets();
      var credential = await LoginToGoogle (user, proxyOrNull);

      var parameters = CreateOAuth2Parameters (clientSecrets, credential);
      var contactsRequest = new ContactsRequest (CreateRequestSettings(parameters));

      ContactsQuery query = new ContactsQuery (ContactsQuery.CreateContactsUri ("default"));
      query.NumberToRetrieve = 1;
      try
      {
        var feed = contactsRequest.Service.Query (query);
      }
      catch (GDataRequestException x)
      {
        s_logger.Error ("Trying to access google contacts API failed. Revoking  token and reauthorizing.", x);

        await credential.RevokeTokenAsync (CancellationToken.None);
        await GoogleWebAuthorizationBroker.ReauthorizeAsync (credential, CancellationToken.None);
        parameters = CreateOAuth2Parameters (clientSecrets, credential);
        contactsRequest = new ContactsRequest (CreateRequestSettings(parameters) );
      }

      if (proxyOrNull != null)
        contactsRequest.Proxy = proxyOrNull;

      return contactsRequest;
    }

    private static RequestSettings CreateRequestSettings(OAuth2Parameters parameters)
    {
      return new RequestSettings ("Outlook CalDav Synchronizer", parameters);
    }
  }
}