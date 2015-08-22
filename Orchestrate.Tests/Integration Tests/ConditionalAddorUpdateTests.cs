using Xunit;
using Orchestrate.Io;
using System.Net;

public class ConditionalAddOrUpdateTests : IClassFixture<TestFixture>
{
    string collectionName;
    Collection collection;

    public ConditionalAddOrUpdateTests(TestFixture testFixture)
    {
        collectionName = testFixture.CollectionName;

        collection = testFixture.Client.GetCollection(testFixture.CollectionName);
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceSucceeds()
    {
        var kvObject = await collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };
        var kvMetaData = await collection.AddOrUpdateAsync<TestData>("1", updatedItem, kvObject.VersionReference);

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
        var kvObject = await collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => collection.AddOrUpdateAsync("2", updatedItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


