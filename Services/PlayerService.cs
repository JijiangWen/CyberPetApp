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

        player.Backpack = backpackItems
            .Where(b => !b.ItemName.StartsWith("skin_"))
            .ToDictionary(b => b.ItemName, b => b.Quantity);

        player.UnlockedCatSkins = backpackItems
            .Where(b => b.ItemName.StartsWith("skin_"))
            .Select(b => b.ItemName.Substring(5))
            .ToHashSet();
        player.UnlockedCatSkins.Add("default");

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

    public static bool TryPrepareBuyShopItem(Player player, ShopItem item, out string error)
    {
        error = "";
        if (player.Money < item.Price)
        {
            error = "金币不足";
            return false;
        }

        return true;
    }

    public static void ApplyBuyShopItemOptimistic(Player player, ShopItem item)
    {
        player.Money -= item.Price;
        player.Backpack[item.Food.Name] = player.Backpack.GetValueOrDefault(item.Food.Name) + 1;
    }

    public static void RollbackBuyShopItem(Player player, ShopItem item)
    {
        player.Money += item.Price;
        int left = player.Backpack.GetValueOrDefault(item.Food.Name) - 1;
        if (left <= 0) player.Backpack.Remove(item.Food.Name);
        else player.Backpack[item.Food.Name] = left;
    }

    /// <summary>乐观 UI 已扣费/加背包后，仅同步数据库。</summary>
    public async Task<(bool Ok, string Message)> CommitBuyShopItemAsync(Player player, ShopItem item)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null)
            return (false, "玩家不存在");

        tracked.Money = player.Money;

        int targetQty = player.Backpack.GetValueOrDefault(item.Food.Name);
        if (targetQty <= 0)
            return (false, "背包同步失败");

        var backpackItem = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == item.Food.Name);
        if (backpackItem is null)
            _context.BackpackItems.Add(new BackpackItem { PlayerId = player.Id, ItemName = item.Food.Name, Quantity = targetQty });
        else
            backpackItem.Quantity = targetQty;

        await _context.SaveChangesAsync();
        return (true, "");
    }

    public static bool TryPrepareFishBackpackUpgrade(Player player, out int increment, out int cost, out string error)
    {
        increment = EconomySinks.FishBackpackNextIncrement(player.FishBackpackCapacity);
        cost = EconomySinks.FishBackpackUpgradeCost(player.FishBackpackCapacity);
        error = "";
        if (increment <= 0 || cost <= 0)
        {
            error = "已达上限";
            return false;
        }

        if (player.Money < cost)
        {
            error = "金币不足，升级失败";
            return false;
        }

        return true;
    }

    public static void ApplyFishBackpackUpgradeOptimistic(Player player, int increment, int cost)
    {
        player.Money -= cost;
        player.FishBackpackCapacity = Math.Max(50, player.FishBackpackCapacity) + increment;
    }

    public static void RollbackFishBackpackUpgrade(Player player, int increment, int cost)
    {
        player.Money += cost;
        player.FishBackpackCapacity = Math.Max(50, player.FishBackpackCapacity - increment);
    }

    /// <summary>乐观 UI 已扣费/加容量后，仅同步数据库。</summary>
    public async Task<bool> CommitFishBackpackUpgradeAsync(Player player)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null) return false;
        tracked.Money = player.Money;
        tracked.FishBackpackCapacity = player.FishBackpackCapacity;
        await _context.SaveChangesAsync();
        return true;
    }

    public static bool TryPrepareConsumeBackpack(Player player, string itemName, out string error, int amount = 1)
    {
        error = "";
        if (!player.Backpack.TryGetValue(itemName, out int qty) || qty < amount)
        {
            error = $"背包里没有 {itemName}";
            return false;
        }

        return true;
    }

    public static void ApplyConsumeBackpackOptimistic(Player player, string itemName, int amount = 1)
    {
        int left = player.Backpack.GetValueOrDefault(itemName) - amount;
        if (left <= 0) player.Backpack.Remove(itemName);
        else player.Backpack[itemName] = left;
    }

    public static void RollbackConsumeBackpackOptimistic(Player player, string itemName, int amount = 1)
    {
        player.Backpack[itemName] = player.Backpack.GetValueOrDefault(itemName) + amount;
    }

    public async Task<bool> CommitConsumeBackpackAsync(Player player, string itemName)
    {
        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == itemName);
        int targetQty = player.Backpack.GetValueOrDefault(itemName);
        if (targetQty <= 0)
        {
            if (item is not null) _context.BackpackItems.Remove(item);
        }
        else
        {
            if (item is null) return false;
            item.Quantity = targetQty;
        }

        await _context.SaveChangesAsync();
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

    /// <summary>升级鱼背包容量：扣金币并增加容量（失败返回 false）。</summary>
    public async Task<bool> TryUpgradeFishBackpackAsync(Player player, int additionalSlots, int cost)
    {
        if (additionalSlots <= 0) return false;
        // 扣费（余额不足返回 false）
        if (!await TrySpendGoldAsync(player, cost)) return false;

        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null) return false;

        // 增加容量并同步内存对象
        tracked.FishBackpackCapacity = Math.Max(50, tracked.FishBackpackCapacity) + additionalSlots;
        player.FishBackpackCapacity = tracked.FishBackpackCapacity;
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>解锁猫咪皮肤：扣费并存入 BackpackItems（"skin_X"）。</summary>
    public async Task<bool> TryUnlockSkinAsync(Player player, string skinId, int price)
    {
        if (player.UnlockedCatSkins.Contains(skinId)) return true;
        
        if (price > 0 && !await TrySpendGoldAsync(player, price)) return false;

        await UpsertBackpackItemAsync(player.Id, "skin_" + skinId, 1);
        await _context.SaveChangesAsync();
        
        player.UnlockedCatSkins.Add(skinId);
        return true;
    }
}
