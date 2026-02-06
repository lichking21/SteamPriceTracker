using Microsoft.Extensions.Configuration;
using Npgsql;
namespace DataBaseOperator;

public abstract class Database
{
    protected readonly string _connectionString;

    protected Database(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                        ?? throw new ArgumentNullException("ConnectionString is null");
    }

    protected NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);
}