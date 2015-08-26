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
        collection = listTestFixture.Client.GetCollection(listTestFixture.CollectionName);
        collectionName = listTestFixture.CollectionName;

        WaitForConsistency();
    }

    private void WaitForConsistency()
    {
        int count = 0;
        SearchResults<TestData> searchResults;
        do
        {
            searchResults = 
                AsyncHelper.RunSync<SearchResults<TestData>>(() => collection.SearchAsync<TestData>("*"));
            count++;
        }
        while (searchResults.Count != 3 && count < 25);
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

    [Fact]
    public async void SearchStarSucceeds()
    {
        var searchResult = await collection.SearchAsync<TestData>("*");
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
    }

    [Fact]
    public async void ExplicitSearchForOneItemSucceeds()
    {
        var searchResult = await collection.SearchAsync<TestData>("value.Id: 1");

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
        var query = String.Format("value.Value: (\"{0}\" AND NOT \"{1}\")", "Initial", "#2");
        var searchResult = await collection.SearchAsync<TestData>(query);
        var sortedList = searchResult.Items.OrderBy(result => result.Value.Id).ToList();

        Assert.Collection(sortedList,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.OrchestratePath.Key);
            }
        );

        Assert.Equal(2, searchResult.TotalCount);
    }


    [Fact]
    public async void SearchWithSortSucceeds()
    {
        var searchResult = await collection.SearchAsync<TestData>("*", sort: "value.Id:asc");

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
    public async void SearchSucceedsWithInvalidSort()
    {
        var searchResults = await collection.SearchAsync<TestData>("value.Id: 1", sort: ":(");

        Assert.Equal(1, searchResults.Count);
    }

    [Fact]
    public async void SearchFailsWithInvalidQuery()
    {
        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.SearchAsync<TestData>(":("));

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

