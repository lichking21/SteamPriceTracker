using Microsoft.Extensions.DependencyInjection;
using GamesListOperator;
using DataBaseOperator;
using Network;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main()
    {
        using IHost host = Bootstrapper.BuildApp();
        await host.StartAsync();
        var services = host.Services;

        var mainDB = host.Services.GetRequiredService<MainDB>();
        var cmd = host.Services.GetRequiredService<CMD>();
        var price = host.Services.GetRequiredService<Price>();
        var getList = host.Services.GetRequiredService<GamesListController>();
        var wishlistDB = host.Services.GetRequiredService<WishlistDB>();

        await mainDB.ImportDataToDB(await getList.ParsedJson());

        price.SetUserPrice(cmd);

        while(true) 
        {
            string title = await cmd.ProccesSelection(mainDB);
            int gameId = await mainDB.GetGameID(title);
            (string gamePrice, int discount) = await price.GetPrice(gameId);
            await wishlistDB.AddWishListItem(gameId, gamePrice, discount, title);

            Console.WriteLine($"(DEBUG) Game price: {gamePrice} (-{discount}%)");
         
            Console.WriteLine("__________________________________");
            Console.WriteLine("|q     - to quit                 |");
            Console.WriteLine("|enter - to continue adding games|");
            Console.WriteLine("__________________________________");

            string? quit = Console.ReadLine();
            if (quit == "q") 
            {
                break;
            } 
        }
    }
}