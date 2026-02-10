using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GamesListOperator;
public class GamesListController
{
    // !!!!TODO: get games list by sending request!!!!
    private static string folderPath = "Lists";
    private readonly string[] jsonPaths = {
        $"{folderPath}/SteamGamesList.json", $"{folderPath}/SteamGamesList2.json",
        $"{folderPath}/SteamGamesList3.json", $"{folderPath}/SteamGamesList4.json"};

    private readonly ILogger<GamesListController> _logger;

    //private readonly JsonSerializerOptions options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true}; 

    public GamesListController(ILogger<GamesListController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parsing json lists with games
    /// </summary>
    public async Task<List<GameItem>> ParsedJson()
    {
        List<GameItem> allGames = new List<GameItem>();
        foreach(string path in jsonPaths)
        {
            if (!File.Exists(path)) continue;

            using (FileStream fs = File.OpenRead(path))
            {
                try
                {
                    var root = await JsonSerializer.DeserializeAsync<RootObject>(fs);
                    
                    if (root?.GamesList?.Apps != null)
                    {
                        allGames.AddRange(root.GamesList.Apps);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"(ERR) >> Error in file {path}: {ex}");   
                }
            }   
        }

        return allGames;
    }
}