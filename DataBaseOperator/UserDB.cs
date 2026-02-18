using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataBaseOperator;

/// TODO:
/// add function to change user state
/// use exceptions instead of if statements
/// return UserItem even on creation
/// return UserItem? in getUser

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
    /// Gets information about user id by his name
    /// </summary>
    /// <returns>ID as integer value type</returns>
    private async Task<long> GetUserId(string name)
    {
        long id = await _context.users
            .Where(u => u.Name != null && EF.Functions.ILike(u.Name, $"%{name}%"))
            .Select(user => user.ID)
            .FirstOrDefaultAsync();

        _logger.LogInformation($"(LOG) >> User {name}'s id: {id}");

        return id;
    }

    public async Task<bool> IsUserExist(long id)
    {
        var exists = await _context.users.AnyAsync(u => u.ID == id);

        if (!exists)
        {
            _logger.LogError($"(ERR) >> User [{id}] not found");
            return false;
        }
        else
            return true;
    }

    public async Task<UserItem> GetUser(long id)
    {
        if (await IsUserExist(id) == true)
        {
            var userItem = await _context.users
                .Where(u => u.ID == id)
                .Select(u => new UserItem { ID = u.ID, Name = u.Name, Region = u.Region, State = u.State })
                .FirstOrDefaultAsync();

            if (userItem != null)
                return userItem;
            else
                _logger.LogError($"(ERR) >> UserItem is null");
        }

        return new UserItem { };
    }

    /// <summary>
    /// Adds user to users table
    /// </summary>
    public async Task AddUserItem(UserItem item)
    {
        if (item == null)
        {
            _logger.LogError("(ERR) >> UserItem can't be null");
            return;
        }

        if (await IsUserExist(item.ID) == false)
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

    /// <summary>
    /// Gets user region
    /// </summary>
    /// <returns>Region as string nullable value</returns>
    public async Task<string?> GetUserRegion(long id)
    {
        if (id == 0)
        {
            _logger.LogError("ID can't be 0");
            return "";
        }

        if (await IsUserExist(id) == true)
        {
            return await _context.users
                .Where(u => u.ID == id)
                .Select(u => u.Region)
                .FirstOrDefaultAsync();
        }

        return "";
    }

    /// <summary>
    /// Updates user's dialog state.
    /// </summary>
    public async Task SetUserState(long id, string state)
    {
        var user = await _context.users.FirstOrDefaultAsync(u => u.ID == id);
        if (user == null)
        {
            _logger.LogError($"(ERR) >> User [{id}] not found");
            return;
        }

        user.State = state;
        await _context.SaveChangesAsync();
        _logger.LogInformation($"(LOG) >> User [{id}] state changed to '{state}'");
    }
}
