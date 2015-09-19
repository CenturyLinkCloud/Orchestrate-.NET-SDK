using System.IO;
using System.Threading.Tasks;
using Orchestrate.Io;

public class GraphTestFixture : TestFixture
{
    public Product Bread { get; private set; }
    public string BreadKey { get; private set; }
    public Product Milk { get; private set; }
    public string MilkKey { get; private set; }

    public User User { get; private set;  }
    public string UserKey { get; private set; }
    public Collection UserCollection { get; private set; }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();

        Bread = new Product { Id = 1, Name = "Bread", Description = "Grain Bread", Price = 2.50M, Rating = 4 };
        BreadKey = "1";
        await Collection.TryAddAsync(BreadKey, Bread);

        Milk = new Product { Id = 2, Name = "Milk", Description = "2% Milk", Price = 2.90M, Rating = 2 };
        MilkKey = "2";
        await Collection.TryAddAsync(MilkKey, Milk);

        var userCollectionName = Path.GetRandomFileName();
        UserCollection = Client.GetCollection(userCollectionName);
        User = new User { Id = 1, Name = "Example" };
        UserKey = "1";
        await UserCollection.TryAddAsync(UserKey, User);
    }

    public async override Task DisposeAsync()
    {
        await Collection.DeleteAsync(BreadKey);
        await Collection.DeleteAsync(MilkKey);
        await UserCollection.DeleteAsync(UserKey);
        await Client.DeleteCollectionAsync(UserCollection.CollectionName);

        await base.DisposeAsync();
    }
}

