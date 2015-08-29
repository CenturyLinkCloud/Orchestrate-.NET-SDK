﻿using System;
using Orchestrate.Io;
using Xunit;

public class ListTests : IClassFixture<ListTestFixture>
{
    Collection collection;

    public ListTests(ListTestFixture listTestFixture)
    {
        collection = listTestFixture.Collection;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<Product>(-1));
        Assert.Equal("limit", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => collection.ListAsync<Product>(101));
        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public async void ListAsync()
    {
        var listResult = await collection.ListAsync<Product>();

        Assert.Collection(listResult.Items,
            result =>
            {
                Assert.Equal(1, result.Value.Id);
                Assert.Equal("1", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(2, result.Value.Id);
                Assert.Equal("2", result.OrchestratePath.Key);
            },
            result =>
            {
                Assert.Equal(3, result.Value.Id);
                Assert.Equal("3", result.OrchestratePath.Key);
            }
        );

        Assert.Null(listResult.Next);
    }

    [Fact]
    public async void ListWithLimitFewerThanNumberOfElementsAsync()
    {
        var listResult = await collection.ListAsync<Product>(2);

        Assert.Collection(listResult.Items,
            result => Assert.Equal(1, result.Value.Id),
            result => Assert.Equal(2, result.Value.Id)
        );

        Assert.Contains("afterKey=2", listResult.Next);
    }
}