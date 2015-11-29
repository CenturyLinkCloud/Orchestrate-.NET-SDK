using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class Client
    {
        readonly IApplication application;
        JsonSerializer serializer;
        RestClient restClient;

        public Client(IApplication application) : this(application, JsonSerializer.CreateDefault())
        {
        }

        public Client(IApplication application, JsonSerializer serializer)
        {
            this.application = application;
            this.serializer = serializer;
            this.restClient = new RestClient(application.Key, serializer);
        }

        public Collection GetCollection(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            return new Collection(collectionName, application.Key, application.HostUrl, serializer);
        }

        public Task PingAsync()
        {
            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl);
            return restClient.SendAsync(uri, HttpMethod.Head);
        }

        public Task UnlinkAsync(GraphNode fromNode, string kind, GraphNode toNode)
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

            return restClient.SendAsync(uri, HttpMethod.Delete);
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

            var response = await restClient.SendAsync(uri, HttpMethod.Put, item);
            return KvMetadata.Make(collectionName, response);
        }

        public Task DeleteCollectionAsync(string collectionName)
        {
            Guard.ArgumentNotNullOrEmpty("collectionName", collectionName);

            HttpUrlBuilder uri = new HttpUrlBuilder(application.HostUrl)
                                        .AppendPath(collectionName)
                                        .AddQuery("force", "true");

            return restClient.SendAsync(uri, HttpMethod.Delete);
        }

        public async Task<KvMetadata> LinkAsync(GraphNode fromNode, string kind, GraphNode toNode)
        {
            return await LinkAsync<object>(fromNode, kind, toNode, null);
        }

        public async Task<KvMetadata> LinkAsync<T>(GraphNode fromNode, string kind, GraphNode toNode, T properties, string reference = null)
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

            var response = await restClient.SendIfMatchAsync(uri, HttpMethod.Put, properties, reference);
            return KvMetadata.Make(fromNode.CollectionName, response);
        }
    }
}