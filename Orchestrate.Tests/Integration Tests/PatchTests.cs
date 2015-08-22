using System;
using Xunit;
using Orchestrate.Io;
using System.Collections.Generic;
using System.Net;
using NSubstitute;

public class PatchTests : IClassFixture<TestFixture>
{
    string collectionName;
    Collection collection;

    public PatchTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;

        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
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
        var application = Substitute.For<IApplication>();
        application.Key.Returns("HaHa");
        application.HostUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.PatchAsync("key", new List<PatchOperation>()));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}