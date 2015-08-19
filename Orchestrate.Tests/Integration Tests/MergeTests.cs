using System;
using Xunit;
using Orchestrate.Io;
using System.Net;
using NSubstitute;

public class MergeTests : IClassFixture<TestFixture>
{
    TestFixture testFixture; 

    public MergeTests(TestFixture testFixture)
    {
        this.testFixture = testFixture; 
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.MergeAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.MergeAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.MergeAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void MergeSuccess()
    {
        var testData = await testFixture.Collection.GetAsync<TestData>("1");

        var item = new MergeTestData
        { MergeValue = "Merge Value" };

        var kvMetaData = await testFixture.Collection.MergeAsync(testData.Key, item);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");

        Assert.Contains("MergeValue", kvObject.RawValue);
    }

    [Fact]
    public async void NonExistentKeyThrowsNotFoundException()
    {
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => testFixture.Collection.MergeAsync<string>("9999", "item")
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
                                () => collection.MergeAsync<object>("key", "item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}