﻿using System;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class InclusiveListTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public InclusiveListTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.InclusiveListAsync<Product>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.InclusiveListAsync<Product>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void StartKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<Product>(startKey: "1");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void EndKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<Product>(endKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void StartKeyAndEndKeyAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<Product>(startKey: "1",
                                                         endKey: "2");

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.PathMetadata.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.PathMetadata.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void StartKeyGreaterThanExistingKeysAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<Product>(startKey: "4");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidKeysAsync()
    {
        var listResult =
            await collection.InclusiveListAsync<Product>(startKey: "3",
                                                         endKey: "1");

        Assert.Equal(0, listResult.Count);
        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection("collection");

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.InclusiveListAsync<Product>());

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}