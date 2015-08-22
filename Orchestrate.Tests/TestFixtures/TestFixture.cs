using System;
using Orchestrate.Io;
using System.IO;

public class TestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }
    public Collection Collection { get; private set; }
    public TestData TestData { get; private set; }

    public TestFixture()
    {
        Application = new Application("OrchestrateApiKey");
        Client = new Client(Application);

        TestData = new TestData { Id = 1, Value = "Initial Test Data" };
       
        CollectionName = Path.GetRandomFileName();
        AsyncHelper.RunSync(() => Client.CreateCollectionAsync(CollectionName, "1", TestData));
        Collection = Client.GetCollection(CollectionName);
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => Client.DeleteCollectionAsync(CollectionName));
    }
}