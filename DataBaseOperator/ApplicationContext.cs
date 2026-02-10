using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<GameItem> games {get;set;}
    public DbSet<WishlistItem> wishlist {get;set;}
}