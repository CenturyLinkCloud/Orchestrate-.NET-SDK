using System;
using Orchestrate.Io;
using Xunit;

public class UnixTimeTests
{
    public class ToDateTimeOffsetUtc
    {
        [Fact]
        public void ConvertsToCorrectDate()
        {
            var unixTime = new UnixTime(1432754581121);

            Assert.Equal(DateTimeOffset.Parse("2015-05-27T19:23:01.1210000+00:00"), unixTime.ToDateTimeOffsetUtc());
        }
    }

    public class FromDateTimeOffset
    {
        [Fact]
        public void ConvertsToCorrectTime()
        {
            var dateTime = DateTimeOffset.Parse("2015-05-27T19:23:01.1210000+00:00");
            var unixTime = UnixTime.FromDateTimeOffset(dateTime);

            Assert.Equal(new UnixTime(1432754581121), unixTime);
        }
    }

    public class ToString
    {
        [Fact]
        public void ReturnsValueAsString()
        {
            var unixTime = new UnixTime(1432754581121);

            Assert.Equal("1432754581121", unixTime.ToString());
        }
    }
}

