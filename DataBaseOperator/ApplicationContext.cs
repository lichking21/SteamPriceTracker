using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    DbSet<GameItem> games {get;set;}
    DbSet<WishlistItem> wishlist {get;set;}
}