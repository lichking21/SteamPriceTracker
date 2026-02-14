using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

    public async Task<bool> AddLink(long userId, int gameId)
    {
        bool exists = await _context.usersWishlist.AnyAsync(uw => 
            uw.UserId == userId && uw.GameId == gameId);
        
        if (exists) return false;

        var link = new UserWishlistItem(userId, gameId);

        _context.usersWishlist.Add(link);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"(LOG) >> Link created: User: {userId} ==> game: {gameId}");
        return true;
    }
}