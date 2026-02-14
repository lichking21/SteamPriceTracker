using Microsoft.Extensions.DependencyInjection;
using GamesListOperator;
using DataBaseOperator;
using DataBaseOperator.Entities;
using Network;
using Microsoft.Extensions.Hosting;

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

            if (Console.IsInputRedirected)
            {
                Console.WriteLine("(LOG) >>> No interactive input detected. Running background services only.");
                await Task.Delay(Timeout.Infinite);
                return;
            }

            string region = "kg";
            string gameTitle = "Mortal Kombat X";
            string gameTitle2 = "Mortal Kombat 1";

            await mainDB.ImportDataToDB(await getList.ParsedJson());

            price.SetUserPrice(region);
            string title = await cmd.ProccesSelection(mainDB, gameTitle);
            int gameId = await mainDB.GetGameID(title);
            (string gamePrice, int discount) = await price.GetPrice(gameId);
            
            await userDB.AddUserItem(new UserItem("Sadyr"));

            await trackedGamesDB.AddTrackingGame(new TrackedGamesItem(gameId, gamePrice, discount, title));

            await dbSerivce.AddToUserWishlist(await userDB.GetUserId("Sadyr"), gameTitle);
            await dbSerivce.AddToUserWishlist(await userDB.GetUserId("Sadyr"), gameTitle2);
        }
    }
}
