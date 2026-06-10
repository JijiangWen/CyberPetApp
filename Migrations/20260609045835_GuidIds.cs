using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CyberPetApp.Migrations
{
    /// <inheritdoc />
    public partial class GuidIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Money = table.Column<int>(type: "integer", nullable: false),
                    IsWorking = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AutoFeeders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodCount = table.Column<int>(type: "integer", nullable: false),
                    MaxFoodCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AutoFeeders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AutoFeeders_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BackpackItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackpackItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BackpackItems_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CyberCats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Health = table.Column<int>(type: "integer", nullable: false),
                    Happiness = table.Column<int>(type: "integer", nullable: false),
                    Energy = table.Column<int>(type: "integer", nullable: false),
                    Hunger = table.Column<int>(type: "integer", nullable: false),
                    Thirst = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CyberCats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CyberCats_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fishes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    HungerRestore = table.Column<int>(type: "integer", nullable: false),
                    EnergyRestore = table.Column<int>(type: "integer", nullable: false),
                    HappinessRestore = table.Column<int>(type: "integer", nullable: false),
                    ActualWeight = table.Column<double>(type: "double precision", nullable: false),
                    SizePercentage = table.Column<double>(type: "double precision", nullable: false),
                    SellPrice = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fishes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fishes_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerHouses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    House_Level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerHouses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerHouses_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeederFoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AutoFeederId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    HungerRestore = table.Column<int>(type: "integer", nullable: false),
                    EnergyRestore = table.Column<int>(type: "integer", nullable: false),
                    HappinessRestore = table.Column<int>(type: "integer", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeederFoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeederFoods_AutoFeeders_AutoFeederId",
                        column: x => x.AutoFeederId,
                        principalTable: "AutoFeeders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerHouseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "boolean", nullable: false),
                    UnlockPrice = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_PlayerHouses_PlayerHouseId",
                        column: x => x.PlayerHouseId,
                        principalTable: "PlayerHouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Furnitures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    FurnitureId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false),
                    IsUnlocked = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    RoomId1 = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Furnitures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Furnitures_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Furnitures_Rooms_RoomId1",
                        column: x => x.RoomId1,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AutoFeeders_PlayerId",
                table: "AutoFeeders",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BackpackItems_PlayerId_ItemName",
                table: "BackpackItems",
                columns: new[] { "PlayerId", "ItemName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CyberCats_PlayerId",
                table: "CyberCats",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FeederFoods_AutoFeederId_SlotIndex",
                table: "FeederFoods",
                columns: new[] { "AutoFeederId", "SlotIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fishes_PlayerId",
                table: "Fishes",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Furnitures_RoomId_FurnitureId",
                table: "Furnitures",
                columns: new[] { "RoomId", "FurnitureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Furnitures_RoomId1",
                table: "Furnitures",
                column: "RoomId1");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerHouses_PlayerId",
                table: "PlayerHouses",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_PlayerHouseId_Name",
                table: "Rooms",
                columns: new[] { "PlayerHouseId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackpackItems");

            migrationBuilder.DropTable(
                name: "CyberCats");

            migrationBuilder.DropTable(
                name: "FeederFoods");

            migrationBuilder.DropTable(
                name: "Fishes");

            migrationBuilder.DropTable(
                name: "Furnitures");

            migrationBuilder.DropTable(
                name: "AutoFeeders");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "PlayerHouses");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
