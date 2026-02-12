using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("user_wishlist")]
public class UserWishlistItem
{
    [Key]
    [Column("user_id")]
    public int UserId {get;set;}

    [Column("game_id")]
    public int GameId {get;set;}
}