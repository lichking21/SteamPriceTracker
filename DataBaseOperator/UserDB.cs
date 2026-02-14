using System.Threading.Tasks;
using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataBaseOperator;

public class UserDB
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UserDB> _logger;

    public UserDB(ApplicationContext context, ILogger<UserDB> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Use it to get information about user id by his name
    /// </summary>
    /// <returns>Id as integer value type</returns>
    public async Task<long> GetUserId(string name)
    {
        long id = await _context.users
            .Where(u => u.Name != null && EF.Functions.ILike(u.Name, $"%{name}%"))
            .Select(user => user.ID)
            .FirstOrDefaultAsync();

        _logger.LogInformation($"(LOG) >> User {name}'s id: {id}");

        return id;
    }

    public async Task AddUserItem(UserItem item)
    {
        bool ifExists = await _context.users.AnyAsync(u => u.ID ==item.ID);

        if (ifExists == false)
        {
            item.Name = string.IsNullOrEmpty(item.Name) ? "UNKNOWN" : item.Name;

            _context.users.Add(item);

            await _context.SaveChangesAsync();
            _logger.LogInformation($"(LOG) >> User {item.Name} was added to users table");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> User {item.Name} already exists in users table");
        }
    }
}