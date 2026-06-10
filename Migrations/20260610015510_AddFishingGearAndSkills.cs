using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFishingGearAndSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CookingLevel",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CookingXp",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FishingLevel",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "FishingXp",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FishingLures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Attraction = table.Column<double>(type: "double precision", nullable: false),
                    RarityBonus = table.Column<double>(type: "double precision", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingLures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishingLures_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FishingReels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DragPower = table.Column<double>(type: "double precision", nullable: false),
                    GearRatio = table.Column<double>(type: "double precision", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingReels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishingReels_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FishingRods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Sensitivity = table.Column<double>(type: "double precision", nullable: false),
                    MaxStrength = table.Column<double>(type: "double precision", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishingRods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishingRods_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FishingLures_PlayerId_Name",
                table: "FishingLures",
                columns: new[] { "PlayerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FishingReels_PlayerId_Name",
                table: "FishingReels",
                columns: new[] { "PlayerId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FishingRods_PlayerId_Name",
                table: "FishingRods",
                columns: new[] { "PlayerId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FishingLures");

            migrationBuilder.DropTable(
                name: "FishingReels");

            migrationBuilder.DropTable(
                name: "FishingRods");

            migrationBuilder.DropColumn(
                name: "CookingLevel",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CookingXp",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "FishingLevel",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "FishingXp",
                table: "Players");
        }
    }
}
