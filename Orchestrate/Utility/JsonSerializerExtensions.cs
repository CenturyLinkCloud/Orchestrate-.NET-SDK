using System.IO;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public static class JsonSerializerExtensions
    {
        public static T DeserializeObject<T>(this JsonSerializer serializer, string json)
        {
            using (var stringReader = new StringReader(json))
            using (var jsonTextReader = new JsonTextReader(stringReader))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        public static string SerializeObject<T>(this JsonSerializer serializer, T obj)
        {
            using (var stringWriter = new StringWriter())
            {
                serializer.Serialize(stringWriter, obj);
                return stringWriter.ToString();
            }
        }
    }
}
