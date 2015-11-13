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
        public static void AddAuthentication(this HttpClient httpClient, string apiKey)
        {
            var authorization = Encoding.UTF8.GetBytes(string.Format("{0}:", apiKey));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authorization));
        }
    }
}
