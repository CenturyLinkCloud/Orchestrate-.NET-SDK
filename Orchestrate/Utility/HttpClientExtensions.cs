using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Orchestrate.Io
{
    internal static class HttpClientExtensions
    {
        public static void AddAuthenticaion(this HttpClient httpClient, string apiKey)
        {
            var authorization = Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authorization));
        }
    }
}
