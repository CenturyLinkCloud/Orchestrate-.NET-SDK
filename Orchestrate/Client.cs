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

            return new Collection(collectionName, application.Key, application.HostUrl);
        }

        public async Task PingAsync()
        {
            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);

                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, uri.ToString());
                var response = await httpClient.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task UnlinkAsync(GraphNode fromNode, string kind, GraphNode toNode)
        {
            Guard.ArgumentNotNull("fromNode", fromNode);
            Guard.ArgumentNotNullOrEmpty("kind", kind);
            Guard.ArgumentNotNull("toNode", toNode);

            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl)
                                                    .AppendPath(fromNode.CollectionName)
                                                    .AppendPath(fromNode.Key)
                                                    .AppendPath("relation")
                                                    .AppendPath(kind)
                                                    .AppendPath(toNode.CollectionName)
                                                    .AppendPath(toNode.Key)
                                                    .AddQuery("purge", "true");

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Delete, uri.ToUri());

                var response = await httpClient.SendAsync(message);

                if (!response.IsSuccessStatusCode)
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task<KvMetadata> CreateCollectionAsync<T>(string collectionName, 
                                                               string key, 
                                                               T item)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);
            Guard.ArgumentNotNullOrEmpty("key", key);
            Guard.ArgumentNotNull("item", item);

            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl)
                                        .AppendPath(collectionName)
                                        .AppendPath(key);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);
                var response = await httpClient.PutAsJsonAsync(uri.ToString(), item);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(collectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }

        public async Task DeleteCollectionAsync(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl)
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

        public async Task<KvMetadata> LinkAsync(GraphNode fromNode, string kind, GraphNode toNode)
        {
            return await LinkAsync<object>(fromNode, kind, toNode, null);
        }

        public async Task<KvMetadata> LinkAsync<T>(GraphNode fromNode, string kind, GraphNode toNode, T properties)
        {
            Guard.ArgumentNotNull("fromNode", fromNode);
            Guard.ArgumentNotNullOrEmpty("kind", kind);
            Guard.ArgumentNotNull("toNode", toNode);

            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl)
                                                    .AppendPath(fromNode.CollectionName)
                                                    .AppendPath(fromNode.Key)
                                                    .AppendPath("relation")
                                                    .AppendPath(kind)
                                                    .AppendPath(toNode.CollectionName)
                                                    .AppendPath(toNode.Key);

            using (var httpClient = new HttpClient())
            {
                httpClient.AddAuthenticaion(application.Key);
                HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Put, uri.ToUri());

                if (properties != null)
                    message.AddContent(properties);

                var response = await httpClient.SendAsync(message);

                if (response.IsSuccessStatusCode)
                    return KvMetadata.Make(fromNode.CollectionName, response);
                else
                    throw await RequestExceptionUtility.Make(response);
            }
        }
    }
}