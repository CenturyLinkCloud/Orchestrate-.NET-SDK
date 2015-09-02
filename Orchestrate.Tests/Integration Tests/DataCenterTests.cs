using Orchestrate.Io;
using Xunit;

public class DataCenterTests
{
    [Fact]
    public async void SpecifyDataCenter()
    {
        Application application = new Application(EnvironmentHelper.ApiKey("OrchestrateApiKey"), 
                                                  DataCenter.AmazonUsEast);
        var client = new Client(application);

        await client.PingAsync();
    }
}