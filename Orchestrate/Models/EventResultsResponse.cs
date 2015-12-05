using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    [JsonObject]
    public class EventResultsResponse<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public IReadOnlyList<EventItem<T>> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        public EventResults<T> ToResults(Uri host, RestClient client)
        {
            return new EventResults<T>(Count, Items, Next, host, client);
        }
    }
}