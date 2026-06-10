namespace CyberPetApp.Models;

/// <summary>
/// 猫咪战斗属性对钓鱼的数值快照（每轮循环由 CatFishingStats.Compute 生成）。
/// 与 CatFishingBuff（心情/饥饿状态）叠加顺序：先加本结构体加成，再乘/减状态 buff。
/// </summary>
public record CatFishingStats(
    int CatLevel,
    int Str,
    int Agi,
    int Sen,
    int Sta,
    int Chm,
    int Luk,
    /// <summary>抓口率加算（已含等级与属性，不含状态惩罚）。</summary>
    double HookBonus,
    /// <summary>起鱼率加算。</summary>
    double LandBonus,
    /// <summary>等口时间乘算因子（&lt;1 缩短）。</summary>
    double WaitMultiplier,
    /// <summary>咬钩窗口乘算因子（&gt;1 延长）。</summary>
    double BiteWindowMultiplier,
    /// <summary>超重惩罚乘算减免（&lt;1 减轻惩罚）。</summary>
    double WeightPenaltyReduction,
    /// <summary>稀有度权重加算（小量）。</summary>
    double RarityBonus,
    /// <summary>活动精力消耗乘算（&lt;1 减免）。</summary>
    double EnergyCostMultiplier,
    IReadOnlyList<string> StatusLines);

/// <summary>
/// 猫咪等级属性 → 钓鱼公式系数。
/// 设计目标：1 级猫 vs 10 级猫抓口率差约 5~15%，装备仍为成长主轴。
/// </summary>
public static class CatFishingStatsHelper
{
    public const int StatMin = 1;
    public const int StatMax = 100;
    public const int BaseStat = 10;

    /// <summary>升级所需经验：Lv1~39 为 100×level^1.4；Lv40+ 指数放缓。</summary>
    public static int XpToNext(int level)
    {
        double exp = level >= 40 ? 1.58 : 1.4;
        double xp = 100 * Math.Pow(level, exp);
        if (level >= 40)
            xp *= Math.Pow(1.065, level - 40);
        return Math.Max(100, (int)xp);
    }

    public static CatFishingStats Compute(CyberCat cat)
    {
        int lv = Math.Max(1, cat.CatLevel);
        int str = ClampStat(cat.Str);
        int agi = ClampStat(cat.Agi);
        int sen = ClampStat(cat.Sen);
        int sta = ClampStat(cat.Sta);
        int chm = ClampStat(cat.Chm);
        int luk = ClampStat(cat.Luk);

        // 抓口加算：SEN×0.08% + STR×0.03% + 等级×0.3% + LUK×0.01%（soft cap 在 FishingManager）
        double hookBonus = sen * 0.0008 + str * 0.0003 + lv * 0.003 + luk * 0.0001;

        // 起鱼加算：STR×0.06% + STA×0.04% + 等级×0.3%
        double landBonus = str * 0.0006 + sta * 0.0004 + lv * 0.003;

        // 等口：×(1 - AGI×0.001)，AGI100 → -10%
        double waitMult = Math.Max(0.75, 1.0 - agi * 0.001);

        // 咬钩窗口：×(1 + AGI×0.003)，AGI100 → +30%
        double biteWindowMult = 1.0 + agi * 0.003;

        // 超重惩罚减免：×(1 - STR×0.002)，STR100 → 惩罚×0.8
        double weightReduction = Math.Max(0.7, 1.0 - str * 0.002);

        // 稀有度：LUK×0.02%，上限 +2%
        double rarityBonus = Math.Min(0.02, luk * 0.0002);

        // 精力消耗：×(1 - STA×0.003)，STA100 → -30%
        double energyMult = Math.Max(0.6, 1.0 - sta * 0.003);

        var lines = new List<string>
        {
            $"猫 Lv.{lv} · STR{str} AGI{agi} SEN{sen} STA{sta}",
            $"抓口+{hookBonus:P1} · 起鱼+{landBonus:P1} · 等口×{waitMult:0.##}",
            $"咬钩窗×{biteWindowMult:0.##} · 精力消耗×{energyMult:0.##}"
        };

        return new CatFishingStats(lv, str, agi, sen, sta, chm, luk,
            hookBonus, landBonus, waitMult, biteWindowMult, weightReduction,
            rarityBonus, energyMult, lines);
    }

    public static int ClampStat(int value) => Math.Clamp(value, StatMin, StatMax);

    /// <summary>属性档位 1~10（10→Lv.1，100→Lv.10）。</summary>
    public static int StatTier(int value) => Math.Clamp((ClampStat(value) + 9) / 10, 1, 10);

    public readonly record struct StatDisplay(string Abbr, string Name, int Value, int Tier, string ShortHint, string Tooltip);

    public static IReadOnlyList<StatDisplay> GetStatDisplays(CyberCat cat) =>
    [
        new("STR", "力量", ClampStat(cat.Str), StatTier(cat.Str), "起鱼·承重", "起鱼率 + 超重惩罚减免"),
        new("AGI", "敏捷", ClampStat(cat.Agi), StatTier(cat.Agi), "等口·咬钩", "缩短等口时间 · 延长咬钩窗口"),
        new("SEN", "感知", ClampStat(cat.Sen), StatTier(cat.Sen), "抓口", "提高咬钩 / 抓口成功率"),
        new("STA", "耐力", ClampStat(cat.Sta), StatTier(cat.Sta), "起鱼·省⚡", "起鱼率 + 降低活动精力消耗"),
        new("CHM", "魅力", ClampStat(cat.Chm), StatTier(cat.Chm), "社交", "市场 / NPC 互动加成（成长属性）"),
        new("LUK", "幸运", ClampStat(cat.Luk), StatTier(cat.Luk), "稀有·抓口", "稀有鱼权重 + 微量抓口加成"),
    ];

    /// <summary>钓获鱼给猫的经验（独立于玩家 FishingXp）。</summary>
    public static int XpFromFish(Fish fish)
    {
        int xp = fish.Rarity switch
        {
            FishRarity.Legendary => 48,
            FishRarity.Epic => 22,
            FishRarity.Rare => 10,
            _ => 4
        };
        if (fish.Name.StartsWith("神话·", StringComparison.Ordinal)) xp = (int)(xp * 1.25);
        if (fish.SizePercentage > 100) xp = (int)(xp * 1.4);
        return xp;
    }

    /// <summary>喂食给猫的经验。</summary>
    public static int XpFromFood(string foodName)
    {
        if (foodName == "金枪鱼罐头") return 12;
        if (foodName == "普通猫粮") return 4;
        var recipe = CookBook.Recipes.FirstOrDefault(r => r.FoodName == foodName);
        return recipe is not null ? Math.Max(6, recipe.Xp / 2) : 2;
    }
}
