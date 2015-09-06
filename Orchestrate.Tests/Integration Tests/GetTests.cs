using System;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class GetTests : IClassFixture<TestFixture>, IDisposable
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public GetTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);

        product = new Product { Id = 1, Name = "Bread", Description = "Whole grain bread", Price = 2.50M, Rating = 4 };
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
            () => collection.GetAsync<object>(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.GetAsync<object>(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void GetSuccess()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);

        Assert.Equal(collectionName, kvObject.CollectionName);
        Assert.Equal(productKey, kvObject.Key);
        Assert.True(kvObject.VersionReference.Length > 0);
        Assert.Empty(kvObject.Location);

        Product actualProduct = kvObject.Value;
        Assert.Equal(product.Id, actualProduct.Id);
        Assert.Equal(product.Name, actualProduct.Name);
    }

    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.GetAsync<object>("9999")
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
                                () => collection.GetAsync<object>("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
