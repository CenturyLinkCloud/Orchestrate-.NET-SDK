﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class Collection
    {
        string baseUrl;
        string apiKey;

        public string CollectionName { get; private set; }

        public Collection(string collectionName, 
                          string apiKey,
                          string baseUrl = "https://api.orchestrate.io/v0/")
        {
            this.apiKey = apiKey;
            this.baseUrl = baseUrl;
            CollectionName = collectionName; 
        }

        public async Task<KvMetaData> AddAsync<T>(T item)
        {
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
                                        .AppendPath(CollectionName);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.PostAsJsonAsync(uri.ToString(), item);

                if (response.IsSuccessStatusCode)
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetaData> AddOrUpdateAsync<T>(string key,
                                                          T item, 
                                                          string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetaData> TryAdd<T>(string key,
                                                T item)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }


        public async Task DeleteAsync(string key, bool purge = true, string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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


        public async Task<KvMetaData> MergeAsync<T>(string key,
                                                    T item, 
                                                    string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }


        public async Task<KvMetaData> PatchAsync(string key,
                                                 IEnumerable<PatchOperation> patchOperations, 
                                                 string reference = null)
        {
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("operations", patchOperations);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetaData> UpdateAsync<T>(string key,
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

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
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
                    return KvMetaData.Make(CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}