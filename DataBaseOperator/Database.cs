using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
namespace DataBaseOperator;

public abstract class Database
{
    protected readonly string _connectionString;
    protected readonly ILogger _logger;

    protected Database(IConfiguration configuration, ILogger logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                        ?? throw new ArgumentNullException("ConnectionString is null");

        _logger = logger;
    }

    protected NpgsqlConnection GetConnection() => new NpgsqlConnection(_connectionString);
}