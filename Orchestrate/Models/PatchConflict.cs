using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class PatchConflict
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class Details
    {
        [JsonProperty("op")]
        public Op Op { get; set; }

        [JsonProperty("opIndex")]
        public int OpIndex { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }
    }

    public class Op
    {
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("op")]
        public string Operation { get; set; }
    }
}
