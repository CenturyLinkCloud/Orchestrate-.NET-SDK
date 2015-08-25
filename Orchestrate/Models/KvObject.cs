using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class KvObject<T> : KvMetadata
    {
        public string RawValue { get; private set; }

        public KvObject(string content, string collectionName, string key, string reference, string location)
            : base(collectionName, key, reference, location)
        {
            RawValue = content; 
        }

        public T Value
        {
            get
            {
                return JsonConvert.DeserializeObject<T>(RawValue);
            }
        }
    }
}
