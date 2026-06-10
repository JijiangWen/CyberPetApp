using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCatExpedition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CookCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpeditionEndsAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpeditionZoneId",
                table: "Players",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegendaryCatchCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LifetimeFeedCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MarketSalesCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MilestonePoints",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RareCatchCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalFishGoldEarned",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Durability",
                table: "FishingRods",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "Durability",
                table: "FishingReels",
                type: "integer",
                nullable: false,
                defaultValue: 100);

            migrationBuilder.CreateTable(
                name: "PlayerAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<string>(type: "text", nullable: false),
                    Progress = table.Column<int>(type: "integer", nullable: false),
                    RewardClaimed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAchievements_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerMilestoneUnlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<string>(type: "text", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerMilestoneUnlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerMilestoneUnlocks_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotLicenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpotName = table.Column<string>(type: "text", nullable: false),
                    HasPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    RentalPaidDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotLicenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotLicenses_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAchievements_PlayerId_AchievementId",
                table: "PlayerAchievements",
                columns: new[] { "PlayerId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerMilestoneUnlocks_PlayerId_ItemId",
                table: "PlayerMilestoneUnlocks",
                columns: new[] { "PlayerId", "ItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpotLicenses_PlayerId_SpotName",
                table: "SpotLicenses",
                columns: new[] { "PlayerId", "SpotName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerAchievements");

            migrationBuilder.DropTable(
                name: "PlayerMilestoneUnlocks");

            migrationBuilder.DropTable(
                name: "SpotLicenses");

            migrationBuilder.DropColumn(
                name: "CookCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ExpeditionEndsAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ExpeditionZoneId",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LegendaryCatchCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LifetimeFeedCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MarketSalesCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MilestonePoints",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RareCatchCount",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TotalFishGoldEarned",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Durability",
                table: "FishingRods");

            migrationBuilder.DropColumn(
                name: "Durability",
                table: "FishingReels");
        }
    }
}
