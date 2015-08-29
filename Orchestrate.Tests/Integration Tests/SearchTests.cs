using NSubstitute;
using Orchestrate.Io;
using System;
using System.Linq;
using System.Net;
using Xunit;

public class SearchTests : IClassFixture<ListTestFixture>
{
    Collection collection;
    string collectionName;

    public SearchTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
        collectionName = listTestFixture.CollectionName;

        SearchHelper.WaitForConsistency(collection, 3);
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
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.OrchestratePath.Key);
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
                Assert.Equal("1", result.OrchestratePath.Key);
            }
        );

        Assert.Equal(1, searchResult.TotalCount);
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
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.OrchestratePath.Key);
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
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.OrchestratePath.Key);
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
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = Substitute.For<IApplication>();
        application.Key.Returns("HaHa");
        application.HostUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.TryAddAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

