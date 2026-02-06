using Microsoft.Extensions.Configuration;

namespace DataBaseOperator;

public class WishlistDB : Database
{
    public WishlistDB(IConfiguration configuration) : base(configuration) {}
}