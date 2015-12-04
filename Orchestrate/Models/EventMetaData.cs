using System;
using System.Collections.Generic;

namespace Orchestrate.Io
{
    public class EventMetaData
    {
        public static EventMetaData Make(RestResponse response)
        {
            var locationParts = response.Location.Split('/');

            var collectionName = locationParts[2];
            var key = locationParts[3];
            var eventType = locationParts[5];
            var timestamp = DateTimeOffsetExtensions.FromUnixTimeUtc(long.Parse(locationParts[6]));
            var ordinal = long.Parse(locationParts[7]);

            return new EventMetaData(collectionName, key, eventType, timestamp, ordinal);
        }

        public string CollectionName { get; }
        public string Key { get; }
        public string EventType { get; }
        public DateTimeOffset Timestamp { get; }
        public long Ordinal { get; }

        private EventMetaData(string collectionName, string key, string eventType, DateTimeOffset timestamp, long ordinal)
        {
            CollectionName = collectionName;
            Key = key;
            EventType = eventType;
            Timestamp = timestamp;
            Ordinal = ordinal;
        }
    }
}