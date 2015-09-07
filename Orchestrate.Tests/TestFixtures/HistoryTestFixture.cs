using System.Threading.Tasks;

public class HistoryTestFixture : TestFixture
{
    public Product Product { get; private set; }
    public string Key { get; private set; }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();

        Product = new Product { Id = 1, Name = "Bread", Description = "Grain Bread", Price = 2.50M, Rating = 4 };
        Key = "1";

        await Collection.TryAddAsync(Key, Product);

        await Collection.DeleteAsync("1", purge: false);

        Product.Description = "Whole Grain Bread";
        await Collection.AddOrUpdateAsync(Key, Product);
    }

    public async override Task DisposeAsync()
    {
        await Collection.DeleteAsync(Key);

        await base.DisposeAsync();
    }
}

