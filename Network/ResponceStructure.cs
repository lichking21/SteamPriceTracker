using System.Text.Json.Serialization;

namespace Network;

public class PriceOverview
{
    [JsonPropertyName("discount_percent")]
    public int DiscountPercent {get; set;}

    [JsonPropertyName("initial_formatted")]
    public string InitialPrice {get;set;} = "";

    [JsonPropertyName("final_formatted")]
    public string FinalPrice {get;set;} = "";
}

public class GameData
{
    [JsonPropertyName("price_overview")]
    public PriceOverview? Price {get;set;}

    [JsonPropertyName("is_free")]
    public bool IsFree {get;set;}
}

public class GameDetails
{
    [JsonPropertyName("success")]
    public bool Success {get;set;}

    [JsonPropertyName("data")]
    public GameData? Data {get;set;}
}