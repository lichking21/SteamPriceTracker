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

            if (Console.IsInputRedirected)
            {
                Console.WriteLine("(LOG) >>> No interactive input detected. Running background services only.");
                await Task.Delay(Timeout.Infinite);
                return;
            }

            string region = "kg";
            string searchTitle = "Mortal Sin";

            await mainDB.ImportDataToDB(await getList.ParsedJson());

            price.SetUserPrice(region);
            string title = await cmd.ProccesSelection(mainDB, searchTitle);
            int gameId = await mainDB.GetGameID(title);
            (string gamePrice, int discount) = await price.GetPrice(gameId);
            
            await userDB.AddUserItem(new UserItem("Eban"));

            await trackedGamesDB.AddTrackingGame(new TrackedGamesItem(gameId, gamePrice, discount, title));
            await trackedGamesDB.RemoveTrackingGame("The Witcher 3: Wild Hunt");
        }
    }
}
