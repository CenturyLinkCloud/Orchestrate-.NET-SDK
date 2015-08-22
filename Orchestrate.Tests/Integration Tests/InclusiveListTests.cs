using System;
using Orchestrate.Io;
using Xunit;

public class InclusiveListTests : IClassFixture<ListTestFixture>
{
    Collection collection;
    ListTestFixture listTestFixture;

    public InclusiveListTests(ListTestFixture listTestFixture)
    {
        this.listTestFixture = listTestFixture;

        collection = listTestFixture.Client.GetCollection(listTestFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.InclusiveListAsync<TestData>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.InclusiveListAsync<TestData>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void StartKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<TestData>(startKey: "1");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 1);
                Assert.Equal("1", result.OrchestratePath.Key);
            },
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
    public async void EndKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<TestData>(endKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 1);
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(result.Value.Id, 2);
                Assert.Equal("2", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void StartKeyAndEndKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<TestData>(startKey: "1",
                                                          endKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(result.Value.Id, 1);
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(result.Value.Id, 2);
                Assert.Equal("2", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void StartKeyGreaterThanExistingKeysAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<TestData>(startKey: "4");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidKeysAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<TestData>(startKey: "3",
                                                          endKey: "1");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }
}