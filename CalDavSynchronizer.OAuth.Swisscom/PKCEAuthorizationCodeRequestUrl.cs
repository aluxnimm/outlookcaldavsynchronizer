using Google.Apis.Auth.OAuth2.Requests;
using System;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    public class PKCEAuthorizationCodeRequestUrl : AuthorizationCodeRequestUrl
    {
       
        [Google.Apis.Util.RequestParameterAttribute("code_challange", Google.Apis.Util.RequestParameterType.Query)]
        public string CodeChallenge { get; set; }

        [Google.Apis.Util.RequestParameterAttribute("code_challenge_method", Google.Apis.Util.RequestParameterType.Query)]
        public string CodeChallengeMethod { get; set; }

        public PKCEAuthorizationCodeRequestUrl(Uri authorizationServerUrl)
            : base(authorizationServerUrl)
        {
            CodeChallengeMethod = "S256";
        }

    }
}