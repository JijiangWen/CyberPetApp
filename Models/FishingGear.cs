namespace CyberPetApp.Models;

/// <summary>鱼竿：敏锐度加抓口；抛投距离缩短等口；最大钓重决定承重。</summary>
public class FishingRod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    /// <summary>敏锐度：抓口 += Sensitivity × 0.1</summary>
    public double Sensitivity { get; set; }
    /// <summary>抛投距离 1~5：等口 ×(1 - CastRange×0.02)，抓口 +CastRange×0.03</summary>
    public int CastRange { get; set; } = 1;
    /// <summary>最大钓重 kg：超重时起鱼成功率大幅下降</summary>
    public double MaxStrength { get; set; }
    /// <summary>装备/购买所需钓鱼等级。</summary>
    public int RequiredLevel { get; set; } = 1;
    /// <summary>耐久 0~100；&lt;30 时敏锐打折。</summary>
    public int Durability { get; set; } = 100;
    public bool IsEquipped { get; set; }
    /// <summary>炼金产出竿（非商店目录）。</summary>
    public bool IsCrafted { get; set; }
}

/// <summary>卷线器：卸力/顺滑度加遛鱼；线杯容量与钓重配合防超重；速比缩短遛鱼时长。</summary>
public class FishingReel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    /// <summary>卸力值：起鱼 += DragPower × 0.05</summary>
    public double DragPower { get; set; }
    /// <summary>速比：遛鱼时长 × (5.0 / GearRatio)</summary>
    public double GearRatio { get; set; } = 5.0;
    /// <summary>线杯容量 kg：与竿钓重、鱼线强度取最小值作为承重上限。</summary>
    public double LineCapacity { get; set; } = 8;
    /// <summary>顺滑度 0~1：起鱼 += Smoothness × 0.08，降低切线概率</summary>
    public double Smoothness { get; set; } = 0.3;
    public int RequiredLevel { get; set; } = 1;
    /// <summary>耐久 0~100；&lt;30 时卸力打折。</summary>
    public int Durability { get; set; } = 100;
    public bool IsEquipped { get; set; }
    public bool IsCrafted { get; set; }
}

/// <summary>鱼线：抗拉强度防超重；敏锐度加抓口；隐蔽度减免精明鱼惩罚；耐磨减免切线损耗。</summary>
public class FishingLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    /// <summary>抗拉强度 kg：与竿钓重、轮线杯取最小值作为承重上限。</summary>
    public double LineStrength { get; set; } = 5;
    /// <summary>敏锐度：抓口 += LineSensitivity × 0.05</summary>
    public double LineSensitivity { get; set; } = 0.3;
    /// <summary>隐蔽度 0~0.15：精明惩罚 × (1 - LineStealth)</summary>
    public double LineStealth { get; set; }
    /// <summary>耐磨 0~0.5：切线时耐久损失 × (1 - AbrasionResistance)</summary>
    public double AbrasionResistance { get; set; }
    /// <summary>目标水层：与拟饵叠加水层匹配加成。</summary>
    public WaterDepth TargetDepth { get; set; } = WaterDepth.Middle;
    public int RequiredLevel { get; set; } = 1;
    /// <summary>耐久 0~100；&lt;30 时顺滑度等价下降。</summary>
    public int Durability { get; set; } = 100;
    public bool IsEquipped { get; set; }
    /// <summary>炼金产出线（非商店目录）。</summary>
    public bool IsCrafted { get; set; }
}

/// <summary>拟饵（消耗品）：吸引度缩短等口；耐久耗尽消耗 1 个；切线时耐久 -1。</summary>
public class FishingLure
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = "";
    public double Attraction { get; set; }
    public double RarityBonus { get; set; }
    /// <summary>目标水层：与鱼种/钓点匹配时抓口 +5%</summary>
    public WaterDepth TargetDepth { get; set; } = WaterDepth.Middle;
    /// <summary>单枚拟饵剩余使用次数（切线 -1，归零扣库存）。</summary>
    public int DurabilityRemaining { get; set; } = 20;
    public int RequiredLevel { get; set; } = 1;
    public int Quantity { get; set; }
    public bool IsEquipped { get; set; }
    public bool IsCrafted { get; set; }
}

// ───────────────────────── 商店目录（静态数据，不入库） ─────────────────────────

public record RodSpec(
    string Name, double Sensitivity, double MaxStrength, int CastRange,
    int RequiredLevel, int Price, string Desc,
    GearTier Tier = GearTier.T1,
    bool ShopAvailable = true,
    bool CraftOnly = false,
    int RequiredCatLevel = 1,
    string? RequiredDexSpot = null,
    double RequiredDexPercent = 0,
    string? RequiredLicenseSpot = null,
    double RequiredOverallDexPercent = 0,
    int RequiredMythicCaught = 0,
    bool RequiredAllMythic = false,
    GearAffix Affix = GearAffix.Balanced);

public record ReelSpec(
    string Name, double DragPower, double GearRatio, double LineCapacity, double Smoothness,
    int RequiredLevel, int Price, string Desc,
    GearTier Tier = GearTier.T1,
    bool ShopAvailable = true,
    bool CraftOnly = false,
    int RequiredCatLevel = 1,
    string? RequiredDexSpot = null,
    double RequiredDexPercent = 0,
    string? RequiredLicenseSpot = null,
    double RequiredOverallDexPercent = 0,
    int RequiredMythicCaught = 0,
    bool RequiredAllMythic = false,
    GearAffix Affix = GearAffix.Balanced);

public record LureSpec(
    string Name, double Attraction, double RarityBonus, WaterDepth TargetDepth,
    int MaxDurability, int RequiredLevel, int PackSize, int PackPrice, string Desc,
    GearTier Tier = GearTier.T1,
    bool ShopAvailable = true,
    bool CraftOnly = false,
    int RequiredCatLevel = 1,
    string? RequiredDexSpot = null,
    double RequiredDexPercent = 0,
    string? RequiredLicenseSpot = null,
    double RequiredOverallDexPercent = 0,
    double MythicBonus = 0,
    int RequiredMythicCaught = 0,
    bool RequiredAllMythic = false,
    GearAffix Affix = GearAffix.Balanced);

public record LineSpec(
    string Name, double LineStrength, double LineSensitivity, double LineStealth,
    double AbrasionResistance, WaterDepth TargetDepth,
    int RequiredLevel, int Price, string Desc,
    GearTier Tier = GearTier.T1,
    bool ShopAvailable = true,
    bool CraftOnly = false,
    int RequiredCatLevel = 1,
    string? RequiredDexSpot = null,
    double RequiredDexPercent = 0,
    string? RequiredLicenseSpot = null,
    double RequiredOverallDexPercent = 0,
    int RequiredMythicCaught = 0,
    bool RequiredAllMythic = false,
    GearAffix Affix = GearAffix.Balanced);

public static class GearCatalog
{
    public static List<RodSpec> Rods => GearProgressionCatalog.Rods;
    public static List<ReelSpec> Reels => GearProgressionCatalog.Reels;
    public static List<LureSpec> Lures => GearProgressionCatalog.Lures;
    public static List<LineSpec> Lines => GearProgressionCatalog.Lines;

    public static RodSpec DefaultRod => Rods[0];
    public static ReelSpec DefaultReel => Reels[0];
    public static LineSpec DefaultLine => Lines[0];
    public static LureSpec DefaultLure => Lures[0];
    public const int DefaultLureQuantity = 10;

    public static RodSpec? FindRod(string name) => Rods.FirstOrDefault(r => r.Name == name);
    public static ReelSpec? FindReel(string name) => Reels.FirstOrDefault(r => r.Name == name);
    public static LureSpec? FindLure(string name) => Lures.FirstOrDefault(l => l.Name == name);
    public static LineSpec? FindLine(string name) => Lines.FirstOrDefault(l => l.Name == name);

    public static string DepthLabel(WaterDepth d) => d switch
    {
        WaterDepth.Shallow => "浅",
        WaterDepth.Middle => "中",
        WaterDepth.Deep => "深",
        _ => "?"
    };
}

/// <summary>
/// 运行时装备快照：页面加载装备后构建，传入 FishingManager 状态机。
/// </summary>
public class FishingLoadout
{
    public string RodName { get; set; } = "";
    public double Sensitivity { get; set; }
    public int CastRange { get; set; } = 1;
    public double MaxStrength { get; set; }
    public int RodRequiredLevel { get; set; } = 1;

    public string ReelName { get; set; } = "";
    public double DragPower { get; set; }
    public double GearRatio { get; set; } = 5.0;
    /// <summary>线杯容量 kg（轮体储线量，与鱼线强度共同参与承重下限）。</summary>
    public double LineCapacity { get; set; } = 8;
    public double Smoothness { get; set; } = 0.3;
    public int ReelRequiredLevel { get; set; } = 1;

    public string LineName { get; set; } = "";
    public double LineStrength { get; set; } = 5;
    public double LineSensitivity { get; set; } = 0.3;
    public double LineStealth { get; set; }
    public double AbrasionResistance { get; set; }
    public WaterDepth LineDepth { get; set; } = WaterDepth.Shallow;
    public int LineRequiredLevel { get; set; } = 1;
    public int LineDurability { get; set; } = 100;

    public string LureName { get; set; } = "";
    public double Attraction { get; set; }
    public double RarityBonus { get; set; }
    public WaterDepth LureDepth { get; set; } = WaterDepth.Middle;
    public int LureDurabilityRemaining { get; set; }
    public int LureRequiredLevel { get; set; } = 1;
    public int LureQuantity { get; set; }

    public int FishingLevel { get; set; } = 1;

    public int RodDurability { get; set; } = 100;
    public int ReelDurability { get; set; } = 100;
    public int RodGearTier { get; set; } = 1;
    public int LureGearTier { get; set; } = 1;
    /// <summary>T8+ 拟饵神话指定率加成（来自装备目录 MythicBonus）。</summary>
    public double LureMythicBonus { get; set; }
    /// <summary>当前钓点装备阶位有效率（竿阶低于钓点门槛时 &lt;1）。</summary>
    public double SpotGearEffectiveness { get; set; } = 1.0;

    public double MilestoneRarityBonus { get; set; }

    public GemBonusSet GemBonuses { get; set; } = GemBonusSet.Empty;

    /// <summary>装备中的特殊路亚饵配方 Id；null 表示未装备。</summary>
    public string? ActiveTargetLureRecipeId { get; set; }
    public int ActiveTargetLureUses { get; set; }
    public Guid? ActiveTargetLureId { get; set; }

    public bool HasActiveTargetLure =>
        ActiveTargetLureRecipeId is not null && ActiveTargetLureUses > 0;

    public bool TargetLureMatchesSpot(string spotName) =>
        HasActiveTargetLure && AlchemyRecipes.TargetLureSpot(ActiveTargetLureRecipeId!) == spotName;

    public double EffectiveMaxStrength => MaxStrength * (1 + GemBonuses.WeightBonus);
    public double EffectiveLineStrength => LineStrength * (1 + GemBonuses.WeightBonus);

    /// <summary>承重下限：竿钓重、轮线杯、鱼线强度三者取最小。</summary>
    public double EffectiveWeightLimit => Math.Min(
        Math.Min(EffectiveMaxStrength, LineCapacity),
        EffectiveLineStrength);

    public bool HasLure => LureQuantity > 0 && LureDurabilityRemaining > 0;
    public double EffectiveAttraction => HasLure ? Attraction : 0;
    public double EffectiveRarityBonus => HasLure ? RarityBonus + MilestoneRarityBonus : MilestoneRarityBonus;

    public int MinGearRequiredLevel => Math.Max(RodRequiredLevel,
        Math.Max(ReelRequiredLevel, Math.Max(LineRequiredLevel, LureRequiredLevel)));
    public bool MeetsGearLevel => FishingLevel >= MinGearRequiredLevel;

    public double RodDurabilityMult => RodDurability < EconomySinks.DurabilityLowThreshold
        ? EconomySinks.DurabilityLowMultiplier : 1.0;
    public double ReelDurabilityMult => ReelDurability < EconomySinks.DurabilityLowThreshold
        ? EconomySinks.DurabilityLowMultiplier : 1.0;
    public double LineDurabilityMult => LineDurability < EconomySinks.DurabilityLowThreshold
        ? EconomySinks.DurabilityLowMultiplier : 1.0;

    public double EffectiveSensitivity => Sensitivity * RodDurabilityMult * SpotGearEffectiveness;
    public double EffectiveLineSensitivity => LineSensitivity * LineDurabilityMult;
    public double EffectiveDragPower => DragPower * ReelDurabilityMult * SpotGearEffectiveness;
    /// <summary>轮顺滑 × 线低耐久惩罚（线 &lt;30 等价降低顺滑）。</summary>
    public double EffectiveSmoothness => Smoothness * ReelDurabilityMult * LineDurabilityMult;

    /// <summary>水层匹配：拟饵与鱼线各 +5%，上限 +10%。</summary>
    public double DepthMatchBonus(FishTemplate template, WaterDepth spotDepth)
    {
        double bonus = 0;
        if (HasLure && (LureDepth == template.PreferredDepth || LureDepth == spotDepth))
            bonus += 0.05;
        if (LineDepth == template.PreferredDepth || LineDepth == spotDepth)
            bonus += 0.05;
        return Math.Min(0.10, bonus);
    }
}

