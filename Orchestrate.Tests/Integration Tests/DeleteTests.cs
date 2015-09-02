using System;
using Orchestrate.Io;
using Xunit;
using System.Net;

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
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);

        await collection.DeleteAsync(kvMetaData.Key);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => collection.GetAsync<object>(kvMetaData.Key, kvMetaData.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", kvMetaData.Key, collectionName);
        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public async void DeleteSuccessNoPurge()
    {
        var item = new Product { Id = 3, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);

        await collection.DeleteAsync(kvMetaData.Key, purge: false);

        var old = await collection.GetAsync<Product>(kvMetaData.Key, kvMetaData.VersionReference);
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
        var application = new Application("HaHa");
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
        var item = new Product { Id = 4, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);
        var kvObject = await collection.GetAsync<Product>(kvMetaData.Key);

        await collection.DeleteAsync(kvMetaData.Key, 
                                     reference: kvObject.VersionReference, 
                                     purge: false);

        var graveyard = await collection.GetAsync<Product>(kvMetaData.Key, kvObject.VersionReference);
        Assert.NotNull(graveyard.Value);
    }

    [Fact]
    public async void DeleteThrowsRequestExceptionWhenVersionReferenceDoesNotMatch()
    {
        var item = new Product { Id = 1, Name = "Bread", Description = "Whole Grain Bread", Price = 2.75M, Rating = 3 };
        var kvMetaData = await collection.AddAsync(item);
        var kvObject = await collection.GetAsync<Product>(kvMetaData.Key);

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.DeleteAsync("2",
                                         reference: kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
