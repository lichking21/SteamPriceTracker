using Microsoft.Extensions.Configuration;
using GamesListOperator;
using DataBaseOperator;
using Network;

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
        Price price = new Price();

        await dbController.ImportDataToDB(await getList.ParsedJson());

        int gameId = await dbController.GetGameID();
        Console.WriteLine($"(DEBUG) Your ID is: {gameId}");

        string gamePrice = await price.GetPrice(gameId);
        Console.WriteLine($"(DEBUG) Game price: {gamePrice}");
    }
}