using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class PlayerService
{
    private readonly AppDbContext _context;

    public PlayerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> LoadPlayerAsync(Guid playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player is null) return null;

        player.FishBackpack = await _context.Fishes
            .Where(f => f.PlayerId == playerId)
            .ToListAsync();

        var backpackItems = await _context.BackpackItems
            .Where(b => b.PlayerId == playerId)
            .ToListAsync();

        player.Backpack = backpackItems.ToDictionary(b => b.ItemName, b => b.Quantity);
        player.Items = [];

        // 老玩家已选非默认工种：视为已解锁，避免重复扣培训费
        if (player.SelectedWorkJob != WorkJobType.Construction && !player.IsWorkJobUnlocked(player.SelectedWorkJob))
        {
            player.UnlockWorkJob(player.SelectedWorkJob);
            var tracked = await _context.Players.FindAsync(playerId);
            if (tracked is not null)
            {
                tracked.UnlockedWorkJobMask = player.UnlockedWorkJobMask;
                await _context.SaveChangesAsync();
            }
        }

        return player;
    }

    /// <summary>持久化玩家进度（金币 + 钓鱼/烹饪技能等级与经验，含 LastActiveAt）。</summary>
    public async Task SaveProgressAsync(Player player)
    {
        SyncProgressToTracked(player);
        await _context.SaveChangesAsync();
    }

    /// <summary>P0-1：将内存 Player 同步到 EF 跟踪实体，不单独 SaveChanges。</summary>
    public void SyncProgressToTracked(Player player)
    {
        var tracked = _context.Players.Local.FirstOrDefault(p => p.Id == player.Id)
            ?? _context.Players.Find(player.Id);
        if (tracked is null) return;

        if (!ReferenceEquals(tracked, player))
        {
            tracked.Money = player.Money;
            tracked.FishingLevel = player.FishingLevel;
            tracked.FishingXp = player.FishingXp;
            tracked.CookingLevel = player.CookingLevel;
            tracked.CookingXp = player.CookingXp;
            tracked.SelectedWorkJob = player.SelectedWorkJob;
            tracked.IsWorking = player.IsWorking;
            tracked.LastMaintenanceAt = player.LastMaintenanceAt;
            tracked.MaintenanceOverdue = player.MaintenanceOverdue;
            tracked.WorkTickCount = player.WorkTickCount;
            tracked.LastActiveAt = player.LastActiveAt;
            tracked.DailyBountyDate = player.DailyBountyDate;
            tracked.DailyBountyFishName = player.DailyBountyFishName;
            tracked.DailyBountyReward = player.DailyBountyReward;
            tracked.DailyBountyClaimed = player.DailyBountyClaimed;
            tracked.DailyBountyManualRefreshCount = player.DailyBountyManualRefreshCount;
            tracked.UnlockedWorkJobMask = player.UnlockedWorkJobMask;
            tracked.MilestonePoints = player.MilestonePoints;
            tracked.LifetimeFeedCount = player.LifetimeFeedCount;
            tracked.TotalFishGoldEarned = player.TotalFishGoldEarned;
            tracked.MarketSalesCount = player.MarketSalesCount;
            tracked.RareCatchCount = player.RareCatchCount;
            tracked.LegendaryCatchCount = player.LegendaryCatchCount;
            tracked.CookCount = player.CookCount;
            tracked.ExpeditionZoneId = player.ExpeditionZoneId;
            tracked.ExpeditionEndsAt = player.ExpeditionEndsAt;
        }
    }

    /// <summary>扣金币（余额不足返回 false）。</summary>
    public async Task<bool> TrySpendGoldAsync(Player player, int amount)
    {
        if (amount <= 0) return true;
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null || tracked.Money < amount) return false;
        tracked.Money -= amount;
        player.Money = tracked.Money;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>解锁工种：扣培训费并写入位掩码。</summary>
    public async Task<(bool Ok, string Message)> TryUnlockWorkJobAsync(Player player, WorkJobType job)
    {
        if (player.IsWorkJobUnlocked(job))
            return (true, "");

        int fee = EconomySinks.WorkJobUnlockFee(job);
        if (fee > 0 && !await TrySpendGoldAsync(player, fee))
            return (false, $"金币不足，解锁{WorkingPlace.JobLabel(job)}需 {fee}g 培训费");

        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null) return (false, "玩家不存在");

        player.UnlockWorkJob(job);
        tracked.UnlockedWorkJobMask = player.UnlockedWorkJobMask;
        await _context.SaveChangesAsync();

        return fee > 0
            ? (true, $"已解锁{WorkingPlace.JobLabel(job)}，扣培训费 {fee}g")
            : (true, "");
    }

    /// <summary>商店购买：扣金币并将食物写入 BackpackItems。</summary>
    public async Task<bool> BuyShopItemAsync(Player player, ShopItem item)
    {
        if (player.Money < item.Price) return false;

        player.Money -= item.Price;
        await UpsertBackpackItemAsync(player.Id, item.Food.Name, +1);
        await SyncMoneyAsync(player);
        await _context.SaveChangesAsync();

        player.Backpack[item.Food.Name] = player.Backpack.GetValueOrDefault(item.Food.Name) + 1;
        return true;
    }

    /// <summary>消耗背包物品（喂食/装入喂食器），同步 BackpackItems 与内存字典。</summary>
    public async Task<bool> ConsumeBackpackItemAsync(Player player, string itemName, int amount = 1)
    {
        if (amount <= 0) return false;

        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == itemName);
        if (item is null || item.Quantity < amount) return false;

        item.Quantity -= amount;
        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);
        await _context.SaveChangesAsync();

        int left = player.Backpack.GetValueOrDefault(itemName) - amount;
        if (left <= 0) player.Backpack.Remove(itemName);
        else player.Backpack[itemName] = left;
        return true;
    }

    /// <summary>发放背包物品（如打工摊位券）。</summary>
    public async Task GrantBackpackItemAsync(Player player, string itemName, int amount = 1)
    {
        if (amount <= 0) return;
        await UpsertBackpackItemAsync(player.Id, itemName, amount);
        await _context.SaveChangesAsync();
        player.Backpack[itemName] = player.Backpack.GetValueOrDefault(itemName) + amount;
    }

    private async Task UpsertBackpackItemAsync(Guid playerId, string itemName, int delta)
    {
        var item = _context.BackpackItems.Local
                       .FirstOrDefault(b => b.PlayerId == playerId && b.ItemName == itemName)
                   ?? await _context.BackpackItems
                       .FirstOrDefaultAsync(b => b.PlayerId == playerId && b.ItemName == itemName);

        if (item is null)
            _context.BackpackItems.Add(new BackpackItem { PlayerId = playerId, ItemName = itemName, Quantity = delta });
        else
            item.Quantity += delta;
    }

    private async Task SyncMoneyAsync(Player player)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is not null && !ReferenceEquals(tracked, player))
            tracked.Money = player.Money;
    }
}
