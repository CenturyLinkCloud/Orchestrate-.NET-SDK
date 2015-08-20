using Xunit;
using Orchestrate.Io;
using System.Net;
using System;
using NSubstitute;

public class TryAddTests : IClassFixture<TestFixture>
{
    TestFixture testFixture;

    public TryAddTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.TryAddAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.TryAddAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.TryAddAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }


    [Fact]
    public async void TryAddSucceeds()
    {
        var item = new TestData { Id = 88, Value = "Test Value 88" };

        var kvMetaData = await testFixture.Collection.TryAddAsync<TestData>("88", item);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Equal("88", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void TryAddFailsWithExistingKey()
    {
        var item = new TestData { Id = 88, Value = "Test Value 88" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.TryAddAsync<TestData>("1", item));

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
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
                                () => collection.TryAddAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}


