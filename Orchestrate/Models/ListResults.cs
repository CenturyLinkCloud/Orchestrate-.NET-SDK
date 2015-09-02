using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;

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
            foreach (ListItem<T> listItem in Items)
                yield return listItem.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }
    }
}
