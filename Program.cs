using Microsoft.Extensions.DependencyInjection;
using GamesListOperator;
using DataBaseOperator;
using DataBaseOperator.Entities;
using Network;
using Microsoft.Extensions.Hosting;
using Telegram.Bot.Types;

class Program
{
    static async Task Main()
    {
        using IHost host = Bootstrapper.BuildApp();
        await host.StartAsync();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

// CLASSES DECLARATIONS
            var mainDB = services.GetRequiredService<MainDB>();
            var price = services.GetRequiredService<Price>();
            var getList = services.GetRequiredService<GamesListController>();
            var trackedGamesDB = services.GetRequiredService<TrackedGamesDB>();
            var userDB = services.GetRequiredService<UserDB>();
            var userWishlistService = services.GetRequiredService<UserWishlistService>();
            var userWishlistDB = services.GetRequiredService<UserWishlistDB>();

// SET UP
            string userRegion = "kg";
            string gameTitle = "The witcher 3: wild hunt";

            await mainDB.ImportDataToDB(await getList.ParsedJson());

// SET UP user
            string name = "Sooronbai";
            long userId = 16111958;
            var user = new UserItem(name, userId, userRegion);

            await userDB.AddUserItem(user);

// SET UP tracking game
            string title = await mainDB.ProccesSelection(gameTitle);
            int gameId = await mainDB.GetGameID(title);
            (string gamePrice, int discount) = await price.GetPrice(gameId, userRegion);
            var game = new TrackedGamesItem(gameId, userRegion, gamePrice, discount, title);
            
            await trackedGamesDB.AddTrackingGame(game);

// SET UP users wishlist
            await userWishlistService.AddByTitle(userId, gameTitle);

            Console.WriteLine($"User: {userId} games: ");
            foreach(var g in await userWishlistDB.GetGamesFromWishlist(userId))
            {
                Console.WriteLine($" -{g}");
            }
        }
    }
}
