using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddFishDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Fishes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Fishes");
        }
    }
}
