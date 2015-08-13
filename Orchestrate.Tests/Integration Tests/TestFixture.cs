using System;
using System.IO;
using Orchestrate.Io;

public class TestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }

    public TestFixture()
    {
        Client = new Client(TestHelper.ApiKey);
        var item = new TestData { Id = 1, Value = "Initial Test Data" };
       
        CollectionName = Path.GetRandomFileName();
        AsyncHelper.RunSync(() => Client.CreateCollectionAsync(CollectionName, "1", item));
    }

    public void Dispose()
    {
        AsyncHelper.RunSync(() => Client.DeleteCollectionAsync(CollectionName));
    }
}