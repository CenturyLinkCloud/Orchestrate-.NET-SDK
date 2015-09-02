using Orchestrate.Io;
using System.Net;
using Xunit;

public class PingTests
{
    [Fact]
    public async void PingSuccess()
    {
        Application application = new Application(EnvironmentHelper.ApiKey("OrchestrateApiKey"));
        var client = new Client(application);

        await client.PingAsync();
    }

    [Fact]
    public async void InvalidCredentialsThrowsRequestException()
    {
        var application = new Application("HaHa");
        var client = new Client(application);

        var exception = await Assert.ThrowsAsync<RequestException>(
                                () => client.PingAsync());

        Assert.Equal(HttpStatusCode.Unauthorized, exception.StatusCode);
        Assert.NotEmpty(exception.RequestId);
    }
}

