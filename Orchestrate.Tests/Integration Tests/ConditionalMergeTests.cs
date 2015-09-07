using System;
using System.Dynamic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class ConditionalMergeTests : IClassFixture<ProductTestFixture>
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public ConditionalMergeTests(ProductTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        product = testFixture.Product;
        productKey = testFixture.Key;
    }

    [Fact]
    public async void MergeWithReferenceSucceeds()
    {
        var existingItem = await collection.GetAsync<Product>(productKey);
        string dateTime = DateTime.UtcNow.ToString("s");
        dynamic mergeItem = new ExpandoObject();
        mergeItem.releaseDate = dateTime;
        var kvMetaData = await collection.MergeAsync(productKey, mergeItem, existingItem.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<dynamic>(productKey);
        Assert.Equal(dateTime, kvObject.Value.releaseDate.ToString("s"));
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        dynamic mergeItem = new ExpandoObject();
        mergeItem.releaseDate = DateTime.UtcNow.ToString("s");

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.MergeAsync("2", mergeItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        dynamic mergeItem = new ExpandoObject();
        mergeItem.releaseDate = DateTime.UtcNow.ToString("s");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.MergeAsync(productKey, mergeItem, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
