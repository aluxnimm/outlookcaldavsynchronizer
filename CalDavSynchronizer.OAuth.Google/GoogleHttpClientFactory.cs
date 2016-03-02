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
using log4net;

namespace CalDavSynchronizer.OAuth.Google
{
  public static class GoogleHttpClientFactory
  {
    private static readonly ILog s_logger = LogManager.GetLogger (MethodInfo.GetCurrentMethod().DeclaringType);

    public static async Task<HttpClient> CreateHttpClient (string user, string userAgentHeader, IWebProxy proxy)
    {
      var userCredential = await LoginToGoogle (user,proxy);

      var client = new ProxySupportedHttpClientFactory (proxy).CreateHttpClient (new CreateHttpClientArgs() { ApplicationName = userAgentHeader });
      userCredential.Initialize (client);

      return client;
    }

    private static async Task<UserCredential> LoginToGoogle (string user, IWebProxy proxyOrNull)
    {
      GoogleAuthorizationCodeFlow.Initializer initializer = new GoogleAuthorizationCodeFlow.Initializer ();
      initializer.ClientSecrets = new ClientSecrets
                                  {
                                      ClientId = "13856942399-rp437ddbn6406hfokpe5rqnosgnejodc.apps.googleusercontent.com",
                                      ClientSecret = "WG276vw5WCcc2H4SSaYJ03VO"
                                  };
      initializer.HttpClientFactory = new ProxySupportedHttpClientFactory (proxyOrNull);

      UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (
          initializer,
          new[]
          {
              "https://www.googleapis.com/auth/calendar",
              "https://www.googleapis.com/auth/carddav",
              "https://www.googleapis.com/auth/tasks"
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
    public static async Task<TasksService> LoginToGoogleTasksService (string user,IWebProxy proxyOrNull)
    {

      var credential = await LoginToGoogle (user, proxyOrNull);
      var service = CreateTaskService (credential);

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
        return CreateTaskService (credential);
      }
    }

    private static TasksService CreateTaskService (UserCredential credential)
    {
      return new TasksService (new BaseClientService.Initializer
                               {
                                   HttpClientInitializer = credential,
                                   ApplicationName = "Outlook CalDav Synchronizer",
                               });
    }
  }
}