using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CalDavSynchronizer.OAuth.Swisscom
{
    public partial class AddressbookInfo
    {
        [JsonProperty("abook_public_id")]
        public String PublicId { get; set; }

        [JsonProperty("location_info")]
        public LocationInfo LocationInfo { get; set; }

        [JsonProperty("telephone_numbers")]
        public String[] ListOfMsisdn { get; set; }
    }

    public partial class LocationInfo
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("addressLine3")]
        public string AddressLine3 { get; set; }
    }

    public partial class CredentialSet
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("interval")]
        public long Interval { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("chunkSize")]
        public long ChunkSize { get; set; }

        [JsonProperty("persistentConnection")]
        public bool PersistentConnection { get; set; }

        [JsonProperty("collectionSync")]
        public bool CollectionSync { get; set; }
    }
}
