using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DataBaseOperator;

public class WishlistDB : Database
{
    public WishlistDB(IConfiguration configuration) : base(configuration) {}

    public async Task<List<int>> GetIDs()
    {
        List<int> ids = new List<int>();

        var sql = "SELECT game_id FROM public.wishlist";
        using (var conn = GetConnection())
        {
            await conn.OpenAsync();

            using (var commnad = new NpgsqlCommand(sql, conn))
            {
                try
                {
                    using (var reader = await commnad.ExecuteReaderAsync())
                    {
                        while(await reader.ReadAsync())
                            ids.Add(reader.GetInt32(0));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"(ERRO) Failed to get IDs from wishlist: {ex}");
                }
            }

            return ids;
        }
    }

    public async Task UpdateWishlistItem(int gameId, string price, int discount)
    {
        var sql = @"UPDATE public.wishlist
                    SET price=@updPrice, discount=@updDiscount, last_update=NOW()
                    WHERE game_id=@id";

        using (var conn = GetConnection())
        {
            await conn.OpenAsync();

            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@updPrice", price);
                    cmd.Parameters.AddWithValue("@updDiscount", discount);
                    cmd.Parameters.AddWithValue("@id", gameId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to write data to WishlistDB: {ex}");
                }

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task AddWishListItem(int gameId, string price, int discount, string title)
    {
        var sql = @"INSERT INTO public.wishlist(game_id, price, discount, last_update, title)
                    VALUES (@i, @p, @d, NOW(), @t)
                    ON CONFLICT (game_id) DO NOTHING";

        using (var conn = GetConnection())
        {
            await conn.OpenAsync();

            try
            {
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@i", gameId);
                    cmd.Parameters.AddWithValue("@p", price);
                    cmd.Parameters.AddWithValue("@d", discount);
                    cmd.Parameters.AddWithValue("@t", title);

                    await cmd.ExecuteNonQueryAsync();
                    Console.WriteLine($"(DEBUG) {title} was added to WishlistDB");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"(ERROR)Failed to add {title} into WishlistDB: {ex}");
            }
        }
    }

    public async Task RemoveWishListItem(string title)
    {
        
    }
}