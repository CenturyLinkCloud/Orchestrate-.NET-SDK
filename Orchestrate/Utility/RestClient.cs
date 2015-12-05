using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class RestClient
    {
        private readonly string apiKey;
        private readonly JsonSerializer serializer;
        private readonly TimeSpan timeout; 

        public RestClient(string apiKey, JsonSerializer serializer)
            : this(apiKey, serializer, TimeSpan.FromSeconds(100))
        { }

        public RestClient(string apiKey, JsonSerializer serializer, TimeSpan timeout)
        {
            this.apiKey = apiKey;
            this.serializer = serializer;
            this.timeout = timeout; 
        }

        public async Task<T> GetAsync<T>(Uri uri)
        {
            var response = await GetAsync(uri);
            return serializer.DeserializeObject<T>(response.Content);
        }

        public Task<RestResponse> GetAsync(Uri uri)
        {
            return SendAsync(uri, HttpMethod.Get);
        }

        public Task<RestResponse> SendIfMatchAsync<T>(Uri uri, HttpMethod method, T item, string reference)
        {
            var message = new HttpRequestMessage(method, uri.ToString());

            if (!String.IsNullOrEmpty(reference))
                message.AddIfMatch(reference);

            return SendAsync(message, method, item);
        }

        public Task<RestResponse> SendIfMatchAsync(Uri uri, HttpMethod method, string reference)
        {
            return SendIfMatchAsync(uri, method, (object) null, reference);
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

        public Task<RestResponse> SendAsync(Uri uri, HttpMethod method)
        {
            return SendAsync(uri, method, (object) null);
        }

        public async Task<RestResponse> SendAsync<T>(HttpRequestMessage message, HttpMethod method, T content)
        {
            message.AddUserAgent();

            if (content != null)
                message.AddContent(content, serializer);

            using (var httpClient = CreateHttpClient())
            {
                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return await RestResponse.CreateAsync(response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        private HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();
            var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);

            httpClient.Timeout = timeout;

            return httpClient;
        }
    }
}