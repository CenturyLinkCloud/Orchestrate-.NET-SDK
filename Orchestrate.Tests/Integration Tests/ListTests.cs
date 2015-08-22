using System;
using Orchestrate.Io;
using Xunit;

public class ListTests : IClassFixture<ListTestFixture>
{
    Collection collection;
    ListTestFixture listTestFixture;

    public ListTests(ListTestFixture listTestFixture)
    {
        this.listTestFixture = listTestFixture;

        collection = listTestFixture.Client.GetCollection(listTestFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<TestData>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<TestData>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void ListAsync()
    {
        var listResult = await collection.ListAsync<TestData>();

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
    public async void ListWithLimitFewerThanNumberOfElementsAsync()
    {
        var listResult = await collection.ListAsync<TestData>(2);

        Assert.Collection(listResult.Items,
            result => Assert.Equal(result.Value.Id, 1),
            result => Assert.Equal(result.Value.Id, 2)
        );

        Assert.Contains("afterKey=2", listResult.Next);
    }
}