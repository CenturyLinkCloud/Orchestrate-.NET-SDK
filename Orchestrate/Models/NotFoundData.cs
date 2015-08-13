using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orchestrate.Io
{
    public class NotFoundData
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("details")]
        public NotFoundDetails Details { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }

    public class Item
    {
        [JsonProperty("collection")]
        public string Collection { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }
    }

    public class NotFoundDetails
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }
    }
}
