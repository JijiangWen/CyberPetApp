using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoRefill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRefillEnabled",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRefillUnlocked",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRefillEnabled",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AutoRefillUnlocked",
                table: "Players");
        }
    }
}
