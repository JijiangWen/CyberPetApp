using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCatLevelStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Agi",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "BackgroundTickCount",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CatLevel",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "CatXp",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Chm",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "Luk",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "Sen",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "Sta",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "Str",
                table: "CyberCats",
                type: "integer",
                nullable: false,
                defaultValue: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Agi",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "BackgroundTickCount",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "CatLevel",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "CatXp",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "Chm",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "Luk",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "Sen",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "Sta",
                table: "CyberCats");

            migrationBuilder.DropColumn(
                name: "Str",
                table: "CyberCats");
        }
    }
}
