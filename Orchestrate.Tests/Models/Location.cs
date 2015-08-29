using Newtonsoft.Json;


public class Location
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("coordinates")]
    public GpsCoordinates Coordinates { get; set; }
}

public class GpsCoordinates
{
    [JsonProperty("latitude")]
    public decimal Latitude { get; set; }

    [JsonProperty("longitude")]
    public decimal Longitude { get; set; }
}