using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class HouseService
{
    private readonly AppDbContext _context;

    public HouseService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>从 DB 加载房屋；不存在则创建默认布局（客厅已解锁）。</summary>
    public async Task<PlayerHouse> LoadHouseAsync(Guid playerId)
    {
        var house = await _context.PlayerHouses.FirstOrDefaultAsync(h => h.PlayerId == playerId);
        if (house is null)
            return await CreateDefaultHouseAsync(playerId);

        var rooms = await _context.Rooms.Where(r => r.PlayerHouseId == house.Id).ToListAsync();
        var roomIds = rooms.Select(r => r.Id).ToList();
        var furniture = roomIds.Count == 0
            ? []
            : await _context.Furnitures.Where(f => roomIds.Contains(f.RoomId)).ToListAsync();

        var result = new PlayerHouse
        {
            Id = house.Id,
            PlayerId = playerId,
            House_Level = house.House_Level,
            Rooms = []
        };

        foreach (var room in rooms)
        {
            room.Furniture = furniture.Where(f => f.RoomId == room.Id).ToList();
            result.Rooms[room.Name] = room;
        }

        await EnsureMissingFurnitureAsync(result);
        return result;
    }

    /// <summary>为老存档补全模板中新增的家具条目。</summary>
    private async Task EnsureMissingFurnitureAsync(PlayerHouse loaded)
    {
        var template = new PlayerHouse { PlayerId = loaded.PlayerId };
        bool changed = false;

        foreach (var (roomName, templateRoom) in template.Rooms)
        {
            if (!loaded.Rooms.TryGetValue(roomName, out var room)) continue;
            var existingIds = room.Furniture.Select(f => f.FurnitureId).ToHashSet();

            foreach (var furn in templateRoom.Furniture)
            {
                if (existingIds.Contains(furn.FurnitureId)) continue;

                var dbFurn = new Furniture
                {
                    Id = Guid.NewGuid(),
                    RoomId = room.Id,
                    FurnitureId = furn.FurnitureId,
                    Name = furn.Name,
                    Price = furn.Price,
                    IsUnlocked = false,
                    Description = furn.Description
                };
                _context.Furnitures.Add(dbFurn);
                room.Furniture.Add(dbFurn);
                changed = true;
            }
        }

        if (changed)
            await _context.SaveChangesAsync();
    }

    /// <summary>新玩家注册时创建默认 PlayerHouse（客厅已解锁）。</summary>
    public async Task<PlayerHouse> CreateDefaultHouseAsync(Guid playerId)
    {
        var template = new PlayerHouse { PlayerId = playerId };
        var house = new PlayerHouse
        {
            Id = Guid.NewGuid(),
            PlayerId = playerId,
            House_Level = template.House_Level
        };
        _context.PlayerHouses.Add(house);

        foreach (var (name, room) in template.Rooms)
        {
            var dbRoom = new Room
            {
                Id = Guid.NewGuid(),
                PlayerHouseId = house.Id,
                Name = room.Name,
                IsUnlocked = room.IsUnlocked,
                UnlockPrice = room.UnlockPrice,
                Furniture = []
            };
            _context.Rooms.Add(dbRoom);

            foreach (var furn in room.Furniture)
            {
                _context.Furnitures.Add(new Furniture
                {
                    Id = Guid.NewGuid(),
                    RoomId = dbRoom.Id,
                    FurnitureId = furn.FurnitureId,
                    Name = furn.Name,
                    Price = furn.Price,
                    IsUnlocked = furn.IsUnlocked,
                    Description = furn.Description
                });
            }
        }

        await _context.SaveChangesAsync();
        return await LoadHouseAsync(playerId);
    }

    /// <summary>解锁房间：扣金币并持久化。</summary>
    public async Task<bool> SaveRoomUnlockAsync(Player player, Room room)
    {
        if (room.IsUnlocked || player.Money < room.UnlockPrice)
            return false;

        var dbRoom = await _context.Rooms.FindAsync(room.Id);
        if (dbRoom is null || dbRoom.IsUnlocked)
            return false;

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null || dbPlayer.Money < room.UnlockPrice)
            return false;

        dbPlayer.Money -= room.UnlockPrice;
        dbRoom.IsUnlocked = true;
        await _context.SaveChangesAsync();

        player.Money = dbPlayer.Money;
        room.IsUnlocked = true;
        return true;
    }

    /// <summary>购买家具：扣金币并持久化。</summary>
    public async Task<bool> SaveFurniturePurchaseAsync(Player player, Furniture furniture)
    {
        if (furniture.IsUnlocked || player.Money < furniture.Price)
            return false;

        var dbFurn = await _context.Furnitures.FindAsync(furniture.Id);
        if (dbFurn is null || dbFurn.IsUnlocked)
            return false;

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null || dbPlayer.Money < furniture.Price)
            return false;

        dbPlayer.Money -= furniture.Price;
        dbFurn.IsUnlocked = true;
        await _context.SaveChangesAsync();

        player.Money = dbPlayer.Money;
        furniture.IsUnlocked = true;
        return true;
    }

    /// <summary>家具升级一级：扣金币，加成 +5%，维护费 +3g/日。</summary>
    public async Task<(bool Ok, string Message)> UpgradeFurnitureAsync(Player player, Furniture furniture)
    {
        if (!furniture.IsUnlocked)
            return (false, "需先购买家具");
        if (furniture.UpgradeLevel >= EconomySinks.MaxFurnitureUpgradeLevel)
            return (false, "已达最高升级等级");

        int cost = EconomySinks.FurnitureUpgradeCost(furniture);
        var dbFurn = await _context.Furnitures.FindAsync(furniture.Id);
        if (dbFurn is null)
            return (false, "家具不存在");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null || dbPlayer.Money < cost)
            return (false, $"金币不足，升级需 {cost}g");

        dbPlayer.Money -= cost;
        dbFurn.UpgradeLevel++;
        await _context.SaveChangesAsync();

        player.Money = dbPlayer.Money;
        furniture.UpgradeLevel = dbFurn.UpgradeLevel;
        return (true, $"[{furniture.Name}] 升级至 Lv.{furniture.UpgradeLevel}，被动加成 +5%，维护费 +{EconomySinks.FurnitureUpgradeMaintenancePerLevel}g/日");
    }
}
