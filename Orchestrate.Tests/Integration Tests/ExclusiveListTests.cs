using System;
using Xunit;

public class ExclusiveListTests : IClassFixture<ListTestFixture>
{
    ListTestFixture listTestFixture;

    public ExclusiveListTests(ListTestFixture listTestFixture)
    {
        this.listTestFixture = listTestFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => listTestFixture.Collection.ExclusiveListAsync<TestData>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => listTestFixture.Collection.ExclusiveListAsync<TestData>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void AfterKeyAsync()
    {
        var listResult =
            await listTestFixture.Collection.ExclusiveListAsync<TestData>(afterKey: "1");

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
            await listTestFixture.Collection.ExclusiveListAsync<TestData>(beforeKey: "2");

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
            await listTestFixture.Collection.ExclusiveListAsync<TestData>(afterKey: "1",
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
            await listTestFixture.Collection.ExclusiveListAsync<TestData>(afterKey: "4");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidKeysAsync()
    {
        var listResult =
            await listTestFixture.Collection.ExclusiveListAsync<TestData>(afterKey: "3",
                                                                          beforeKey: "1");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }
}