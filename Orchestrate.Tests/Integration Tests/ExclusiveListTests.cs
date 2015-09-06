using System;
using Orchestrate.Io;
using Xunit;

public class ExclusiveListTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public ExclusiveListTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ExclusiveListAsync<Product>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ExclusiveListAsync<Product>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void AfterKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<Product>(afterKey: "1");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void BeforeKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<Product>(beforeKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void AfterKeyandBeforeKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<Product>(afterKey: "1",
                                                         beforeKey: "3");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void AfterKeyGreaterThanExistingKeysAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<Product>(afterKey: "4");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidKeysAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<Product>(afterKey: "3",
                                                         beforeKey: "1");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }
}