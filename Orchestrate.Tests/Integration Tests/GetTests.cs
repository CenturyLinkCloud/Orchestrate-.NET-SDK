using System;
using Xunit;
using Orchestrate.Io;
using System.Net;
using NSubstitute;

[Collection("Integration Test")]
public class GetTests : IDisposable
{
    CollectionTestFixture testFixture;
    Collection collection;
    TestData testData;

    public GetTests(CollectionTestFixture testFixture)
    {
        this.testFixture = testFixture;
        collection = testFixture.Client.GetCollection(testFixture.CollectionName);

        testData = new TestData { Id = 1, Value = "Initial Test Data" };
        AsyncHelper.RunSync(() => collection.TryAddAsync("1", testData));
    }

    public async void Dispose()
    {
        await collection.DeleteAsync("1");
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.GetAsync<object>(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.GetAsync<object>(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void GetSuccess()
    {
        var kvObject = await collection.GetAsync<TestData>("1");

        Assert.Equal(testFixture.CollectionName, kvObject.CollectionName);
        Assert.Equal("1", kvObject.Key);
        Assert.True(kvObject.VersionReference.Length > 0);
        Assert.Empty(kvObject.Location);

        TestData actualTestData = kvObject.Value;
        Assert.Equal(testData.Id, actualTestData.Id);
        Assert.Equal(testData.Value, actualTestData.Value);
    }

    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.GetAsync<object>("9999")
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
        string expected = String.Format("Key: {0} was not found in collection: {1}", "9999", testFixture.CollectionName);
        Assert.Equal(expected, exception.Message);
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
                                () => collection.GetAsync<object>("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
