using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Orchestrate.Io.Utility
{
    public class RestResponse
    {
        public RestResponse(EntityTagHeaderValue eTag, Uri location, string content)
        {
            ETag = eTag?.Tag ?? String.Empty;
            Location = location?.ToString() ?? String.Empty;
            Content = content;
        }

        public string ETag { get; }
        public string Location { get; }
        public string Content { get; }

        public static async Task<RestResponse> CreateAsync(HttpResponseMessage response)
        {
            string content = null;

            if (response.Content != null)
                content = await response.Content.ReadAsStringAsync();

            return new RestResponse(response.Headers.ETag, response.Headers.Location, content);
        }
    }
}