using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class ListItem<T>
    {
        [JsonProperty("path")]
        public PathMetadata OrchestratePath { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }

        [JsonProperty("reftime")]
        public object ReferenceTime { get; set; }
    }
}

