using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataBaseOperator;

public class WishlistDB
{
    private readonly ApplicationContext _context;
    public WishlistDB(ApplicationContext context)
    {
        _context = context;
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

            await _context.SaveChangesAsync();
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
            Console.WriteLine($"(DEBUG) {item.Title} was added to wishlist");
        }
        else
        {
            Console.WriteLine($"(WARNING) {item.Title} already exists in wishlist");
        }
    }

    public async Task RemoveWishlistItem(string title)
    {
        int rows = await _context.wishlist.Where(w => w.Title == title).ExecuteDeleteAsync();

        if (rows > 0)
        {
            Console.WriteLine($"(DEBUG) {title} was deleted from wishlist");
        }
        else
        {
            Console.WriteLine($"(WARNING) {title} wasn't found in wishlist");
        }
    }

}