using System.Threading.Tasks;
using Microsoft.Extensions.FileSystemGlobbing;
using DataBaseOperator;

/// TODO: 
/// class will be changed into TgBotController
/// input from cmd will be replaced by user input from telegram bot
public class CMD()
{
    /// <summary>
    /// Reads user input from CMD
    /// </summary>
    /// <returns>userInput value</returns>
    private string GetUserInput()
    {
        string? userInput = Console.ReadLine();

        while (string.IsNullOrEmpty(userInput))
        {
            Console.WriteLine("(WARNING) This field can't be empty or null");
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

    public string GetUserRegion()
    {
        Console.Write("Enter store region: ");
        string region = GetUserInput();
        return region;
    }

    /// <summary>
    /// Returns game title appearances by userInput
    /// </summary>
    /// <returns>Game title</returns>
    public async Task<string> ProccesSelection(MainDB db, string title)
    {
        string resultTitle = "";

        List<string> matches = await db.ExportDataByTitle(title);
        int count = matches.Count;
        string? exactMatch = matches.FirstOrDefault(title => title.Equals(title, StringComparison.OrdinalIgnoreCase));
        
        if (exactMatch != null)
        {
            Console.WriteLine($"(DEBUG) Exact match: {exactMatch}");
            return exactMatch;
        }

        if (count == 0)
        {
            Console.WriteLine($"(WARNING) There is no game with title: {title}");
        }
        else if (count == 1)
        {
            resultTitle = matches[0];
            return resultTitle;
        }
        else if (count > 1)
        {
            ShowSimilar(matches);
            Console.WriteLine("(WARNING) Specify your request: ");
        }
    
        return resultTitle;
    }    
}