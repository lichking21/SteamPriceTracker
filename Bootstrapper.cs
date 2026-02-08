using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DataBaseOperator;
using Network;
using GamesListOperator;

public static class Bootstrapper
{
    public static IHost BuildApp()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        ConfigureServices(builder.Services);
        
        return builder.Build();
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainDB>();
        services.AddSingleton<WishlistDB>();
        services.AddSingleton<CMD>();
        services.AddSingleton<Price>();
        services.AddSingleton<GamesListController>();

        services.AddHostedService<PriceUpdateWorker>();
    }
}