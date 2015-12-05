using System;
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
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        public DateTimeOffset ReferenceTime { get; set; }

        [JsonProperty("tombstone")]
        public bool Tombstone { get; set; }

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("ordinal")]
        public long Ordinal { get; set; }

        [JsonProperty("ordinal_str")]
        public string OrdinalString { get; set; }
    }
}
