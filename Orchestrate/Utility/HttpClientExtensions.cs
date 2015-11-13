using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Orchestrate.Io.Utility;

namespace Orchestrate.Io
{
    internal static class HttpClientExtensions
    {
        public static void AddAuthenticaion(this HttpClient httpClient, string apiKey)
        {
            var authorization = Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authorization));
        }

        public async static Task<T> GetAsync<T>(this HttpClient httpClient, string apiKey, Uri uri, JsonSerializer serializer)
        {
            httpClient.AddAuthenticaion(apiKey);
            var response = await httpClient.GetAsync(uri.ToString());

            if (response.IsSuccessStatusCode)
                return serializer.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            else
                throw await RequestExceptionUtility.Make(response);
        }

        public static Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, Uri uri, T content, JsonSerializer serializer)
        {
            var json = serializer.SerializeObject(content);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return httpClient.PostAsync(uri, stringContent);
        }
    }
}
