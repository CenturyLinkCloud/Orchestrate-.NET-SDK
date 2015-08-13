using System;
using Xunit;
using Orchestrate.Io;
using System.Net;

public class CreateCollectionTests
{
    readonly string collectionName = typeof(CreateCollectionTests).FullName;

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestHelper.ApiKey);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateCollectionAsync(string.Empty, string.Empty, string.Empty)
        );
        Assert.Equal("collectionName", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.CreateCollectionAsync(null, string.Empty, string.Empty)
        );
        Assert.Equal("collectionName", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentException>(
            () => client.CreateCollectionAsync(collectionName, string.Empty, string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.CreateCollectionAsync(collectionName, null, string.Empty)
        );
        Assert.Equal("key", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.CreateCollectionAsync<object>(collectionName, "jguid", null)
        );
        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public async void CreateCollectionSuccess()
    {
        var client = new Client(TestHelper.ApiKey);

        try
        {
            var item = new TestData { Id = 1, Value = "CreateCollectionWithItemAsObject" };
            string guid = Guid.NewGuid().ToString();
            KvMetaData metaData = await client.CreateCollectionAsync(collectionName, guid, item);

            Assert.Equal(collectionName, metaData.CollectionName);
            Assert.Equal(guid, metaData.Key);
            Assert.True(metaData.VersionReference.Length > 0);
            Assert.Contains(metaData.VersionReference, metaData.Location);
        }
        finally
        {
            await client.DeleteCollectionAsync(collectionName);
        }
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("HaHa");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => client.CreateCollectionAsync(collectionName, "key", "item")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("Valid credentials are required.", exception.Message);
    }
}