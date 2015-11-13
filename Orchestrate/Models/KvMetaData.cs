using System.Net.Http;
using Orchestrate.Io.Utility;

namespace Orchestrate.Io
{
    public class KvMetadata
    {
        public static KvMetadata Make(string collectionName, RestResponse response)
        {
            var reference = response.ETag.Replace("\"", "");
            var key = ExtractKeyFromLocation(response.Location);
            return new KvMetadata(collectionName, key, reference, response.Location);
        }

        public string CollectionName { get; private set; }
        public string Key { get; private set; }
        public string VersionReference { get; private set; }
        public string Location { get; private set; }

        protected KvMetadata(string collectionName, string key, string reference, string location)
        {
            CollectionName = collectionName;
            Key = key;
            VersionReference = reference.Replace("\"", "");
            Location = location; 
        }

        private static string ExtractKeyFromLocation(string location)
        {
            var locationParts = location.Split('/');

            if(locationParts.Length > 4)
                return locationParts[3];

            return string.Empty;
        }
    }
}
