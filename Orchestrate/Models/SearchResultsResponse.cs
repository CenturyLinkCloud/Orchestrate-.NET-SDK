using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    [JsonObject]
    public class SearchResultsResponse<T>
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

        public SearchResults<T> ToResults(Uri host, RestClient restClient)
        {
            return new SearchResults<T>(Count, Items, TotalCount, Next, Prev, host, restClient);
        }
    }
}