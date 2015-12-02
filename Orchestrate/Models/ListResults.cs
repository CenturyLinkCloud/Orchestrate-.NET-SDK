using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class ListResults<T> : IEnumerable<T>
    {
        private readonly Uri host;
        private readonly RestClient restClient;

        public int Count { get; }

        public IReadOnlyList<ListItem<T>> Items { get; }

        public string Next { get; }

        public ListResults(int count, IReadOnlyList<ListItem<T>> items, string next, Uri host, RestClient restClient)
        {
            this.host = host;
            this.restClient = restClient;
            Count = count;
            Items = items;
            Next = next;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Select(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        { return GetEnumerator(); }

        public bool HasNext()
        {
            return Next != null;
        }

        public async Task<ListResults<T>> GetNextAsync()
        {
            if (!HasNext())
                throw new InvalidOperationException("There are no more items available in the list results.");

            var nextUri = new Uri(host, Next);

            var response = await restClient.GetAsync<ListResultsResponse<T>>(nextUri);
            return response.ToResults(host, restClient);
        }
    }
}
