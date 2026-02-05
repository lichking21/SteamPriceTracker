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

        GamesListController getList = new GamesListController();
        CMD cmd = new CMD();
        DBController dbController = new DBController(configuration, cmd);

        await dbController.ImportDataToDB(await getList.ParsedJson());

        int id = await dbController.GetGameID();
        Console.WriteLine($"(DEBUG) Your ID is: {id}");
    }
}