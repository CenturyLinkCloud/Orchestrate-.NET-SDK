using Xunit;
using Orchestrate.Io;
using System.Net;

public class ConditionalAddOrUpdateTests : IClassFixture<TestFixture>
{
    TestFixture testFixture; 

    public ConditionalAddOrUpdateTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceSucceeds()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };
        var kvMetaData = await testFixture.Collection.AddorUpdateAsync<TestData>("1", updatedItem, kvObject.VersionReference);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Equal("1", kvMetaData.Key);
        Assert.True(kvMetaData.VersionReference.Length > 0);

        var updatedObject = await testFixture.Collection.GetAsync<TestData>("1");
        TestData testData = updatedObject.Value;
        Assert.Equal(1, testData.Id);
        Assert.Equal("New and improved value!", testData.Value);
    }

    [Fact]
    public async void AddOrUpdateWithVersionReferenceThrowsWithInvalidReference()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var updatedItem = new TestData { Id = 1, Value = "New and improved value!" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.AddorUpdateAsync("2", updatedItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}


