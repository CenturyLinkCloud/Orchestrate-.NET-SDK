using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class EventResults<T> : IEnumerable<T>
    {
        private readonly string next;
        private readonly Uri host;
        private readonly RestClient client;

        public int Count { get; }

        public IReadOnlyList<EventItem<T>> Items { get; }

        public EventResults(int count, IReadOnlyList<EventItem<T>> items, string next, Uri host, RestClient client)
        {
            this.next = next;
            this.host = host;
            this.client = client;
            Count = count;
            Items = items;
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
            return next != null;
        }

        public async Task<EventResults<T>> GetNextAsync()
        {
            var response = await client.GetAsync<EventResultsResponse<T>>(new Uri(host, next));
            return response.ToResults(host, client);
        }
    }
}