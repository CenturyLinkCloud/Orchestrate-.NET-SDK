using System;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class EventItem<T>
    {
        [JsonProperty("path")]
        public PathMetadata PathMetadata { get; set; }

        [JsonProperty("value")]
        public T Value { get; set; }

        [JsonProperty("timestamp")]
        [JsonConverter(typeof(UnixTimeJsonConverter))]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("ordinal")]
        public long Ordinal { get; set; }
    }
}