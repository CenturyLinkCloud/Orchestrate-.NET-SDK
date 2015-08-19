using System;
using Xunit;
using Orchestrate.Io;
using System.Net;
using NSubstitute;

public class AddOrUpdateTests : IClassFixture<TestFixture>
{
    TestFixture testFixture;

    public AddOrUpdateTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => testFixture.Collection.AddorUpdateAsync<object>(string.Empty, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.AddorUpdateAsync<object>(null, null)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.AddorUpdateAsync<object>("jguids", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        var item = new TestData { Id = 3, Value = "Added Object" };
        var key = Guid.NewGuid().ToString();

        var kvMetaData = await testFixture.Collection.AddorUpdateAsync<TestData>(key, item);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Equal(key, kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);
    }

    [Fact]
    public async void UpdateSuccess()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var testData = kvObject.Value;
        Assert.Equal("Initial Test Data", testData.Value);
        testData.Value = "Updated Test Data";

        var kvMetaData = await testFixture.Collection.AddorUpdateAsync<TestData>("1", testData);

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
        application.V0ApiUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);
        var collection = client.GetCollection(testFixture.CollectionName);        

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.AddorUpdateAsync<object>("key", string.Empty));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

