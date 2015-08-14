using Xunit;
using Orchestrate.Io;
using System.Net;

public class ConditionalAddorUpdateTests : IClassFixture<TestFixture>
{
    readonly string collectionName;

    public ConditionalAddorUpdateTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceSucceeds()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };
        var kvMetaData = await collection.AddorUpdateAsync<TestData>("1", updatedItem, kvObject.VersionReference);

        Assert.Equal(collectionName, kvMetaData.CollectionName);
        Assert.Equal("1", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);

        var updatedObject = await collection.GetAsync<TestData>("1");
        TestData testData = updatedObject.Value;
        Assert.Equal(1, testData.Id);
        Assert.Equal("New and improved value!", testData.Value);
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceThrowsWithInvalidReference()
    {
        var client = new Client(TestUtility.ApplicationKey);
        var collection = client.GetCollection(collectionName);

        var kvObject = await collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.AddorUpdateAsync("2", updatedItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


