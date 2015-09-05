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
    public static HttpClient CreateHttpClient (string user)
    {
      ManualResetEventSlim evt = new ManualResetEventSlim();

      var t = Task<int>.Run (() =>
      {
        var result = LoginToGoogle (user).Result;
        evt.Set();
        return result;
      });

      evt.Wait();

      var client = new HttpClientFactory().CreateHttpClient (new CreateHttpClientArgs() { ApplicationName = "CalDavSynchronizer/1.0" });
      t.Result.Initialize (client);

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
          new[] { "https://www.googleapis.com/auth/calendar" },
          user,
          CancellationToken.None);

      return credential;

      // Create the service.
      //var service = new BooksService(new BaseClientService.Initializer()
      //    {
      //        HttpClientInitializer = credential,
      //        ApplicationName = "Books API Sample",
      //    });

      //var bookshelves = await service.Mylibrary.Bookshelves.List().ExecuteAsync();
    }
  }
}