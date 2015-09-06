using System.IO;
using System.Threading.Tasks;
using Orchestrate.Io;
using Xunit;

public class TestFixture : IAsyncLifetime
{
    public string CollectionName { get; private set; }
    public Client Client { get; private set; }
    public Application Application { get; private set; }
    public Collection Collection { get; private set; }

    public virtual Task InitializeAsync()
    {
        Application = new Application(EnvironmentHelper.ApiKey("OrchestrateApiKey"));
        Client = new Client(Application);

        CollectionName = Path.GetRandomFileName();
        Collection = Client.GetCollection(CollectionName);
        return Task.FromResult(0);
    }

    public virtual Task DisposeAsync()
    {
        return Client.DeleteCollectionAsync(CollectionName);
    }
}