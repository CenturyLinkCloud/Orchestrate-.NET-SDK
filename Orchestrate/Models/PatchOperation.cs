using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public abstract class PatchOperation
    {
        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }

    public class PatchOperation<T> : PatchOperation
    {
        [JsonProperty("value")]
        public T Value { get; set; }
    }
}
