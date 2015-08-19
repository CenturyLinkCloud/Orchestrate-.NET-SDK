using System;
using Orchestrate.Io;
using Xunit;

public class CollectionTests
{
    [Fact]
    public void Guards()
    {
        Application application = new Application("OrchestrateApiKey");
        Client client = new Client(application);

        var exception = Assert.Throws<ArgumentException>(
            () => client.GetCollection(string.Empty)
            );
        Assert.Equal("collectionName", exception.ParamName);

        exception = Assert.Throws<ArgumentNullException>(
            () => client.GetCollection(null)
            );
        Assert.Equal("collectionName", exception.ParamName);
    }
}
