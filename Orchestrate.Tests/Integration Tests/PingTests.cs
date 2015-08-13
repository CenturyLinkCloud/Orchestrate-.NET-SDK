using Orchestrate.Io;
using System.Net;
using Xunit;

public class PingTests
{
    [Fact]
    public async void PingSuccess()
    {
        var client = new Client(TestHelper.ApiKey);

        await client.PingAsync();
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var client = new Client("ApiKey");

        var exception = await Assert.ThrowsAsync<RequestException>(
                                () => client.PingAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.NotEmpty(exception.RequestId);
    }
}

