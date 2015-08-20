using System;
using System.Net;
using Orchestrate.Io;
using NSubstitute;
using Xunit;

public class AddTests : IClassFixture<TestFixture>
{
    TestFixture testFixture; 

    public AddTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void Guards()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => testFixture.Collection.AddAsync<object>(null)
            );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        var item = new TestData { Id = 3, Value = "A successful object Add" };
        var kvMetaData = await testFixture.Collection.AddAsync(item);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await testFixture.Collection.GetAsync<TestData>(kvMetaData.Key);

        TestData testData = kvObject.Value;
        Assert.Equal(3, testData.Id);
        Assert.Equal("A successful object Add", testData.Value);
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
                                () => collection.AddAsync<object>("item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

