using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Io.Utility
{
    public class RestClient
    {
        private readonly string apiKey;
        private readonly JsonSerializer serializer;

        public RestClient(string apiKey, JsonSerializer serializer)
        {
            this.apiKey = apiKey;
            this.serializer = serializer;
        }

        public async Task<T> GetAsync<T>(Uri uri)
        {
            var response = await GetAsync(uri);
            return serializer.DeserializeObject<T>(response.Content);
        }

        public async Task<RestResponse> GetAsync(Uri uri)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthentication(apiKey);
                var response = await httpClient.GetAsync(uri);

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);

                return await RestResponse.CreateAsync(response);
            }
        }

        public Task<RestResponse> SendIfMatchAsync<T>(Uri uri, HttpMethod method, T item, string reference)
        {
            var message = new HttpRequestMessage(method, uri.ToString());

            if (!string.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            return SendAsync(message, method, item);
        }

        public Task<RestResponse> SendIfNoneMatchAsync<T>(Uri uri, HttpMethod method, T item)
        {
            var message = new HttpRequestMessage(method, uri.ToString());
            message.AddIfNoneMatch();

            return SendAsync(message, method, item);
        }

        public Task<RestResponse> SendAsync<T>(Uri uri, HttpMethod method, T item)
        {
            var message = new HttpRequestMessage(method, uri.ToString());

            return SendAsync(message, method, item);
        }

        public async Task<RestResponse> SendAsync<T>(HttpRequestMessage message, HttpMethod method, T content)
        {
            if (content != null)
                message.AddContent(content, serializer);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthentication(apiKey);
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return await RestResponse.CreateAsync(response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}