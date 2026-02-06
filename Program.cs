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
        MainDB mainDB = new MainDB(configuration, cmd);
        Price price = new Price();

        await mainDB.ImportDataToDB(await getList.ParsedJson());


        while(true) 
        {
            string title = await cmd.ProccesSelection(mainDB);
            int gameId = await mainDB.GetGameID(title);
            string gamePrice = await price.GetPrice(gameId);

            Console.WriteLine($"(DEBUG) Game price: {gamePrice}");
         
            Console.WriteLine("__________________________________");
            Console.WriteLine("|q     - to quit                 |");
            Console.WriteLine("|enter - to continue adding games|");
            Console.WriteLine("__________________________________");

            string? quit = Console.ReadLine();
            if (quit == "q") 
            {
                break;
            } 
        }
    }
}