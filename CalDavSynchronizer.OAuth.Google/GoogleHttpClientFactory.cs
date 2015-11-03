using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;

namespace CalDavSynchronizer.OAuth.Google
{
  public static class GoogleHttpClientFactory
  {
    public static async Task<HttpClient> CreateHttpClient (string user, string userAgentHeader)
    {
      var userCredential = await LoginToGoogle (user);

      var client = new HttpClientFactory().CreateHttpClient (new CreateHttpClientArgs() { ApplicationName = userAgentHeader });
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
              "https://www.googleapis.com/auth/carddav"
          },
          user,
          CancellationToken.None);

      return credential;
    }
  }
}