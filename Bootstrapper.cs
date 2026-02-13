using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Network;
using GamesListOperator;
using Bot;
using Microsoft.EntityFrameworkCore;
using DataBaseOperator;

public static class Bootstrapper
{
    public static IHost BuildApp()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        ConfigureServices(builder.Services, builder.Configuration);

        return builder.Build();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationContext>(
            options => options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );
        services.AddScoped<WishlistDB>();
        services.AddScoped<UserDB>();
    
        services.AddSingleton<MainDB>();
        services.AddSingleton<CMD>();
        services.AddSingleton<Price>();
        services.AddSingleton<GamesListController>();

        services.AddHostedService<PriceUpdateWorker>();
        services.AddHostedService<EchoBotWorker>();
    }
}
