using log4net;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    public class SwisscomOauth
    {
        private static readonly ILog s_logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const String CLIENT_ID = "pNsqznF1eB7bqcXghjyAnIQvee4a63l5";
        private const String CLIENT_SECRET = "hACyM2wWgM3Sh4PB";
        private const String AUTH_ENDPOINT = "https://consent.swisscom.com/c/oauth2/auth";
        private const String TOKEN_ENDPOINT = "https://api.swisscom.com/oauth2/token";
        private const String API_HOST = "https://api.swisscom.com";

        private readonly String UUID = Guid.NewGuid().ToString();

        private string _accessToken;

        public SwisscomOauth()
        {
        }

        public async Task<CredentialSet> GetCredentialsAsync(string carddavUrl)
        {
            try
            {
                // if carddavUrl is given directly invoke get credentials with publicId as argument
                if (carddavUrl != null)
                {
                    string[] sa = carddavUrl.Split('/');
                    return await QueryCredentialSetAsync(sa[8]);
                }

                AddressbookInfo[] r = await QueryAddressbookInfosAsync();
                if (r == null || r.Length == 0)
                {
                    s_logger.Error(String.Format("{0} | No location or addressbook found", UUID));
                    ErrorWindow errorWindow = new ErrorWindow(Globalization.Strings.Localize("LABEL_NO_ADDRESSBOOK_FOUND"));
                    errorWindow.ShowDialog();
                    return null;
                }

                if (r.Length == 1)
                {
                    return await QueryCredentialSetAsync(r[0].PublicId);
                }

                LocationSelectorWindow locationSelectorWindow = new LocationSelectorWindow(r);
                locationSelectorWindow.ShowDialog();

                AddressbookInfo location = locationSelectorWindow.GetSelectedLocationInfo();
                if (location != null)
                {
                    return await QueryCredentialSetAsync(location.PublicId);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                s_logger.Error(String.Format("{0} | Error while querying API: {1}", UUID, ex.ToString()));
                ErrorWindow errorWindow = new ErrorWindow(errorMessage: Globalization.Strings.Localize("LABEL_UNEXPECTED_ERROR"));
                errorWindow.ShowDialog();
                return null;
            }
        }

        private async Task<AddressbookInfo[]> QueryAddressbookInfosAsync()
        {
            String response = await GetResponseAsync(
                API_HOST + "/layer/addressbook-ng/nextgenerationaddressbook/plugin-conf/addressbooks"
            );

            AddressbookInfo[] listOfAddressbookInfo = null;
            if (response != null)
            {
                listOfAddressbookInfo = JsonConvert.DeserializeObject<AddressbookInfo[]>(response);
            }

            return listOfAddressbookInfo;
        }

        private async Task<CredentialSet> QueryCredentialSetAsync(string publicId)
        {
            String response = await GetResponseAsync(
                API_HOST + "/layer/addressbook-ng/nextgenerationaddressbook/plugin-conf/credentials/" + publicId
            );
            CredentialSet credentialSet = null;
            if (response != null)
            {
                credentialSet = JsonConvert.DeserializeObject<CredentialSet>(response);
                MessageBox.Show(Globalization.Strings.Localize("LABEL_AUTH_OK"), "Swisscom OAuth 2.0", MessageBoxButtons.OK);
            }

            return credentialSet;
        }

        private async Task<string> GetResponseAsync(string url)
        {
            if (_accessToken == null)
            {
                await AuthenticateAsync();
            }

            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + _accessToken);
            request.Headers.Add("Scs-Correlation-Id", UUID);
            request.Headers.Add("Scs-Version", "1");
            request.Headers.Add("Scs-AccessToken", _accessToken);

            try
            {
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return new StreamReader(response.GetResponseStream()).ReadToEnd();
                }

                return null;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    s_logger.Error(String.Format("{0} | Error while querying API: {1} | {2}", UUID, request.ToString(), response.ToString()));
                    if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }

                    throw;
                }

                throw;
            }
            catch (Exception ex)
            {
                s_logger.Error(String.Format("{0} | Error while querying API: {1} | {2}", UUID, request.ToString(), ex.ToString()));
                throw;
            }
        }

        private async Task AuthenticateAsync()
        {
            var initializer = new PKCEAuthorizationCodeFlow.Initializer(AUTH_ENDPOINT, TOKEN_ENDPOINT)
             {
                 ClientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets {ClientId = CLIENT_ID, ClientSecret = CLIENT_SECRET},
             };

             var authorizer = new Google.Apis.Auth.OAuth2.AuthorizationCodeInstalledApp(
                 new PKCEAuthorizationCodeFlow(initializer),
                 new Google.Apis.Auth.OAuth2.LocalServerCodeReceiver(String.Format(Globalization.Strings.Localize("LABEL_CLOSE_AUTH_WINDOW")), Google.Apis.Auth.OAuth2.LocalServerCodeReceiver.CallbackUriChooserStrategy.ForceLocalhost));

             var result = await authorizer.AuthorizeAsync(string.Empty, System.Threading.CancellationToken.None);
            
            _accessToken = result.Token.AccessToken;
        }
    }
}