namespace CyberPetApp.Models;

/// <summary>重复性金币 sink 数值表（经济通胀控制）。</summary>
/// <remarks>
/// 500h 毕业曲线（挂机 2s/tick，1800 tick/h）：
/// <code>
/// | 阶段      | 累计目标 | 里程碑摘要              |
/// |-----------|---------|-------------------------|
/// | T1~T3     | ~40h    | 镇外溪流/雾海熟练           |
/// | T4~T6     | ~160h   | 引渠+派遣素材           |
/// | T7~T8     | ~310h   | 冰湾+鱼市回收           |
/// | T9~T10    | ~500h   | 远礁+神话+终极锻造      |
/// </code>
/// </remarks>
public static class EconomySinks
{
    // 市场摊位费：max(5g, SellPrice×2%)，拒绝/下架不退
    public const int MarketListingFeeMin = 5;
    public const double MarketListingFeeRate = 0.02;

    // 钓鱼：镇外溪流基础抛竿费；高阶钓点按阶递增
    public const int CastFeeBase = 3;
    public const int LineRepairFee = 8;

    // 喂食器：装入口粮加工费 / 份
    public const int FeederProcessingFee = 2;

    // 饮水器：装入凉白开水加工费 / 份
    public const int WatererProcessingFee = 1;

    // 每日求购：手动刷新（当日首次免费）
    public const int BountyRefreshFee = 50;

    // 猫医疗/美容
    public const int CatTreatFee = 30;
    public const int CatTreatRestore = 200;
    public const int CatTreatHealthThreshold = 500;
    public const int CatTreatHappinessThreshold = 200;

    // 工种培训解锁费
    public const int CatCafeUnlockFee = 200;
    public const int FishMarketPorterUnlockFee = 300;

    // 家具升级：+5% 加成，维护费 +3g/级
    public const int FurnitureUpgradeMaintenancePerLevel = 3;
    public const double FurnitureUpgradeBonusRate = 0.05;
    public const int MaxFurnitureUpgradeLevel = 1;

    public static int MarketListingFee(int sellPrice) =>
        Math.Max(MarketListingFeeMin, (int)Math.Ceiling(sellPrice * MarketListingFeeRate));

    public static int FurnitureUpgradeCost(Furniture furniture) => furniture.Price switch
    {
        <= 150 => 200,
        <= 300 => 350,
        _ => 500
    };

    public static int WorkJobUnlockFee(WorkJobType job) => job switch
    {
        WorkJobType.CatCafe => CatCafeUnlockFee,
        WorkJobType.FishMarketPorter => FishMarketPorterUnlockFee,
        _ => 0
    };

    // 装备耐久：低耐久阈值；修理按阶递增
    public const int DurabilityLowThreshold = 30;
    public const double DurabilityLowMultiplier = 0.5;
    public const int GearRepairPartialAmount = 20;

    public static int GearRepairPartialCost(int gearTier = 1) =>
        gearTier switch
        {
            >= 9 => 180,
            8 => 140,
            7 => 110,
            6 => 85,
            5 => 65,
            4 => 50,
            3 => 40,
            _ => 30
        };

    public static int GearFullRepairCost(double maxStrengthOrDrag, int gearTier = 1)
    {
        int baseCost = Math.Max(50, (int)(maxStrengthOrDrag * 10));
        double tierMult = gearTier switch
        {
            >= 9 => 2.8,
            8 => 2.3,
            7 => 1.9,
            6 => 1.6,
            5 => 1.35,
            4 => 1.15,
            _ => 1.0
        };
        return (int)Math.Ceiling(baseCost * tierMult);
    }

    // 钓点许可证（日租 / 永久）— 中期费用提高，需重复周常维护
    public const int DeepSeaPermanentLicense = 14000;
    public const int DeepSeaDailyRental = 480;
    public const int NeonCanalPermanentLicense = 22000;
    public const int NeonCanalDailyRental = 680;
    public const int GlacierNetPermanentLicense = 32000;
    public const int GlacierNetDailyRental = 980;
    public const int DataPortPermanentLicense = 48000;
    public const int DataPortDailyRental = 1350;
    public const int ReedBayPermanentLicense = 12000;
    public const int ReedBayDailyRental = 380;
    public const int RiftValleyPermanentLicense = 26000;
    public const int RiftValleyDailyRental = 780;
    public const int CoralReefPermanentLicense = 36000;
    public const int CoralReefDailyRental = 1050;
    public const int CoralRiftPermanentLicense = CoralReefPermanentLicense;
    public const int CoralRiftDailyRental = CoralReefDailyRental;
    public const int WreckGravePermanentLicense = 38000;
    public const int WreckGraveDailyRental = 1100;
    public const int AbyssCorridorPermanentLicense = 62000;
    public const int AbyssCorridorDailyRental = 1680;
    public const int StarTidePermanentLicense = 65000;
    public const int StarTideDailyRental = 1750;
    public const int VoidDomainPermanentLicense = 120000;
    public const int VoidDomainDailyRental = 3200;

    public static int SpotPermanentLicenseCost(string spotName) => spotName switch
    {
        "近海礁石" => DeepSeaPermanentLicense,
        "芦苇湿地" => ReedBayPermanentLicense,
        "地下暗河" => NeonCanalPermanentLicense,
        "深水海湾" => RiftValleyPermanentLicense,
        "极光冰湾" => GlacierNetPermanentLicense,
        "沉船墓场" => WreckGravePermanentLicense,
        "珊瑚暗流" => CoralReefPermanentLicense,
        "远礁外海" => DataPortPermanentLicense,
        "深渊回廊" => AbyssCorridorPermanentLicense,
        "星潮海沟" => StarTidePermanentLicense,
        "虚空钓域" => VoidDomainPermanentLicense,
        _ => 0
    };

    public static int SpotDailyRentalCost(string spotName) => spotName switch
    {
        "近海礁石" => DeepSeaDailyRental,
        "芦苇湿地" => ReedBayDailyRental,
        "地下暗河" => NeonCanalDailyRental,
        "深水海湾" => RiftValleyDailyRental,
        "极光冰湾" => GlacierNetDailyRental,
        "沉船墓场" => WreckGraveDailyRental,
        "珊瑚暗流" => CoralReefDailyRental,
        "远礁外海" => DataPortDailyRental,
        "深渊回廊" => AbyssCorridorDailyRental,
        "星潮海沟" => StarTideDailyRental,
        "虚空钓域" => VoidDomainDailyRental,
        _ => 0
    };

    /// <summary>抛竿费随钓点阶上升（开始挂机时扣一次）。</summary>
    public static int CastFeeForSpot(string? spotName) => spotName switch
    {
        "虚空钓域" => 58,
        "星潮海沟" => 48,
        "深渊回廊" => 45,
        "远礁外海" => 42,
        "沉船墓场" => 35,
        "珊瑚暗流" => 32,
        "极光冰湾" => 28,
        "深水海湾" => 22,
        "地下暗河" => 16,
        "芦苇湿地" => 11,
        "近海礁石" => 9,
        "废弃鱼塘" => 4,
        _ => CastFeeBase
    };

    /// <summary>高阶炼金素材鱼市回收价乘算（鼓励长期钓而非倒爷）。</summary>
    public static double MaterialRecycleRate(string materialName) => materialName switch
    {
        AlchemyMaterials.MythScalePowder => 0.45,
        AlchemyMaterials.VoidFiber => 0.48,
        AlchemyMaterials.OpenSeaStarCore => 0.52,
        AlchemyMaterials.AbyssGel => 0.55,
        AlchemyMaterials.AuroraIceCrystal => 0.60,
        AlchemyMaterials.CanalGlowPowder => 0.65,
        AlchemyMaterials.DeepSeaCrystal => 0.68,
        AlchemyMaterials.CarbonFiber => 0.72,
        AlchemyMaterials.ScalePowder => 0.78,
        _ => 0.85
    };

    // 烹饪加工费（按鱼稀有度）
    public const int CookingFeeCommon = 5;
    public const int CookingFeeRare = 15;
    public const int CookingFeeEpic = 40;
    public const int CookingFeeLegendary = 100;

    public static int CookingProcessingFee(FishRarity rarity) => rarity switch
    {
        FishRarity.Rare => CookingFeeRare,
        FishRarity.Epic => CookingFeeEpic,
        FishRarity.Legendary => CookingFeeLegendary,
        _ => CookingFeeCommon
    };

    // 炼金加工费
    public const int AlchemyGemCraftCost = 500;
    public const int AlchemyLureCraftCost = 800;
    public const int AlchemySocketFee = 200;

    // 装备锻造加工费（T6+ ×2~3；全套 T10 等效约 300万~500万 g 含素材时间）
    public const int AlchemyGearCraftT3Cost = 1200;
    public const int AlchemyGearCraftT4Cost = 6500;
    public const int AlchemyGearCraftT5Cost = 20000;
    public const int AlchemyGearCraftT6Cost = 42000;
    public const int AlchemyGearCraftT7Cost = 68000;
    public const int AlchemyGearCraftT8Cost = 98000;
    public const int AlchemyGearCraftT9Cost = 135000;
    public const int AlchemyGearCraftT10Cost = 185000;

    public static int AlchemyGearCraftCost(GearTier tier) => tier switch
    {
        GearTier.T10 => AlchemyGearCraftT10Cost,
        GearTier.T9 => AlchemyGearCraftT9Cost,
        GearTier.T8 => AlchemyGearCraftT8Cost,
        GearTier.T7 => AlchemyGearCraftT7Cost,
        GearTier.T6 => AlchemyGearCraftT6Cost,
        GearTier.T5 => AlchemyGearCraftT5Cost,
        GearTier.T4 => AlchemyGearCraftT4Cost,
        _ => AlchemyGearCraftT3Cost
    };

    // 兼容旧引用（无参修理费 = T1~T2 阶）
    public const int CastFee = CastFeeBase;

    // Fish backpack upgrades
    public const int FishBackpackBaseCapacity = 50;
    public const int FishBackpackMaxCapacity = 300;
    public const int FishBackpackUpgradeIncrement = 10;
    public const int FishBackpackBaseUpgradeCostPerTier = 200;

    public static int FishBackpackNextIncrement(int currentCapacity) =>
        Math.Max(0, Math.Min(FishBackpackUpgradeIncrement, FishBackpackMaxCapacity - Math.Max(FishBackpackBaseCapacity, currentCapacity)));

    public static int FishBackpackUpgradeCost(int currentCapacity)
    {
        int cur = Math.Max(FishBackpackBaseCapacity, currentCapacity);
        if (cur >= FishBackpackMaxCapacity) return 0;
        int tier = ((cur - FishBackpackBaseCapacity) / FishBackpackUpgradeIncrement) + 1;
        return FishBackpackBaseUpgradeCostPerTier * tier;
    }
}
