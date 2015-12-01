using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Orchestrate.Io;
using Orchestrate.Tests.Utility;
using Xunit;

public class SearchTests : IClassFixture<ListTestFixture>
{
    Collection collection;
    string collectionName;
    Application application;

    public SearchTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
        collectionName = listTestFixture.CollectionName;
        application = listTestFixture.Application;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.SearchAsync<Product>(String.Empty));
        Assert.Equal("query", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.SearchAsync<Product>(null));
        Assert.Equal("query", exception.ParamName);
    }

    [Fact]
    public async void SearchStarSucceeds()
    {
        var searchResult = await collection.SearchAsync<Product>("*");
        var sortedList = searchResult.Items.OrderBy(result => result.Value.Id).ToList();

        Assert.Collection(sortedList,
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

        Assert.Equal(3, searchResult.TotalCount);
        Assert.Null(searchResult.Next);
        Assert.Null(searchResult.Prev);
    }

    [Fact]
    public async void ExplicitSearchForOneItemSucceeds()
    {
        var searchResult = await collection.SearchAsync<Product>("value.id: 1");

        Assert.Collection(searchResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            }
        );

        Assert.Equal(1, searchResult.TotalCount);
    }

    [Fact]
    public async void SupportsCustomSerialization()
    {
        var client = new Client(application, CustomSerializer.Create());
        var collectionName = Path.GetRandomFileName();
        var collection = client.GetCollection(collectionName);
        var key = Guid.NewGuid().ToString();

        try
        {
            await collection.AddOrUpdateAsync(key, new Product {Category = ProductCategory.Sprocket, Id = 1});

            var searchResult = await collection.SearchAsync<JObject>("value.id: 1");

            Assert.Collection(searchResult.Items,
                result =>
                {
                    Assert.Equal(key, result.PathMetadata.Key);
                    Assert.Equal(ProductCategory.Sprocket.ToString(), result.Value.Value<string>("category"));
                }
                );

            Assert.Equal(1, searchResult.TotalCount);
        }
        finally
        {
            await client.DeleteCollectionAsync(collectionName);
        }
    }

    [Fact]
    public async void SearchWithBooleanExpressionSucceeds()
    {
        var query = String.Format("value.description: (\"{0}\" AND NOT \"{1}\")", "Low Fat", "Americana");
        var searchResult = await collection.SearchAsync<Product>(query);
        var sortedList = searchResult.Items.OrderBy(result => result.Value.Id).ToList();

        Assert.Collection(sortedList,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            }
        );

        Assert.Equal(2, searchResult.TotalCount);
    }


    [Fact]
    public async void SearchWithSortSucceeds()
    {
        SearchOptions options = new SearchOptions(sort: "value.id:asc");
        var searchResult = await collection.SearchAsync<Product>("*", options);

        Assert.Collection(searchResult.Items,
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

        Assert.Equal(3, searchResult.TotalCount);
    }

    [Fact]
    public async void SearchWithLimitSucceeds()
    {
        SearchOptions options = new SearchOptions(limit: 2);
        var searchResult = await collection.SearchAsync<Product>("*", options);

        Assert.Equal(2, searchResult.Count);
        Assert.Equal(3, searchResult.TotalCount);
        Assert.Contains("offset=2", searchResult.Next);
        Assert.Null(searchResult.Prev);
        Assert.True(searchResult.HasNext());
    }

    [Fact]
    public async void SearchWithOffsetSucceeds()
    {
        SearchOptions options = new SearchOptions(offset: 2);
        var searchResult = await collection.SearchAsync<Product>("*", options);

        Assert.Equal(1, searchResult.Count);
        Assert.Equal(3, searchResult.TotalCount);
        Assert.Null(searchResult.Next);
        Assert.Contains("offset=0", searchResult.Prev);
        Assert.False(searchResult.HasNext());
    }

    [Fact]
    public async void SearchAllowsPagination()
    {
        var totalResults = new List<Product>();

        SearchOptions options = new SearchOptions(limit: 1);

        var searchResult = await collection.SearchAsync<Product>("*", options);
        totalResults.AddRange(searchResult);

        while (searchResult.HasNext())
        {
            searchResult = await searchResult.GetNextAsync();
            totalResults.AddRange(searchResult);
        }
        
        Assert.Collection(totalResults.OrderBy(r => r.Id),
            r => Assert.Equal(1, r.Id),
            r => Assert.Equal(2, r.Id),
            r => Assert.Equal(3, r.Id)
        );
    }

    [Fact]
    public async void ThrowsWhenNextPageNotAvailable()
    {
        SearchOptions options = new SearchOptions(limit: 100);

        var searchResult = await collection.SearchAsync<Product>("*", options);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => searchResult.GetNextAsync());
        Assert.Equal("There are no more items available in the search results.", ex.Message);
    }

    [Fact]
    public async void SearchSucceedsWithInvalidSort()
    {
        SearchOptions options = new SearchOptions(sort: ":(");
        var searchResults = await collection.SearchAsync<Product>("value.id: 1", options);

        Assert.Equal(1, searchResults.Count);
    }

    [Fact]
    public async void SearchFailsWithInvalidQuery()
    {
        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.SearchAsync<Product>(":("));

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
    }

    [Fact]
    public async void EnumerateSearchAsync()
    {
        var searchResult = await collection.SearchAsync<Product>("*");
        Assert.Equal(3, searchResult.Count);

        int count = 0;
        foreach (Product product in searchResult)
            count++;

        Assert.Equal(3, count);
    }


    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.TryAddAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

