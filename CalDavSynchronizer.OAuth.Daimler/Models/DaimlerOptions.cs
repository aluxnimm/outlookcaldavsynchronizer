using Newtonsoft.Json;
using System;

namespace CalDavSynchronizer.OAuth.Daimler.Models
{
    public class DaimlerOptions
    {
        [JsonProperty("environments")]
        //public Dictionary<string, string> Environments { get; set; } = new Dictionary<string, string>();
        public DaimlerEnvironment[] Environments { get; set; } = Array.Empty<DaimlerEnvironment>();
    }
}
