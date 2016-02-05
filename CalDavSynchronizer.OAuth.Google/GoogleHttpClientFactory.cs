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
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;

namespace CalDavSynchronizer.OAuth.Google
{
  public static class GoogleHttpClientFactory
  {
    public static async Task<HttpClient> CreateHttpClient (string user, string userAgentHeader, IWebProxy proxy)
    {
      var userCredential = await LoginToGoogle (user);

      var client = new ProxySupportedHttpClientFactory (proxy).CreateHttpClient (new CreateHttpClientArgs() { ApplicationName = userAgentHeader });
      userCredential.Initialize (client);

      return client;
    }

    private static async Task<UserCredential> LoginToGoogle (string user)
    {
      UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync (
          new ClientSecrets
          {
              ClientId = "13856942399-rp437ddbn6406hfokpe5rqnosgnejodc.apps.googleusercontent.com",
              ClientSecret = "WG276vw5WCcc2H4SSaYJ03VO"
          },
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
    public static async Task<TasksService> LoginToGoogleTasksService (string user)
    {
      var credential = await OAuth.Google.GoogleHttpClientFactory.LoginToGoogle (user);

      var service = new TasksService (new BaseClientService.Initializer ()
      {
        HttpClientInitializer = credential,
        ApplicationName = "Outlook CalDav Synchronizer",
      });

      return service;
    }
  }
}