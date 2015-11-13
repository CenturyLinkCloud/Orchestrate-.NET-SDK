using System;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Orchestrate.Io;
using Orchestrate.Tests.Utility;
using Xunit;

public class AddTests : IClassFixture<TestFixture>
{
    string collectionName;
    Collection collection;
    Application application;

    public AddTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        application = testFixture.Application;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddAsync<object>(null)
            );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<Product>(kvMetaData.Key);

        Product product = kvObject.Value;
        Assert.Equal(3, product.Id);
        Assert.Equal("Bread", product.Name);
    }

    [Fact]
    public async void SupportsCustomSerializedContent()
    {
        var client = new Client(application, CustomSerializer.Create());
        var collection = client.GetCollection(collectionName);

        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3, Category = ProductCategory.Widget};
        var kvMetaData = await collection.AddAsync(item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<Product>(kvMetaData.Key);

        Assert.Equal(ProductCategory.Widget.ToString(), JObject.Parse(kvObject.RawValue)["category"].Value<string>());

        Product product = kvObject.Value;
        Assert.Equal(3, product.Id);
        Assert.Equal("Bread", product.Name);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.AddAsync<object>("item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

