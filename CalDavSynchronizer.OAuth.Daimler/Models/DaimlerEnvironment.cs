using Newtonsoft.Json;

namespace CalDavSynchronizer.OAuth.Daimler.Models
{
    public class DaimlerEnvironment
    {
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("wellKnownEndpoint")]
        public string WellKnownEndpoint { get; set; }
    }
}
