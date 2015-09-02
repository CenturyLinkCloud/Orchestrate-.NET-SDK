using System;
using Orchestrate.Io;
using System.IO;

public class TestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }
    public Collection Collection { get; private set; }

    public TestFixture()
    {
        Application = new Application(EnvironmentHelper.ApiKey("OrchestrateApiKey"));
        Client = new Client(Application);

        CollectionName = Path.GetRandomFileName();
        Collection = Client.GetCollection(CollectionName);
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => Client.DeleteCollectionAsync(CollectionName));
    }
}