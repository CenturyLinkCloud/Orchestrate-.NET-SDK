using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Orchestrate.Io;
using Orchestrate.Tests.Utility;
using Xunit;

public class AddOrUpdateTests : IClassFixture<ProductTestFixture>
{
    string collectionName;
    Collection collection;
    Product product;
    string productKey;
    Application application;

    public AddOrUpdateTests(ProductTestFixture testFixture)
    {
        application = testFixture.Application;
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        product = testFixture.Product;
        productKey = testFixture.Key;
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
    public async void SupportsCustomSerialization()
    {
        var client = new Client(application, CustomSerializer.Create());
        var collection = client.GetCollection(collectionName);
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3, Category = ProductCategory.Widget };
        var key = Guid.NewGuid().ToString();

        var kvMetaData = await collection.AddOrUpdateAsync(key, item);
        
        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(key, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);

        var kvObject = await collection.GetAsync<Product>(kvMetaData.Key);

        Assert.Equal(ProductCategory.Widget.ToString(), JObject.Parse(kvObject.RawValue)["category"].Value<string>());
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

