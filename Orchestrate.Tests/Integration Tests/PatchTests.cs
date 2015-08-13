using System;
using Xunit;
using Orchestrate.Io;
using System.Collections.Generic;
using System.Net;

public class PatchTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public PatchTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.PatchAsync(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.PatchAsync(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.PatchAsync("jguids", null)
        );
        Assert.Equal("operations", exception.ParamName);
    }

    [Fact]
    public async void PatchSuccess()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Value", Value = "Replaced Test Data" });

        var kvMetaData = await collection.PatchAsync("1", patchOperations);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        Assert.Equal("Replaced Test Data", testData.Value);
    }

    [Fact]
    public async void InvalidPathThrowsPatchConflictException()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "replace", Path = "/Huh", Value = "Replaced Test Data" });

        var exception = await Assert.ThrowsAsync<PatchConflictException>(
            () => collection.PatchAsync("1", patchOperations)
        );

        Assert.Equal(HttpStatusCode.Conflict, exception.StatusCode);
        Assert.Equal("patch_conflict", exception.PatchConflict.Code);
        Assert.Equal("/Huh", exception.PatchConflict.Details.Op.Path);
    }

    [Fact]
    public async void InvalidOperationThrowsBadRequestException()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        List<PatchOperation> patchOperations = new List<PatchOperation>();
        patchOperations.Add(new PatchOperation<string>
        { Operation = "huh", Path = "/Value", Value = "Replaced Test Data" });

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => collection.PatchAsync("1", patchOperations)
        );

        Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
        Assert.Contains("huh", exception.Message);
    }


    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        Client client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.PatchAsync("9999", new List<PatchOperation>())
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
                                () => collection.PatchAsync("key", new List<PatchOperation>()));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}