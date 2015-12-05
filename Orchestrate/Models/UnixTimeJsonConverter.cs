using System;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public class UnixTimeJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(UnixTime.FromDateTimeOffset((DateTimeOffset) value).Value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Integer)
                throw new NotSupportedException();
            return new UnixTime((long) reader.Value).ToDateTimeOffsetUtc();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (DateTimeOffset);
        }
    }
}