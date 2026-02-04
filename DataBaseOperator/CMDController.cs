using System.Threading.Tasks;

namespace DataBaseOperator;

/// TODO: 
/// class will be changed into TgBotController
/// input from cmd will be replaced by user input from telegram bot
public class CMD(DBController dBController)
{
    private DBController _dbController = dBController;

    /// <summary>
    /// Reads user input from CMD
    /// </summary>
    /// <returns>userInput value</returns>
    private string GetUserInput()
    {
        Console.Write("Enter game title: ");
        string? userInput = Console.ReadLine();

        while (string.IsNullOrEmpty(userInput))
        {
            Console.WriteLine("(WARNING) Game title can't be empty or null");
            Console.Write("Enter game title: ");
            userInput = Console.ReadLine();
        }

        return userInput.Trim();
    }

    /// <summary>
    /// Prints all similarities to user's gameTitle
    /// </summary>
    private void ShowSimilar(List<string> similarities)
    {
        Console.WriteLine($"=== Found similarities: {similarities.Count} ===");
        foreach (string title in similarities)
        {
            Console.WriteLine($"- {title}");
        }
    }

    /// <summary>
    /// Returns game title appearances by userInput
    /// </summary>
    /// <returns>Game title</returns>
    public async Task<string> GetGameTitle()
    {
        while (true)
        {
            string userInput = GetUserInput();

            List<string> matches = await _dbController.ExportDataByTitle(userInput);
            int count = matches.Count;
            
            if (count == 0)
            {
                Console.WriteLine($"(WARNING) There is no game with title: {userInput}");
                continue;
            }
            else if (count == 1)
            {
                string resultTitle = matches[0];
                Console.WriteLine($"(DEBUG) You choosed game: {resultTitle}");
                return resultTitle;
            }
            else if (count > 1)
            {
                ShowSimilar(matches);
                Console.WriteLine("(WARNING) Specify your request: ");
            }
        }
    }

}