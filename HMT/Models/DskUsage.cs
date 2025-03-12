using Newtonsoft.Json;

namespace suiren.Models
{
    public class DskUsage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonProperty("prompt_cache_hit_tokens")]
        public int PromptCacheHitTokens { get; set; }

        [JsonProperty("prompt_cache_miss_tokens")]
        public int PromptCacheMissTokens { get; set; }
    }
}
