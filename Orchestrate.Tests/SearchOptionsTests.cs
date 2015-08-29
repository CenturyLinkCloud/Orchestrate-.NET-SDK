using Orchestrate.Io;
using System;
using Xunit;

public class SearchOptionsTests
{
    [Fact]
    public void LimitGuards()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new SearchOptions(limit: -1));
        Assert.Equal("limit", exception.ParamName);

        exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new SearchOptions(limit: 101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public void OffsetGuards()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new SearchOptions(offset: -1));
        Assert.Equal("offset", exception.ParamName);
    }

}

