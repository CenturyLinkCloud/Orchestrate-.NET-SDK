using System;
using System.Collections.Generic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class ListTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public ListTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<Product>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<Product>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void ListAsync()
    {
        var listResult = await collection.ListAsync<Product>();

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            },
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
    public async void ListWithLimitFewerThanNumberOfElementsAsync()
    {
        var listResult = await collection.ListAsync<Product>(2);

        Assert.Collection(listResult.Items,
            result => Assert.Equal(1, result.Value.Id),
            result => Assert.Equal(2, result.Value.Id)
        );

        Assert.Contains("afterKey=2", listResult.Next);
    }

    [Fact]
    public async void AllowsPagination()
    {
        var totalResults = new List<Product>();

        var listResult = await collection.ListAsync<Product>(1);
        totalResults.AddRange(listResult);

        while (listResult.HasNext())
        {
            listResult = await listResult.GetNextAsync();
            totalResults.AddRange(listResult);
        }

        Assert.Collection(totalResults, 
            r => Assert.Equal(1, r.Id),
            r => Assert.Equal(2, r.Id),
            r => Assert.Equal(3, r.Id)
        );
    }

    [Fact]
    public async void GetNextThrowsWhenNoPagesAreAvailable()
    {
        var result = await collection.ListAsync<Product>(100);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => result.GetNextAsync());
        Assert.Equal("There are no more items available in the list results.", ex.Message);
    }

    [Fact]
    public async void EnumerateListAsync()
    {
        var listResult = await collection.ListAsync<Product>();
        Assert.Equal(3, listResult.Count);

        int count = 0;
        foreach (Product product in listResult)
            count++;

        Assert.Equal(3, count);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection("collection");

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.ListAsync<Product>());

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}