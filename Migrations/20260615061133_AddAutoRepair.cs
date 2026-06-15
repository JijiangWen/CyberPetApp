using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoRepair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRepairEnabled",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AutoRepairThreshold",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 20);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRepairUnlocked",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRepairEnabled",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AutoRepairThreshold",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AutoRepairUnlocked",
                table: "Players");
        }
    }
}
