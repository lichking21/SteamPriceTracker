using System.Text.Json;

namespace Network;

public class Price
{
    private static readonly HttpClient _client = new HttpClient();
    private string? _region;
    public bool IsConfigured = false;

    /// <summary>
    /// Sets store prices according to users region
    /// </summary>
    public void SetUserPrice(string region) {
    
        _region = region;
        IsConfigured = true;
    } 

    public async Task<(string finalPrice, int discount)> GetPrice(int gameId)
    {
        string url = $"https://store.steampowered.com/api/appdetails?appids={gameId}&cc={_region}&l=english";

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
                            Console.WriteLine($"Discount is: {price.DiscountPercent}%");
                            Console.WriteLine($"Initial price is: {price.InitialPrice}");
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
            Console.WriteLine($"(ERROR) Couldn't update price for {gameId}: {ex}");
        }

        return ("N/A", 0);   
    }
}