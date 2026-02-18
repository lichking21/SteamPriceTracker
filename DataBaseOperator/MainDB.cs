using GamesListOperator;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace DataBaseOperator;

/// TODO:
/// Return gameItem in ExportDataByTitle

public class MainDB
{
    private readonly ApplicationContext _context;
    private readonly ILogger<MainDB> _logger;

    public MainDB(ApplicationContext context, ILogger<MainDB> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Imports games list into databse. Uses raw SQL.
    /// </summary>
    public async Task ImportDataToDB(List<GameItem> gamesList)
    {
        if (gamesList == null || gamesList.Count == 0)
        {
            _logger.LogError("(ERR) >> GamesList is empty");
            return;
        }

        bool exists = await _context.games.AnyAsync();
        if (exists)
        {
            _logger.LogInformation("(LOG) >> Data is up to date");
            return;
        }

        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                string sql = @"INSERT INTO public.games(id, title) VALUES ({0}, {1})
                            ON CONFLICT (id)
                            DO UPDATE SET title = EXCLUDED.title
                            WHERE public.games.title IS DISTINCT FROM EXCLUDED.title;";

                foreach (var game in gamesList)
                {
                    string title = game.Title ?? "UNKNOWN";

                    await _context.Database.ExecuteSqlRawAsync(sql, game.ID, title);
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"(LOG) >> {gamesList.Count} games were imported to DB.");
            }

            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError($"(ERR) >> Failed to import data to MainDB: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Searches titles. Uses EF core.
    /// </summary>
    /// <returns>List with all title appearances</returns>
    public async Task<List<string>> ExportDataByTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            _logger.LogError("(ERR) >> Title can't be null or empty");
            return new List<string>();
        }

        var res = await _context.games
            .Where(g => EF.Functions.ILike(g.Title, $"%{title}%"))
            .Select(g => g.Title)
            .ToListAsync();

        return res;
    }

    /// <summary>
    /// Gets game's ID by it's title. Uses EF core.
    /// </summary>
    public async Task<int> GetGameID(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            _logger.LogError("Title can't be null or empty");
            return 0;
        }

        int id = await _context.games
            .Where(g => EF.Functions.ILike(g.Title, $"%{title}%"))
            .Select(g => g.Id)
            .FirstOrDefaultAsync();

        return id;
    }

    /// <summary>
    /// Gets game's title by it's ID. Uses EF core.
    /// </summary>
    public async Task<string> GetGameTitle(int id)
    {
        if (id == 0)
        {
            _logger.LogError("ID can't be 0");
            return "";
        }

        string? title = await _context.games
            .Where(g => g.Id == id)
            .Select(g => g.Title)
            .FirstOrDefaultAsync();

        return title ?? "";
    }

    /// <summary>
    /// Finds all matches with searching title
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
    /// Finds match with searching title
    /// </summary>
    public async Task<string> ProccesSelection(string title)
    {
        string resultTitle = "";

        List<string> matches = await ExportDataByTitle(title);
        int count = matches.Count;
        string? exactMatch = matches.FirstOrDefault(title => title.Equals(title, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            _logger.LogInformation($"(LOG) >> Exact match: {exactMatch}");
            return exactMatch;
        }

        if (count == 0)
        {
            _logger.LogInformation($"(LOG) >> There is no game with title: {title}");
        }
        else if (count == 1)
        {
            resultTitle = matches[0];
            return resultTitle;
        }
        else if (count > 1)
        {
            ShowSimilar(matches);
            _logger.LogWarning("(WARN) >> Specify your request: ");
        }

        return resultTitle;
    }
}
