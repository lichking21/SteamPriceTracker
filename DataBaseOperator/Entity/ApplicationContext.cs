using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) {}

    public DbSet<GameItem> games {get;set;}
    public DbSet<TrackedGamesItem> trackedGames {get;set;}
    public DbSet<UserItem> users {get;set;}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GameItem>().ToTable("games", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<TrackedGamesItem>().ToTable("tracked_games", t => t.ExcludeFromMigrations());

        base.OnModelCreating(modelBuilder);
    }
}