using System;
using System.Dynamic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class MergeTests : IClassFixture<TestFixture>, IDisposable
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public MergeTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);

        product = new Product { Id = 1, Name = "Bread", Description = "Grain bread", Price = 2.50M, Rating = 4 };
        productKey = "1";
        AsyncHelper.RunSync(() => collection.TryAddAsync(productKey, product));
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => collection.DeleteAsync(productKey));
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.MergeAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.MergeAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.MergeAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void MergeSuccess()
    {
        var productMetaData = await collection.GetAsync<Product>(productKey);

        string dateTime = DateTime.UtcNow.ToString("s");
        dynamic mergeItem = new ExpandoObject();
        mergeItem.releaseDate = dateTime;

        var kvMetaData = await collection.MergeAsync(productMetaData.Key, mergeItem);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<dynamic>(productKey);
        var product = kvObject.Value;
        Assert.Equal(dateTime, product.releaseDate.ToString("s"));
    }

    [Fact]
    public async void NonExistentKeyThrowsNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.MergeAsync<string>("9999", "item")
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", "9999", collectionName);
        Assert.Equal(expected, exception.Message);
    }


    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.MergeAsync<object>("key", "item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}