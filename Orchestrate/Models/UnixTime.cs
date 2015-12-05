using System;
using Newtonsoft.Json;

namespace Orchestrate.Io
{
    public struct UnixTime
    {
        public long Value { get; }

        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public UnixTime(long value)
        {
            this.Value = value;
        }

        public DateTimeOffset ToDateTimeOffsetUtc()
        {
            return Epoch.AddMilliseconds(Value);
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public static UnixTime FromDateTimeOffset(DateTimeOffset dateTime)
        {
            return new UnixTime((long)dateTime.ToUniversalTime().Subtract(Epoch).TotalMilliseconds);
        }
    }
}