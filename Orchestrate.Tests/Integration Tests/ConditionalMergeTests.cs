using Orchestrate.Io;
using System.Net;
using Xunit;

public class ConditionalMergeTests : IClassFixture<TestFixture>
{
    TestFixture testFixture;

    public ConditionalMergeTests(TestFixture testFixture)
    {
        this.testFixture = testFixture;
    }

    [Fact]
    public async void MergeWithReferenceSucceeds()
    {
        var existingItem = await testFixture.Collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };
        var kvMetaData = await testFixture.Collection.MergeAsync("1", mergeItem, existingItem.VersionReference);

        Assert.Equal(testFixture.CollectionName, kvMetaData.CollectionName);
        Assert.Contains(kvMetaData.Key, kvMetaData.Location);
        Assert.True(kvMetaData.VersionReference.Length > 0);
        Assert.Contains(kvMetaData.VersionReference, kvMetaData.Location);

        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        Assert.Contains("MergeValue", kvObject.RawValue);
    }

    [Fact]
    public async void ThrowsNotFoundExceptionWhenPassingInvalidKey()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => testFixture.Collection.MergeAsync("2", mergeItem, kvObject.VersionReference)
        );

        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    [Fact]
    public async void ThrowsRequestFoundExceptionWhenPassingInvalidReference()
    {
        var kvObject = await testFixture.Collection.GetAsync<TestData>("1");
        var mergeItem = new MergeTestData { MergeValue = "This is a merged value" };

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => testFixture.Collection.MergeAsync("1", mergeItem, "86754321")
        );

        Assert.Equal(HttpStatusCode.PreconditionFailed, exception.StatusCode);
    }
}
