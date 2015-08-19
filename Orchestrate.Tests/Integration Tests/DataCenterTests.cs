using Orchestrate.Io;
using Xunit;

public class DataCenterTests
{
    [Fact]
    public async void SpecifyDataCenter()
    {
        Application application = new Application("OrchestrateApiKey", DataCenter.AmazonUsEast);
        var client = new Client(application);

        await client.PingAsync();
    }
}