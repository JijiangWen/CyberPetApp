using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class Phase1EconomyLoop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketListings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FishName = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    ActualWeight = table.Column<double>(type: "double precision", nullable: false),
                    SizePercentage = table.Column<double>(type: "double precision", nullable: false),
                    HungerRestore = table.Column<int>(type: "integer", nullable: false),
                    EnergyRestore = table.Column<int>(type: "integer", nullable: false),
                    HappinessRestore = table.Column<int>(type: "integer", nullable: false),
                    BaseSellPrice = table.Column<int>(type: "integer", nullable: false),
                    ListingFloorPrice = table.Column<int>(type: "integer", nullable: false),
                    ListedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketListings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketListings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NpcOffers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerType = table.Column<int>(type: "integer", nullable: false),
                    OfferPrice = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAccepted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NpcOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NpcOffers_MarketListings_ListingId",
                        column: x => x.ListingId,
                        principalTable: "MarketListings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketListings_PlayerId_IsActive",
                table: "MarketListings",
                columns: new[] { "PlayerId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_NpcOffers_ListingId_IsAccepted",
                table: "NpcOffers",
                columns: new[] { "ListingId", "IsAccepted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NpcOffers");

            migrationBuilder.DropTable(
                name: "MarketListings");
        }
    }
}
