using System;
using Orchestrate.Io;
using System.IO;

public class ListTestFixture : IDisposable
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }

    public ListTestFixture()
    {
        Application = new Application("OrchestrateApiKey");
        Client = new Client(Application);
        CollectionName = Path.GetRandomFileName();

        var collection = Client.GetCollection(CollectionName);
        var item1 = new TestData { Id = 1, Value = "Initial Test Item" };
        AsyncHelper.RunSync(() => collection.TryAddAsync("1", item1));
        var item2 = new TestData { Id = 2, Value = "Initial Test Item #2" };
        AsyncHelper.RunSync(() => collection.TryAddAsync("2", item2));
        var item3 = new TestData { Id = 3, Value = "Initial Test Item #3" };
        AsyncHelper.RunSync(() => collection.TryAddAsync("3", item3));
    }

    public async void Dispose()
    {
        await Client.DeleteCollectionAsync(CollectionName);
    }
}