using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class MergeTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public MergeTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.MergeAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.MergeAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.MergeAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void MergeSuccess()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var testData = await collection.GetAsync<TestData>("1");

        var item = new MergeTestData
        { MergeValue = "Merge Value" };

        var kvMetaData = await collection.MergeAsync(testData.Key, item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<TestData>("1");

        Assert.Contains("MergeValue", kvObject.RawValue);
    }

    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        Client client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.MergeAsync<string>("9999", "item")
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
                                () => collection.MergeAsync<object>("key", "item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}