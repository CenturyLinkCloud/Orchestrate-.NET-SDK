using Xunit;
using Orchestrate.Io;
using System.Net;
using System;

public class TryAddTests : IClassFixture<TestFixture>
{
    readonly string collectionName;

    public TryAddTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.TryAdd<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.TryAdd<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.TryAdd<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }


    [Fact]
    public async void TryAddSucceeds()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);
        var item = new TestData { Id = 88, Value = "Test Value 88" };

        var kvMetaData = await collection.TryAdd<TestData>("88", item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("88", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void TryAddFailsWithExistingKey()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);
        var item = new TestData { Id = 88, Value = "Test Value 88" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.TryAdd<TestData>("1", item));

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.TryAdd<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}


