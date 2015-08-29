using System;
using Orchestrate.Io;
using Xunit;
using System.Net;
using NSubstitute;
using System.Dynamic;

public class DeleteCollectionTests
{
    readonly string collectionName = typeof(CreateCollectionTests).FullName;

    [Fact]
    public async void Guards()
    {
        Application application = new Application("OrchestrateApiKey");
        var client = new Client(application);

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => client.DeleteCollectionAsync(string.Empty)
        );
        Assert.Equal("collectionName", exception.ParamName);

        exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.DeleteCollectionAsync(null)
        );
        Assert.Equal("collectionName", exception.ParamName);
    }

    [Fact]
    public async void DeleteSuccess()
    {
        Application application = new Application("OrchestrateApiKey");
        var client = new Client(application);

        dynamic item = new ExpandoObject();
        item.Id = 1;

        await client.CreateCollectionAsync(collectionName, Guid.NewGuid().ToString(), item);

        await client.DeleteCollectionAsync(collectionName);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = Substitute.For<IApplication>();
        application.Key.Returns("HaHa");
        application.HostUrl.Returns("https://api.orchestrate.io/v0");

        var client = new Client(application);

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => client.DeleteCollectionAsync(collectionName)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("Valid credentials are required.", exception.Message);
    }


    [Fact]
    public async void DeleteNonExistantCollectionSuccess()
    {
        Application application = new Application("OrchestrateApiKey");
        var client = new Client(application);

        await client.DeleteCollectionAsync("NonExistantCollection");
    }
}

