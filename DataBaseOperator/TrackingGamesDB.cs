using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataBaseOperator;

public class TrackedGamesDB
{
    private readonly ApplicationContext _context;
    private readonly ILogger<TrackedGamesDB> _logger;
    public TrackedGamesDB(ApplicationContext context, ILogger<TrackedGamesDB> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<int>> GetIDs()
    {
        List<int> ids = await _context.trackedGames.Select(w => w.GameId).ToListAsync();

        return ids;
    } 

    public async Task UpdateWishlistItem(TrackedGamesItem item)
    {
        var existingItem = await _context.trackedGames.FindAsync(item.GameId);

        if (existingItem != null)
        {
            existingItem.Price = item.Price;
            existingItem.Discount = item.Discount;
            existingItem.LastUpdate = DateTime.Now;
        }
    }

    public async Task AddTrackingGame(TrackedGamesItem item)
    {
        bool isExists = await _context.trackedGames.AnyAsync(w => item.GameId == w.GameId);

        if (isExists == false)
        {
            item.LastUpdate = DateTime.Now;
            item.Title = (item.Title == null) ? "UNKNOWN" : item.Title;
            
            _context.trackedGames.Add(item);

            await _context.SaveChangesAsync();
            _logger.LogInformation($"(LOG) >> {item.Title} was added to tracking_games");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> {item.Title} already exists in tracking_games");
        }
    }

    public async Task RemoveTrackingGame(string title)
    {
        int rows = await _context.trackedGames
            .Where(w => 
                w.Title != null && EF.Functions.ILike(w.Title, $"%{title}%"))
            .ExecuteDeleteAsync();

        if (rows > 0)
        {
            _logger.LogInformation($"(LOG) >> {title} was deleted from tracking_games");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> {title} wasn't found in tracking_games");
        }
    }

}