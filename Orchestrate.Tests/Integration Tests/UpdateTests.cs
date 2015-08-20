using System;
using Xunit;
using Orchestrate.Io;
using System.Net;
using NSubstitute;

public class UpdateTests : IClassFixture<TestFixture>
{
    TestFixture testFixture;

    public UpdateTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.UpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.UpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.UpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var kvMetaData = await testFixture.Collection.UpdateAsync("1", testData);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Equal("1", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        testData = kvObject.Value;
        Assert.Equal("Updated Test Data", testData.Value);
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
                                () => collection.UpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionIfKeyIsNotPresent()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => testFixture.Collection.UpdateAsync("2", testData)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.UpdateAsync("1", testData, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


