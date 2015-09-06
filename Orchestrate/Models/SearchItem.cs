using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class SearchItem<T>
    {
        [JsonProperty("path")]
        public PathMetadata PathMetadata { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }

        [JsonProperty("reftime")]
        public object ReferenceTime { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }
    }
}
