using Microsoft.Extensions.Configuration;
using GamesListOperator;
using DataBaseOperator;

class Program
{
    static async Task Main()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        DBController dBController = new DBController(configuration);
        GamesListController getList = new GamesListController();
        CMD cmd = new CMD(dBController);

        await dBController.ImportDataToDB(await getList.ParsedJson());

        try {
            await cmd.GetGameTitle();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"(ERROR) Couldn't get game title: {ex}");
        }
    }
}