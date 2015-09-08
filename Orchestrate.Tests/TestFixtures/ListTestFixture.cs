using System.Threading.Tasks;
using Orchestrate.Io;

public class ListTestFixture : TestFixture
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var item1 = new Product { Id = 1, Name = "Bread", Description = "Low Fat Whole Grain Bread", Price = 2.75M, Rating = 4 };
        await Collection.TryAddAsync("1", item1);

        var item2 = new Product { Id = 2, Name = "Milk", Description = "Low Fat Milk", Price = 3.5M, Rating = 3 };
        await Collection.TryAddAsync("2", item2);

        var item3 = new Product { Id = 3, Name = "Vint Soda", Description = "Americana Variety - Mix of 6 flavors", Price = 20.90M, Rating = 3 };
        await Collection.TryAddAsync("3", item3);

        await SearchHelper.WaitForConsistency(Collection, "*", 3);
    }
}