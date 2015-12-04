using System;

namespace Orchestrate.Io
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public static long ToUnixTime(this DateTimeOffset dateTimeOffset)
        {
            return Convert.ToInt64((dateTimeOffset.ToUniversalTime() - Epoch).TotalMilliseconds);
        }

        public static DateTimeOffset FromUnixTimeUtc(long unixTime)
        {
            return Epoch.AddMilliseconds(unixTime);
        }
        
    }
}