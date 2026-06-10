using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoWatererAndPassiveCatCare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AutoWaterers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaxWaterCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoWaterers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoWaterers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatererWaters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoWatererId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ThirstRestore = table.Column<int>(type: "integer", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatererWaters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatererWaters_AutoWaterers_AutoWatererId",
                        column: x => x.AutoWatererId,
                        principalTable: "AutoWaterers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoWaterers_PlayerId",
                table: "AutoWaterers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_WatererWaters_AutoWatererId_SlotIndex",
                table: "WatererWaters",
                columns: new[] { "AutoWatererId", "SlotIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatererWaters");

            migrationBuilder.DropTable(
                name: "AutoWaterers");
        }
    }
}
