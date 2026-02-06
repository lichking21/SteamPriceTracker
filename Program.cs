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

        List<int> gameIDs = new List<int>();

        while(true) 
        {
            int gameId = await dbController.GetGameID();
            string gamePrice = await price.GetPrice(gameId);
            Console.WriteLine($"(DEBUG) Game price: {gamePrice}");
            gameIDs.Add(gameId);
         
            Console.WriteLine("__________________________________");
            Console.WriteLine("|q     - to quit                 |");
            Console.WriteLine("|enter - to continue adding games|");
            Console.WriteLine("__________________________________");

            string? quit = Console.ReadLine();
            if (quit == "q") 
            {
                gameIDs.Clear();
                break;
            } 
        }


        /*а как мне использовать на фоне отправлять хттп запрос? мне надо же как то через ASP.NET Core Hosted Services(хз че это) для периодического обновления цен с учетом Rate Limiting внешнего API. */
    }
}