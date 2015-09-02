using Newtonsoft.Json;


public class Location
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("coordinates")]
    public GeoCoordinate GeoCoordinate { get; set; }
}

public class GeoCoordinate
{
    [JsonProperty("latitude")]
    public decimal Latitude { get; set; }

    [JsonProperty("longitude")]
    public decimal Longitude { get; set; }
}