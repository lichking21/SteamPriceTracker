using System.Net.Http;
using System.Text.Json;

namespace Network;

public class Price
{
    public async Task<string> GetPrice(int gameId, string region)
    {
        string url = $"https://store.steampowered.com/api/appdetails?appids={gameId}&cc={region}&l=english";
        string finalPrice = "";

        HttpClient client = new HttpClient();

        try
        {
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, GameDetails>>(jsonContent);

            if (dictionary != null && dictionary.TryGetValue(gameId.ToString(), out var gameDetails))
            {
                if (gameDetails.Success && gameDetails.Data != null)
                {
                    if (gameDetails.Data.IsFree)
                    {
                        finalPrice = "free";
                    }
                    else if (gameDetails.Data.Price != null)
                    {
                        var price = gameDetails.Data.Price;
                        
                        if (price.DiscountPercent != 0)
                        {
                            Console.WriteLine($"Discount is: {price.DiscountPercent}%");
                            Console.WriteLine($"Initial price is: {price.InitialPrice}");
                            finalPrice = price.FinalPrice;
                        }
                        else
                        {
                            finalPrice = price.FinalPrice;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(ERROR) Bad request: {ex}");
            return "";
        }

        return finalPrice;   
    }
}