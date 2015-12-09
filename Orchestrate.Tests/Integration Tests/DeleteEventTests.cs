using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orchestrate.Io;
using Orchestrate.Tests.Models;
using Xunit;

public class DeleteEventTests: IClassFixture<EventTestFixture>
{
    string collectionName;
    Collection collection;
    string minivanKey;

    public DeleteEventTests(EventTestFixture testFixture)
    {
        this.collectionName = testFixture.CollectionName;
        this.collection = testFixture.Collection;
        this.minivanKey = testFixture.MinivanKey;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
           () => collection.DeleteEventAsync(string.Empty, "eventType", default(DateTimeOffset), default(long))
       );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.DeleteEventAsync(null, "eventType", default(DateTimeOffset), default(long))
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
           () => collection.DeleteEventAsync("key", string.Empty, default(DateTimeOffset), default(long))
        );
        Assert.Equal("eventType", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.DeleteEventAsync("key", null, default(DateTimeOffset), default(long))
        );
        Assert.Equal("eventType", exception.ParamName);
    }

    [Fact]
    public async void DeleteSuccess()
    {
        var eventType = Guid.NewGuid().ToString();
        var sale = new ProductSale {Amount = 25000.00M, Notes = "Sold for a song."};
        var eventMetadata = await collection.AddEventAsync(minivanKey, eventType, sale);

        await collection.DeleteEventAsync(minivanKey, eventType, eventMetadata.Timestamp, eventMetadata.Ordinal);

        var events = collection.GetEventsAsync<ProductSale>(minivanKey, eventType);
        Assert.Empty(events.Result);
    }
}
