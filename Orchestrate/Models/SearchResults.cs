using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class SearchResults<T> : IEnumerable<T>
    {
        private readonly RestClient restClient;
        private readonly Uri host;

        public int Count { get; }

        public IReadOnlyList<SearchItem<T>> Items { get; }

        public int TotalCount { get; }

        public string Next { get; }

        public string Prev { get; }

        public SearchResults(int count, IReadOnlyList<SearchItem<T>> items, int totalCount, string next, string prev, Uri host, RestClient restClient)
        {
            this.host = host;
            this.restClient = restClient;
            Count = count;
            Items = items;
            TotalCount = totalCount;
            Next = next;
            Prev = prev;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Items.Select(i => i.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool HasNext()
        {
            return Next != null;
        }

        public async Task<SearchResults<T>> GetNextAsync()
        {
            if (!HasNext())
                throw new InvalidOperationException("There are no more items available in the search results.");

            var nextUri = new Uri(host, Next);
            
            var response = await restClient.GetAsync<SearchResultsResponse<T>>(nextUri);
            return response.ToResults(host, restClient);
        }
    }
}
