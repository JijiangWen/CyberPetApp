using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class BoatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public BoatService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    /// <summary>
    /// 获取玩家拥有的小船列表
    /// </summary>
    public async Task<List<PlayerBoat>> GetPlayerBoatsAsync(Guid playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PlayerBoats
            .Where(b => b.PlayerId == playerId)
            .OrderBy(b => b.PurchasedAt)
            .ToListAsync();
    }

    /// <summary>
    /// 购买小船
    /// </summary>
    public async Task<(bool Success, string Message)> PurchaseBoatAsync(Guid playerId, string boatType, string boatName, int price, int capacity)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var player = await db.Players.FindAsync(playerId);
        if (player == null) return (false, "玩家不存在");
        if (player.Money < price) return (false, "金币不足，小船需要 " + price + " g");

        // 检查是否已拥有同种类型的小船
        var alreadyOwns = await db.PlayerBoats.AnyAsync(b => b.PlayerId == playerId && b.BoatType == boatType);
        if (alreadyOwns) return (false, "你已经拥有这艘小船了！");

        // 扣钱与创建
        player.Money -= price;
        var newBoat = new PlayerBoat
        {
            PlayerId = playerId,
            BoatType = boatType,
            CustomName = boatName,
            MaxCapacity = capacity,
            PurchasePrice = price
        };

        db.PlayerBoats.Add(newBoat);
        await db.SaveChangesAsync();
        return (true, $"成功购入 {boatName}！");
    }

    /// <summary>
    /// 写入一次船钓捕获的记录
    /// </summary>
    public async Task SaveBoatCatchAsync(Guid playerId, string playerName, string boatName, Fish fish)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var record = new BoatCatchRecord
        {
            PlayerId = playerId,
            PlayerName = playerName,
            BoatName = boatName,
            FishName = fish.Name,
            Rarity = fish.Rarity,
            Weight = fish.ActualWeight,
            SizePercentage = fish.SizePercentage,
            CaughtAt = DateTime.UtcNow
        };
        db.BoatCatchRecords.Add(record);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 获取指定玩家所有的船钓鱼货记录
    /// </summary>
    public async Task<List<BoatCatchRecord>> GetBoatCatchHistoryAsync(Guid playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BoatCatchRecords
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.CaughtAt)
            .ToListAsync();
    }
}
