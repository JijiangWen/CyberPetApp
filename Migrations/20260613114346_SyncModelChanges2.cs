using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Furnitures_Rooms_RoomId1",
                table: "Furnitures");

            migrationBuilder.DropIndex(
                name: "IX_Furnitures_RoomId1",
                table: "Furnitures");

            migrationBuilder.DropColumn(
                name: "RoomId1",
                table: "Furnitures");

            migrationBuilder.AlterColumn<int>(
                name: "TargetDepth",
                table: "FishingLures",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "TargetDepth",
                table: "FishingLines",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RoomId1",
                table: "Furnitures",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TargetDepth",
                table: "FishingLures",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TargetDepth",
                table: "FishingLines",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Furnitures_RoomId1",
                table: "Furnitures",
                column: "RoomId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Furnitures_Rooms_RoomId1",
                table: "Furnitures",
                column: "RoomId1",
                principalTable: "Rooms",
                principalColumn: "Id");
        }
    }
}
