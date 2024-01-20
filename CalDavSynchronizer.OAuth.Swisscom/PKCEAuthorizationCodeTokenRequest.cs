using Google.Apis.Auth.OAuth2.Requests;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    public class PKCEAuthorizationCodeTokenRequest : AuthorizationCodeTokenRequest
    {
       
        /// <summary>
        /// Gets or sets the code_verifier.
        /// </summary>
        [Google.Apis.Util.RequestParameterAttribute("code_verifier")]
        public new string CodeVerifier { get; set; }

    }
}