using System;
using Orchestrate.Io;
using Xunit;
using System.Net;
using NSubstitute;

public class DeleteTests : IClassFixture<TestFixture>
{
    TestFixture testFixture; 

    public DeleteTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.DeleteAsync(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.DeleteAsync(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void DeleteSuccessPurge()
    {
        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await testFixture.Collection.AddOrUpdateAsync("3", item);

        await testFixture.Collection.DeleteAsync("3");

        var exception = await Assert.ThrowsAsync<NotFoundException>(
                                () => testFixture.Collection.GetAsync<object>("3", kvMetaData.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", "3", testFixture.CollectionName);
        Assert.Equal(expected, exception.Message);
    }

    [Fact]
    public async void DeleteSuccessNoPurge()
    {
        var item = new TestData { Id = 3, Value = "A successful object PUT" };
        var kvMetaData = await testFixture.Collection.AddOrUpdateAsync("3", item);

        await testFixture.Collection.DeleteAsync("3", purge: false);

        var old = await testFixture.Collection.GetAsync<TestData>("3", kvMetaData.VersionReference);
        Assert.Equal(3, old.Value.Id);
    }

    [Fact]
    public async void DeleteNonExistantKeySuccess()
    {
        await testFixture.Collection.DeleteAsync("9999");
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = Substitute.For<IApplication>();
        application.Key.Returns("HaHa");
        application.HostUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);
        var collection = client.GetCollection(testFixture.CollectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.DeleteAsync("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void DeleteWithVersionNoPurge()
    {
        var item = new TestData { Id = 4, Value = "A successful object PUT" };
        await testFixture.Collection.AddOrUpdateAsync("4", item);
        var kvObject = await testFixture.Collection.GetAsync<TestData>("4");

        await testFixture.Collection.DeleteAsync("4", 
                                                 reference: kvObject.VersionReference, 
                                                 purge: false);

        var graveyard = await testFixture.Collection.GetAsync<TestData>("4", kvObject.VersionReference);
        Assert.NotNull(graveyard.Value);
    }

    [Fact]
    public async void DeleteThrowsRequestExceptionWhenVersionReferenceDoesNotMatch()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.DeleteAsync("2",
                                                     reference: kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
