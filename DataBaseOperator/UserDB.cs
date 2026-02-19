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
        if (id == 0)
        {
            _logger.LogError("(ERR) >> User id can't be 0");
            return false;
        }

        try
        {
            var exists = await _context.users.AnyAsync(u => u.ID == id);
            return true;
        }
        catch (ArgumentNullException nullEx)
        {
            _logger.LogError($"(ERR) >> User [{id}] not found: {nullEx}");
            return false;
        }
        catch (OperationCanceledException cancelEx)
        {
            _logger.LogWarning($"(WARN) >> IsUserExist() operation vas cancelled: {cancelEx}");
            return false;
        }
    }

    public async Task<UserItem?> GetUser(long id)
    {
        try
        {
            var userItem = await _context.users
                .Where(u => u.ID == id)
                .Select(u => new UserItem { ID = u.ID, Name = u.Name, Region = u.Region, State = u.State })
                .FirstOrDefaultAsync();

            return userItem;
        }
        catch (ArgumentNullException nullEx)
        {
            _logger.LogError($"(ERR) >> User [{id}] not found: {nullEx}");
            return null;
        }
        catch (OperationCanceledException cancellEx)
        {
            _logger.LogWarning($"(WARN) >> GetUser() operation was cancelled: {cancellEx}");
            return null;
        }
    }

    /// <summary>
    /// Adds user to users table
    /// </summary>
    public async Task AddUserItem(UserItem item)
    {
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
        try
        {
            return await _context.users
                .Where(u => u.ID == id)
                .Select(u => u.Region)
                .FirstOrDefaultAsync();
        }
        catch (ArgumentNullException nullEx)
        {
            _logger.LogError($"(ERR) >> User [{id}] not found: {nullEx}");
            return "";
        }
        catch (OperationCanceledException cancellEx)
        {
            _logger.LogWarning($"(WARN) >> GetUserRegion() operation was cancelled: {cancellEx}");
            return "";
        }
    }

    /// <summary>
    /// Updates user's dialog state.
    /// </summary>
    public async Task SetUserState(long id, string state)
    {
        try
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
        catch (OperationCanceledException cancelEx)
        {
            _logger.LogWarning($"(WARN) >> SetUserState() operation was cancelled: {cancelEx}");
            return;
        }
    }
}
