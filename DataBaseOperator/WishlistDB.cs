using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataBaseOperator;

public class WishlistDB
{
    private readonly ApplicationContext _context;
    private readonly ILogger<WishlistDB> _logger;
    public WishlistDB(ApplicationContext context, ILogger<WishlistDB> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<int>> GetIDs()
    {
        List<int> ids = await _context.wishlist.Select(w => w.GameId).ToListAsync();

        return ids;
    } 

    public async Task UpdateWishlistItem(WishlistItem item)
    {
        var existingItem = await _context.wishlist.FindAsync(item.GameId);

        if (existingItem != null)
        {
            existingItem.Price = item.Price;
            existingItem.Discount = item.Discount;
            existingItem.LastUpdate = DateTime.Now;
        }
    }

    public async Task AddWishlistItem(WishlistItem item)
    {
        bool isExists = await _context.wishlist.AnyAsync(w => item.GameId == w.GameId);

        if (isExists == false)
        {
            item.LastUpdate = DateTime.Now;
            item.Title = (item.Title == null) ? "UNKNOWN" : item.Title;
            
            _context.wishlist.Add(item);

            await _context.SaveChangesAsync();
            _logger.LogInformation($"(LOG) >> {item.Title} was added to wishlist");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> {item.Title} already exists in wishlist");
        }
    }

    public async Task RemoveWishlistItem(string title)
    {
        int rows = await _context.wishlist.Where(w => w.Title == title).ExecuteDeleteAsync();

        if (rows > 0)
        {
            _logger.LogInformation($"(LOG) >> {title} was deleted from wishlist");
        }
        else
        {
            _logger.LogWarning($"(WARN) >> {title} wasn't found in wishlist");
        }
    }

}