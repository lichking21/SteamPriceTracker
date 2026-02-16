using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<GameItem> games {get;set;}
    public DbSet<TrackedGamesItem> trackedGames {get;set;}
    public DbSet<UserItem> users {get;set;}
    public DbSet<UserWishlistItem> usersWishlist {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserItem>().HasIndex(u => u.ID).IsUnique();

        modelBuilder.Entity<TrackedGamesItem>()
            .ToTable("tracked_games")
            .HasKey(tg => new {tg.GameId, tg.Region});

        modelBuilder.Entity<UserWishlistItem>()
            .ToTable("user_wishlist")
            .HasKey(uw => new {uw.UserId, uw.GameId});

        base.OnModelCreating(modelBuilder);
    }
}