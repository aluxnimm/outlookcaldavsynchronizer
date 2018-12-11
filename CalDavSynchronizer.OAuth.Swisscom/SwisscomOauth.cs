using DotNetOpenAuth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    public class SwisscomOauth
    {
        private const String AUTH_ENDPOINT = "https://consent.swisscom.com/c/oauth2/auth";
        private const String TOKEN_ENDPOINT = "https://api.swisscom.com/oauth2/token";
        private const String API_HOST = "https://api.swisscom.com";
        private readonly String CLIENT_ID;
        private readonly String CLIENT_SECRET;

        private IAuthorizationState _authorization = null;

        public SwisscomOauth(String clientId, String clientSecret)
        {
            CLIENT_ID = clientId;
            CLIENT_SECRET = clientSecret;
        }

        public void Authenticate()
        {
            var authServer = new AuthorizationServerDescription()
            {
                AuthorizationEndpoint = new Uri(AUTH_ENDPOINT),
                TokenEndpoint = new Uri(TOKEN_ENDPOINT)
            };
            var client = new UserAgentClient(authServer, CLIENT_ID, CLIENT_SECRET);
            var authorizePopup = new Authorize(client);
            bool? result = authorizePopup.ShowDialog();
            if (result.HasValue && result.Value)
            {
                _authorization = authorizePopup.Authorization;
            }
            else
            {
                throw new Exception("Not authorized");
            }
        }

        public String GetResponse(String url)
        {
            if (null == _authorization)
            {
                Authenticate();
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + _authorization.AccessToken);
            using (var resourceResponse = request.GetResponse())
            {
                using (var responseStream = new StreamReader(resourceResponse.GetResponseStream()))
                {
                    return responseStream.ReadToEnd();
                }
            }
        }
        public Credentials GetCredentials()
        {
            var response = GetResponse(API_HOST + "/addressbooks/configs");
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse[]>(response);

            return apiResponse[0].Carddav[0];
        }
    }
}
