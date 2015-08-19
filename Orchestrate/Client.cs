using System.Net.Http;
using System.Threading.Tasks;

namespace Orchestrate.Io
{
    public class Client
    {
        readonly IApplication application;

        public Client(IApplication application)
        {
            this.application = application;
        }

        public Collection GetCollection(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            return new Collection(collectionName, application.Key, application.V0ApiUrl);
        }

        public async Task PingAsync()
        {
            HttpUrlBuilder uri = new HttpUrlBuilder(application.V0ApiUrl);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);

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

            HttpUrlBuilder uri = new HttpUrlBuilder(application.V0ApiUrl)
                                        .AppendPath(collectionName)
                                        .AppendPath(key);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);
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

            HttpUrlBuilder uri = new HttpUrlBuilder(application.V0ApiUrl)
                                        .AppendPath(collectionName)
                                        .AddQuery("force", "true");

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);
                var response = await httpClient.DeleteAsync(uri.ToString());

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}