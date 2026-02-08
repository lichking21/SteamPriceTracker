public class WishlistItem
{
    public int GameId {get;set;}
    public string Price {get;set;} = "N/A";
    public int Discount {get;set;}
    public string? Title {get;set;}

    public WishlistItem(int gameId, string price, int discount, string? title = null)
    {
        GameId = gameId;
        Price = price;
        Discount = discount;
        Title = title;
    }
}