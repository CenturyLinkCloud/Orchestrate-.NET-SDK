using System;
using Xunit;
using Orchestrate.Io;
using System.Net;
using NSubstitute;

public class GetTests : IClassFixture<TestFixture>
{
    TestFixture testFixture;

    public GetTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.GetAsync<object>(string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.GetAsync<object>(null)
        );
        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public async void GetSuccess()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");

        Assert.Equal(testFixture.CollectionName, kvObject.CollectionName);
        Assert.Equal("1", kvObject.Key);
        Assert.True(kvObject.VersionReference.Length > 0);
        Assert.Empty(kvObject.Location);

        TestData testData = kvObject.Value;
        Assert.Equal(1, testData.Id);
        Assert.Equal("Initial Test Data", testData.Value);
    }

    [Fact]
    public async void NonExistantKeyThrowsNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => testFixture.Collection.GetAsync<object>("9999")
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
        application.V0ApiUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);
        var collection = client.GetCollection(testFixture.CollectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.GetAsync<object>("key"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}
