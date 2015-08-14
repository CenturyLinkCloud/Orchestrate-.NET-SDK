using System;
using Orchestrate.Io;
using Xunit;
using System.Net;

public class DeleteTests : IClassFixture<TestFixture>
{
    readonly string collectionName;

    public DeleteTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.DeleteAsync(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.DeleteAsync(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void DeleteSuccessPurge()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await collection.AddorUpdateAsync("3", item);

        await collection.DeleteAsync("3");

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => collection.GetAsync<object>("3", kvMetaData.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", "3", collectionName);
        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public async void DeleteSuccessNoPurge()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await collection.AddorUpdateAsync("3", item);

        await collection.DeleteAsync("3", purge: false);

        var old = await collection.GetAsync<TestData>("3", kvMetaData.VersionReference);
        Assert.Equal(3, old.Value.Id);
    }

    [Fact]
    public async void DeleteNonExistantKeySuccess()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        await collection.DeleteAsync("9999");
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.DeleteAsync("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void DeleteWithVersionNoPurge()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var item = new TestData { Id = 4, Value = "A successful object PUT" };
        await collection.AddorUpdateAsync("4", item);
        var kvObject = await collection.GetAsync<TestData>("4");

        await collection.DeleteAsync("4", 
                                    reference: kvObject.VersionReference, 
                                    purge: false);

        var graveyard = await collection.GetAsync<TestData>("4", kvObject.VersionReference);
        Assert.NotNull(graveyard.Value);
    }

    [Fact]
    public async void DeleteThrowsRequestExceptionWhenVersionReferenceDoesNotMatch()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.DeleteAsync("2",
                                         reference: kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
