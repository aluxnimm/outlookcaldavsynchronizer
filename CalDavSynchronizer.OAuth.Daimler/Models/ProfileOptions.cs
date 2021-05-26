using Newtonsoft.Json;
using System;

namespace CalDavSynchronizer.OAuth.Daimler.Models
{
    public class ProfileOptionsData
    {
        [JsonProperty("profiles")]
        public ProfileOptions[] Profiles { get; set; } = Array.Empty<ProfileOptions>();
    }

    public class ProfileOptions
    {
        [JsonProperty("profileId")]
        public Guid ProfileId { get; set; }

        [JsonProperty("selectedEnvironment")]
        public string SelectedEnvironment { get; set; }

        [JsonProperty("calenderUrl")]
        public string CalenderUrl { get; set; }
    }
}
