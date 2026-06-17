using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiplayerBoatSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BoatCatchRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerName = table.Column<string>(type: "text", nullable: false),
                    BoatName = table.Column<string>(type: "text", nullable: false),
                    FishName = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    SizePercentage = table.Column<double>(type: "double precision", nullable: false),
                    CaughtAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoatCatchRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoatCatchRecords_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerBoats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BoatType = table.Column<string>(type: "text", nullable: false),
                    CustomName = table.Column<string>(type: "text", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    PurchasePrice = table.Column<int>(type: "integer", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerBoats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerBoats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BoatCatchRecords_PlayerId",
                table: "BoatCatchRecords",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerBoats_PlayerId_BoatType",
                table: "PlayerBoats",
                columns: new[] { "PlayerId", "BoatType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoatCatchRecords");

            migrationBuilder.DropTable(
                name: "PlayerBoats");
        }
    }
}
