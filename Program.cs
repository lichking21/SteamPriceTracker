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

            var mainDB = services.GetRequiredService<MainDB>();
            var cmd = services.GetRequiredService<CMD>();
            var price = services.GetRequiredService<Price>();
            var getList = services.GetRequiredService<GamesListController>();
            var trackedGamesDB = services.GetRequiredService<TrackedGamesDB>();
            var userDB = services.GetRequiredService<UserDB>();
            var dbSerivce = services.GetRequiredService<DBService>();
            var userWishlistDB = services.GetRequiredService<UserWishlistDB>();

            if (Console.IsInputRedirected)
            {
                Console.WriteLine("(LOG) >>> No interactive input detected. Running background services only.");
                await Task.Delay(Timeout.Infinite);
                return;
            }

            string region = "kg";
            string gameTitle = "Mortal Kombat X";

            await mainDB.ImportDataToDB(await getList.ParsedJson());

            price.SetUserPrice(region);
            string title = await cmd.ProccesSelection(mainDB, gameTitle);
            int gameId = await mainDB.GetGameID(title);
            (string gamePrice, int discount) = await price.GetPrice(gameId);
            var game = new TrackedGamesItem(gameId, gamePrice, discount, title);
            
            await trackedGamesDB.AddTrackingGame(game);

            string name = "Sadyr";
            long userId = await userDB.GetUserId(name);
            var user = new UserItem(name, userId);

            await userDB.AddUserItem(user);
            await dbSerivce.AddToUserWishlist(userId, gameTitle);

            Console.WriteLine($"User: {userId} games: ");
            foreach(var g in await userWishlistDB.GetGamesFromWishlist(userId))
            {
                Console.WriteLine($" -{g}");
            }
        }
    }
}
