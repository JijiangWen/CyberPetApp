using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerCatBuffs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FishingLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    LineStrength = table.Column<double>(type: "double precision", nullable: false),
                    LineSensitivity = table.Column<double>(type: "double precision", nullable: false),
                    LineStealth = table.Column<double>(type: "double precision", nullable: false),
                    AbrasionResistance = table.Column<double>(type: "double precision", nullable: false),
                    TargetDepth = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RequiredLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Durability = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    IsCrafted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishingLines_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCatBuffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuffType = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    TickIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    RemainingTicks = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextTickAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SourceFoodName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCatBuffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerCatBuffs_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerGems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    GemType = table.Column<int>(type: "integer", nullable: false),
                    BonusValue = table.Column<double>(type: "double precision", nullable: false),
                    IsSocketed = table.Column<bool>(type: "boolean", nullable: false),
                    SocketedSlot = table.Column<int>(type: "integer", nullable: true),
                    SocketedGearId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerGems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerGems_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerTargetLures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<string>(type: "text", nullable: false),
                    RemainingUses = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerTargetLures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerTargetLures_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FishingLines_PlayerId_Name",
                table: "FishingLines",
                columns: new[] { "PlayerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCatBuffs_PlayerId_BuffType",
                table: "PlayerCatBuffs",
                columns: new[] { "PlayerId", "BuffType" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerGems_PlayerId_IsSocketed_SocketedSlot",
                table: "PlayerGems",
                columns: new[] { "PlayerId", "IsSocketed", "SocketedSlot" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerTargetLures_PlayerId_RecipeId",
                table: "PlayerTargetLures",
                columns: new[] { "PlayerId", "RecipeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FishingLines");

            migrationBuilder.DropTable(
                name: "PlayerCatBuffs");

            migrationBuilder.DropTable(
                name: "PlayerGems");

            migrationBuilder.DropTable(
                name: "PlayerTargetLures");
        }
    }
}
