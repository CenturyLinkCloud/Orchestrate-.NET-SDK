using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    internal static class HttpClientExtensions
    {
        public static void AddAuthenticaion(this HttpClient httpClient, string apiKey)
        {
            var authorization = Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authorization));
        }

        public async static Task<T> GetAsync<T>(this HttpClient httpClient, string apiKey, Uri uri)
        {
            httpClient.AddAuthenticaion(apiKey);
            var response = await httpClient.GetAsync(uri.ToString());

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadAsAsync<T>();
            else
                throw await RequestExceptionUtility.Make(response);

        }
    }
}
