using System;
using Orchestrate.Io;
using Xunit;
using System.Net;
using NSubstitute;

public class DeleteTests : IClassFixture<TestFixture>
{
    string collectionName;
    Collection collection;

    public DeleteTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;

        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
    }

    [Fact]
    public async void Guards()
    {
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
        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await collection.AddOrUpdateAsync("3", item);

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
        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await collection.AddOrUpdateAsync("3", item);

        await collection.DeleteAsync("3", purge: false);

        var old = await collection.GetAsync<TestData>("3", kvMetaData.VersionReference);
        Assert.Equal(3, old.Value.Id);
    }

    [Fact]
    public async void DeleteNonExistantKeySuccess()
    {
        await collection.DeleteAsync("9999");
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
                                () => collection.DeleteAsync("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void DeleteWithVersionNoPurge()
    {
        var item = new TestData { Id = 4, Value = "A successful object PUT" };
        await collection.AddOrUpdateAsync("4", item);
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
        var kvObject = await collection.GetAsync<TestData>("1");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.DeleteAsync("2",
                                         reference: kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
