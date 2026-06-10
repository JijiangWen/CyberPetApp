using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class AchievementView
{
    public AchievementDef Def { get; init; } = null!;
    public PlayerAchievement Row { get; init; } = null!;
    public int CurrentValue { get; init; }
    public bool Completed => CurrentValue >= Def.Target;
}

public class AchievementService
{
    private readonly AppDbContext _context;

    public AchievementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnsureInitializedAsync(Guid playerId)
    {
        var existing = await _context.PlayerAchievements
            .Where(a => a.PlayerId == playerId)
            .Select(a => a.AchievementId)
            .ToListAsync();
        var set = existing.ToHashSet();
        bool changed = false;
        foreach (var def in MilestoneCatalog.Achievements)
        {
            if (set.Contains(def.Id)) continue;
            _context.PlayerAchievements.Add(new PlayerAchievement
            {
                PlayerId = playerId,
                AchievementId = def.Id
            });
            changed = true;
        }
        if (changed) await _context.SaveChangesAsync();
    }

    public async Task<List<AchievementView>> GetViewsAsync(Player player, IReadOnlyList<FishCatchRecord> fishRecords, bool hasDeepSeaPermanent)
    {
        await EnsureInitializedAsync(player.Id);
        var rows = await _context.PlayerAchievements
            .Where(a => a.PlayerId == player.Id)
            .ToDictionaryAsync(a => a.AchievementId);

        return MilestoneCatalog.Achievements.Select(def =>
        {
            rows.TryGetValue(def.Id, out var row);
            row ??= new PlayerAchievement { AchievementId = def.Id };
            int current = GetTrackValue(player, def, fishRecords, hasDeepSeaPermanent);
            if (row.Progress < current)
            {
                row.Progress = current;
                var tracked = rows.GetValueOrDefault(def.Id);
                if (tracked is not null) tracked.Progress = current;
            }
            return new AchievementView { Def = def, Row = row, CurrentValue = current };
        }).ToList();
    }

    public async Task SyncProgressAsync(Player player, IReadOnlyList<FishCatchRecord> fishRecords, bool hasDeepSeaPermanent)
    {
        await EnsureInitializedAsync(player.Id);
        var rows = await _context.PlayerAchievements
            .Where(a => a.PlayerId == player.Id)
            .ToListAsync();

        foreach (var row in rows)
        {
            var def = MilestoneCatalog.FindAchievement(row.AchievementId);
            if (def is null) continue;
            int current = GetTrackValue(player, def, fishRecords, hasDeepSeaPermanent);
            if (current > row.Progress) row.Progress = current;
        }
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Ok, string Message)> TryClaimRewardAsync(Player player, string achievementId)
    {
        var def = MilestoneCatalog.FindAchievement(achievementId);
        if (def is null) return (false, "成就不存在");

        var row = await _context.PlayerAchievements
            .FirstOrDefaultAsync(a => a.PlayerId == player.Id && a.AchievementId == achievementId);
        if (row is null) return (false, "成就未初始化");
        if (row.RewardClaimed) return (false, "奖励已领取");
        if (row.Progress < def.Target) return (false, $"进度 {row.Progress}/{def.Target}，未完成");

        row.RewardClaimed = true;
        var db = await _context.Players.FindAsync(player.Id);
        if (db is null) return (false, "玩家不存在");
        db.MilestonePoints += def.PointReward;
        player.MilestonePoints = db.MilestonePoints;
        await _context.SaveChangesAsync();
        return (true, $"领取 [{def.Title}] +{def.PointReward} 里程碑点数");
    }

    public async Task<(bool Ok, string Message)> TryBuyShopItemAsync(Player player, string itemId)
    {
        var item = MilestoneCatalog.FindShopItem(itemId);
        if (item is null) return (false, "商品不存在");

        if (await _context.PlayerMilestoneUnlocks
            .AnyAsync(u => u.PlayerId == player.Id && u.ItemId == itemId))
            return (false, "已兑换过该商品");

        var db = await _context.Players.FindAsync(player.Id);
        if (db is null) return (false, "玩家不存在");
        if (db.MilestonePoints < item.PointCost)
            return (false, $"里程碑点数不足，需 {item.PointCost} pt（当前 {db.MilestonePoints}）");
        if (db.Money < item.GoldCost)
            return (false, $"金币不足，需 {item.GoldCost}g");

        db.MilestonePoints -= item.PointCost;
        db.Money -= item.GoldCost;
        player.MilestonePoints = db.MilestonePoints;
        player.Money = db.Money;

        _context.PlayerMilestoneUnlocks.Add(new PlayerMilestoneUnlock
        {
            PlayerId = player.Id,
            ItemId = itemId
        });
        await _context.SaveChangesAsync();
        return (true, $"兑换 [{item.Title}]，-{item.PointCost} pt · -{item.GoldCost}g");
    }

    public Task<List<string>> GetUnlockedItemIdsAsync(Guid playerId) =>
        _context.PlayerMilestoneUnlocks
            .Where(u => u.PlayerId == playerId)
            .Select(u => u.ItemId)
            .ToListAsync();

    public async Task<MilestoneBuffs> GetBuffsAsync(Guid playerId)
    {
        var ids = await GetUnlockedItemIdsAsync(playerId);
        return MilestoneBuffs.FromUnlocks(ids);
    }

    private static int GetTrackValue(Player player, AchievementDef def, IReadOnlyList<FishCatchRecord> fishRecords, bool deepSea) =>
        def.Track switch
        {
            AchievementTrackType.FirstLegendary => player.LegendaryCatchCount,
            AchievementTrackType.TotalFishGold => player.TotalFishGoldEarned,
            AchievementTrackType.DexSpecies => fishRecords.Count,
            AchievementTrackType.FeedCount => player.LifetimeFeedCount,
            AchievementTrackType.RareCatchCount => player.RareCatchCount,
            AchievementTrackType.MarketSales => player.MarketSalesCount,
            AchievementTrackType.CookCount => player.CookCount,
            AchievementTrackType.DeepSeaLicense => deepSea ? 1 : 0,
            AchievementTrackType.NamedFishCatch when def.TrackKey is not null =>
                fishRecords.FirstOrDefault(r => r.FishName == def.TrackKey)?.CatchCount ?? 0,
            AchievementTrackType.TargetFishSpecies =>
                fishRecords.Count(r => r.IsTargetExclusive),
            _ => 0
        };
}
