using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using DataBaseOperator;
using Network;
using GamesListOperator;
using Bot;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

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

        services.AddSingleton<CMD>();
        services.AddSingleton<Price>();
        services.AddSingleton<GamesListController>();

        services.AddHostedService<PriceUpdateWorker>();
        services.AddHostedService<EchoBotWorker>();
    }
}
