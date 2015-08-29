using NSubstitute;
using Orchestrate.Io;
using System;
using System.Dynamic;
using System.Net;
using Xunit;

public class CreateCollectionTests
{
    readonly string collectionName = typeof(CreateCollectionTests).FullName;

    [Fact]
    public async void Guards()
    {
        Application application = new Application("OrchestrateApiKey");
        var client = new Client(application);

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
        Application application = new Application("OrchestrateApiKey");
        var client = new Client(application);

        try
        {
            dynamic item = new ExpandoObject();
            item.Id = 1;

            string guid = Guid.NewGuid().ToString();
            KvMetadata metaData = await client.CreateCollectionAsync(collectionName, guid, item);

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
        var application = Substitute.For<IApplication>();
        application.Key.Returns("HaHa");
        application.HostUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => client.CreateCollectionAsync(collectionName, "key", "item")
        );

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("Valid credentials are required.", exception.Message);
    }
}