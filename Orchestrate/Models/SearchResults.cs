using Newtonsoft.Json;
using System.Collections.Generic;

namespace Orchestrate.Io
{
    public class SearchResults<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<SearchItem<T>> Items { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }
}
