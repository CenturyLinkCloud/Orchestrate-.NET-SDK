using Newtonsoft.Json;

public class Product
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("rating")]
    public int Rating { get; set; }

    [JsonProperty("price")]
    public decimal Price { get; set; }

    [JsonProperty("category")]
    public ProductCategory Category { get; set; }
}