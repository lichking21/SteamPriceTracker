using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPriceTracker.Migrations
{
    /// <inheritdoc />
    public partial class AddGamesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    // Указываем integer, так как в GameItem у вас int
                    id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false, defaultValue: "UNKNOWN")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "games");
        }
    }
}
