using System.Threading.Tasks;

public class LocationTestFixture : TestFixture
{
    public Location Location { get; private set; }
    public string Key { get; private set; }

    public async override Task InitializeAsync()
    {
        await base.InitializeAsync();

        GeoCoordinate coordinate = new GeoCoordinate { Latitude = 48.8582M, Longitude = 2.2945M };
        Location = new Location { Name = "Eiffel Tower", GeoCoordinate = coordinate };
        Key = "1";
        await Collection.TryAddAsync(Key, Location);

        await SearchHelper.WaitForConsistency(Collection, "value.coordinates:NEAR:{lat:48.8 lon:2.3 dist:100km}", 1);
    }

    public async override Task DisposeAsync()
    {
        await Collection.DeleteAsync(Key);

        await base.DisposeAsync();
    }
}

