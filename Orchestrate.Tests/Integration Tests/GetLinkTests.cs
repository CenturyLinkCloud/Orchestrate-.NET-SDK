using System;
using System.Dynamic;
using System.Threading;
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

    [Fact(Skip = "Link with properties is not working correctly")]
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

            Assert.Equal(properties.rating, linkProperties.rating);
        }
        finally
        {
            await client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }
}
