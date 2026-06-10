using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class ExpandFishingSpotsGearParams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CastRange",
                table: "FishingRods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "FishingRods",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<double>(
                name: "LineCapacity",
                table: "FishingReels",
                type: "double precision",
                nullable: false,
                defaultValue: 8.0);

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "FishingReels",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<double>(
                name: "Smoothness",
                table: "FishingReels",
                type: "double precision",
                nullable: false,
                defaultValue: 0.29999999999999999);

            migrationBuilder.AddColumn<int>(
                name: "DurabilityRemaining",
                table: "FishingLures",
                type: "integer",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<int>(
                name: "RequiredLevel",
                table: "FishingLures",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "TargetDepth",
                table: "FishingLures",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CastRange",
                table: "FishingRods");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "FishingRods");

            migrationBuilder.DropColumn(
                name: "LineCapacity",
                table: "FishingReels");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "FishingReels");

            migrationBuilder.DropColumn(
                name: "Smoothness",
                table: "FishingReels");

            migrationBuilder.DropColumn(
                name: "DurabilityRemaining",
                table: "FishingLures");

            migrationBuilder.DropColumn(
                name: "RequiredLevel",
                table: "FishingLures");

            migrationBuilder.DropColumn(
                name: "TargetDepth",
                table: "FishingLures");
        }
    }
}
