using Microsoft.Extensions.DependencyInjection;
using GamesListOperator;
using DataBaseOperator;
using Network;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

class Program
{
    static async Task Main()
    {
        using IHost host = Bootstrapper.BuildApp();

        using (var scope = host.Services.CreateScope())
        {
            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<ApplicationContext>();
            await dbContext.Database.MigrateAsync();

// CLASSES DECLARATIONS
            var mainDB = services.GetRequiredService<MainDB>();
            var price = services.GetRequiredService<Price>();
            var getList = services.GetRequiredService<GamesListController>();
            var trackedGamesDB = services.GetRequiredService<TrackedGamesDB>();
            var userDB = services.GetRequiredService<UserDB>();
            var userWishlistService = services.GetRequiredService<UserWishlistService>();
            var userWishlistDB = services.GetRequiredService<UserWishlistDB>();

            await mainDB.ImportDataToDB(await getList.ParsedJson());
        }
        await host.RunAsync();
    }
}
