namespace CyberPetApp.Models;

/// <summary>家具被动加成类型（数值含义见 FurnitureCatalog）。</summary>
public enum FurnitureBonusType
{
    EnergyDecayReduction,
    HappinessDecayReduction,
    ThirstDecayReduction,
    SleepEnergyBonus,
    CookingXpBonus,
    FeederExtraSlots,
    FishingHappinessThreshold,
    WorkGoldBonus,
    StallTicketProgressBonus,
    NpcOfferFrequencyBonus,
    PassiveCatCareLabel,
    AutoWatererUnlock,
    AutoFeederUnlock,
}

/// <summary>加成软上限分组：同组只取最优一项（递减叠加规则）。</summary>
public enum FurnitureBonusCategory
{
    CatEnergyDecay,
    CatHappinessDecay,
    CatThirstDecay,
    CatSleep,
    KitchenCooking,
    KitchenStorage,
    FishingLink,
    WorkIncome,
    WorkTicket,
    MarketNpc,
}

public record FurnitureBonusDef(
    FurnitureBonusType Type,
    FurnitureBonusCategory Category,
    double Value,
    string ActiveLabel,
    string LockedHint);

/// <summary>各家具 ID 的静态加成配置（不存 DB，按 FurnitureId 查表）。</summary>
public static class FurnitureCatalog
{
    private static readonly Dictionary<string, FurnitureBonusDef> Map = new(StringComparer.Ordinal)
    {
        ["Sofa"] = new(FurnitureBonusType.EnergyDecayReduction, FurnitureBonusCategory.CatEnergyDecay,
            0.20, "精力活动消耗 -20%", "解锁后：精力活动消耗 -20%"),
        ["TV"] = new(FurnitureBonusType.WorkGoldBonus, FurnitureBonusCategory.WorkIncome,
            0.10, "打工金币 +10%", "解锁后：打工金币 +10%"),
        ["Fridge"] = new(FurnitureBonusType.FeederExtraSlots, FurnitureBonusCategory.KitchenStorage,
            2, "喂食器 +2 槽", "解锁后：喂食器 +2 槽"),
        ["Stove"] = new(FurnitureBonusType.CookingXpBonus, FurnitureBonusCategory.KitchenCooking,
            0.25, "烹饪 XP +25%", "解锁后：烹饪 XP +25%"),
        ["Bed"] = new(FurnitureBonusType.SleepEnergyBonus, FurnitureBonusCategory.CatSleep,
            0.50, "睡觉精力 +50%", "解锁后：睡觉精力 +50%"),
        ["Toilet"] = new(FurnitureBonusType.HappinessDecayReduction, FurnitureBonusCategory.CatHappinessDecay,
            0.15, "幸福活动消耗 -15%", "解锁后：幸福活动消耗 -15%"),
        ["Sink"] = new(FurnitureBonusType.FishingHappinessThreshold, FurnitureBonusCategory.FishingLink,
            750, "钓鱼开心阈值 750", "解锁后：开心≥750 触发稀有加成"),
        ["Garden"] = new(FurnitureBonusType.NpcOfferFrequencyBonus, FurnitureBonusCategory.MarketNpc,
            0.15, "NPC 报价频率 +15%", "解锁后：NPC 报价频率 +15%"),
        ["CatToy"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatHappinessDecay,
            0, "每3s ♥+20 ⚡+5", "解锁后：电动逗猫棒被动恢复"),
        ["JoyPad"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatHappinessDecay,
            0, "每3s ♥+10（<700）", "解锁后：每3s 爬架娱乐 ♥+10"),
        ["AutoFeederUnit"] = new(FurnitureBonusType.FeederExtraSlots, FurnitureBonusCategory.KitchenStorage,
            3, "喂食器 +3 槽 · 饥饿<600 自动取食", "解锁后：智能喂食站扩容+自动喂"),
        ["WaterDispenser"] = new(FurnitureBonusType.AutoWatererUnlock, FurnitureBonusCategory.CatThirstDecay,
            0, "解锁宠物饮水泉", "解锁后：口渴<600 自动补水"),
        ["WaterFountain"] = new(FurnitureBonusType.AutoWatererUnlock, FurnitureBonusCategory.CatThirstDecay,
            0, "解锁宠物饮水泉", "解锁后：口渴<600 自动补水"),
        ["FishTank"] = new(FurnitureBonusType.FishingHappinessThreshold, FurnitureBonusCategory.FishingLink,
            770, "每3s ♥+6 · 钓鱼开心阈值 770", "解锁后：观赏鱼缸+稀有加成"),
        ["SunLamp"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatEnergyDecay,
            0, "每3s ♥+4 ⚡+6", "解锁后：每3s 日照灯 ♥+4 ⚡+6"),
        ["AromaDiffuser"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatHappinessDecay,
            0, "每3s ♥+12", "解锁后：每3s 香薰 ♥+12"),
        ["LuxuryTower"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatHappinessDecay,
            0, "每3s ♥+15 ⚡+10", "解锁后：每3s 豪华爬架 ♥+15 ⚡+10"),
        ["CozyBed"] = new(FurnitureBonusType.PassiveCatCareLabel, FurnitureBonusCategory.CatSleep,
            0, "每3s ⚡+8（<800）", "解锁后：每3s 猫窝 ⚡+8（精力<800）"),
    };

    public static bool TryGet(string furnitureId, out FurnitureBonusDef def) =>
        Map.TryGetValue(furnitureId, out def!);

    public static string BonusTag(string furnitureId) =>
        TryGet(furnitureId, out var d) ? d.ActiveLabel : "";

    public static string LockedBonusHint(string furnitureId) =>
        TryGet(furnitureId, out var d) ? d.LockedHint : "";
}

/// <summary>房屋家具汇总后的运行时加成快照。</summary>
public readonly struct HouseBuffs
{
    public static HouseBuffs None => default;

    public HouseBuffs() { }

    /// <summary>精力类活动消耗倍率（越小扣越少，下限 0.60；沙发等家具 -20%）。</summary>
    public double EnergyDecayMultiplier { get; init; } = 1.0;

    /// <summary>幸福类活动消耗倍率。</summary>
    public double HappinessDecayMultiplier { get; init; } = 1.0;

    /// <summary>口渴类活动消耗倍率。</summary>
    public double ThirstDecayMultiplier { get; init; } = 1.0;

    /// <summary>睡觉单次精力恢复倍率。</summary>
    public double SleepEnergyMultiplier { get; init; } = 1.0;

    /// <summary>烹饪经验倍率。</summary>
    public double CookingXpMultiplier { get; init; } = 1.0;

    /// <summary>自动喂食器额外槽位（基础上限 10）。</summary>
    public int FeederExtraSlots { get; init; }

    /// <summary>触发钓鱼稀有度加成的 Happiness 阈值（越低越好，下限 700）。</summary>
    public int FishingHappinessThreshold { get; init; } = 800;

    /// <summary>打工每 tick 金币倍率。</summary>
    public double WorkGoldMultiplier { get; init; } = 1.0;

    /// <summary>摊位券进度倍率（&gt;1 更快攒券）。</summary>
    public double StallTicketProgressMultiplier { get; init; } = 1.0;

    /// <summary>NPC 报价生成概率倍率。</summary>
    public double NpcOfferChanceMultiplier { get; init; } = 1.0;

    public IReadOnlyList<string> SummaryLines { get; init; } = [];

    public HouseBuffs WithMilestoneScale(double scale)
    {
        if (scale <= 1.0) return this;
        static double Boost(double mult, double s) => 1.0 + (mult - 1.0) * s;
        static double DecayBoost(double mult, double s) => 1.0 - (1.0 - mult) * s;
        var scaledLines = SummaryLines.ToList();
        if (scale > 1.0 && !scaledLines.Any(l => l.Contains("金杯展架")))
            scaledLines.Add("[里程碑] 金杯展架：家具被动 +3%");
        return new HouseBuffs
        {
            EnergyDecayMultiplier = DecayBoost(EnergyDecayMultiplier, scale),
            HappinessDecayMultiplier = DecayBoost(HappinessDecayMultiplier, scale),
            ThirstDecayMultiplier = DecayBoost(ThirstDecayMultiplier, scale),
            SleepEnergyMultiplier = Boost(SleepEnergyMultiplier, scale),
            CookingXpMultiplier = Boost(CookingXpMultiplier, scale),
            FeederExtraSlots = FeederExtraSlots,
            FishingHappinessThreshold = FishingHappinessThreshold,
            WorkGoldMultiplier = Boost(WorkGoldMultiplier, scale),
            StallTicketProgressMultiplier = Boost(StallTicketProgressMultiplier, scale),
            NpcOfferChanceMultiplier = Boost(NpcOfferChanceMultiplier, scale),
            SummaryLines = scaledLines
        };
    }
}

/// <summary>根据已解锁家具 ID 汇总 HouseBuffs（同类取最优；衰减减免字段语义为活动消耗倍率）。</summary>
public static class HouseBuffAggregator
{
    private const double MinDecayMultiplier = 0.60;
    private const int MinFishingHappyThreshold = 700;
    private const int MaxFeederExtraSlots = 7;
    private const double MaxPercentBonus = 0.35;

    public static HouseBuffs Aggregate(IEnumerable<(string FurnitureId, int UpgradeLevel)> unlockedFurniture)
    {
        double bestEnergyReduction = 0;
        double bestHappyReduction = 0;
        double bestThirstReduction = 0;
        double bestSleepBonus = 0;
        double bestCookingBonus = 0;
        int feederSlots = 0;
        int? bestFishThreshold = null;
        double bestWorkGold = 0;
        double bestStallProgress = 0;
        double bestNpcOffer = 0;
        var lines = new List<string>();

        foreach (var (id, upgradeLevel) in unlockedFurniture.DistinctBy(f => f.FurnitureId))
        {
            if (!FurnitureCatalog.TryGet(id, out var def)) continue;
            double value = def.Value * (1 + EconomySinks.FurnitureUpgradeBonusRate * upgradeLevel);
            string upgradeTag = upgradeLevel > 0 ? $" Lv.{upgradeLevel}" : "";

            switch (def.Type)
            {
                case FurnitureBonusType.EnergyDecayReduction:
                    if (value > bestEnergyReduction)
                    {
                        bestEnergyReduction = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.HappinessDecayReduction:
                    if (value > bestHappyReduction)
                    {
                        bestHappyReduction = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.ThirstDecayReduction:
                    if (value > bestThirstReduction)
                    {
                        bestThirstReduction = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.SleepEnergyBonus:
                    if (value > bestSleepBonus)
                    {
                        bestSleepBonus = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.CookingXpBonus:
                    if (value > bestCookingBonus)
                    {
                        bestCookingBonus = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.FeederExtraSlots:
                    feederSlots += (int)value;
                    lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    break;
                case FurnitureBonusType.FishingHappinessThreshold:
                    var threshold = (int)value;
                    if (bestFishThreshold is null || threshold < bestFishThreshold)
                    {
                        bestFishThreshold = threshold;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.WorkGoldBonus:
                    if (value > bestWorkGold)
                    {
                        bestWorkGold = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.StallTicketProgressBonus:
                    if (value > bestStallProgress)
                    {
                        bestStallProgress = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.NpcOfferFrequencyBonus:
                    if (value > bestNpcOffer)
                    {
                        bestNpcOffer = value;
                        lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    }
                    break;
                case FurnitureBonusType.PassiveCatCareLabel:
                case FurnitureBonusType.AutoWatererUnlock:
                    lines.Add($"[{id}{upgradeTag}] {def.ActiveLabel}");
                    break;
            }
        }

        bestEnergyReduction = Math.Min(bestEnergyReduction, MaxPercentBonus);
        bestHappyReduction = Math.Min(bestHappyReduction, MaxPercentBonus);
        bestThirstReduction = Math.Min(bestThirstReduction, MaxPercentBonus);
        bestCookingBonus = Math.Min(bestCookingBonus, MaxPercentBonus);
        bestWorkGold = Math.Min(bestWorkGold, MaxPercentBonus);
        bestNpcOffer = Math.Min(bestNpcOffer, MaxPercentBonus);

        feederSlots = Math.Min(feederSlots, MaxFeederExtraSlots);

        if (lines.Count == 0)
            lines.Add("暂无家具加成（购买家具解锁被动效果）");

        return new HouseBuffs
        {
            EnergyDecayMultiplier = Math.Max(MinDecayMultiplier, 1.0 - bestEnergyReduction),
            HappinessDecayMultiplier = Math.Max(MinDecayMultiplier, 1.0 - bestHappyReduction),
            ThirstDecayMultiplier = Math.Max(MinDecayMultiplier, 1.0 - bestThirstReduction),
            SleepEnergyMultiplier = 1.0 + bestSleepBonus,
            CookingXpMultiplier = 1.0 + bestCookingBonus,
            FeederExtraSlots = feederSlots,
            FishingHappinessThreshold = Math.Max(MinFishingHappyThreshold, bestFishThreshold ?? 800),
            WorkGoldMultiplier = 1.0 + bestWorkGold,
            StallTicketProgressMultiplier = 1.0 + bestStallProgress,
            NpcOfferChanceMultiplier = 1.0 + bestNpcOffer,
            SummaryLines = lines
        };
    }

    public static HouseBuffs Aggregate(PlayerHouse house) =>
        Aggregate(house.Rooms.Values
            .SelectMany(r => r.Furniture)
            .Where(f => f.IsUnlocked)
            .Select(f => (f.FurnitureId, f.UpgradeLevel)));
}
