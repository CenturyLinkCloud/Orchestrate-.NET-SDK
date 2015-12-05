using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchestrate.Io;
using Orchestrate.Tests.Models;
using Xunit;

public class AddEventTests : IClassFixture<EventTestFixture>
{
    string collectionName;
    Collection collection;
    Application application;
    string minivanKey;

    public AddEventTests(EventTestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
        application = testFixture.Application;
        minivanKey = testFixture.MinivanKey;
    }

    [Fact]
    public async void Guards()
    {
        var @event = new ProductSale();
        var eventKey = "productSale";

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.AddEventAsync(string.Empty, eventKey, @event)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddEventAsync(null, eventKey, @event)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.AddEventAsync(minivanKey, string.Empty, @event)
        );
        Assert.Equal("eventType", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddEventAsync(minivanKey, null, @event)
        );
        Assert.Equal("eventType", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddEventAsync<object>(minivanKey, eventKey, null)
        );
        Assert.Equal("event", exception.ParamName);
    }

    [Fact]
    public async void AddEventWithoutTimestampSucceeds()
    {
        var sale = new ProductSale { Amount = 25000.00M, Notes = "Sold for a song."};
        var eventMetaData = await collection.AddEventAsync(minivanKey, "productSale", sale);
        var now = DateTimeOffset.UtcNow;

        Assert.Equal(collectionName, eventMetaData.CollectionName);
        Assert.Equal("productSale", eventMetaData.EventType);
        Assert.Equal(minivanKey, eventMetaData.Key);
        Assert.InRange(eventMetaData.Timestamp, now.AddMinutes(-5), now.AddMinutes(5));
        Assert.True(eventMetaData.Ordinal > 0);
    }

    [Fact]
    public async void AddEventSetsCustomTimestamp()
    {
        var sale = new ProductSale { Amount = 25000.00M, Notes = "Sold for a song." };
        var timestamp = new DateTimeOffset(2015, 3, 14, 15, 9, 26, TimeSpan.Zero);
        var eventMetaData = await collection.AddEventAsync(minivanKey, "productSale", sale, timestamp);

        Assert.Equal(timestamp, eventMetaData.Timestamp);
    }

}
