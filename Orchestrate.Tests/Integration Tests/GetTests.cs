using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class GetTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public GetTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.GetAsync<object>(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.GetAsync<object>(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void GetSuccess()
    {
        Client client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");

        Assert.Equal(collectionName, kvObject.CollectionName);
        Assert.Equal("1", kvObject.Key);
        Assert.True(kvObject.VersionReference.Length > 0);
        Assert.Empty(kvObject.Location);

        TestData testData = kvObject.Value;
        Assert.Equal(1, testData.Id);
        Assert.Equal("Initial Test Data", testData.Value);
    }

    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        Client client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.GetAsync<object>("9999")
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", "9999", collectionName);
        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.GetAsync<object>("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
