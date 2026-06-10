namespace CyberPetApp.Models;

public class PlayerAchievement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string AchievementId { get; set; } = "";
    public int Progress { get; set; }
    public bool RewardClaimed { get; set; }
}

public class PlayerMilestoneUnlock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string ItemId { get; set; } = "";
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
}

public record AchievementDef(
    string Id,
    string Title,
    string Desc,
    int Target,
    int PointReward,
    AchievementTrackType Track,
    string? TrackKey = null);

public enum AchievementTrackType
{
    FirstLegendary,
    TotalFishGold,
    DexSpecies,
    FeedCount,
    RareCatchCount,
    MarketSales,
    CookCount,
    DeepSeaLicense,
    /// <summary>钓获指定鱼种（TrackKey = 鱼名）。</summary>
    NamedFishCatch,
    /// <summary>图鉴收录不同神话鱼种数。</summary>
    TargetFishSpecies
}

public record MilestoneShopItem(
    string Id,
    string Title,
    string Desc,
    int PointCost,
    int GoldCost,
    string BuffSummary);

public static class MilestoneCatalog
{
    public static readonly List<AchievementDef> Achievements =
    [
        new("first_legendary", "传说初现", "首次钓获传说鱼", 1, 15, AchievementTrackType.FirstLegendary),
        new("sell_5000g", "鱼贩大亨", "累计卖鱼/市场成交 5000g", 5000, 20, AchievementTrackType.TotalFishGold),
        new("dex_5", "图鉴收藏家", "图鉴收录 5 种鱼", 5, 10, AchievementTrackType.DexSpecies),
        new("feed_100", "投喂达人", "手动喂食猫 100 次", 100, 10, AchievementTrackType.FeedCount),
        new("rare_50", "稀有猎手", "累计钓获 Rare+ 50 条", 50, 15, AchievementTrackType.RareCatchCount),
        new("market_10", "摊位老手", "市场成交 10 次", 10, 10, AchievementTrackType.MarketSales),
        new("cook_20", "厨房新星", "烹饪 20 次", 20, 10, AchievementTrackType.CookCount),
        new("deep_sea", "雾海通行证", "获得雾海深渊永久许可证", 1, 15, AchievementTrackType.DeepSeaLicense),

        new("target_fish_mirror_koi", "镜湖神话", "钓获神话·镜湖神鲤", 1, 30, AchievementTrackType.NamedFishCatch, "神话·镜湖神鲤"),
        new("target_fish_creek_eel", "翠影鳗王", "钓获神话·翠影鳗王", 1, 25, AchievementTrackType.NamedFishCatch, "神话·翠影鳗王"),
        new("target_fish_abyss_squid", "古神之鱿", "钓获神话·雾海古神鱿", 1, 30, AchievementTrackType.NamedFishCatch, "神话·雾海古神鱿"),
        new("target_fish_canal_dragon", "引渠幻龙", "钓获神话·引渠幻龙", 1, 30, AchievementTrackType.NamedFishCatch, "神话·引渠幻龙"),
        new("target_fish_aurora_dragon", "极光霜龙", "钓获神话·极光霜龙", 1, 35, AchievementTrackType.NamedFishCatch, "神话·极光霜龙"),
        new("target_fish_reef_leviathan", "远海沧龙", "钓获神话·远海沧龙", 1, 40, AchievementTrackType.NamedFishCatch, "神话·远海沧龙"),
        new("target_fish_sea_emperor", "金鳞海皇", "钓获神话·金鳞海皇", 1, 40, AchievementTrackType.NamedFishCatch, "神话·金鳞海皇"),
        new("target_fish_collector", "神话收藏家", "图鉴收录 3 种神话鱼", 3, 50, AchievementTrackType.TargetFishSpecies),
    ];

    public static readonly List<MilestoneShopItem> ShopItems =
    [
        new("lure_skin_abyss", "深渊拟饵皮肤", "永久 +15% 稀有权重", 25, 500, "稀有权重 +15%"),
        new("title_bargain", "称号·还价大师", "鱼市还价成功率 +8%", 30, 1000, "还价 +8%"),
        new("deco_trophy", "装饰·金杯展架", "家具被动加成 +3%", 20, 800, "家具 buff +3%"),
    ];

    public static AchievementDef? FindAchievement(string id) =>
        Achievements.FirstOrDefault(a => a.Id == id);

    public static MilestoneShopItem? FindShopItem(string id) =>
        ShopItems.FirstOrDefault(s => s.Id == id);
}

/// <summary>里程碑商店已解锁 buff 汇总。</summary>
public record MilestoneBuffs(
    double RarityBonus,
    double CounterBonus,
    double HouseBuffMultiplier)
{
    public static MilestoneBuffs Empty => new(0, 0, 1.0);

    public static MilestoneBuffs FromUnlocks(IEnumerable<string> itemIds)
    {
        var set = itemIds.ToHashSet();
        return new MilestoneBuffs(
            set.Contains("lure_skin_abyss") ? 0.15 : 0,
            set.Contains("title_bargain") ? 0.08 : 0,
            set.Contains("deco_trophy") ? 1.03 : 1.0);
    }
}
