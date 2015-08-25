using Orchestrate.Io;
using System;
using Xunit;

public class SearchTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public SearchTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Client.GetCollection(listTestFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.SearchAsync<TestData>(String.Empty));
        Assert.Equal("query", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.SearchAsync<TestData>(null));
        Assert.Equal("query", exception.ParamName);
    }
}

