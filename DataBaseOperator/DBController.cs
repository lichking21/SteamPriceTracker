using Microsoft.Extensions.Configuration;
using Npgsql;
using GamesListOperator;

namespace DataBaseOperator;
public class DBController
{
    private readonly string _connectionString;

    public DBController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                        ?? throw new ArgumentNullException("ConnectionString is null");
    }


    /// <summary>
    /// Imports games list into DataBase
    /// </summary>
    public async Task ImportDataToDB(List<GameItem> gamesList)
    {
        if (gamesList == null || gamesList.Count == 0)
        {
            Console.WriteLine("(ERROR) GamesList is empty");
            return;
        }

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var transaction =  await conn.BeginTransactionAsync())
            {
                try
                {
                    string sql = @"INSERT INTO public.games(id, title) VALUES (@id, @title) 
                                ON CONFLICT (id) 
                                DO UPDATE SET title = EXCLUDED.title
                                WHERE public.games.title IS DISTINCT FROM EXCLUDED.title;";
                    
                    using (var cmd = new NpgsqlCommand(sql, conn, transaction))
                    {
                        var idParam = cmd.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Integer);
                        var titleParam = cmd.Parameters.Add("title", NpgsqlTypes.NpgsqlDbType.Text);

                        await cmd.PrepareAsync();

                        foreach (var game in gamesList)
                        {
                            idParam.Value = game.ID;
                            titleParam.Value = game.Title ?? "UNKNOWN";

                            await cmd.ExecuteNonQueryAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    Console.WriteLine($"(SUCCESS) {gamesList.Count} games were imported to DB.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"(ERROR) Failed to import data to DB: {ex.Message}");
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Takes a string parametr 'title' and searches all appearances with that 'title'
    /// </summary>
    /// <param name="title"></param>
    /// <returns>List with all 'title' appearances</returns>
    public async Task<List<string>> ExportDataByTitle(string title)
    {
        List<string> result = new List<string>();

        if (string.IsNullOrEmpty(title))
        {
            Console.WriteLine("(WARNING) title value can't be empty or null");
            return result;   
        }

        var sql = @"SELECT title FROM games WHERE title ILIKE @titleSearch";

        using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            try
            {
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@titleSearch", $"%{title}%");

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var gameTitle = reader.GetString(0);
                            result.Add(gameTitle);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"(ERROR) Seach failed {title}: {ex}");
            }
        }

        return result;
    } 

    /// <summary>
    /// Returns game's ID by its Title
    /// </summary>
    // public async Task GetGameID(string title)
    // {
        
    // }
}