using System;
using System.Collections.Generic;
using Orchestrate.Io;
using Orchestrate.Tests.Models;
using Xunit;

public class GetEventsTests : IClassFixture<EventTestFixture>
{
    string collectionName;
    Collection collection;
    Application application;
    string minivanKey;

    public GetEventsTests(EventTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        application = testFixture.Application;
        minivanKey = testFixture.MinivanKey;
    }

    [Fact]
    public async void Guards()
    {
        var eventKey = "productSale";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.GetEventsAsync<ProductSale>(string.Empty, eventKey)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.GetEventsAsync<ProductSale>(null, eventKey)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.GetEventsAsync<ProductSale>(minivanKey, string.Empty)
        );
        Assert.Equal("eventType", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.GetEventsAsync<ProductSale>(minivanKey, null)
        );
        Assert.Equal("eventType", exception.ParamName);
    }

    [Fact]
    public async void ReturnsAddedEvents()
    {
        var sales = new[]
        {
            new ProductSale() {Amount = 25000.00M, Notes = "Hot van!"},
            new ProductSale() {Amount = 22000.00M, Notes = "Meh"},
            new ProductSale() {Amount = 15000.00M, Notes = "What a deal!"}
        };

        var eventType = Guid.NewGuid().ToString();

        foreach (var sale in sales)
        {
            await collection.AddEventAsync(minivanKey, eventType, sale);
        }

        var results = await collection.GetEventsAsync<ProductSale>(minivanKey, eventType);

        Assert.Equal(3, results.Count);
        Assert.Collection(results,
            r => Assert.Equal(sales[2].Amount, r.Amount),
            r => Assert.Equal(sales[1].Amount, r.Amount),
            r => Assert.Equal(sales[0].Amount, r.Amount)
        );

        var now = DateTimeOffset.UtcNow;
        Assert.Collection(results.Items,
            i =>
            {
                Assert.True(i.PathMetadata.Ordinal > 0);
                Assert.True(i.Ordinal > 0);
                Assert.NotNull(i.PathMetadata.OrdinalString);
                Assert.InRange(i.PathMetadata.Timestamp, now.AddHours(-1), now.AddHours(1));
                Assert.InRange(i.Timestamp, now.AddHours(-1), now.AddHours(1));
            },
            i =>
            {
                Assert.True(i.PathMetadata.Ordinal > 0);
                Assert.True(i.Ordinal > 0);
                Assert.NotNull(i.PathMetadata.OrdinalString);
                Assert.InRange(i.PathMetadata.Timestamp, now.AddHours(-1), now.AddHours(1));
                Assert.InRange(i.Timestamp, now.AddHours(-1), now.AddHours(1));
            },
            i =>
            {
                Assert.True(i.PathMetadata.Ordinal > 0);
                Assert.True(i.Ordinal > 0);
                Assert.NotNull(i.PathMetadata.OrdinalString);
                Assert.InRange(i.PathMetadata.Timestamp, now.AddHours(-1), now.AddHours(1));
                Assert.InRange(i.Timestamp, now.AddHours(-1), now.AddHours(1));
            }
        );
    }

    [Fact]
    public async void RespectsLimit()
    {
        var sales = new[]
        {
            new ProductSale() {Amount = 25000.00M, Notes = "Hot van!"},
            new ProductSale() {Amount = 22000.00M, Notes = "Meh"},
            new ProductSale() {Amount = 15000.00M, Notes = "What a deal!"}
        };

        var options = new EventsOptions(limit: 1);
        var eventType = Guid.NewGuid().ToString();

        foreach (var sale in sales)
        {
            await collection.AddEventAsync(minivanKey, eventType, sale);
        }

        var results = await collection.GetEventsAsync<ProductSale>(minivanKey, eventType, options);

        Assert.Collection(results,
            r => Assert.Equal(sales[2].Amount, r.Amount)
        );
    }

    [Fact]
    public async void AllowsPagination()
    {
        var sales = new[]
        {
            new ProductSale() {Amount = 25000.00M, Notes = "Hot van!"},
            new ProductSale() {Amount = 22000.00M, Notes = "Meh"},
            new ProductSale() {Amount = 15000.00M, Notes = "What a deal!"}
        };

        var totalResults = new List<ProductSale>();
        var options = new EventsOptions(limit: 1);
        var eventType = Guid.NewGuid().ToString();

        foreach (var sale in sales)
        {
            await collection.AddEventAsync(minivanKey, eventType, sale);
        }

        var results = await collection.GetEventsAsync<ProductSale>(minivanKey, eventType, options);
        totalResults.AddRange(results);

        while (results.HasNext())
        {
            results = await results.GetNextAsync();
            totalResults.AddRange(results);
        }

        Assert.Collection(totalResults,
            r => Assert.Equal(sales[2].Amount, r.Amount),
            r => Assert.Equal(sales[1].Amount, r.Amount),
            r => Assert.Equal(sales[0].Amount, r.Amount)
        );
    }
}