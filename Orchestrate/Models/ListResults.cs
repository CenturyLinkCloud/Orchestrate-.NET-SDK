using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Orchestrate.Io
{
    [JsonObject]
    public class ListResults<T> : IEnumerable<T>
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("results")]
        public List<ListItem<T>> Items { get; set; }

        [JsonProperty("next")]
        public string Next { get; set; }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Select(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
