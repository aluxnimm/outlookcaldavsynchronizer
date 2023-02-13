using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util;
using Google.Apis.Util.Store;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using System.Security.Cryptography;
using System.Text;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    
    public class PkceAuthorizationCodeFlowDecorator : IAuthorizationCodeFlow
    {
        readonly AuthorizationCodeFlow _decorated;

        private readonly string _codeVerifier;
        private readonly string _codeChallenge;
     
        /// <summary>Constructs a new flow using the initializer's properties.</summary>
        public PkceAuthorizationCodeFlowDecorator(AuthorizationCodeFlow decorated)
        {
            _decorated = decorated ?? throw new ArgumentNullException(nameof(decorated));
            _codeVerifier = GenerateCodeVerifier();
            _codeChallenge = GenerateCodeChallenge(_codeVerifier);

        }

        private static string GenerateCodeVerifier()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
            }

            return Convert.ToBase64String(randomNumber)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            // Generate the code challenge by taking the SHA256 hash of the code verifier
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
                return Convert.ToBase64String(hash)
                    .TrimEnd('=')
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }


        public async Task<TokenResponse> LoadTokenAsync(string userId, CancellationToken taskCancellationToken)
        {
            return await _decorated.LoadTokenAsync(userId, taskCancellationToken);
        }

        public async Task DeleteTokenAsync(string userId, CancellationToken taskCancellationToken)
        {
            await _decorated.DeleteTokenAsync(userId, taskCancellationToken);
        }

        /// <inheritdoc/>
        public virtual AuthorizationCodeRequestUrl CreateAuthorizationCodeRequest(string redirectUri)
        {
            return new PKCEAuthorizationCodeRequestUrl(new Uri(_decorated.AuthorizationServerUrl))
            {
                ClientId = _decorated.ClientSecrets.ClientId,
                Scope = _decorated.Scopes == null ? null : string.Join(" ", _decorated.Scopes),
                RedirectUri = redirectUri,
                CodeChallenge = _codeChallenge
            };
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> ExchangeCodeForTokenAsync(string userId, string code, string redirectUri,
            CancellationToken taskCancellationToken)
        {
            var authorizationCodeTokenReq = new PKCEAuthorizationCodeTokenRequest
            {
                Scope = _decorated.Scopes == null ? null : string.Join(" ", _decorated.Scopes),
                RedirectUri = redirectUri,
                Code = code,
                CodeVerifier = _codeVerifier
            };

            var token = await _decorated.FetchTokenAsync(userId, authorizationCodeTokenReq, taskCancellationToken)
                .ConfigureAwait(false);
            await StoreTokenAsync(userId, token, taskCancellationToken).ConfigureAwait(false);
            return token;
        }

        private async Task StoreTokenAsync(string userId, TokenResponse token, CancellationToken taskCancellationToken)
        {
            taskCancellationToken.ThrowIfCancellationRequested();
            if (_decorated.DataStore != null)
            {
                await _decorated.DataStore.StoreAsync<TokenResponse>(userId, token).ConfigureAwait(false);
            }
        }

        public async Task<TokenResponse> RefreshTokenAsync(string userId, string refreshToken, CancellationToken taskCancellationToken)
        {
            return await _decorated.RefreshTokenAsync(userId, refreshToken, taskCancellationToken);
        }

        public async Task RevokeTokenAsync(string userId, string token, CancellationToken taskCancellationToken)
        {
            await _decorated.RevokeTokenAsync(userId, token, taskCancellationToken);
        }

        public bool ShouldForceTokenRetrieval()
        {
            return _decorated.ShouldForceTokenRetrieval();
        }

        public IAccessMethod AccessMethod => _decorated.AccessMethod;

        public IClock Clock => _decorated.Clock;

        public IDataStore DataStore => _decorated.DataStore;

        public void Dispose()
        {
            _decorated.Dispose();
        }
    }
}
