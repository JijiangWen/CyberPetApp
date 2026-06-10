namespace CyberPetApp.Models;

/// <summary>装备阶位 T1~T10；数值与解锁条件集中于此便于调参。</summary>
/// <remarks>
/// 500h 端到端毕业曲线（挂机 2s/tick，毕业=全套 T10 + 图鉴 95% + 全神话鱼各 1）：
/// | 阶段   | 目标时长 | 里程碑              |
/// |--------|---------|---------------------|
/// | T1~T3  | ~40h    | 静溪/雾海熟练       |
/// | T4~T6  | ~160h   | 引渠+派遣素材       |
/// | T7~T8  | ~310h   | 冰湾+鱼市回收       |
/// | T9~T10 | ~500h   | 远礁+神话+终极锻造 |
/// </remarks>
public enum GearTier
{
    T1 = 1,
    T2 = 2,
    T3 = 3,
    T4 = 4,
    T5 = 5,
    T6 = 6,
    T7 = 7,
    T8 = 8,
    T9 = 9,
    T10 = 10
}

/// <summary>炼金锻造素材（BackpackItem 名称常量）。</summary>
public static partial class AlchemyMaterials
{
    public const string BambooStrip = "竹片";
    public const string CarbonFiber = "碳纤维丝";
    public const string DeepSeaCrystal = "深海结晶";
    public const string FishBone = "鱼骨";
    public const string FishScale = "鱼鳞";
    public const string WaterWeed = "水草";
    public const string GearSet = "齿轮组";
    public const string Resin = "环氧树脂";
    public const string Bearing = "精密轴承";
    public const string AlloyFrame = "钛合金框";
    public const string NylonFilament = "尼龙原丝";
    public const string ScalePowder = "鱼鳞粉";
    public const string MythScalePowder = "神话鳞粉";
    public const string CanalGlowPowder = "引渠荧光粉";
    public const string StarfallAlloy = "星陨合金";
    public const string VoidCore = "虚空原核";
    public const string AbyssEssence = "沧溟神髓";
    public const string CoralShard = "珊瑚碎片";
    public const string AuroraIceCrystal = "极光冰晶";
    public const string AbyssGel = "深渊凝胶";
    public const string WreckRust = "沉船铁锈";
    public const string OpenSeaStarCore = "外海星核";
    public const string DragonResin = "龙息树脂";
    public const string VoidFiber = "虚空纤维";
    public const string ReedFiber = "芦苇纤维";
    public const string RiftSlag = "裂谷矿渣";
    public const string StarTideShard = "星潮碎片";
    public const string VoidMote = "虚空微粒";

    public static readonly IReadOnlyList<string> All =
    [
        BambooStrip, CarbonFiber, DeepSeaCrystal, FishBone, FishScale, WaterWeed,
        GearSet, Resin, Bearing, AlloyFrame, NylonFilament, ScalePowder, MythScalePowder,
        CanalGlowPowder, CoralShard, AuroraIceCrystal, AbyssGel, WreckRust,
        OpenSeaStarCore, DragonResin, VoidFiber, ReedFiber, RiftSlag, StarTideShard, VoidMote,
        StarfallAlloy, VoidCore, AbyssEssence
    ];

    public static string SourceHint(string name) => name switch
    {
        BambooStrip => "静溪普通鱼分解",
        CarbonFiber => "雾海稀有+鱼分解/副产",
        DeepSeaCrystal => "远礁外海鱼分解",
        FishBone => "任意鱼分解",
        FishScale => "任意鱼分解",
        WaterWeed => "静溪/common 分解",
        GearSet => "派遣/打工",
        Resin => "生活商店",
        Bearing => "生活商店",
        AlloyFrame => "生活商店",
        NylonFilament => "生活商店",
        ScalePowder => "炼金/分解副产",
        MythScalePowder => "神话鱼分解",
        CanalGlowPowder => "夜光引渠稀有+分解",
        CoralShard => "珊瑚暗流/雾海分解",
        AuroraIceCrystal => "极光冰湾史诗+分解",
        AbyssGel => "远礁外海史诗+分解",
        WreckRust => "沉船墓场稀有+派遣",
        OpenSeaStarCore => "远礁传说+神话副产",
        DragonResin => "远礁史诗+炼金",
        VoidFiber => "虚空钓域神话+炼金",
        ReedFiber => "芦苇湾普通鱼分解",
        RiftSlag => "暗涌裂谷稀有+分解",
        StarTideShard => "星潮海沟史诗+分解",
        VoidMote => "虚空钓域传说+分解",
        _ => "?"
    };
}

public enum GearCraftSlot { Rod, Reel, Line, Lure }

public record GearCraftRecipe(
    string Id,
    string DisplayName,
    GearCraftSlot Slot,
    string OutputGearName,
    GearTier Tier,
    IReadOnlyList<FishRequirement> Fish,
    IReadOnlyList<MaterialRequirement> Materials,
    int GoldCost,
    int RequiredFishingLevel,
    int RequiredCatLevel,
    string? RequiredDexSpot,
    double RequiredDexPercent,
    string? RequiredLicenseSpot,
    double RequiredOverallDexPercent,
    string Description,
    int RequiredMythicCaught = 0,
    bool RequiredAllMythic = false);

public static class GearProgressionCatalog
{
    public const int MaxCatLevel = 80;
    public const int TotalMythFishSpecies = 16;
    public const double GraduationDexTarget = 0.95;

    public const double TierGapHookPenalty = 0.18;
    public const double TierGapLandPenalty = 0.12;

    /// <summary>该阶位累计目标游玩时长（小时，策划估算）。</summary>
    public static int TierCumulativeTargetHours(GearTier tier) => tier switch
    {
        <= GearTier.T3 => 40,
        <= GearTier.T6 => 160,
        <= GearTier.T8 => 310,
        _ => 500
    };

    /// <summary>UI 阶位时长提示，如「est. ~160h cumulative」。</summary>
    public static string TierHoursHint(GearTier tier) =>
        $"est. ~{TierCumulativeTargetHours(tier)}h cumulative";

    public static string TierLabel(GearTier t) => t switch
    {
        GearTier.T1 => "入门",
        GearTier.T2 => "进阶",
        GearTier.T3 => "精工",
        GearTier.T4 => "深海",
        GearTier.T5 => "神话",
        GearTier.T6 => "星陨",
        GearTier.T7 => "虚空",
        GearTier.T8 => "沧溟",
        GearTier.T9 => "神谕",
        GearTier.T10 => "终极",
        _ => "?"
    };

    public static string TierPrefix(GearTier t) => $"[{TierLabel(t)}]";

    public static readonly GearTier[] AllTiers =
        [GearTier.T1, GearTier.T2, GearTier.T3, GearTier.T4, GearTier.T5,
         GearTier.T6, GearTier.T7, GearTier.T8, GearTier.T9, GearTier.T10];

    // ── 钓点最低有效竿阶（低于此阶抓口/起鱼打折）──
    public static readonly Dictionary<string, int> SpotMinRodTier = new()
    {
        ["静溪"] = 1,
        ["浅塘"] = 1,
        ["雾海深渊"] = 2,
        ["芦苇湾"] = 3,
        ["夜光引渠"] = 4,
        ["暗涌裂谷"] = 5,
        ["极光冰湾"] = 6,
        ["珊瑚暗流"] = 6,
        ["沉船墓场"] = 7,
        ["远礁外海"] = 8,
        ["深渊回廊"] = 9,
        ["星潮海沟"] = 9,
        ["虚空钓域"] = 10,
    };

    public static int SpotMinRodTierFor(string spotName) =>
        SpotMinRodTier.GetValueOrDefault(spotName, 1);

    /// <summary>T10@静溪=1.0；T1@远礁≈0.15。</summary>
    public static double SpotGearEffectiveness(int rodTier, string spotName)
    {
        int min = SpotMinRodTierFor(spotName);
        if (rodTier >= min) return 1.0;
        return Math.Max(0.15, rodTier / (double)min * 0.75);
    }

    public static int SpotExtraWear(string? spotName) => spotName switch
    {
        "虚空钓域" => 12,
        "星潮海沟" or "深渊回廊" => 10,
        "远礁外海" => 8,
        "沉船墓场" => 7,
        "珊瑚暗流" or "极光冰湾" => 6,
        "暗涌裂谷" => 5,
        "夜光引渠" => 4,
        "雾海深渊" or "芦苇湾" => 2,
        _ => 0
    };

    /// <summary>高阶装备基础耐久上限（仍 0~100 刻度，但 T8+ 钓点消耗更高）。</summary>
    public static int MaxDurabilityForTier(GearTier tier) => (int)tier switch
    {
        >= 9 => 130,
        8 => 120,
        7 => 115,
        6 => 110,
        _ => 100
    };

    // ── 鱼竿（12）──
    public static readonly List<RodSpec> Rods =
    [
        new("新手竹竿", 0.5, 5, 1, 1, 0, "敏锐+5% · 抛投1 · 钓重5kg", GearTier.T1),
        new("竹制溪流竿", 0.7, 7, 1, 1, 120, "入门加长 · 抛投1 · 钓重7kg", GearTier.T1),
        new("碳素路亚竿", 1.2, 12, 2, 5, 500, "敏锐+12% · 抛投2 · 钓重12kg · 需钓鱼Lv5", GearTier.T2, RequiredCatLevel: 3),
        new("进阶旋压竿", 1.5, 15, 2, 5, 900, "均衡入门+ · 抛投2 · 钓重15kg", GearTier.T2, RequiredCatLevel: 3),
        new("钛合金战竿", 2.0, 30, 3, 12, 4500, "敏锐+20% · 抛投3 · 钓重30kg", GearTier.T3, RequiredDexSpot: "静溪", RequiredDexPercent: 0.5),
        new("静溪赋形竿", 1.8, 25, 3, 10, 3200, "静溪特化 · 浅层+5%等口", GearTier.T3, RequiredDexSpot: "静溪", RequiredDexPercent: 0.4),
        new("精工碳纤竿", 2.4, 35, 3, 12, 0, "炼金 · 敏锐+24% · 抛投3", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("深海碳纤竿", 2.8, 50, 4, 20, 0, "炼金主路径 · 敏锐+28% · 抛投4", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("雾海潜行竿", 2.6, 45, 4, 18, 0, "炼金 · 深层隐蔽+ · 抛投4", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("量子折叠竿", 3.5, 80, 5, 30, 0, "炼金 · 敏锐+35% · 抛投5", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("神话龙脊竿", 3.8, 90, 5, 30, 0, "炼金 · 神话素材 · 钓重90kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("虚鲸神谕竿", 4.0, 100, 5, 32, 0, "炼金 T5 · 抛投5 · 钓重100kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.85),
        ..GearT6T10Data.ExtraRods,
    ];

    // ── 渔轮（10）──
    public static readonly List<ReelSpec> Reels =
    [
        new("基础纺车轮", 2, 5.0, 8, 0.30, 1, 0, "卸力+10% · 线杯8kg", GearTier.T1),
        new("便携迷你轮", 2.5, 5.2, 10, 0.32, 1, 180, "轻便入门轮 · 线杯10kg", GearTier.T1),
        new("金属水滴轮", 5, 6.2, 15, 0.50, 5, 600, "卸力+25% · 线杯15kg", GearTier.T2, RequiredCatLevel: 3),
        new("磁力轻量轮", 6, 6.5, 18, 0.55, 5, 1100, "顺滑提升 · 线杯18kg", GearTier.T2, RequiredCatLevel: 3),
        new("电动深海轮", 8, 7.5, 25, 0.65, 12, 5200, "卸力+40% · 线杯25kg", GearTier.T3, RequiredDexSpot: "静溪", RequiredDexPercent: 0.45),
        new("精工纺车轮", 9, 7.8, 28, 0.70, 12, 0, "炼金 · 顺滑70%", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("深海纺车轮", 12, 8.5, 40, 0.78, 20, 0, "炼金主路径 · 线杯40kg", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("磁力刹车轮", 11, 8.2, 38, 0.76, 18, 0, "炼金 · 高顺滑", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("神经接口轮", 18, 10.0, 60, 0.92, 30, 0, "炼金神话轮 · 线杯60kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("沧龙液压轮", 16, 9.5, 55, 0.88, 28, 0, "炼金 · 远礁特化", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.75),
        ..GearT6T10Data.ExtraReels,
    ];

    // ── 鱼线（10）──
    public static readonly List<LineSpec> Lines =
    [
        new("尼龙主线", 5, 0.3, 0.00, 0.00, WaterDepth.Shallow, 1, 0, "强度5kg · 敏锐+3%", GearTier.T1),
        new("加强尼龙线", 8, 0.4, 0.01, 0.05, WaterDepth.Shallow, 1, 150, "入门加强 · 耐磨5%", GearTier.T1),
        new("编织尼龙线", 12, 0.6, 0.03, 0.10, WaterDepth.Shallow, 5, 400, "强度12kg · 隐蔽3%", GearTier.T2, RequiredCatLevel: 3),
        new("荧光编织线", 15, 0.7, 0.04, 0.12, WaterDepth.Middle, 5, 750, "夜钓向 · 中层", GearTier.T2, RequiredCatLevel: 3),
        new("碳纤主线", 25, 0.9, 0.06, 0.20, WaterDepth.Middle, 12, 4200, "强度25kg · 耐磨20%", GearTier.T3, RequiredDexSpot: "静溪", RequiredDexPercent: 0.5),
        new("精工编织线", 30, 1.0, 0.08, 0.25, WaterDepth.Middle, 12, 0, "炼金 · 100m编织", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("钛合金织线", 45, 1.2, 0.10, 0.35, WaterDepth.Middle, 20, 0, "炼金深海线 · 强度45kg", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("深渊碳纤线", 52, 1.3, 0.12, 0.38, WaterDepth.Deep, 22, 0, "炼金 · 深层特化", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("量子丝导线", 70, 1.5, 0.15, 0.50, WaterDepth.Deep, 30, 0, "炼金神话线 · 耐磨50%", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("龙筋编织线", 65, 1.8, 0.18, 0.45, WaterDepth.Deep, 28, 0, "炼金 · 高敏锐", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.75),
        ..GearT6T10Data.ExtraLines,
    ];

    // ── 拟饵（10）──
    public static readonly List<LureSpec> Lures =
    [
        new("普通亮片", 0.10, 0.0, WaterDepth.Shallow, 20, 1, 5, 25, "浅层 · 等口-10%", GearTier.T1),
        new("草虾拟饵", 0.12, 0.0, WaterDepth.Shallow, 18, 1, 5, 40, "静溪入门 · 浅层", GearTier.T1),
        new("夜光软虫", 0.20, 0.3, WaterDepth.Middle, 15, 5, 5, 125, "中层 · 品质+30%", GearTier.T2, RequiredCatLevel: 3),
        new("仿生小鱼", 0.35, 0.8, WaterDepth.Middle, 12, 5, 5, 400, "中层 · 品质+80%", GearTier.T2, RequiredCatLevel: 3),
        new("冰川幼虫", 0.28, 0.5, WaterDepth.Deep, 10, 12, 5, 680, "深层 · 品质+50%", GearTier.T3, RequiredDexSpot: "静溪", RequiredDexPercent: 0.5),
        new("精工磷光饵", 0.32, 0.6, WaterDepth.Deep, 10, 12, 3, 0, "炼金 · 深层 lure", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("深渊拟态饵", 0.42, 1.0, WaterDepth.Deep, 8, 20, 3, 0, "炼金 T4 · 品质+100%", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("雾海幽光饵", 0.38, 0.9, WaterDepth.Deep, 9, 18, 3, 0, "炼金 · 雾海特化", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "雾海深渊"),
        new("量子拟态饵", 0.50, 1.2, WaterDepth.Deep, 8, 30, 2, 0, "炼金神话饵 · 品质+120%", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8, MythicBonus: 0.05),
        new("神谕鳞粉饵", 0.55, 1.5, WaterDepth.Deep, 6, 32, 2, 0, "神话饵率+8% · T5", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.85, MythicBonus: 0.08),
        ..GearT6T10Data.ExtraLures,
    ];

    // ── craft.gear 炼金配方 ──
    public static readonly List<GearCraftRecipe> GearCraftRecipes =
    [
        // T3 竿
        new("craft_rod_t3_carbon", "精工碳纤竿", GearCraftSlot.Rod, "精工碳纤竿", GearTier.T3,
            [], [new(AlchemyMaterials.BambooStrip, 3), new(AlchemyMaterials.CarbonFiber, 2), new(AlchemyMaterials.Resin, 1)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "静溪", 0.5, null, 0,
            "竹片×3 + 碳纤维丝×2 + 树脂×1 → 精工碳纤竿"),

        // T4 竿/轮
        new("craft_rod_t4_deep", "深海碳纤竿", GearCraftSlot.Rod, "深海碳纤竿", GearTier.T4,
            [new FishRequirement("雾海锦鳞", 1, FishRarity.Epic, "雾海深渊")],
            [new(AlchemyMaterials.BambooStrip, 2), new(AlchemyMaterials.CarbonFiber, 4), new(AlchemyMaterials.Resin, 2)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "雾海深渊", 0,
            "雾海史诗鱼 + 碳纤维 → 深海碳纤竿"),

        new("craft_reel_t4_deep", "深海纺车轮", GearCraftSlot.Reel, "深海纺车轮", GearTier.T4,
            [],
            [new(AlchemyMaterials.GearSet, 2), new(AlchemyMaterials.Bearing, 1), new(AlchemyMaterials.AlloyFrame, 1)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "雾海深渊", 0,
            "齿轮组×2 + 轴承×1 + 合金框×1 → 深海纺车轮"),

        // T3 线
        new("craft_line_t3_braid", "精工编织线", GearCraftSlot.Line, "精工编织线", GearTier.T3,
            [],
            [new(AlchemyMaterials.NylonFilament, 5), new(AlchemyMaterials.ScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "静溪", 0.5, null, 0,
            "尼龙原丝×5 + 鱼鳞粉×2 → 100m编织线"),

        // T4 线
        new("craft_line_t4_titan", "钛合金织线", GearCraftSlot.Line, "钛合金织线", GearTier.T4,
            [new FishRequirement("*", 2, FishRarity.Rare, "雾海深渊")],
            [new(AlchemyMaterials.CarbonFiber, 3), new(AlchemyMaterials.NylonFilament, 3)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "雾海深渊", 0,
            "雾海稀有鱼×2 + 碳纤维 → 钛合金织线"),

        // T3 饵
        new("craft_lure_t3_glow", "精工磷光饵", GearCraftSlot.Lure, "精工磷光饵", GearTier.T3,
            [new FishRequirement("*", 3, FishRarity.Common, "静溪")],
            [new(AlchemyMaterials.ScalePowder, 4), new(AlchemyMaterials.Resin, 1)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "静溪", 0.5, null, 0,
            "静溪普通鱼×3 + 鱼鳞粉 → 精工磷光饵×3"),

        // T4 饵
        new("craft_lure_t4_abyss", "深渊拟态饵", GearCraftSlot.Lure, "深渊拟态饵", GearTier.T4,
            [new FishRequirement("幽蓝安康", 1, FishRarity.Epic, "雾海深渊")],
            [new(AlchemyMaterials.ScalePowder, 6), new(AlchemyMaterials.CarbonFiber, 2)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "雾海深渊", 0,
            "雾海史诗鱼 + 材料 → 深渊拟态饵×3"),

        // T5 竿
        new("craft_rod_t5_quantum", "量子折叠竿", GearCraftSlot.Rod, "量子折叠竿", GearTier.T5,
            [new FishRequirement("黄金锦鲤", 1, FishRarity.Legendary, "静溪")],
            [new(AlchemyMaterials.DeepSeaCrystal, 3), new(AlchemyMaterials.CarbonFiber, 5), new(AlchemyMaterials.MythScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "传说鱼 + 深海结晶 + 神话鳞粉 → 量子折叠竿"),

        new("craft_rod_t5_dragon", "神话龙脊竿", GearCraftSlot.Rod, "神话龙脊竿", GearTier.T5,
            [new FishRequirement("深湾巨口鲨", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.DeepSeaCrystal, 5), new(AlchemyMaterials.MythScalePowder, 3)],
            EconomySinks.AlchemyGearCraftT5Cost + 2000, 30, 12, null, 0, null, 0.8,
            "远礁传说 + 深海结晶×5 + 神话鳞粉×3"),

        // T5 轮
        new("craft_reel_t5_neural", "神经接口轮", GearCraftSlot.Reel, "神经接口轮", GearTier.T5,
            [new FishRequirement("霜龙传说鱼", 1, FishRarity.Legendary, "极光冰湾")],
            [new(AlchemyMaterials.GearSet, 4), new(AlchemyMaterials.Bearing, 3), new(AlchemyMaterials.DeepSeaCrystal, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "冰湾传说 + 齿轮组×4 + 轴承×3 → 神经接口轮"),

        // T5 线
        new("craft_line_t5_quantum", "量子丝导线", GearCraftSlot.Line, "量子丝导线", GearTier.T5,
            [new FishRequirement("*", 2, FishRarity.Legendary)],
            [new(AlchemyMaterials.CarbonFiber, 6), new(AlchemyMaterials.MythScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "传说鱼×2 + 碳纤维×6 + 神话鳞粉×2"),

        // T5 饵
        new("craft_lure_t5_quantum", "量子拟态饵", GearCraftSlot.Lure, "量子拟态饵", GearTier.T5,
            [new FishRequirement("神话·镜湖神鲤", 1, FishRarity.Legendary, "静溪")],
            [new(AlchemyMaterials.MythScalePowder, 4), new(AlchemyMaterials.ScalePowder, 8)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "神话鱼 + 神话鳞粉×4 → 量子拟态饵×2"),

        new("craft_lure_t5_oracle", "神谕鳞粉饵", GearCraftSlot.Lure, "神谕鳞粉饵", GearTier.T5,
            [new FishRequirement("神话·远海沧龙", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.MythScalePowder, 6), new(AlchemyMaterials.DeepSeaCrystal, 2)],
            EconomySinks.AlchemyGearCraftT5Cost + 1500, 32, 12, null, 0, null, 0.85,
            "远海神话 + 神话鳞粉×6 → 神谕鳞粉饵×2"),

        // T4 额外
        new("craft_rod_t4_mist", "雾海潜行竿", GearCraftSlot.Rod, "雾海潜行竿", GearTier.T4,
            [new FishRequirement("夜光乌贼", 2, FishRarity.Rare, "雾海深渊")],
            [new(AlchemyMaterials.CarbonFiber, 3), new(AlchemyMaterials.WaterWeed, 4)],
            EconomySinks.AlchemyGearCraftT4Cost - 500, 18, 5, null, 0, "雾海深渊", 0,
            "雾海稀有鱼 + 水草 → 雾海潜行竿"),

        new("craft_reel_t4_mag", "磁力刹车轮", GearCraftSlot.Reel, "磁力刹车轮", GearTier.T4,
            [],
            [new(AlchemyMaterials.GearSet, 3), new(AlchemyMaterials.Bearing, 2), new(AlchemyMaterials.Resin, 2)],
            EconomySinks.AlchemyGearCraftT4Cost - 300, 18, 5, null, 0, "雾海深渊", 0,
            "齿轮组×3 + 轴承×2 + 树脂×2"),
        ..GearT6T10Data.ExtraCraftRecipes,
    ];

    public static GearCraftRecipe? FindCraftRecipe(string id) =>
        GearCraftRecipes.FirstOrDefault(r => r.Id == id);

    public static int GetRodTier(string? name)
    {
        var spec = Rods.FirstOrDefault(r => r.Name == name);
        return spec is null ? 1 : (int)spec.Tier;
    }

    public static double DexPercentForSpot(IReadOnlyList<FishDexEntry> dex, string spotName)
    {
        var spotFish = dex.Where(e => e.SpotName == spotName && e.TargetLureRecipeId is null).ToList();
        if (spotFish.Count == 0) return 0;
        return (double)spotFish.Count(e => e.IsCaught) / spotFish.Count;
    }

    public static double OverallDexPercent(IReadOnlyList<FishDexEntry> dex)
    {
        var regular = dex.Where(e => e.TargetLureRecipeId is null).ToList();
        if (regular.Count == 0) return 0;
        return (double)regular.Count(e => e.IsCaught) / regular.Count;
    }

    public static bool MeetsGearUnlock(
        RodSpec spec,
        int fishingLevel,
        int catLevel,
        IReadOnlyList<FishDexEntry> dex,
        Func<string, bool> hasLicense)
    {
        if (fishingLevel < spec.RequiredLevel) return false;
        if (catLevel < spec.RequiredCatLevel) return false;
        if (spec.RequiredDexSpot is not null && DexPercentForSpot(dex, spec.RequiredDexSpot) < spec.RequiredDexPercent)
            return false;
        if (spec.RequiredLicenseSpot is not null && !hasLicense(spec.RequiredLicenseSpot))
            return false;
        if (spec.RequiredOverallDexPercent > 0 && OverallDexPercent(dex) < spec.RequiredOverallDexPercent)
            return false;
        if (!MeetsMythicGate(spec.RequiredMythicCaught, spec.RequiredAllMythic, dex))
            return false;
        return true;
    }

    public static bool MeetsGearUnlock(ReelSpec spec, int fishingLevel, int catLevel,
        IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense) =>
        MeetsGenericUnlock(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
            spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
            spec.RequiredMythicCaught, spec.RequiredAllMythic,
            fishingLevel, catLevel, dex, hasLicense);

    public static bool MeetsGearUnlock(LineSpec spec, int fishingLevel, int catLevel,
        IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense) =>
        MeetsGenericUnlock(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
            spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
            spec.RequiredMythicCaught, spec.RequiredAllMythic,
            fishingLevel, catLevel, dex, hasLicense);

    public static bool MeetsGearUnlock(LureSpec spec, int fishingLevel, int catLevel,
        IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense) =>
        MeetsGenericUnlock(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
            spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
            spec.RequiredMythicCaught, spec.RequiredAllMythic,
            fishingLevel, catLevel, dex, hasLicense);

    public static bool MeetsCraftUnlock(
        GearCraftRecipe recipe,
        int fishingLevel,
        int catLevel,
        IReadOnlyList<FishDexEntry> dex,
        Func<string, bool> hasLicense)
    {
        if (fishingLevel < recipe.RequiredFishingLevel) return false;
        if (catLevel < recipe.RequiredCatLevel) return false;
        if (recipe.RequiredDexSpot is not null && DexPercentForSpot(dex, recipe.RequiredDexSpot) < recipe.RequiredDexPercent)
            return false;
        if (recipe.RequiredLicenseSpot is not null && !hasLicense(recipe.RequiredLicenseSpot))
            return false;
        if (recipe.RequiredOverallDexPercent > 0 && OverallDexPercent(dex) < recipe.RequiredOverallDexPercent)
            return false;
        if (!MeetsMythicGate(recipe.RequiredMythicCaught, recipe.RequiredAllMythic, dex))
            return false;
        return true;
    }

    public static int MythicSpeciesCaught(IReadOnlyList<FishDexEntry> dex) =>
        dex.Count(e => e.TargetLureRecipeId is not null && e.IsCaught);

    public static bool MeetsMythicGate(int requiredMythicCaught, bool requiredAllMythic, IReadOnlyList<FishDexEntry> dex)
    {
        int caught = MythicSpeciesCaught(dex);
        if (requiredAllMythic && caught < TotalMythFishSpecies) return false;
        if (requiredMythicCaught > 0 && caught < requiredMythicCaught) return false;
        return true;
    }

    private static bool MeetsGenericUnlock(
        int requiredLevel, int requiredCatLevel, string? dexSpot, double dexPercent,
        string? licenseSpot, double overallDex,
        int requiredMythicCaught, bool requiredAllMythic,
        int fishingLevel, int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense)
    {
        if (fishingLevel < requiredLevel) return false;
        if (catLevel < requiredCatLevel) return false;
        if (dexSpot is not null && DexPercentForSpot(dex, dexSpot) < dexPercent) return false;
        if (licenseSpot is not null && !hasLicense(licenseSpot)) return false;
        if (overallDex > 0 && OverallDexPercent(dex) < overallDex) return false;
        if (!MeetsMythicGate(requiredMythicCaught, requiredAllMythic, dex)) return false;
        return true;
    }

    public static string UnlockShortfall(
        int requiredLevel, int requiredCatLevel, string? dexSpot, double dexPercent,
        string? licenseSpot, double overallDex,
        int requiredMythicCaught, bool requiredAllMythic,
        int fishingLevel, int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense)
    {
        if (fishingLevel < requiredLevel) return $"钓鱼 Lv.{requiredLevel}";
        if (catLevel < requiredCatLevel) return $"猫 Lv.{requiredCatLevel}";
        if (dexSpot is not null && DexPercentForSpot(dex, dexSpot) < dexPercent)
            return $"{dexSpot}图鉴 {(int)(dexPercent * 100)}%";
        if (licenseSpot is not null && !hasLicense(licenseSpot))
            return $"[{licenseSpot}]许可证";
        if (overallDex > 0 && OverallDexPercent(dex) < overallDex)
            return $"全图鉴 {(int)(overallDex * 100)}%";
        if (requiredAllMythic && MythicSpeciesCaught(dex) < TotalMythFishSpecies)
            return $"全神话 {MythicSpeciesCaught(dex)}/{TotalMythFishSpecies}";
        if (requiredMythicCaught > 0 && MythicSpeciesCaught(dex) < requiredMythicCaught)
            return $"神话鱼≥{requiredMythicCaught}";
        return "";
    }

    public static string UnlockShortfall(
        int requiredLevel, int requiredCatLevel, string? dexSpot, double dexPercent,
        string? licenseSpot, double overallDex,
        int fishingLevel, int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense) =>
        UnlockShortfall(requiredLevel, requiredCatLevel, dexSpot, dexPercent, licenseSpot, overallDex,
            0, false, fishingLevel, catLevel, dex, hasLicense);

    public static int GetGearTier(string? gearName)
    {
        if (string.IsNullOrEmpty(gearName)) return 1;
        var rod = Rods.FirstOrDefault(r => r.Name == gearName);
        if (rod is not null) return (int)rod.Tier;
        var reel = Reels.FirstOrDefault(r => r.Name == gearName);
        if (reel is not null) return (int)reel.Tier;
        var line = Lines.FirstOrDefault(l => l.Name == gearName);
        if (line is not null) return (int)line.Tier;
        var lure = Lures.FirstOrDefault(l => l.Name == gearName);
        if (lure is not null) return (int)lure.Tier;
        return 1;
    }

    /// <summary>终极毕业进度 0~100（T10 四槽 + 图鉴 95% + 神话鱼）。</summary>
    public static double ComputeGraduationPercent(
        int rodTier, int reelTier, int lineTier, int lureTier,
        double overallDexPercent,
        int mythSpeciesCaught)
    {
        double gear = (Math.Min(rodTier, 10) + Math.Min(reelTier, 10) + Math.Min(lineTier, 10) + Math.Min(lureTier, 10)) / 40.0;
        double dex = Math.Min(overallDexPercent / GraduationDexTarget, 1.0);
        double myth = Math.Min((double)mythSpeciesCaught / TotalMythFishSpecies, 1.0);
        return Math.Round((gear * 0.40 + dex * 0.35 + myth * 0.25) * 100, 1);
    }
}
