using System;
using Newtonsoft.Json;
using Orchestrate.Io;
using Xunit;

public class UnixTimeJsonConverterTests
{
    [Fact]
    public void SerializesAsLong()
    {
        var dateTime = DateTimeOffset.Parse("2015-05-27T19:23:01.1210000+00:00");
        
        string json = JsonConvert.SerializeObject(dateTime, Formatting.None, new UnixTimeJsonConverter());

        Assert.Equal("1432754581121", json);
    }

    [Fact]
    public void DeserializesFromLong()
    {
        var json = "1432754581121";
        var dateTimeOffset = JsonConvert.DeserializeObject<DateTimeOffset>(json, new UnixTimeJsonConverter());

        Assert.Equal(DateTimeOffset.Parse("2015-05-27T19:23:01.1210000+00:00"), dateTimeOffset);
    }

    [Theory]
    [InlineData("\"foo\"")]
    [InlineData("24.34")]
    [InlineData("\"24.34\"")]
    [InlineData("null")]
    public void DeserializingWrongTypeNotSupported(string json)
    {
        Assert.Throws<NotSupportedException>(() => JsonConvert.DeserializeObject<DateTimeOffset>(json, new UnixTimeJsonConverter()));
    }
}