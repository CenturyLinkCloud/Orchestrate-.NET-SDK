using Orchestrate.Io;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class ConditionalMergeTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public ConditionalMergeTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void MergeWithReferenceSucceeds()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var existingItem = await collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };
        var kvMetaData = await collection.MergeAsync("1", mergeItem, existingItem.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<TestData>("1");
        Assert.Contains("MergeValue", kvObject.RawValue);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.MergeAsync("2", mergeItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.MergeAsync("1", mergeItem, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
