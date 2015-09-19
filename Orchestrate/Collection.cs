using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class Collection
    {
        string host;
        string apiKey;

        public string CollectionName { get; private set; }

        public Collection(string collectionName, 
                          string apiKey,
                          string host)
        {
            this.apiKey = apiKey;
            this.host = host;
            CollectionName = collectionName; 
        }

        public async Task<SearchResults<T>> SearchAsync<T>(string query, SearchOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("query", query);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("query", query);

            if (opts != null)
            {
                if (!String.IsNullOrEmpty(opts.Sort))
                    uri.AddQuery("sort", opts.Sort);

                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<SearchResults<T>>(apiKey, uri);
        }

        public async Task<ListResults<T>> GetLinkAsync<T>(string key, string kind, LinkOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNullOrEmpty("kind", kind);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                    .AppendPath(CollectionName)
                                                    .AppendPath(key)
                                                    .AppendPath("relations")
                                                    .AppendPath(kind);

            if (opts != null)
            {
                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<ListResults<T>>(apiKey, uri);
        }

        public async Task<T> GetLinkAsync<T>(string key, string kind, GraphNode destinationNode)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNullOrEmpty("kind", kind);
            Guard.ArgumentNotNull("destination node", destinationNode);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                    .AppendPath(CollectionName)
                                                    .AppendPath(key)
                                                    .AppendPath("relation")
                                                    .AppendPath(kind)
                                                    .AppendPath(destinationNode.CollectionName)
                                                    .AppendPath(destinationNode.Key);

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<T>(apiKey, uri);
        }

        public async Task<ListResults<T>> HistoryAsync<T>(string productKey, HistoryOptions opts = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", productKey);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                       .AppendPath(CollectionName)
                                       .AppendPath(productKey)
                                       .AppendPath("refs");

            if(opts != null)
            {
                if (opts.Values == true)
                    uri.AddQuery("values", "true");

                uri.AddQuery("limit", opts.Limit.ToString());
                uri.AddQuery("offset", opts.Offset.ToString());
            }

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<ListResults<T>>(apiKey, uri);
        }

        public Task<SearchResults<T>> SearchAsync<T>(string field, decimal latitude, decimal longitude, string distance)
        {
            string luceneQuery = string.Format("{0}:NEAR:{4}lat:{1} lon:{2} dist:{3}{5}", field, 
                                                                                          latitude.ToString(), 
                                                                                          longitude.ToString(), 
                                                                                          distance,
                                                                                          "{",
                                                                                          "}");
            return SearchAsync<T>(luceneQuery);
        }

        public async Task<ListResults<T>> ListAsync<T>(int limit = 100)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<ListResults<T>>(apiKey, uri);
        }

        public async Task<KvMetadata> AddAsync<T>(T item)
        {
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.PostAsJsonAsync(uri.ToString(), item);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<ListResults<T>> ExclusiveListAsync<T>(int limit = 100,
                                                                string afterKey = null,
                                                                string beforeKey = null)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            if (!string.IsNullOrEmpty(beforeKey))
                uri.AddQuery("beforeKey", beforeKey);

            if (!string.IsNullOrEmpty(afterKey))
                uri.AddQuery("afterKey", afterKey);

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<ListResults<T>>(apiKey, uri);
        }


        public async Task<ListResults<T>> InclusiveListAsync<T>(int limit = 100, 
                                                                string startKey = null, 
                                                                string endKey = null)
        {
            if (limit < 1 || limit > 100)
                throw new ArgumentOutOfRangeException("limit", "limit must be between 1 and 100");

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                                   .AppendPath(CollectionName)
                                                   .AddQuery("limit", limit.ToString());

            if (!string.IsNullOrEmpty(startKey))
                uri.AddQuery("startKey", startKey);

            if (!string.IsNullOrEmpty(endKey))
                uri.AddQuery("endKey", endKey);

            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<ListResults<T>>(apiKey, uri);
        }

        public async Task<KvMetadata> AddOrUpdateAsync<T>(string key,
                                                          T item, 
                                                          string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            var message = new HttpRequestMessage(HttpMethod.Put, uri.ToString());

            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            if (item != null)
                message.AddContent(item);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetadata> TryAddAsync<T>(string key,
                                                     T item)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            var message = new HttpRequestMessage(HttpMethod.Put, uri.ToString());
            message.AddIfNoneMatch();

            if (item != null)
                message.AddContent(item);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }


        public async Task DeleteAsync(string key, bool purge = true, string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);
            if (purge)
                uri.AddQuery("purge", "true");
            else
                uri.AddQuery("purge", "false");

            var message = new HttpRequestMessage(HttpMethod.Delete, uri.ToString());

            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvObject<T>> GetAsync<T>(string key, string versionReference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            if (versionReference != null)
            {
                uri.AppendPath("refs")
                   .AppendPath(versionReference);
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.GetAsync(uri.ToString());

                if (response.IsSuccessStatusCode)
                {
                    var eTag = (response.Headers.ETag != null) ? response.Headers.ETag.Tag : string.Empty;
                    var location = (response.Headers.Location != null) ? response.Headers.Location.ToString() : string.Empty;
                    string content = await response.Content.ReadAsStringAsync();
                    return new KvObject<T>(content, CollectionName, key, eTag, location);
                }
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }


        public async Task<KvMetadata> MergeAsync<T>(string key,
                                                    T item, 
                                                    string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            var patchMethod = new HttpMethod("PATCH");
            HttpRequestMessage message = new HttpRequestMessage(patchMethod, uri.ToString());
            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                message.AddContent(item);

                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }


        public async Task<KvMetadata> PatchAsync(string key,
                                                 IEnumerable<PatchOperation> patchOperations, 
                                                 string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("operations", patchOperations);

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            var patchMethod = new HttpMethod("PATCH");
            HttpRequestMessage message = new HttpRequestMessage(patchMethod, uri.ToString());

            if(!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                message.AddContent(patchOperations.ToArray());

                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetadata> UpdateAsync<T>(string key,
                                                          T item,
                                                          string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            try
            {
                await GetAsync<T>(key);
            }
            catch(NotFoundException exception)
            {
                throw exception;
            }

            HttpUrlBuilder uri = new HttpUrlBuilder(host)
                                        .AppendPath(CollectionName)
                                        .AppendPath(key);

            var message = new HttpRequestMessage(HttpMethod.Put, uri.ToString());

            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            if (item != null)
                message.AddContent(item);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}