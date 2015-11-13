using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Io.Utility
{
    public class RestClient
    {
        private readonly string collectionName;
        private readonly string apiKey;
        private readonly JsonSerializer serializer;

        public RestClient(string collectionName, string apiKey, JsonSerializer serializer)
        {
            this.collectionName = collectionName;
            this.apiKey = apiKey;
            this.serializer = serializer;
        }

        public async Task<T> GetAsync<T>(Uri uri)
        {
            using (var httpClient = new HttpClient())
                return await httpClient.GetAsync<T>(apiKey, uri, serializer);
        }

        public async Task<KvObject<T>> GetAsync<T>(string key, Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthentication(apiKey);
                var response = await httpClient.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var eTag = (response.Headers.ETag != null) ? response.Headers.ETag.Tag : string.Empty;
                    var location = (response.Headers.Location != null) ? response.Headers.Location.ToString() : string.Empty;
                    string content = await response.Content.ReadAsStringAsync();
                    return new KvObject<T>(content, collectionName, key, eTag, location, serializer);
                }
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public Task<KvMetadata> SendIfMatchAsync<T>(Uri uri, HttpMethod method, T item, string reference)
        {
            var message = new HttpRequestMessage(method, uri.ToString());

            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            return SendAsync(message, method, item);
        }

        public Task<KvMetadata> SendIfNoneMatchAsync<T>(Uri uri, HttpMethod method, T item)
        {
            var message = new HttpRequestMessage(method, uri.ToString());
            message.AddIfNoneMatch();

            return SendAsync(message, method, item);
        }

        public Task<KvMetadata> SendAsync<T>(Uri uri, HttpMethod method, T item)
        {
            var message = new HttpRequestMessage(method, uri.ToString());

            return SendAsync(message, method, item);
        }

        public async Task<KvMetadata> SendAsync<T>(HttpRequestMessage message, HttpMethod method, T item)
        {
            if (item != null)
                message.AddContent(item, serializer);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthentication(apiKey);
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(collectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}