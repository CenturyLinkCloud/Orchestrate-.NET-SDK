﻿using System;
using System.Net;
using Newtonsoft.Json.Linq;
using Orchestrate.Io;
using Orchestrate.Tests.Utility;
using Xunit;

public class TryAddTests : IClassFixture<TestFixture>
{
    string collectionName;
    Collection collection;
    Application application;

    public TryAddTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        application = testFixture.Application;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.TryAddAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.TryAddAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.TryAddAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }


    [Fact]
    public async void TryAddSucceeds()
    {
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.TryAddAsync("88", item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("88", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void TryAddFailsWithExistingKey()
    {
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.TryAddAsync<Product>(kvMetaData.Key, item));

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }

    [Fact]
    public async void SupportsCustomSerialization()
    {
        var client = new Client(application, CustomSerializer.Create());
        var collection = client.GetCollection(collectionName);

        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3, Category = ProductCategory.Widget };
        var kvMetaData = await collection.TryAddAsync("99", item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("99", kvMetaData.Key);

        Assert.True(kvMetaData.VersionReference.Length > 0);

        var kvObject = await collection.GetAsync<JObject>(kvMetaData.Key);
        Assert.Equal(ProductCategory.Widget.ToString(), kvObject.Value["category"].Value<string>());
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.TryAddAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}


