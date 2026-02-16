using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataBaseOperator.Entities;

[Table("tracked_games")]
public class TrackedGamesItem
{
    [Column("game_id")]
    public int GameId {get;set;}

    [Column("region")]
    public string Region {get;set;} = "kg";

    [Column("price")]
    public string Price {get;set;} = "N/A";
    
    [Column("discount")]
    public int Discount {get;set;}
    
    [Column("title")]
    public string? Title {get;set;}

    [Column("last_update")]
    public DateTime LastUpdate {get;set;} = DateTime.UtcNow;

    public TrackedGamesItem() {}

    public TrackedGamesItem(int gameId, string region, string price, int discount, string? title = null)
    {
        GameId = gameId;
        Price = price;
        Discount = discount;
        Title = title;
        Region = region;
    }
}