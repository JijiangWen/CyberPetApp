using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>房屋每日维护费：每 720 tick（游戏日）扣款。</summary>
public class MaintenanceService
{
    public const int TicksPerGameDay = 720;
    public const int FeePerRoom = 15;
    public const int FeePerFurniture = 8;
    public const int OverdueHappinessPenalty = 50;
    public const int OverdueFeeMultiplier = 2;

    private readonly AppDbContext _context;

    public MaintenanceService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>维护费 = 房间×15g + 家具×8g + 升级×3g/级；拖欠时翻倍。</summary>
    public static int CalculateFee(PlayerHouse house, bool maintenanceOverdue = false)
    {
        int rooms = house.Rooms.Values.Count(r => r.IsUnlocked);
        var unlocked = house.Rooms.Values.SelectMany(r => r.Furniture).Where(f => f.IsUnlocked).ToList();
        int furniture = unlocked.Count;
        int upgradeExtra = unlocked.Sum(f => f.UpgradeLevel * EconomySinks.FurnitureUpgradeMaintenancePerLevel);
        int fee = rooms * FeePerRoom + furniture * FeePerFurniture + upgradeExtra;
        if (maintenanceOverdue)
            fee *= OverdueFeeMultiplier;
        return fee;
    }

    /// <summary>尝试扣维护费；余额不足则标记拖欠并惩罚猫幸福。</summary>
    public async Task<(bool Paid, int Fee, string Message)> TryChargeAsync(
        Player player, PlayerHouse house, CyberCat cat)
    {
        int fee = CalculateFee(house, player.MaintenanceOverdue);
        if (fee <= 0)
        {
            player.LastMaintenanceAt = DateTime.UtcNow;
            await PersistAsync(player, cat);
            return (true, 0, "无需维护费（无解锁房间/家具）");
        }

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, fee, "玩家不存在");

        if (dbPlayer.Money >= fee)
        {
            dbPlayer.Money -= fee;
            dbPlayer.LastMaintenanceAt = DateTime.UtcNow;
            dbPlayer.MaintenanceOverdue = false;
            player.Money = dbPlayer.Money;
            player.LastMaintenanceAt = dbPlayer.LastMaintenanceAt;
            player.MaintenanceOverdue = false;
            await _context.SaveChangesAsync();
            return (true, fee, $"已扣维护费 {fee}g（房间+家具）");
        }

        // 余额不足：轻惩罚
        if (!dbPlayer.MaintenanceOverdue)
        {
            cat.Happiness = Math.Max(0, cat.Happiness - OverdueHappinessPenalty);
            await SaveCatAsync(cat);
        }

        dbPlayer.MaintenanceOverdue = true;
        dbPlayer.LastMaintenanceAt = DateTime.UtcNow;
        player.MaintenanceOverdue = true;
        player.LastMaintenanceAt = dbPlayer.LastMaintenanceAt;
        await _context.SaveChangesAsync();
        return (false, fee, $"余额不足，维护费 {fee}g 拖欠（猫幸福 -{OverdueHappinessPenalty}，钓鱼 -8%，下次费用×{OverdueFeeMultiplier}）");
    }

    public bool IsDue(Player player) =>
        player.LastMaintenanceAt is null
        || DateTime.UtcNow - player.LastMaintenanceAt.Value >= TimeSpan.FromHours(24);

    private async Task PersistAsync(Player player, CyberCat cat)
    {
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is not null)
            dbPlayer.LastMaintenanceAt = player.LastMaintenanceAt;
        await _context.SaveChangesAsync();
    }

    private async Task SaveCatAsync(CyberCat cat)
    {
        var existing = await _context.CyberCats.FirstOrDefaultAsync(c => c.PlayerId == cat.PlayerId);
        if (existing is not null)
            existing.Happiness = cat.Happiness;
        await _context.SaveChangesAsync();
    }
}
