using System.IO;
using System.Threading.Tasks;
using Orchestrate.Io;

public class GraphTestFixture : TestFixture
{
    public Product Product { get; private set; }
    public string ProductKey { get; private set; }
    public User User { get; private set;  }
    public string UserKey { get; private set; }
    public Collection UserCollection { get; private set; }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();

        Product = new Product { Id = 1, Name = "Bread", Description = "Grain Bread", Price = 2.50M, Rating = 4 };
        ProductKey = "1";
        await Collection.TryAddAsync(ProductKey, Product);

        var userCollectionName = Path.GetRandomFileName();
        UserCollection = Client.GetCollection(userCollectionName);
        User = new User { Id = 1, Name = "Example" };
        UserKey = "1";
        await UserCollection.TryAddAsync(UserKey, User);
    }

    public async override Task DisposeAsync()
    {
        await Collection.DeleteAsync(ProductKey);
        await UserCollection.DeleteAsync(UserKey);
        await Client.DeleteCollectionAsync(UserCollection.CollectionName);

        await base.DisposeAsync();
    }
}

