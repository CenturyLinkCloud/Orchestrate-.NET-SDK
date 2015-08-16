using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class Client
    {
        readonly string apiKey;
        readonly string baseUrl;

        public Client(string apiKey,
                      string baseUrl = "https://api.orchestrate.io/")
        {
            this.apiKey = apiKey;
            this.baseUrl = baseUrl + "v0/";
        }

        public Collection GetCollection(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            return new Collection(collectionName, apiKey, baseUrl);
        }

        public async Task PingAsync()
        {
            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);

                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, uri.ToString());
                var response = await httpClient.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetaData> CreateCollectionAsync<T>(string collectionName, string key, T item)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
                                        .AppendPath(collectionName)
                                        .AppendPath(key);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.PutAsJsonAsync(uri.ToString(), item);

                if (response.IsSuccessStatusCode)
                    return KvMetaData.Make(collectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task DeleteCollectionAsync(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            HttpUrlBuilder uri = new HttpUrlBuilder(baseUrl)
                                        .AppendPath(collectionName)
                                        .AddQuery("force", "true");

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(apiKey);
                var response = await httpClient.DeleteAsync(uri.ToString());

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}