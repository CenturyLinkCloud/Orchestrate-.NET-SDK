using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class PathMetadata
    {
        [JsonProperty("collection")]
        public string Collection { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("ref")]
        public string VersionReference { get; set; }

        [JsonProperty("reftime")]
        public object ReferenceTime { get; set; }

        [JsonProperty("tombstone")]
        public bool Tombstone { get; set; }
    }
}
