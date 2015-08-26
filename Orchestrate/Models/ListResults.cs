using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orchestrate.Io
{
    public class ListResults<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<ListItem<T>> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }
    }
}
