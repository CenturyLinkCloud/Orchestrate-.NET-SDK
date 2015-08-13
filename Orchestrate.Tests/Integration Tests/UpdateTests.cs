using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class UpdateTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public UpdateTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => collection.UpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.UpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.UpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var kvMetaData = await collection.UpdateAsync("1", testData);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("1", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        kvObject = await collection.GetAsync<TestData>("1");
        testData = kvObject.Value;
        Assert.Equal("Updated Test Data", testData.Value);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.UpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }

    [Fact]
    public async void ThrowsNotFOundExceptionIfKeyIsNotPresent()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => collection.UpdateAsync("2", testData)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        testData.Value = "Updated Test Data";

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.UpdateAsync("1", testData, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


