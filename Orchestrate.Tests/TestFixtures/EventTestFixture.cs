using System.IO;
using System.Threading.Tasks;
using Orchestrate.Io;

public class EventTestFixture : TestFixture
{
    public Product Minivan { get; private set; }
    public string MinivanKey { get; private set; }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();

        Minivan = new Product { Id = 1, Name = "Minivan", Description = "Handy Odometry", Price = 35000.00M, Rating = 5 };
        MinivanKey = "1";
        await Collection.TryAddAsync(MinivanKey, Minivan);
    }

    public async override Task DisposeAsync()
    {
        await Collection.DeleteAsync(MinivanKey);
        await base.DisposeAsync();
    }
}

