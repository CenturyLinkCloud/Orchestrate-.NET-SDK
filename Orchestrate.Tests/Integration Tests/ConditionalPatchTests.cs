using Orchestrate.Io;
using System.Collections.Generic;
using System.Net;
using Xunit;

public class ConditionalPatchTests : IClassFixture<TestFixture>
{
    TestFixture testFixture; 

    public ConditionalPatchTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void PatchWithReferenceSucceeds()
    {
        var existingItem = await testFixture.Collection.GetAsync<TestData>("1");

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var kvMetaData = await testFixture.Collection.PatchAsync("1", patchOperations.ToArray(), existingItem.VersionReference);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        Assert.Equal("New and improved value", testData.Value);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => testFixture.Collection.PatchAsync("2", patchOperations.ToArray(), kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "New and improved value" });

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.PatchAsync("1", patchOperations.ToArray(), "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
