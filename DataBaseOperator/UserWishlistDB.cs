using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace DataBaseOperator;

public class UserWishlistDB
{
    private readonly ILogger<UserWishlistDB> _logger;
    private readonly ApplicationContext _context;
    public UserWishlistDB(ILogger<UserWishlistDB> logger, ApplicationContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Links tracking games IDs with user's ID  
    /// </summary>
    public async Task<bool> AddLink(long userId, int gameId)
    {
        bool exists = await _context.usersWishlist.AnyAsync(uw => 
            uw.UserId == userId && uw.GameId == gameId);
        
        if (exists) 
            return false;

        var link = new UserWishlistItem(userId, gameId);

        _context.usersWishlist.Add(link);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"(LOG) >> Link created: User: {userId} ==> game: {gameId}");
        return true;
    }

    /// <summary>
    /// Use this method to get all games from user's wishlist 
    /// </summary>
    /// <returns>List of game titles</returns>
    public async Task<List<string?>> GetGamesFromWishlist(long userId)
    {
        bool exists = await _context.usersWishlist.AnyAsync(uw => uw.UserId == userId);
        if (!exists)
        {
            _logger.LogError($"(ERR) >> User: {userId} doesn't exists in user_wishlist table");
            return new List<string?>{};
        }
        
        var titles = await _context.usersWishlist
            .Join(_context.trackedGames, // Link usersWishlist table with trackedGames table 
                uw => uw.GameId, // Takes the key from the 1st table
                tg => tg.GameId, // and looks for the similliar key in the 2nd table
                (uw, tg) => tg.Title // Result is game title
            ).ToListAsync();
        
        return titles;
    }

    /// <summary>
    /// Use this method to remove game from user's wishlist by it's ID
    /// </summary>
    public async Task RemoveGameById(long userId, int gameId)
    {
        int deletedRows = await _context.usersWishlist
            .Where(uw => uw.UserId == userId && uw.GameId == gameId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            _logger.LogError($"(ERR) >> Pair [User: {userId}; Game: {gameId}] doesn't exists");
            return;
        }
    }

    /// <summary>
    /// Use this method to remove game from user's wishlist by it's Title
    /// </summary>
    /// <returns></returns>
    public async Task RemoveGameByTitle(long userId, string title)
    {
        int gameId = await _context.trackedGames
            .Where(tg => tg.Title == title)     
            .Select(tg => tg.GameId)
            .FirstOrDefaultAsync();  

        if (gameId == 0)
        {
            _logger.LogError($"Game [{title}] not found");
            return;
        }

        await RemoveGameById(userId, gameId);
    }
}