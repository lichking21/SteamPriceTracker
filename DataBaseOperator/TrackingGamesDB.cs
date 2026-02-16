using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataBaseOperator;

/// <summary>
/// Provides access to all user wishlist data.
/// </summary>
public class TrackedGamesDB
{
    private readonly ApplicationContext _context;
    private readonly ILogger<TrackedGamesDB> _logger;
    public TrackedGamesDB(ApplicationContext context, ILogger<TrackedGamesDB> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets IDs of all games from tracking list
    /// </summary>
    public async Task<List<TrackedGamesItem>> GetTrackedItem()
    {
        return await _context.trackedGames
            .Select(tg => new TrackedGamesItem {GameId = tg.GameId, Region = tg.Region})
            .ToListAsync();
    } 

    /// <summary>
    /// Updates information about tracking game
    /// </summary>
    public async Task UpdateTrackItem(TrackedGamesItem item)
    {
        var existingItem = await _context.trackedGames
            .FirstOrDefaultAsync(tg => tg.GameId == item.GameId && tg.Region == item.Region);

        if (existingItem != null)
        {
            existingItem.Price = item.Price;
            existingItem.Discount = item.Discount;
            existingItem.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Adds game to tracking list 
    /// </summary>
    public async Task AddTrackingGame(TrackedGamesItem item)
    {
        bool isExists = await _context.trackedGames
            .AnyAsync(w => item.GameId == w.GameId && item.Region == w.Region);

        if (isExists == false)
        {
            item.LastUpdate = DateTime.UtcNow;
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

    /// <summary>
    /// Removes game from tracking list by ID
    /// </summary>
    public async Task RemoveTrackingGame(int gameId)
    {
        int deletedRows = await _context.trackedGames.Where(tg => tg.GameId == gameId).ExecuteDeleteAsync();

        if (deletedRows > 0)
        {
            _logger.LogInformation($"(LOG) >> Game [{gameId}] was deleted from tracking_games");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> Game [{gameId}] wasn't found in tracking_games");
        }
    }

    /// <summary>
    /// Checks if game is already in tracking list
    /// </summary>
    public async Task<bool> IsTracking(int gameId, string? region)
    {
        return await _context.trackedGames.AnyAsync(t => t.GameId == gameId && t.Region == region);
    }
}