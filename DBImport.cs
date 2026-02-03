using Microsoft.Extensions.Configuration;
using Npgsql;

public class DBImport
{
    private readonly string _connectionString;

    public DBImport(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                        ?? throw new ArgumentNullException("ConnectionString is null");
    }

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
                                ON CONFLICT (id) DO UPDATE SET title = EXCLUDED.title;";
                    
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
}