using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    [JsonObject]
    public class SearchResults<T> : IEnumerable<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<SearchItem<T>> Items { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        [JsonProperty("prev")]
        public string Prev { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Select(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
