using System;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class UpdateTests : IClassFixture<ProductTestFixture>
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;

    public UpdateTests(ProductTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        product = testFixture.Product;
        productKey = testFixture.Key;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.UpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.UpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.UpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var product = kvObject.Value;
        product.Description = "Updated Description!";

        var kvMetaData = await collection.UpdateAsync(productKey, product);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(productKey, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        kvObject = await collection.GetAsync<Product>(productKey);
        product = kvObject.Value;
        Assert.Equal("Updated Description!", product.Description);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.UpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionIfKeyIsNotPresent()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var product = kvObject.Value;
        product.Description = "Updated Description";

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.UpdateAsync("2", product)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await collection.GetAsync<Product>(productKey);
        var product = kvObject.Value;
        product.Description = "Updated Description";

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.UpdateAsync(productKey, product, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


