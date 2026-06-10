using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class Phase3OfflineBountyDex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DailyBountyClaimed",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DailyBountyDate",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DailyBountyFishName",
                table: "Players",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DailyBountyReward",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveAt",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WorkTickCount",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FishCatchRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FishName = table.Column<string>(type: "text", nullable: false),
                    CatchCount = table.Column<int>(type: "integer", nullable: false),
                    MaxWeight = table.Column<double>(type: "double precision", nullable: false),
                    BestRarity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FishCatchRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FishCatchRecords_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FishCatchRecords_PlayerId_FishName",
                table: "FishCatchRecords",
                columns: new[] { "PlayerId", "FishName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FishCatchRecords");

            migrationBuilder.DropColumn(
                name: "DailyBountyClaimed",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DailyBountyDate",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DailyBountyFishName",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "DailyBountyReward",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LastActiveAt",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "WorkTickCount",
                table: "Players");
        }
    }
}
