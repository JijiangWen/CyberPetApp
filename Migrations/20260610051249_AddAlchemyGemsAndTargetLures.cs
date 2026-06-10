using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAlchemyGemsAndTargetLures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTargetExclusive",
                table: "FishCatchRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTargetExclusive",
                table: "FishCatchRecords");
        }
    }
}
