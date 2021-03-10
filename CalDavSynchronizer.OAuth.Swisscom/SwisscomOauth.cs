using DotNetOpenAuth.OAuth2;
using log4net;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

        internal IAuthorizationState _authorization = null;

        public SwisscomOauth(string cultureInfo)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureInfo);
        }

        public CredentialSet GetCredentials(string carddavUrl)
        {
            try
            {
                // if carddavUrl is given directly invoke get credentials with publicId as argument
                if (carddavUrl != null)
                {
                    string[] sa = carddavUrl.Split('/');
                    return QueryCredentialSet(sa[8]);
                }

                AddressbookInfo[] r = QueryAddressbookInfos();
                if (r == null || r.Length == 0)
                {
                    s_logger.Error(String.Format("{0} | No location or addressbook found", UUID));
                    ErrorWindow errorWindow = new ErrorWindow(Globalization.Strings.Localize("LABEL_NO_ADDRESSBOOK_FOUND"));
                    errorWindow.ShowDialog();
                    return null;
                }

                if (r.Length == 1)
                {
                    return QueryCredentialSet(r[0].PublicId);
                }

                LocationSelectorWindow locationSelectorWindow = new LocationSelectorWindow(r);
                locationSelectorWindow.ShowDialog();

                AddressbookInfo location = locationSelectorWindow.GetSelectedLocationInfo();
                if (location != null)
                {
                    return QueryCredentialSet(location.PublicId);
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

        private AddressbookInfo[] QueryAddressbookInfos()
        {
            String response = GetResponse(
                    API_HOST + "/layer/addressbook-ng/nextgenerationaddressbook/plugin-conf/addressbooks"
                );

            AddressbookInfo[] listOfAddressbookInfo = null;
            if (response != null)
            {
                listOfAddressbookInfo = JsonConvert.DeserializeObject<AddressbookInfo[]>(response);
            }

            return listOfAddressbookInfo;
        }
        private CredentialSet QueryCredentialSet(string publicId)
        {
            String response = GetResponse(
                    API_HOST + "/layer/addressbook-ng/nextgenerationaddressbook/plugin-conf/credentials/" + publicId
                );
            CredentialSet credentialSet = null;
            if (response != null)
            {
               credentialSet = JsonConvert.DeserializeObject<CredentialSet>(response);
            }

            return credentialSet;
        }
        public string GetResponse(string url)
        {
            if (null == _authorization)
            {
                Authenticate();
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers.Add("Authorization", "Bearer " + _authorization.AccessToken);
            request.Headers.Add("Scs-Correlation-Id", UUID);
            request.Headers.Add("Scs-Version", "1");
            request.Headers.Add("Scs-AccessToken", _authorization.AccessToken);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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
        public virtual Boolean Authenticate()
        {
            AuthorizationServerDescription authServer = new AuthorizationServerDescription()
            {
                AuthorizationEndpoint = new Uri(AUTH_ENDPOINT),
                TokenEndpoint = new Uri(TOKEN_ENDPOINT)
            };
            s_logger.Debug(AUTH_ENDPOINT);
            s_logger.Debug(TOKEN_ENDPOINT);
            s_logger.Debug(CLIENT_ID);
            s_logger.Debug(CLIENT_SECRET);
            UserAgentClient client = new UserAgentClient(authServer, CLIENT_ID, CLIENT_SECRET);
            AuthorizationWindow authorizePopup = new AuthorizationWindow(client)
            {
            };
            s_logger.Debug(authorizePopup);

            bool? result = authorizePopup.ShowDialog();
            s_logger.Debug(result);
            if (result.HasValue && result.Value)
            {
                _authorization = authorizePopup.Authorization;
                return true;
            }
            else
            {
                return false;
                //throw new Exception("Not authorized");
            }
        }
    }
}
