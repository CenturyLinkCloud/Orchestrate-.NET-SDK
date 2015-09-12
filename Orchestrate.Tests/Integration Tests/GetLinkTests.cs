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
    string productKey;
    Client client;
    Product product;

    public GetLinkTests(GraphTestFixture testFixture)
    {
        client = testFixture.Client;
        userCollection = testFixture.UserCollection;
        userKey = testFixture.UserKey;
        productCollection = testFixture.Collection;
        productKey = testFixture.ProductKey;
        product = testFixture.Product;
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
            () => userCollection.GetLinkAsync<dynamic>(null, String.Empty, null));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>(String.Empty, String.Empty, null));
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>("1", null, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => userCollection.GetLinkAsync<dynamic>("1", String.Empty, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => userCollection.GetLinkAsync<dynamic>("1", "kind", null));
        Assert.Equal("destination node", exception.ParamName);
    }

    [Fact]
    public async void GetLinkSuccess()
    {
        GraphNode fromNode = new GraphNode { CollectionName = userCollection.CollectionName, Key = userKey };
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = productKey };

        try
        {
            await client.LinkAsync(fromNode, "purchased", toNode);

            var products = await userCollection.GetLinkAsync<Product>("1", "purchased");

            Assert.Equal(1, products.Count);
            Assert.Equal(product.Description, products.Items[0].Value.Description);
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
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
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = productKey };
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
        GraphNode toNode = new GraphNode { CollectionName = productCollection.CollectionName, Key = productKey };

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.GetLinkAsync<Product>("1", "kind", toNode));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
