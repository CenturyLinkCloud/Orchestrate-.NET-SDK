using System;
using Orchestrate.Io;
using Xunit;

public class LinkOptionsTests
{
    [Fact]
    public void LimitGuards()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new LinkOptions(limit: -1));
        Assert.Equal("limit", exception.ParamName);

        exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new LinkOptions(limit: 101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public void OffsetGuards()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new LinkOptions(offset: -1));
        Assert.Equal("offset", exception.ParamName);
    }
}

