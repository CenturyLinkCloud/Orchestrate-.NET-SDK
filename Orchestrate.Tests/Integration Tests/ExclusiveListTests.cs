using System;
using Orchestrate.Io;
using Xunit;

public class ExclusiveListTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public ExclusiveListTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Client.GetCollection(listTestFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ExclusiveListAsync<TestData>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ExclusiveListAsync<TestData>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void AfterKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<TestData>(afterKey: "1");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 2);
                Assert.Equal("2", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(result.Value.Id, 3);
                Assert.Equal("3", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void BeforeKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<TestData>(beforeKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 1);
                Assert.Equal("1", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void AfterKeyandBeforeKeyAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<TestData>(afterKey: "1",
                                                          beforeKey: "3");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 2);
                Assert.Equal("2", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void AfterKeyGreaterThanExistingKeysAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<TestData>(afterKey: "4");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidKeysAsync()
    {
        var listResult =
            await collection.ExclusiveListAsync<TestData>(afterKey: "3",
                                                          beforeKey: "1");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }
}