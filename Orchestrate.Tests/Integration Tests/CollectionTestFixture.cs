using Orchestrate.Io;
using System;
using System.IO;

public class CollectionTestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }

    public CollectionTestFixture()
    {
        Application = new Application("OrchestrateApiKey");
        Client = new Client(Application);
        CollectionName = Path.GetRandomFileName();
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => Client.DeleteCollectionAsync(CollectionName));
    }
}
