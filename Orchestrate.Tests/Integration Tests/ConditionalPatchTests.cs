using Orchestrate.Io;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class ConditionalPatchTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public ConditionalPatchTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void PatchWithReferenceSucceeds()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var existingItem = await collection.GetAsync<TestData>("1");

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var kvMetaData = await collection.PatchAsync("1", patchOperations.ToArray(), existingItem.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        Assert.Equal("New and improved value", testData.Value);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.PatchAsync("2", patchOperations.ToArray(), kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.PatchAsync("1", patchOperations.ToArray(), "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
