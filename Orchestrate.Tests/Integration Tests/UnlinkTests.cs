using System;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class UnlinkTests : IClassFixture<GraphTestFixture>
{
    GraphTestFixture testFixture;

    public UnlinkTests(GraphTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.UnlinkAsync(null, String.Empty, null));
        Assert.Equal("fromNode", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Client.UnlinkAsync(new GraphNode(), String.Empty, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.UnlinkAsync(new GraphNode(), null, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.UnlinkAsync(new GraphNode(), "1", null));
        Assert.Equal("toNode", exception.ParamName);
    }

    [Fact]
    public async void UnLinkSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };
        await testFixture.Client.LinkAsync(fromNode, "purchased", toNode);

        await testFixture.Client.UnlinkAsync(fromNode, "purchased", toNode);
    }

    [Fact]
    public async void UnlinkWithInvalidFromCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = ":(", Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        try
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                                    () => testFixture.Client.UnlinkAsync(fromNode, "kind", toNode));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            var errorMessage = String.Format("Key: 1 was not found in collection: {0}", fromNode.CollectionName);
            Assert.Equal(errorMessage, exception.Message);
        }
        finally
        {
            await testFixture.Client.DeleteCollectionAsync(":(");
        }
    }

    [Fact]
    public async void UnkinkWithInvalidKeyInFromCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = "9999" };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => testFixture.Client.UnlinkAsync(fromNode, "kind", toNode));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var errorMessage = String.Format("Key: 9999 was not found in collection: {0}", fromNode.CollectionName);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public async void UnlinkWithInvalidKeyInToCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = "9999" };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => testFixture.Client.UnlinkAsync(fromNode, "kind", toNode));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var errorMessage = String.Format("Key: 9999 was not found in collection: {0}", toNode.CollectionName);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public async void UnlinkWithInvalidToCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = ":(", Key = testFixture.ProductKey };

        try
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                                    () => testFixture.Client.UnlinkAsync(fromNode, "kind", toNode));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            var errorMessage = String.Format("Key: 1 was not found in collection: {0}", toNode.CollectionName);
            Assert.Equal(errorMessage, exception.Message);
        }
        finally
        {
            await testFixture.Client.DeleteCollectionAsync(":(");
        }
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var graphNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = "key" };
        var application = new Application("HaHa");
        var client = new Client(application);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => client.UnlinkAsync(graphNode, "relation", graphNode));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}