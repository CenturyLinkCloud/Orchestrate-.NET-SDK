using Orchestrate.Io;
using Xunit;

public class DataCenterTests
{
    [Fact]
    public async void SpecifiyDataCenter()
    {
        var client = new Client(TestUtility.ApplicationKey,
                                DataCenter.AmazonUsEast.ApiUrl);

        await client.PingAsync();
    }
}