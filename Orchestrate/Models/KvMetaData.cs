using System.Net.Http;

namespace Orchestrate.Io
{
    public class KvMetadata
    {
        public static KvMetadata Make(string collectionName, HttpResponseMessage response)
        {
            var eTag = (response.Headers.ETag != null) ? response.Headers.ETag.Tag : string.Empty;
            var reference = eTag.Replace("\"", "");
            var location = (response.Headers.Location != null) ? response.Headers.Location.ToString() : string.Empty;
            var key = ExtractKeyFromLocation(location);
            return new KvMetadata(collectionName, key, reference, location);
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
