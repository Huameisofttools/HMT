using Newtonsoft.Json;

namespace suiren.Models
{
    public class DskMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
