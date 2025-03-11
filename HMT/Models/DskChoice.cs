using Newtonsoft.Json;

namespace suiren.Models
{
    public class DskChoice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public DskMessage Message { get; set; }

        [JsonProperty("logprobs")]
        public object Logprobs { get; set; }  // null in the example, can be defined more specifically if needed

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }
}
