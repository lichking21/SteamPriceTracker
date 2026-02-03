using System.Text.Json.Serialization;

public class RootObject
{
    [JsonPropertyName("response")]
    public GamesList? GamesList {get;set;}
}

public class GamesList
{
    [JsonPropertyName("apps")]
    public List<GameItem>? Apps {get;set;}
}

public class GameItem
{
    [JsonPropertyName("appid")]
    public int ID {get;set;}
    
    [JsonPropertyName("name")]
    public string Title {get;set;} = "UNKNOWN_TITLE";
}