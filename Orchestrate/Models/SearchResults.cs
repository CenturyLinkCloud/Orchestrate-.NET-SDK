using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Collections;

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
            foreach (SearchItem<T> searchItem in Items)
                yield return searchItem.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
