using System;
using System.IO;
using Orchestrate.Io;

public class TestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }
    public Collection Collection { get; private set; }

    public TestFixture()
    {
        Application = new Application("OrchestrateApiKey");
        Client = new Client(Application);

        var item = new TestData { Id = 1, Value = "Initial Test Data" };
       
        CollectionName = Path.GetRandomFileName();
        AsyncHelper.RunSync(() => Client.CreateCollectionAsync(CollectionName, "1", item));
        Collection = Client.GetCollection(CollectionName);
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => Client.DeleteCollectionAsync(CollectionName));
    }
}