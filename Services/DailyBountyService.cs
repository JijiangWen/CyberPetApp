using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>每日高价求购任务板（轻量版）：每天一条目标鱼，钓到即领赏。</summary>
public class DailyBountyService
{
    private readonly AppDbContext _context;
    private readonly PlayerService _playerService;
    private readonly Random _random = new();

    public DailyBountyService(AppDbContext context, PlayerService playerService)
    {
        _context = context;
        _playerService = playerService;
    }

    /// <summary>确保今日任务已生成；返回是否为新任务。</summary>
    public async Task<bool> EnsureTodayBountyAsync(Player player, int fishingLevel)
    {
        var today = DateTime.UtcNow.Date;
        if (player.DailyBountyDate?.Date == today && !string.IsNullOrEmpty(player.DailyBountyFishName))
            return false;

        var target = PickTargetFish(fishingLevel);
        int reward = ComputeReward(target.Rarity);

        var db = await _context.Players.FindAsync(player.Id);
        if (db is null) return false;

        db.DailyBountyDate = today;
        db.DailyBountyFishName = target.Name;
        db.DailyBountyReward = reward;
        db.DailyBountyClaimed = false;
        db.DailyBountyManualRefreshCount = 0;
        await _context.SaveChangesAsync();

        player.DailyBountyDate = today;
        player.DailyBountyFishName = target.Name;
        player.DailyBountyReward = reward;
        player.DailyBountyClaimed = false;
        player.DailyBountyManualRefreshCount = 0;
        return true;
    }

    /// <summary>手动刷新今日求购（当日首次免费，之后 50g/次）。</summary>
    public async Task<(bool Ok, string Message)> RefreshBountyAsync(Player player, int fishingLevel)
    {
        var today = DateTime.UtcNow.Date;
        if (player.DailyBountyDate?.Date != today)
            await EnsureTodayBountyAsync(player, fishingLevel);

        int refreshCount = player.DailyBountyManualRefreshCount;
        int fee = refreshCount == 0 ? 0 : EconomySinks.BountyRefreshFee;

        var db = await _context.Players.FindAsync(player.Id);
        if (db is null) return (false, "玩家不存在");
        if (db.Money < fee)
            return (false, fee == 0 ? "无法刷新" : $"金币不足，刷新求购需 {fee}g");

        if (fee > 0)
        {
            db.Money -= fee;
            player.Money = db.Money;
        }

        var target = PickTargetFish(fishingLevel);
        int reward = ComputeReward(target.Rarity);

        db.DailyBountyFishName = target.Name;
        db.DailyBountyReward = reward;
        db.DailyBountyClaimed = false;
        db.DailyBountyManualRefreshCount = refreshCount + 1;
        await _context.SaveChangesAsync();

        player.DailyBountyFishName = target.Name;
        player.DailyBountyReward = reward;
        player.DailyBountyClaimed = false;
        player.DailyBountyManualRefreshCount = refreshCount + 1;

        string feeNote = fee == 0 ? "（今日首次免费）" : $"（扣 {fee}g）";
        return (true, $"已刷新求购目标 → {target.Name}，赏金 {reward}g{feeNote}");
    }

    /// <summary>钓到目标鱼时尝试领取奖励；返回 (成功, 消息)。</summary>
    public async Task<(bool Ok, string Message)> TryClaimAsync(Player player, Fish fish)
    {
        if (player.DailyBountyClaimed || string.IsNullOrEmpty(player.DailyBountyFishName))
            return (false, "");

        if (player.DailyBountyDate?.Date != DateTime.UtcNow.Date)
            return (false, "");

        string caught = FishRecordService.NormalizeFishName(fish.Name);
        if (!caught.Contains(player.DailyBountyFishName, StringComparison.Ordinal)
            && player.DailyBountyFishName != caught)
            return (false, "");

        var db = await _context.Players.FindAsync(player.Id);
        if (db is null) return (false, "");

        db.Money += player.DailyBountyReward;
        db.DailyBountyClaimed = true;
        await _context.SaveChangesAsync();

        player.Money = db.Money;
        player.DailyBountyClaimed = true;

        string matNote = "";
        string bonusMat = player.FishingLevel >= 40
            ? (_random.NextDouble() < 0.5 ? AlchemyMaterials.OpenSeaStarCore : AlchemyMaterials.AuroraIceCrystal)
            : player.FishingLevel >= 32
                ? (_random.NextDouble() < 0.55 ? AlchemyMaterials.CanalGlowPowder : AlchemyMaterials.AbyssGel)
                : _random.NextDouble() < 0.6 ? AlchemyMaterials.ScalePowder : AlchemyMaterials.GearSet;
        int bonusQty = bonusMat is AlchemyMaterials.GearSet or AlchemyMaterials.OpenSeaStarCore ? 1 : _random.Next(2, 5);
        await _playerService.GrantBackpackItemAsync(player, bonusMat, bonusQty);
        await _context.SaveChangesAsync();
        matNote = $" · {bonusMat}×{bonusQty}";

        return (true, $"每日求购完成！{player.DailyBountyFishName} → +{player.DailyBountyReward}g{matNote}");
    }

    public bool HasActiveBounty(Player player) =>
        !player.DailyBountyClaimed
        && player.DailyBountyDate?.Date == DateTime.UtcNow.Date
        && !string.IsNullOrEmpty(player.DailyBountyFishName);

    private FishTemplate PickTargetFish(int fishingLevel)
    {
        var pool = BuildFishPool(fishingLevel);
        var weighted = pool.SelectMany(f =>
        {
            int w = f.Rarity switch
            {
                FishRarity.Legendary => 2,
                FishRarity.Epic => 4,
                FishRarity.Rare => 6,
                _ => 3
            };
            return Enumerable.Repeat(f, w);
        }).ToList();
        return weighted[_random.Next(weighted.Count)];
    }

    private static List<FishTemplate> BuildFishPool(int fishingLevel)
    {
        var pool = new List<FishTemplate>
        {
            new("野生鲫鱼", 15, 5, 5, FishRarity.Common, 0.5, 3.0),
            new("大口鲈鱼", 20, 10, 8, FishRarity.Common, 1.0, 5.0),
            new("彩虹鳟鱼", 25, 12, 12, FishRarity.Rare, 1.5, 6.0),
            new("溪涧银龙", 35, 15, 20, FishRarity.Epic, 2.0, 8.0),
            new("黄金锦鲤", 50, 20, 30, FishRarity.Legendary, 2.0, 10.0),
        };
        if (fishingLevel >= 3)
        {
            pool.Add(new FishTemplate("雾海鲈鱼", 20, 10, 8, FishRarity.Common, 1.0, 5.0));
            pool.Add(new FishTemplate("夜光乌贼", 25, 15, 10, FishRarity.Rare, 1.0, 7.0));
            pool.Add(new FishTemplate("幽蓝安康", 40, 18, 15, FishRarity.Epic, 3.0, 12.0));
            pool.Add(new FishTemplate("雾海锦鳞", 50, 20, 30, FishRarity.Legendary, 2.0, 10.0));
        }
        return pool;
    }

    private static int ComputeReward(FishRarity rarity) => rarity switch
    {
        FishRarity.Legendary => 500,
        FishRarity.Epic => 280,
        FishRarity.Rare => 120,
        _ => 45
    };
}
