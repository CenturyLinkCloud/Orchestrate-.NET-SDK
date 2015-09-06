using System;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class HistoryTests : IClassFixture<TestFixture>, IDisposable
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public HistoryTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);

        product = new Product { Id = 1, Name = "Bread", Description = "Grain Bread", Price = 2.50M, Rating = 4 };
        productKey = "1";
        AsyncHelper.RunSync(() => collection.TryAddAsync(productKey, product));

        AsyncHelper.RunSync(() => collection.DeleteAsync("1", purge: false));

        product.Description = "Whole Grain Bread";
        AsyncHelper.RunSync(() => collection.AddOrUpdateAsync(productKey, product));
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => collection.DeleteAsync(productKey));
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.HistoryAsync<Product>(String.Empty));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.HistoryAsync<Product>(null));
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void HistoryListWithNoValuesSucceeds()
    {
        var listResults = await collection.HistoryAsync<Product>(productKey);

        Assert.Collection(listResults.Items,
            result =>
            {
                Assert.Null(result.Value);
                Assert.False(result.PathMetadata.Tombstone);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Null(result.Value);
                Assert.True(result.PathMetadata.Tombstone);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Null(result.Value);
                Assert.False(result.PathMetadata.Tombstone);
                Assert.Equal("1", result.PathMetadata.Key);
            }
        );
    }

    [Fact]
    public async void HistoryListWithValuesSucceeds()
    {
        var historyOptions = new HistoryOptions(values: true);
        var listResults = await collection.HistoryAsync<Product>(productKey, historyOptions);

        Assert.Collection(listResults.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
                Assert.Equal("Whole Grain Bread", result.Value.Description);
            },
            result =>
            {
                Assert.Null(result.Value);
                Assert.True(result.PathMetadata.Tombstone);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
                Assert.Equal("Grain Bread", result.Value.Description);
            }
        );
    }

    [Fact]
    public async void HistoryWithLimitSucceeds()
    {
        var options = new HistoryOptions(limit: 1);
        var listResult = await collection.HistoryAsync<Product>("1", options);

        Assert.Equal(1, listResult.Count);
        Assert.Contains("offset=1&limit=1", listResult.Next);
    }

    [Fact]
    public async void HistoryWithOffsetSucceeds()
    {
        var options = new HistoryOptions(offset: 2, values: true);
        var listResult = await collection.HistoryAsync<Product>("1", options);

        Assert.Null(listResult.Next);
        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
                Assert.Equal("Grain Bread", result.Value.Description);
            }
        );
    }

    [Fact]
    public async void GetInitialVersion()
    {
        var listResult = await collection.HistoryAsync<Product>("1");
        var listItem = listResult.Items[listResult.Count-1];

        var product = await collection.GetAsync<Product>("1", listItem.PathMetadata.VersionReference);
        Assert.Equal("Grain Bread", product.Value.Description);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.HistoryAsync<object>("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}