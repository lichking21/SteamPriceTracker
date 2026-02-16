using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Network;

public class Price
{
    private static readonly HttpClient _client = new HttpClient();
    private readonly ILogger<Price> _logger;
    public bool IsConfigured = false;

    public Price(ILogger<Price> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns game price by its ID
    /// </summary>
    public async Task<(string finalPrice, int discount)> GetPrice(int gameId, string userRegion)
    {
        string url = $"https://store.steampowered.com/api/appdetails?appids={gameId}&cc={userRegion}&l=english";

        try
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, GameDetails>>(jsonContent);

            if (dictionary != null && dictionary.TryGetValue(gameId.ToString(), out var gameDetails))
            {
                if (gameDetails.Success && gameDetails.Data != null)
                {
                    if (gameDetails.Data.IsFree)
                    {
                        return ("free", 0);
                    }
                    else if (gameDetails.Data.Price != null)
                    {
                        var price = gameDetails.Data.Price;
                        
                        if (price.DiscountPercent != 0)
                        {
                            _logger.LogInformation($"(LOG) >> Discount is: {price.DiscountPercent}%");
                            _logger.LogInformation($"(LOG) >> Initial price is: {price.InitialPrice}");
                            return (price.FinalPrice, price.DiscountPercent);
                        }
                        else
                        {
                            return (price.FinalPrice, price.DiscountPercent);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"(ERR) >> Couldn't update price for {gameId} [{userRegion}]: {ex}");
        }

        return ("N/A", 0);   
    }
}