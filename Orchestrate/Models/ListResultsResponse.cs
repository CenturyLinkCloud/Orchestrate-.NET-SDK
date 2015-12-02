using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    [JsonObject]
    public class ListResultsResponse<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<ListItem<T>> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        public ListResults<T> ToResults(Uri host, RestClient restClient)
        {
            return new ListResults<T>(Count, Items, Next, host, restClient);
        }
    }
}