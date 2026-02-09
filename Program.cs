﻿using Microsoft.Extensions.DependencyInjection;
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
        await wishlistDB.AddWishListItem(new WishlistItem(gameId, gamePrice, discount, title));
    }
}
