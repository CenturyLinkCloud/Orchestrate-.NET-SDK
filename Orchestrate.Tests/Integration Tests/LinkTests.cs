using System;
using System.Dynamic;
using System.Net;
using Orchestrate.Io;
using Xunit;

public class LinkTests : IClassFixture<GraphTestFixture>
{
    GraphTestFixture testFixture;

    public LinkTests(GraphTestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.LinkAsync(null, String.Empty, null));
        Assert.Equal("fromNode", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Client.LinkAsync(new GraphNode(), String.Empty, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.LinkAsync(new GraphNode(), null, null));
        Assert.Equal("kind", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Client.LinkAsync(new GraphNode(), "1", null));
        Assert.Equal("toNode", exception.ParamName);
    }

    [Fact]
    public async void LinkSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        try
        {
            var kvMetaData = await testFixture.Client.LinkAsync(fromNode, "purchased", toNode);

            Assert.Equal(fromNode.CollectionName, kvMetaData.CollectionName);
            Assert.Equal(fromNode.Key, kvMetaData.Key);
            Assert.True(kvMetaData.VersionReference.Length > 0);
            var location = String.Format("/v0/{0}/{1}/relation/purchased/{2}/{3}",
                                         fromNode.CollectionName,
                                         fromNode.Key,
                                         toNode.CollectionName,
                                         toNode.Key);
            Assert.Equal(location, kvMetaData.Location);
        }
        finally
        {
            await testFixture.Client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void LinkWithPropertiesSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        dynamic properties = new ExpandoObject();
        properties.rating = "3 stars";

        try
        {
            var kvMetaData = await testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties);

            Assert.Equal(fromNode.CollectionName, kvMetaData.CollectionName);
            Assert.Equal(fromNode.Key, kvMetaData.Key);
            Assert.True(kvMetaData.VersionReference.Length > 0);
            var location = String.Format("/v0/{0}/{1}/relation/purchased/{2}/{3}",
                                         fromNode.CollectionName,
                                         fromNode.Key,
                                         toNode.CollectionName,
                                         toNode.Key);
            Assert.Equal(location, kvMetaData.Location);
        }
        finally
        {
            await testFixture.Client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void LinkWithInvalidFromCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = ":(", Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        try
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                                    () => testFixture.Client.LinkAsync(fromNode, "kind", toNode));

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
    public async void LinkWithInvalidKeyInFromCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = "9999" };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => testFixture.Client.LinkAsync(fromNode, "kind", toNode));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var errorMessage = String.Format("Key: 9999 was not found in collection: {0}", fromNode.CollectionName);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public async void LinkWithInvalidKeyInToCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = "9999" };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => testFixture.Client.LinkAsync(fromNode, "kind", toNode));

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        var errorMessage = String.Format("Key: 9999 was not found in collection: {0}", toNode.CollectionName);
        Assert.Equal(errorMessage, exception.Message);
    }

    [Fact]
    public async void LinkWithInvalidToCollectionReturnsNotFound()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = ":(", Key = testFixture.ProductKey };

        try
        {
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                                    () => testFixture.Client.LinkAsync(fromNode, "kind", toNode));

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
    public async void LinkWithVersionReferenceSucceeds()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };
        dynamic properties = new ExpandoObject();
        properties.rating = "3 stars";

        try
        {
            var kvMetadata = await testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties);

            properties.rating = "4 stars";
            await testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties, kvMetadata.VersionReference);

            var linkProperties = await testFixture.UserCollection.GetLinkAsync<dynamic>("1", "purchased", toNode);

            Assert.Equal((string)properties.rating, (string)linkProperties.rating);
        }
        finally
        {
            await testFixture.Client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void LinkWithVersionReferenceThrowsWithInvalidReference()
    {
        GraphNode fromNode = new GraphNode { CollectionName = testFixture.UserCollection.CollectionName, Key = testFixture.UserKey };
        GraphNode toNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = testFixture.ProductKey };
        dynamic properties = new ExpandoObject();
        properties.rating = "3 stars";

        try
        {
            var kvMetadata = await testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties);

            properties.rating = "4 stars";
            await testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties);

            properties.rating = "5 stars";
            var exception = await Assert.ThrowsAsync<RequestException>(
                    () => testFixture.Client.LinkAsync(fromNode, "purchased", toNode, properties, kvMetadata.VersionReference));

            Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
        }
        finally
        {
            await testFixture.Client.UnlinkAsync(fromNode, "purchased", toNode);
        }
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var graphNode = new GraphNode { CollectionName = testFixture.Collection.CollectionName, Key = "key" };
        var application = new Application("HaHa");
        var client = new Client(application);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => client.LinkAsync(graphNode, "relation", graphNode));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}