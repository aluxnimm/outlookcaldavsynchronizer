using CalDavSynchronizer.OAuth.Daimler.Controls;
using CalDavSynchronizer.OAuth.Daimler.Models;
using IdentityModel.OidcClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CalDavSynchronizer.OAuth.Daimler
{
    public class AuthenticationService
    {
        private readonly OidcClientOptions _options = null;
        private readonly OidcClient _client = null;

        public AuthenticationService(DaimlerEnvironment environment)
        {
            _options = new OidcClientOptions
            {
                Authority = environment.WellKnownEndpoint,
                ClientId = environment.ClientId,
                RedirectUri = "https://127.0.0.1/oidc",
                Scope = "openid offline_access",
                FilterClaims = false,
                Flow = OidcClientOptions.AuthenticationFlow.AuthorizationCode,
                ResponseMode = OidcClientOptions.AuthorizeResponseMode.Redirect,
                LoadProfile = true,
                Browser = new AuthPopupBrowser($"Daimler {environment.Name} Login")
            };

            _client = new OidcClient(_options);
        }

        public async Task<TokenData> Authenticate()
        {
            var loginResult = await _client.LoginAsync(new LoginRequest());
            if (loginResult.IsError) throw new Exception("Unauthorized");

            return new TokenData
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                Expiration = loginResult.AccessTokenExpiration,
                Username = loginResult.User.Claims.FirstOrDefault(e => e.Type == "sub")?.Value
            };
        }

        public async Task<TokenData> RefreshToken(string refreshToken)
        {
            var loginResult = await _client.RefreshTokenAsync(refreshToken);
            if (loginResult.IsError) throw new Exception("Unauthorized");

            return new TokenData
            {
                AccessToken = loginResult.AccessToken,
                RefreshToken = loginResult.RefreshToken,
                Expiration = loginResult.AccessTokenExpiration
            };
        }

    }
}
