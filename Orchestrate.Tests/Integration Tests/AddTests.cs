using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class AddTests : IClassFixture<TestFixture>
{
    static string collectionName;

    public AddTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void Guards()
    {
        Client client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => collection.AddAsync<object>(null)
            );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void AddSuccess()
    {
        var client = new Client(TestHelper.ApiKey);
        var collection = client.GetCollection(collectionName);

        var item = new TestData { Id = 3, Value = "A successful object Add" };
        var kvMetaData = await collection.AddAsync(item);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await collection.GetAsync<TestData>(kvMetaData.Key);

        TestData testData = kvObject.Value;
        Assert.Equal(3, testData.Id);
        Assert.Equal("A successful object Add", testData.Value);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");
        var collection = client.GetCollection(collectionName);

        var execption = await Assert.ThrowsAsync<RequestException>(
                                () => collection.AddAsync<object>("item"));

        Assert.Equal(HttpStatusCode.Unauthorized, execption.StatusCode);
        Assert.Equal("Valid credentials are required.", execption.Message);
    }
}

