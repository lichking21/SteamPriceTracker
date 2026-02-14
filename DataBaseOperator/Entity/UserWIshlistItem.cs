using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("user_wishlist")]
public class UserWishlistItem
{
    [Column("user_id")]
    public long UserId {get;set;}

    [Column("game_id")]
    public int GameId {get;set;}

    public UserWishlistItem() {}

    public UserWishlistItem(long userId, int gameId)
    {
        UserId = userId;
        GameId = gameId;
    }
}