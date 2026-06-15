namespace CyberPetApp.Models;

/// <summary>装备阶位 T1~T10；数值与解锁条件集中于此便于调参。</summary>
/// <remarks>
/// 500h 端到端毕业曲线（挂机 2s/tick，毕业=全套 T10 + 图鉴 95% + 全神话鱼各 1）：
/// | 阶段   | 目标时长 | 里程碑              |
/// |--------|---------|---------------------|
/// | T1~T3  | ~40h    | 镇外溪流/雾海熟练       |
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
    public const string BambooStrip = "河边烂木条";
    public const string CarbonFiber = "工业碳纤维";
    public const string DeepSeaCrystal = "沧海蓝晶石";
    public const string FishBone = "鱼骨";
    public const string FishScale = "鱼鳞";
    public const string WaterWeed = "缠绕水草";
    public const string GearSet = "生锈齿轮组";
    public const string Resin = "工业粘合树脂";
    public const string Bearing = "工业精密轴承";
    public const string AlloyFrame = "轻质钛合金框架";
    public const string NylonFilament = "工业尼龙单丝";
    public const string ScalePowder = "研磨细鱼鳞粉";
    public const string MythScalePowder = "五彩神话鳞粉";
    public const string CanalGlowPowder = "地下荧光孢子粉";
    public const string StarfallAlloy = "星陨粗铁胚";
    public const string VoidCore = "裂隙虚空原核";
    public const string AbyssEssence = "沧溟古鱼之髓";
    public const string CoralShard = "红珊瑚碎屑";
    public const string AuroraIceCrystal = "不融极地冰晶";
    public const string AbyssGel = "深渊巨兽粘液";
    public const string WreckRust = "百年沉船铁皮";
    public const string OpenSeaStarCore = "落星陨铁晶核";
    public const string DragonResin = "古树龙涎香";
    public const string VoidFiber = "裂隙虚空丝线";
    public const string ReedFiber = "老韧芦苇丝";
    public const string RiftSlag = "热液喷口矿渣";
    public const string StarTideShard = "月汐重力碎屑";
    public const string VoidMote = "虚空幽浮微粒";

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
        BambooStrip => "镇外溪流普通鱼分解",
        CarbonFiber => "雾海稀有+鱼分解/副产",
        DeepSeaCrystal => "远礁外海鱼分解",
        FishBone => "任意鱼分解",
        FishScale => "任意鱼分解",
        WaterWeed => "镇外溪流/common 分解",
        GearSet => "派遣/打工 · 高阶钓点机械鱼分解",
        Resin => "生活商店",
        Bearing => "生活商店",
        AlloyFrame => "生活商店",
        NylonFilament => "生活商店",
        ScalePowder => "炼金/分解副产",
        MythScalePowder => "神话鱼分解",
        CanalGlowPowder => "地下暗河稀有+分解",
        CoralShard => "珊瑚暗流/雾海分解",
        AuroraIceCrystal => "极光冰湾史诗+分解",
        AbyssGel => "远礁外海史诗+分解",
        WreckRust => "沉船墓场稀有+派遣",
        OpenSeaStarCore => "远礁传说+神话副产",
        DragonResin => "远礁史诗+炼金",
        VoidFiber => "虚空钓域神话+炼金",
        ReedFiber => "芦苇湿地普通鱼分解",
        RiftSlag => "深水海湾稀有+分解",
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
        ["镇外溪流"] = 1,
        ["废弃鱼塘"] = 1,
        ["近海礁石"] = 2,
        ["芦苇湿地"] = 3,
        ["地下暗河"] = 4,
        ["深水海湾"] = 5,
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

    /// <summary>T10@镇外溪流=1.0；T1@远礁≈0.15。</summary>
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
        "深水海湾" => 5,
        "地下暗河" => 4,
        "近海礁石" or "芦苇湿地" => 2,
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
        new("随手捡的柳树条", 0.5, 5, 1, 1, 0, "敏锐+5% · 抛投1 · 钓重5kg", GearTier.T1),
        new("外皮发青的自制竹竿", 0.7, 7, 1, 1, 120, "入门加长 · 抛投1 · 钓重7kg", GearTier.T1),
        new("二手跳蚤市场玻璃钢竿", 1.2, 12, 2, 5, 500, "敏锐+12% · 抛投2 · 钓重12kg · 需钓鱼Lv5", GearTier.T2, RequiredCatLevel: 3),
        new("“迪卡王”入门路亚竿", 1.5, 15, 2, 5, 900, "均衡入门+ · 抛投2 · 钓重15kg", GearTier.T2, RequiredCatLevel: 3),
        new("“小溪流浪者”独节路亚", 2.0, 30, 3, 12, 4500, "敏锐+20% · 抛投3 · 钓重30kg", GearTier.T3, RequiredDexSpot: "镇外溪流", RequiredDexPercent: 0.5),
        new("“老兵”碳纤斑驳直柄竿", 1.8, 25, 3, 10, 3200, "镇外溪流特化 · 浅层+5%等口", GearTier.T3, RequiredDexSpot: "镇外溪流", RequiredDexPercent: 0.4),
        new("“匠心”手工缠线高碳竿", 2.4, 35, 3, 12, 0, "炼金 · 敏锐+24% · 抛投3", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("“极昼”深水抗压实心竿", 2.8, 50, 4, 20, 0, "炼金主路径 · 敏锐+28% · 抛投4", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“夜行者”磨砂哑光隐匿竿", 2.6, 45, 4, 18, 0, "炼金 · 深层隐蔽+ · 抛投4", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“猎手”折叠伸缩直柄竿", 3.5, 80, 5, 30, 0, "炼金 · 敏锐+35% · 抛投5", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("“龙骨”仿生碳化直柄竿", 3.8, 90, 5, 30, 0, "炼金 · 神话素材 · 钓重90kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("“沧溟”特制重底锚竿", 4.0, 100, 5, 32, 0, "炼金 T5 · 抛投5 · 钓重100kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.85),
        ..GearT6T10Data.ExtraRods,
    ];

    // ── 渔轮（12）──
    public static readonly List<ReelSpec> Reels =
    [
        new("生锈的手拨铁线轴", 2, 5.0, 8, 0.30, 1, 0, "卸力+10% · 线杯8kg", GearTier.T1),
        new("“两元店”塑料玩具轮", 2.5, 5.2, 10, 0.32, 1, 180, "轻便入门轮 · 线杯10kg", GearTier.T1),
        new("“沙沙作响”的二手水滴轮", 5, 6.2, 15, 0.50, 5, 600, "卸力+25% · 线杯15kg", GearTier.T2, RequiredCatLevel: 3),
        new("“开拓者”入门级鼓轮", 6, 6.5, 18, 0.55, 5, 1100, "顺滑提升 · 线杯18kg", GearTier.T2, RequiredCatLevel: 3),
        new("“大力士”重型改装鼓轮", 8, 7.5, 25, 0.65, 12, 5200, "卸力+40% · 线杯25kg", GearTier.T3, RequiredDexSpot: "镇外溪流", RequiredDexPercent: 0.45),
        new("“水镜”双卸力纺车轮", 9, 7.8, 28, 0.70, 12, 0, "炼金 · 顺滑70%", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("“旋风”双摇柄高速纺车轮", 8, 6.8, 22, 0.72, 10, 4800, "高速比纺车轮 · 卸力+42%", GearTier.T3, RequiredCatLevel: 8),
        new("“深蓝”密封防水防咸纺车轮", 12, 8.5, 40, 0.78, 20, 0, "炼金主路径 · 线杯40kg", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“游鹰”双离心制动鼓轮", 11, 8.2, 38, 0.76, 18, 0, "炼金 · 高顺滑", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“声呐反馈”电子计数鼓轮", 18, 10.0, 60, 0.92, 30, 0, "炼金神话轮 · 线杯60kg", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("“重压”大扭矩齿轮减速水滴", 16, 9.5, 55, 0.88, 28, 0, "炼金 · 远礁特化", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.75),
        new("“雷神”磁力微物抛投水滴轮", 17, 7.2, 58, 0.90, 28, 0, "双重磁力刹车 · T5", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.78),
        ..GearT6T10Data.ExtraReels,
    ];

    // ── 鱼线（12）──
    public static readonly List<LineSpec> Lines =
    [
        new("外婆缝被子的红棉线", 5, 0.3, 0.00, 0.00, WaterDepth.Shallow, 1, 0, "强度5kg · 敏锐+3%", GearTier.T1),
        new("“地摊货”粗尼龙单丝线", 8, 0.4, 0.01, 0.05, WaterDepth.Shallow, 1, 150, "入门加强 · 耐磨5%", GearTier.T1),
        new("“大力马”4编基础PE线", 12, 0.6, 0.03, 0.10, WaterDepth.Shallow, 5, 400, "强度12kg · 隐蔽3%", GearTier.T2, RequiredCatLevel: 3),
        new("“幽影”夜光涂层线", 15, 0.7, 0.04, 0.12, WaterDepth.Middle, 5, 750, "夜钓向 · 中层", GearTier.T2, RequiredCatLevel: 3),
        new("“隐形”氟碳碳素前导线", 25, 0.9, 0.06, 0.20, WaterDepth.Middle, 12, 4200, "强度25kg · 耐磨20%", GearTier.T3, RequiredDexSpot: "镇外溪流", RequiredDexPercent: 0.5),
        new("“顺滑”8编高密PE线", 30, 1.0, 0.08, 0.25, WaterDepth.Middle, 12, 0, "炼金 · 100m编织", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("“夜游者”深冷反光隐蔽线", 28, 1.1, 0.15, 0.22, WaterDepth.Middle, 15, 5000, "中层夜钓向 · 极高隐蔽性", GearTier.T3, RequiredCatLevel: 10),
        new("“包钢”防咬金属钢丝前导", 45, 1.2, 0.10, 0.35, WaterDepth.Middle, 20, 0, "炼金深海线 · 强度45kg", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“重力”高比重沉水尼龙线", 52, 1.3, 0.12, 0.38, WaterDepth.Deep, 22, 0, "炼金 · 深层特化", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“微脉冲”电感应传导线", 70, 1.5, 0.15, 0.50, WaterDepth.Deep, 30, 0, "炼金神话线 · 耐磨50%", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8),
        new("“巨兽”极限抗磨多股编织", 65, 1.8, 0.18, 0.45, WaterDepth.Deep, 28, 0, "炼金 · 高敏锐", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.75),
        new("“斩魔”高韧防切氟碳线", 68, 1.6, 0.16, 0.48, WaterDepth.Deep, 26, 18000, "深海特化 · 超强防咬耐磨", GearTier.T5, RequiredCatLevel: 18),
        ..GearT6T10Data.ExtraLines,
    ];

    // ── 拟饵（12）──
    public static readonly List<LureSpec> Lures =
    [
        new("压扁的生锈铁勺", 0.10, 0.0, WaterDepth.Shallow, 20, 1, 5, 25, "浅层 · 等口-10%", GearTier.T1),
        new("“大尾巴”橡胶红面包虫", 0.12, 0.0, WaterDepth.Shallow, 18, 1, 5, 40, "镇外溪流入门 · 浅层", GearTier.T1),
        new("“夜魔”简易自发光塑料管", 0.20, 0.3, WaterDepth.Middle, 15, 5, 5, 125, "中层 · 品质+30%", GearTier.T2, RequiredCatLevel: 3),
        new("“歪嘴”手工涂装浮水米诺", 0.35, 0.8, WaterDepth.Middle, 12, 5, 5, 400, "中层 · 品质+80%", GearTier.T2, RequiredCatLevel: 3),
        new("“肥水”沙蚕软饵配铅头钩", 0.28, 0.5, WaterDepth.Deep, 10, 12, 5, 680, "深层 · 品质+50%", GearTier.T3, RequiredDexSpot: "镇外溪流", RequiredDexPercent: 0.5),
        new("“反光”鳞片压制沉水VIB", 0.32, 0.6, WaterDepth.Deep, 10, 12, 3, 0, "炼金 · 深层 lure", GearTier.T3, ShopAvailable: false, CraftOnly: true),
        new("“泼辣小龙虾”双尾软虫", 0.36, 0.7, WaterDepth.Middle, 12, 14, 5, 850, "中层 lure · 逼真震颤吸引", GearTier.T3, RequiredCatLevel: 10),
        new("“红头阿玛尼”经典米诺", 0.42, 1.0, WaterDepth.Deep, 8, 20, 3, 0, "炼金 T4 · 品质+100%", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“幽灵小飞贼”水面波扒", 0.38, 0.9, WaterDepth.Deep, 9, 18, 3, 0, "炼金 · 雾海特化", GearTier.T4, ShopAvailable: false, CraftOnly: true, RequiredLicenseSpot: "近海礁石"),
        new("“激流斩”重盐沉水棒贝软虫", 0.50, 1.2, WaterDepth.Deep, 8, 30, 2, 0, "炼金神话饵 · 品质+120%", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.8, MythicBonus: 0.05),
        new("“金蝉脱壳”避障防挂雷蛙", 0.55, 1.5, WaterDepth.Deep, 6, 32, 2, 0, "神话饵率+8% · T5", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredOverallDexPercent: 0.85, MythicBonus: 0.08),
        new("“巨浪行者”螺旋金属亮片", 0.44, 1.1, WaterDepth.Deep, 10, 22, 3, 0, "深层 lure · 强反光大摆幅", GearTier.T5, ShopAvailable: false, CraftOnly: true, RequiredCatLevel: 14),
        ..GearT6T10Data.ExtraLures,
    ];

    // ── craft.gear 炼金配方 ──
    public static readonly List<GearCraftRecipe> GearCraftRecipes =
    [
        // T3 竿
        new("craft_rod_t3_carbon", "“匠心”手工缠线高碳竿", GearCraftSlot.Rod, "“匠心”手工缠线高碳竿", GearTier.T3,
            [], [new(AlchemyMaterials.BambooStrip, 3), new(AlchemyMaterials.CarbonFiber, 2), new(AlchemyMaterials.Resin, 1)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "镇外溪流", 0.5, null, 0,
            "河边烂木条×3 + 工业碳纤维×2 + 树脂×1 → “匠心”手工缠线高碳竿"),

        // T4 竿/轮
        new("craft_rod_t4_deep", "“极昼”深水抗压实心竿", GearCraftSlot.Rod, "“极昼”深水抗压实心竿", GearTier.T4,
            [new FishRequirement("斑斓大石斑", 1, FishRarity.Epic, "近海礁石")],
            [new(AlchemyMaterials.BambooStrip, 2), new(AlchemyMaterials.CarbonFiber, 4), new(AlchemyMaterials.Resin, 2)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "近海礁石", 0,
            "雾海史诗鱼 + 碳纤维 → “极昼”深水抗压实心竿"),

        new("craft_reel_t4_deep", "“深蓝”密封防水防咸纺车轮", GearCraftSlot.Reel, "“深蓝”密封防水防咸纺车轮", GearTier.T4,
            [],
            [new(AlchemyMaterials.GearSet, 2), new(AlchemyMaterials.Bearing, 1), new(AlchemyMaterials.AlloyFrame, 1)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "近海礁石", 0,
            "生锈齿轮组×2 + 轴承×1 + 合金框×1 → “深蓝”密封防水防咸纺车轮"),

        // T3 线
        new("craft_line_t3_braid", "“顺滑”8编高密PE线", GearCraftSlot.Line, "“顺滑”8编高密PE线", GearTier.T3,
            [],
            [new(AlchemyMaterials.NylonFilament, 5), new(AlchemyMaterials.ScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "镇外溪流", 0.5, null, 0,
            "工业尼龙单丝×5 + 研磨细鱼鳞粉×2 → 100m编织线"),

        // T4 线
        new("craft_line_t4_titan", "“包钢”防咬金属钢丝前导", GearCraftSlot.Line, "“包钢”防咬金属钢丝前导", GearTier.T4,
            [new FishRequirement("*", 2, FishRarity.Rare, "近海礁石")],
            [new(AlchemyMaterials.CarbonFiber, 3), new(AlchemyMaterials.NylonFilament, 3)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "近海礁石", 0,
            "雾海稀有鱼×2 + 碳纤维 → “包钢”防咬金属钢丝前导"),

        // T3 饵
        new("craft_lure_t3_glow", "“反光”鳞片压制沉水VIB", GearCraftSlot.Lure, "“反光”鳞片压制沉水VIB", GearTier.T3,
            [new FishRequirement("*", 3, FishRarity.Common, "镇外溪流")],
            [new(AlchemyMaterials.ScalePowder, 4), new(AlchemyMaterials.Resin, 1)],
            EconomySinks.AlchemyGearCraftT3Cost, 12, 3, "镇外溪流", 0.5, null, 0,
            "镇外溪流普通鱼×3 + 研磨细鱼鳞粉 → “反光”鳞片压制沉水VIB×3"),

        // T4 饵
        new("craft_lure_t4_abyss", "“红头阿玛尼”经典米诺", GearCraftSlot.Lure, "“红头阿玛尼”经典米诺", GearTier.T4,
            [new FishRequirement("大灯笼安康鱼", 1, FishRarity.Epic, "近海礁石")],
            [new(AlchemyMaterials.ScalePowder, 6), new(AlchemyMaterials.CarbonFiber, 2)],
            EconomySinks.AlchemyGearCraftT4Cost, 20, 5, null, 0, "近海礁石", 0,
            "雾海史诗鱼 + 材料 → “红头阿玛尼”经典米诺×3"),

        // T5 竿
        new("craft_rod_t5_quantum", "“猎手”折叠伸缩直柄竿", GearCraftSlot.Rod, "“猎手”折叠伸缩直柄竿", GearTier.T5,
            [new FishRequirement("金背鲤仙", 1, FishRarity.Legendary, "镇外溪流")],
            [new(AlchemyMaterials.DeepSeaCrystal, 3), new(AlchemyMaterials.CarbonFiber, 5), new(AlchemyMaterials.MythScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "传说鱼 + 沧海蓝晶石 + 五彩神话鳞粉 → “猎手”折叠伸缩直柄竿"),

        new("craft_rod_t5_dragon", "“龙骨”仿生碳化直柄竿", GearCraftSlot.Rod, "“龙骨”仿生碳化直柄竿", GearTier.T5,
            [new FishRequirement("大白鲨", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.DeepSeaCrystal, 5), new(AlchemyMaterials.MythScalePowder, 3)],
            EconomySinks.AlchemyGearCraftT5Cost + 2000, 30, 12, null, 0, null, 0.8,
            "远礁传说 + 沧海蓝晶石×5 + 五彩神话鳞粉×3"),

        // T5 轮
        new("craft_reel_t5_neural", "“声呐反馈”电子计数鼓轮", GearCraftSlot.Reel, "“声呐反馈”电子计数鼓轮", GearTier.T5,
            [new FishRequirement("极光冰川巨鳎", 1, FishRarity.Legendary, "极光冰湾")],
            [new(AlchemyMaterials.GearSet, 4), new(AlchemyMaterials.Bearing, 3), new(AlchemyMaterials.DeepSeaCrystal, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "冰湾传说 + 生锈齿轮组×4 + 轴承×3 → “声呐反馈”电子计数鼓轮"),

        new("craft_reel_t5_thor", "“雷神”磁力微物抛投水滴轮", GearCraftSlot.Reel, "“雷神”磁力微物抛投水滴轮", GearTier.T5,
            [new FishRequirement("神话·“远海沧龙”", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.StarfallAlloy, 3), new(AlchemyMaterials.Bearing, 3), new(AlchemyMaterials.DeepSeaCrystal, 2)],
            EconomySinks.AlchemyGearCraftT5Cost + 1000, 30, 12, null, 0, null, 0.8,
            "远海神话 + 轴承×3 + 沧海蓝晶石×2 → “雷神”磁力微物抛投水滴轮"),

        // T5 线
        new("craft_line_t5_quantum", "“微脉冲”电感应传导线", GearCraftSlot.Line, "“微脉冲”电感应传导线", GearTier.T5,
            [new FishRequirement("*", 2, FishRarity.Legendary)],
            [new(AlchemyMaterials.CarbonFiber, 6), new(AlchemyMaterials.MythScalePowder, 2)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "传说鱼×2 + 碳纤维×6 + 五彩神话鳞粉×2"),

        // T5 饵
        new("craft_lure_t5_quantum", "“激流斩”重盐沉水棒贝软虫", GearCraftSlot.Lure, "“激流斩”重盐沉水棒贝软虫", GearTier.T5,
            [new FishRequirement("异变·“镜湖水虎兽”", 1, FishRarity.Legendary, "镇外溪流")],
            [new(AlchemyMaterials.MythScalePowder, 4), new(AlchemyMaterials.ScalePowder, 8)],
            EconomySinks.AlchemyGearCraftT5Cost, 30, 10, null, 0, null, 0.8,
            "神话鱼 + 五彩神话鳞粉×4 → “激流斩”重盐沉水棒贝软虫×2"),

        new("craft_lure_t5_oracle", "“金蝉脱壳”避障防挂雷蛙", GearCraftSlot.Lure, "“金蝉脱壳”避障防挂雷蛙", GearTier.T5,
            [new FishRequirement("神话·“远海沧龙”", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.MythScalePowder, 6), new(AlchemyMaterials.DeepSeaCrystal, 2)],
            EconomySinks.AlchemyGearCraftT5Cost + 1500, 32, 12, null, 0, null, 0.85,
            "远海神话 + 五彩神话鳞粉×6 → “金蝉脱壳”避障防挂雷蛙×2"),

        new("craft_lure_t5_wave", "“巨浪行者”螺旋金属亮片", GearCraftSlot.Lure, "“巨浪行者”螺旋金属亮片", GearTier.T5,
            [new FishRequirement("大白鲨", 1, FishRarity.Legendary, "远礁外海")],
            [new(AlchemyMaterials.StarfallAlloy, 2), new(AlchemyMaterials.ScalePowder, 8)],
            EconomySinks.AlchemyGearCraftT5Cost, 22, 10, null, 0, null, 0.8,
            "远礁传说 + 星陨粗铁胚×2 + 研磨细鱼鳞粉×8 → “巨浪行者”螺旋金属亮片"),

        // T4 额外
        new("craft_rod_t4_mist", "“夜行者”磨砂哑光隐匿竿", GearCraftSlot.Rod, "“夜行者”磨砂哑光隐匿竿", GearTier.T4,
            [new FishRequirement("荧光墨鱼", 2, FishRarity.Rare, "近海礁石")],
            [new(AlchemyMaterials.CarbonFiber, 3), new(AlchemyMaterials.WaterWeed, 4)],
            EconomySinks.AlchemyGearCraftT4Cost - 500, 18, 5, null, 0, "近海礁石", 0,
            "雾海稀有鱼 + 缠绕水草 → “夜行者”磨砂哑光隐匿竿"),

        new("craft_reel_t4_mag", "“游鹰”双离心制动鼓轮", GearCraftSlot.Reel, "“游鹰”双离心制动鼓轮", GearTier.T4,
            [],
            [new(AlchemyMaterials.GearSet, 3), new(AlchemyMaterials.Bearing, 2), new(AlchemyMaterials.Resin, 2)],
            EconomySinks.AlchemyGearCraftT4Cost - 300, 18, 5, null, 0, "近海礁石", 0,
            "生锈齿轮组×3 + 轴承×2 + 树脂×2"),
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
