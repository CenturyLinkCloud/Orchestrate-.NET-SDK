using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class AddOrUpdateTests : IClassFixture<TestFixture>, IDisposable
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public AddOrUpdateTests(TestFixture testFixture)
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
            () => collection.AddOrUpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddOrUpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddOrUpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var key = Guid.NewGuid().ToString();

        var kvMetaData = await collection.AddOrUpdateAsync(key, item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(key, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var retrievedProduct = kvObject.Value;
        Assert.Equal(product.Description, retrievedProduct.Description);
        retrievedProduct.Description = "Updated Description";

        var kvMetaData = await collection.AddOrUpdateAsync(productKey, retrievedProduct);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(productKey, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        kvObject = await collection.GetAsync<Product>(productKey);
        retrievedProduct = kvObject.Value;
        Assert.Equal("Updated Description", retrievedProduct.Description);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);        

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.AddOrUpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

