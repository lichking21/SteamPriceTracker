using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SteamPriceTracker.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tracked_games",
                columns: table => new
                {
                    game_id = table.Column<int>(type: "integer", nullable: false),
                    region = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<string>(type: "text", nullable: false),
                    discount = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    last_update = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracked_games", x => new { x.game_id, x.region });
                });

            migrationBuilder.CreateTable(
                name: "user_wishlist",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    game_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_wishlist", x => new { x.user_id, x.game_id });
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Region = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_ID",
                table: "users",
                column: "ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "tracked_games");

            migrationBuilder.DropTable(
                name: "user_wishlist");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
