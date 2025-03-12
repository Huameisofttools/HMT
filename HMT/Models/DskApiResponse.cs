using Newtonsoft.Json;
using System.Collections.Generic;

namespace suiren.Models
{
    public class DskApiResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("choices")]
        public List<DskChoice> Choices { get; set; }

        [JsonProperty("usage")]
        public DskUsage Usage { get; set; }

        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; set; }
    }
}
