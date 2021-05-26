using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CalDavSynchronizer.ProfileTypes.ConcreteTypes.Daimler.Helpers
{
    public class DaimlerHttpClient : HttpClient
    {
        private DaimlerHttpClient() { }

        public static HttpClient Create(IWebProxy proxy, ProfileDataProvider profileDataProvider) =>
            new HttpClient(
                new AddTokenHandler(
                    new HttpClientHandler
                    {
                        Proxy = proxy,
                        UseProxy = (proxy != null)
                    },
                profileDataProvider));

        public static HttpClient Create(ProfileDataProvider profileDataProvider) => new HttpClient(new AddTokenHandler(profileDataProvider));
    }

    class AddTokenHandler : DelegatingHandler
    {
        private readonly ProfileDataProvider _profileDataProvider;

        public AddTokenHandler(HttpMessageHandler handler, ProfileDataProvider profileDataProvider)
            : base(handler)
        {
            _profileDataProvider = profileDataProvider;
        }

        public AddTokenHandler(ProfileDataProvider profileDataProvider)
        {
            _profileDataProvider = profileDataProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var options = _profileDataProvider.LoadProfileOptions();
            var token = await _profileDataProvider.LoadToken(options.SelectedEnvironment, true);
            if (token == null) throw new UnauthorizedAccessException("Could not load token!");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
