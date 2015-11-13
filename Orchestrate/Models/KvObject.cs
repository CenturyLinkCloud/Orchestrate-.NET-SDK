using Newtonsoft.Json;
using Orchestrate.Io.Utility;

namespace Orchestrate.Io
{
    public class KvObject<T> : KvMetadata
    {
        JsonSerializer serializer;
        public string RawValue { get; private set; }

        public KvObject(string content, string collectionName, string key, string reference, string location, JsonSerializer serializer)
            : base(collectionName, key, reference, location)
        {
            this.serializer = serializer;
            RawValue = content;
        }

        public T Value
        {
            get { return serializer.DeserializeObject<T>(RawValue); }
        }
    }
}
