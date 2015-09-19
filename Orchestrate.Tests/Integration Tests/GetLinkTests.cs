using System;
using System.Dynamic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class GetLinkTests : IClassFixture<GraphTestFixture>
{
    Collection userCollection;
    string userKey;
    Collection productCollection;
    string breadKey;
    Client client;
    Product bread;
    Product milk;
    string milkKey;

    public GetLinkTests(GraphTestFixture testFixture)
    {
        client = testFixture.Client;
        userCollection = testFixture.UserCollection;
        userKey = testFixture.UserKey;
        productCollection = testFixture.Collection;
        breadKey = testFixture.BreadKey;
        bread = testFixture.Bread;
        milkKey = testFixture.MilkKey;
        milk = testFixture.Milk;
    }

    [Fact]
    public async void GetGuards()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>(null, String.Empty));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>(String.Empty, String.Empty));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>("1", null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>("1", String.Empty));
        Assert.Equal("kind", exception.ParamName);
    }

    [Fact]
    public async void GetWithPropertiesGuards()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>(null, String.Empty, (GraphNode)null));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>(String.Empty, String.Empty, (GraphNode)null));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>("1", null, (GraphNode)null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>("1", String.Empty, (GraphNode)null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>("1", "kind", (GraphNode)null));
        Assert.Equal("destination node", exception.ParamName);
    }

    [Fact]
    public async void GetLinkSuccess()
    {
        GraphNode fromNode = new GraphNode { CollectionName = userCollection.CollectionName, Key = userKey };
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = breadKey };

        try
        {
            await client.LinkAsync(fromNode, "purchased", toNode);

            var products = await userCollection.GetLinkAsync<Product>("1", "purchased");

            Assert.Equal(1, products.Count);
            Assert.Equal(bread.Description, products.Items[0].Value.Description);
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void GetMultipleLinksWithLimitSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = userCollection.CollectionName, Key = userKey };
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = breadKey };
        GraphNode milkNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = milkKey };

        try
        {
            await client.LinkAsync(fromNode, "purchased", toNode);
            await client.LinkAsync(fromNode, "purchased", milkNode);

            var linkOptions = new LinkOptions(limit: 1);
            var products = await userCollection.GetLinkAsync<Product>("1", "purchased", linkOptions);

            Assert.Equal(1, products.Count);
            Assert.Contains("offset=1&limit=1", products.Next);
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
            await client.UnlinkAsync(fromNode, "purchased", milkNode);
        }
    }

    [Fact]
    public async void GetMultipleLinksWithOffsetSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = userCollection.CollectionName, Key = userKey };
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = breadKey };
        GraphNode milkNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = milkKey };

        try
        {
            await client.LinkAsync(fromNode, "purchased", toNode);
            await client.LinkAsync(fromNode, "purchased", milkNode);

            var linkOptions = new LinkOptions(offset: 1);
            var products = await userCollection.GetLinkAsync<Product>("1", "purchased", linkOptions);

            Assert.Null(products.Next);
            Assert.Collection(products.Items,
                result =>
                {
                    Assert.Equal(2, result.Value.Id);
                    Assert.Equal("2", result.PathMetadata.Key);
                    Assert.Equal("2% Milk", result.Value.Description);
                }
            );
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
            await client.UnlinkAsync(fromNode, "purchased", milkNode);
        }
    }

    [Fact]
    public async void GetInvalidFromKeyThrowsNotFoundException()
    {

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => userCollection.GetLinkAsync<Product>("9999", "kind"));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var errorMessage = String.Format("Key: 9999 was not found in collection: {0}", userCollection.CollectionName);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public async void GetInvalidToKeyThrowsRequestException()
    {
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = "9999" };

        var exception = await Assert.ThrowsAsync<RequestException>(
                                () => userCollection.GetLinkAsync<Product>("1", "purchased", toNode));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        Assert.Equal("The requested graph relationship could not be found.", exception.Message);
    }

    [Fact]
    public async void GetInvalidToCollectionThrowsRequestException()
    {
        GraphNode toNode = new GraphNode { CollectionName = ":(", Key = "1" };

        try
        {
            var exception = await Assert.ThrowsAsync<RequestException>(
                                    () => userCollection.GetLinkAsync<Product>("1", "purchased", toNode));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("The requested graph relationship could not be found.", exception.Message);
        }
        finally
        {
            await client.DeleteCollectionAsync(":(");
        }
    }

    [Fact]
    public async void GetLinkWithPropertiesSuccess()
    {
        GraphNode fromNode = new GraphNode { CollectionName = userCollection.CollectionName, Key = userKey };
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = breadKey };
        dynamic properties = new ExpandoObject();
        properties.rating = "3 stars";

        try
        {
            await client.LinkAsync(fromNode, "purchased", toNode, properties);

            var linkProperties = await userCollection.GetLinkAsync<dynamic>("1", "purchased", toNode);

            Assert.Equal((string)properties.rating, (string)linkProperties.rating);
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection("collection");

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.GetLinkAsync<Product>("1", "kind"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void GetWithPropertiesInvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);
        var collection = client.GetCollection("collection");
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = breadKey };

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.GetLinkAsync<Product>("1", "kind", toNode));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
