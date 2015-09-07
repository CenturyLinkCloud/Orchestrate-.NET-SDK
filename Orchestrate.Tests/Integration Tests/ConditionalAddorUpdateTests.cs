using System.Net;
using Orchestrate.Io;
using Xunit;

public class ConditionalAddOrUpdateTests : IClassFixture<ProductTestFixture>
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public ConditionalAddOrUpdateTests(ProductTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        product = testFixture.Product;
        productKey = testFixture.Key;
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceSucceeds()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var updatedItem = new Product { Id = 1, Name = "Updated Bread" };
        var kvMetaData = await collection.AddOrUpdateAsync<Product>(productKey, updatedItem, kvObject.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(productKey, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);

        var updatedObject = await collection.GetAsync<Product>(productKey);
        var product = updatedObject.Value;
        Assert.Equal(1, product.Id);
        Assert.Equal("Updated Bread", product.Name);
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceThrowsWithInvalidReference()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var updatedItem = new Product { Id = 1, Name = "Updated Bread" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.AddOrUpdateAsync("2", updatedItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


