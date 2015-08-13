using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class AddorUpdateTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public AddorUpdateTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.AddorUpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddorUpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddorUpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        Client client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var item = new TestData { Id = 3, Value = "Added Object" };
        var key = Guid.NewGuid().ToString();

        var kvMetaData = await collection.AddorUpdateAsync<TestData>(key, item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal(key, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        Assert.Equal("Initial Test Data", testData.Value);
        testData.Value = "Updated Test Data";

        var kvMetaData = await collection.AddorUpdateAsync<TestData>("1", testData);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("1", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        kvObject = await collection.GetAsync<TestData>("1");
        testData = kvObject.Value;
        Assert.Equal("Updated Test Data", testData.Value);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.AddorUpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

