using System;
using Orchestrate.Io;
using Xunit;
using System.Net;

public class DeleteCollectionTests
{
    readonly string collectionName = typeof(CreateCollectionTests).FullName;

    [Fact]
    public async void Guards()
    {
        var client = new Client(TestUtility.ApplicationKey);

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
        var client = new Client(TestUtility.ApplicationKey);

        var item = new TestData { Id = 1, Value = "DeleteCollection" };
        await client.CreateCollectionAsync(collectionName, Guid.NewGuid().ToString(), item);

        await client.DeleteCollectionAsync(collectionName);
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("haha");

        var exception = await Assert.ThrowsAsync<RequestException>(
            () => client.DeleteCollectionAsync(collectionName)
        );

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.Equal("Valid credentials are required.", exception.Message);
    }


    [Fact]
    public async void DeleteNonExistantCollectionSuccess()
    {
        var client = new Client(TestUtility.ApplicationKey);

        await client.DeleteCollectionAsync("NonExistantCollection");
    }
}

