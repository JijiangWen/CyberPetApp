namespace CyberPetApp.Models;

/// <summary>镶嵌宝石类型：对应钓鱼公式不同项。</summary>
public enum GemType
{
    /// <summary>抓口宝石 → 咬钩成功率</summary>
    Hook,
    /// <summary>卸力宝石 → 起鱼成功率</summary>
    Drag,
    /// <summary>幸运宝石 → 稀有度权重</summary>
    Luck,
    /// <summary>钓重宝石 → 有效最大钓重</summary>
    Weight,
    /// <summary>丝导宝石 → 鱼线抓口敏锐（镶鱼线槽）</summary>
    Line
}

/// <summary>装备宝石槽位（竿/轮/线/饵各 1）。</summary>
public enum GearGemSlot
{
    Rod,
    Reel,
    Line,
    Lure
}

public record FishRequirement(string FishName, int Count, FishRarity? MinRarity = null, string? SpotName = null);

public record MaterialRequirement(string ItemName, int Count);

public record GemAlchemyRecipe(
    string Id,
    string DisplayName,
    GemType GemType,
    GearGemSlot SocketSlot,
    IReadOnlyList<FishRequirement> Fish,
    IReadOnlyList<MaterialRequirement> Materials,
    int GoldCost,
    string Description);

public record TargetLureRecipe(
    string Id,
    string DisplayName,
    string TargetFishName,
    string SpotName,
    int MaxUses,
    IReadOnlyList<FishRequirement> Fish,
    IReadOnlyList<MaterialRequirement> Materials,
    int GoldCost,
    string Description);

public record LineAlchemyRecipe(
    string Id,
    string DisplayName,
    string OutputLineName,
    double LineStrength,
    double LineSensitivity,
    double LineStealth,
    double AbrasionResistance,
    WaterDepth TargetDepth,
    IReadOnlyList<FishRequirement> Fish,
    IReadOnlyList<MaterialRequirement> Materials,
    int GoldCost,
    string Description);

/// <summary>炼金配方静态目录（不入库）。</summary>
public static class AlchemyRecipes
{
    public static readonly List<GemAlchemyRecipe> GemRecipes =
    [
        new("gem_hook_creek", "镇外溪流抓口宝石", GemType.Hook, GearGemSlot.Rod,
            [new FishRequirement("纯色金化鲤", 2, FishRarity.Legendary, "镇外溪流")],
            [new MaterialRequirement(ExpeditionCatalog.ScrapItem, 3)],
            EconomySinks.AlchemyGemCraftCost, "2×镇外溪流传说鱼 + 赛博废料×3 → 抓口 +3~8%"),

        new("gem_drag_abyss", "深渊卸力宝石", GemType.Drag, GearGemSlot.Reel,
            [new FishRequirement("斑斓石斑", 2, FishRarity.Legendary, "近海礁石")],
            [new MaterialRequirement(ExpeditionCatalog.DataChipItem, 3)],
            EconomySinks.AlchemyGemCraftCost, "2×雾海传说鱼 + 数据碎片×3 → 卸力 +3~8%"),

        new("gem_luck_canal", "引渠幸运宝石", GemType.Luck, GearGemSlot.Lure,
            [new FishRequirement("地下金丝鲃", 2, FishRarity.Legendary, "地下暗河")],
            [new MaterialRequirement(ExpeditionCatalog.DecorTokenItem, 2)],
            EconomySinks.AlchemyGemCraftCost, "2×引渠传说鱼 + 装饰凭证×2 → 幸运 +3~8%"),

        new("gem_weight_glacier", "冰湾钓重宝石", GemType.Weight, GearGemSlot.Rod,
            [new FishRequirement("霜龙传说鱼", 2, FishRarity.Legendary, "极光冰湾")],
            [new MaterialRequirement(ExpeditionCatalog.ScrapItem, 3)],
            EconomySinks.AlchemyGemCraftCost + 100, "2×冰湾传说鱼 + 赛博废料×3 → 钓重 +3~8%"),

        new("gem_line_silk", "丝导敏锐宝石", GemType.Line, GearGemSlot.Line,
            [new FishRequirement("地下金丝鲃", 2, FishRarity.Epic, "地下暗河")],
            [new MaterialRequirement(ExpeditionCatalog.DataChipItem, 2)],
            EconomySinks.AlchemyGemCraftCost, "2×引渠史诗鱼 + 数据碎片×2 → 抓口 +3~8%（鱼线槽）"),
    ];

    public static readonly List<LineAlchemyRecipe> LineRecipes =
    [
        new("line_carbon_fiber", "碳纤维线", "碳纤维线",
            35, 0.5, 0.05, 0.30, WaterDepth.Middle,
            [new FishRequirement("*", 2, FishRarity.Epic)],
            [new MaterialRequirement(ExpeditionCatalog.DataChipItem, 5)],
            600, "史诗鱼×2 + 数据碎片×5 + 600g → 高强度耐磨线"),

        new("line_mithril", "秘银编织线", "秘银编织线",
            28, 1.8, 0.08, 0.20, WaterDepth.Middle,
            [new FishRequirement("*", 2, FishRarity.Legendary)],
            [new MaterialRequirement(ExpeditionCatalog.DecorTokenItem, 3)],
            1200, "传说鱼×2 + 装饰凭证×3 + 1200g → 高敏锐线"),

        new("line_dragon", "龙筋线", "龙筋线",
            55, 1.0, 0.20, 0.40, WaterDepth.Deep,
            [
                new FishRequirement("纯色金化鲤", 1, FishRarity.Legendary, "镇外溪流"),
                new FishRequirement("斑斓石斑", 1, FishRarity.Legendary, "近海礁石"),
                new FishRequirement("霜龙传说鱼", 1, FishRarity.Legendary, "极光冰湾")
            ],
            [],
            1200, "3种传说鱼各×1 + 1200g → 顶级隐蔽耐磨线"),
    ];

    public static readonly List<TargetLureRecipe> TargetLures =
    [
        new("lure_mutant_piranha", "镜湖灵饵", "异变·巨型水虎鱼", "镇外溪流", 6,
            [
                new FishRequirement("纯色金化鲤", 2, FishRarity.Legendary, "镇外溪流"),
                new FishRequirement("老油条鳟鱼", 1, FishRarity.Epic, "镇外溪流")
            ],
            [new MaterialRequirement(ExpeditionCatalog.DecorTokenItem, 1)],
            EconomySinks.AlchemyLureCraftCost,
            "镇外溪流专用 · 目标：镜湖神鲤（神话）"),

        new("lure_mutant_eel", "溪影软饵", "异变·装甲溪鳗", "镇外溪流", 7,
            [
                new FishRequirement("大个体溪哥", 1, FishRarity.Epic, "镇外溪流"),
                new FishRequirement("彩虹鳟鱼", 2, FishRarity.Rare, "镇外溪流")
            ],
            [new MaterialRequirement(ExpeditionCatalog.ScrapItem, 3)],
            EconomySinks.AlchemyLureCraftCost + 200,
            "镇外溪流专用 · 目标：翠影鳗王（神话）"),

        new("lure_giant_squid", "深渊之核", "异变·深海大王乌贼", "近海礁石", 5,
            [
                new FishRequirement("斑斓石斑", 2, FishRarity.Legendary, "近海礁石"),
                new FishRequirement("幽蓝安康", 1, FishRarity.Epic, "近海礁石")
            ],
            [new MaterialRequirement(ExpeditionCatalog.ScrapItem, 5)],
            EconomySinks.AlchemyLureCraftCost + 300,
            "近海礁石专用 · 目标：雾海古神鱿（神话）"),

        new("lure_albino_salamander", "夜光核心饵", "异变·白化巨螈", "地下暗河", 6,
            [
                new FishRequirement("地下金丝鲃", 1, FishRarity.Legendary, "地下暗河"),
                new FishRequirement("彩光鲑", 2, FishRarity.Epic, "地下暗河")
            ],
            [new MaterialRequirement(ExpeditionCatalog.DataChipItem, 4)],
            EconomySinks.AlchemyLureCraftCost + 400,
            "地下暗河专用 · 目标：引渠幻龙（神话）"),

        new("lure_aurora_crystal", "极光冰晶饵", "神话·极光霜龙", "极光冰湾", 5,
            [
                new FishRequirement("霜龙传说鱼", 1, FishRarity.Legendary, "极光冰湾"),
                new FishRequirement("冰川巨鲈", 1, FishRarity.Epic, "极光冰湾")
            ],
            [new MaterialRequirement(ExpeditionCatalog.DataChipItem, 3)],
            EconomySinks.AlchemyLureCraftCost + 500,
            "极光冰湾专用 · 目标：极光霜龙（神话）"),

        new("lure_reef_leviathan", "礁心活饵", "神话·远海沧龙", "远礁外海", 5,
            [
                new FishRequirement("深湾巨口鲨", 1, FishRarity.Legendary, "远礁外海"),
                new FishRequirement("远礁蝠鲼", 1, FishRarity.Epic, "远礁外海")
            ],
            [new MaterialRequirement(ExpeditionCatalog.DecorTokenItem, 2)],
            EconomySinks.AlchemyLureCraftCost + 700,
            "远礁外海专用 · 目标：远海沧龙（神话）"),

        new("lure_sea_emperor", "海皇树脂饵", "神话·金鳞海皇", "远礁外海", 8,
            [
                new FishRequirement("金鳞锦鲤", 2, FishRarity.Legendary, "远礁外海"),
                new FishRequirement("深湾金枪", 1, FishRarity.Rare, "远礁外海")
            ],
            [new MaterialRequirement(ExpeditionCatalog.ScrapItem, 4)],
            EconomySinks.AlchemyLureCraftCost + 700,
            "远礁外海专用 · 目标：金鳞海皇（神话）"),

        new("lure_swamp_devourer", "废弃鱼塘莲饵", "神话·废弃鱼塘幻鳞", "废弃鱼塘", 4,
            [new FishRequirement("塘主·独眼老鲤", 1, FishRarity.Legendary, "废弃鱼塘")],
            [new MaterialRequirement(AlchemyMaterials.WaterWeed, 6)],
            EconomySinks.AlchemyLureCraftCost,
            "废弃鱼塘专用 · 目标：废弃鱼塘幻鳞（神话）"),

        new("lure_poison_croc", "苇歌软饵", "异变·毒沼巨鳄", "芦苇湿地", 5,
            [new FishRequirement("湿地霸主鲤", 1, FishRarity.Legendary, "芦苇湿地")],
            [new MaterialRequirement(AlchemyMaterials.ReedFiber, 4)],
            EconomySinks.AlchemyLureCraftCost + 250,
            "芦苇湿地专用 · 目标：芦苇幽歌（神话）"),

        new("lure_mutant_ray", "裂谷矿饵", "神话·深水海湾兽", "深水海湾", 6,
            [new FishRequirement("深湾老船长巨鳕", 1, FishRarity.Legendary, "深水海湾")],
            [new MaterialRequirement(AlchemyMaterials.RiftSlag, 5)],
            EconomySinks.AlchemyLureCraftCost + 450,
            "深水海湾专用 · 目标：深水海湾兽（神话）"),

        new("lure_wreck_soul", "亡魂铁锈饵", "神话·沉船亡魂", "沉船墓场", 7,
            [new FishRequirement("墓主传说鳕", 1, FishRarity.Legendary, "沉船墓场")],
            [new MaterialRequirement(AlchemyMaterials.WreckRust, 5)],
            EconomySinks.AlchemyLureCraftCost + 600,
            "沉船墓场专用 · 目标：沉船亡魂（神话）"),

        new("lure_coral_heart", "珊瑚心饵", "神话·珊瑚心海", "珊瑚暗流", 7,
            [new FishRequirement("珊瑚心金鳞", 1, FishRarity.Legendary, "珊瑚暗流")],
            [new MaterialRequirement(AlchemyMaterials.CoralShard, 6)],
            EconomySinks.AlchemyLureCraftCost + 550,
            "珊瑚暗流专用 · 目标：珊瑚心海（神话）"),

        new("lure_abyss_walker", "回廊凝胶饵", "神话·深渊巡礼者", "深渊回廊", 8,
            [new FishRequirement("回廊传说鲨", 1, FishRarity.Legendary, "深渊回廊")],
            [new MaterialRequirement(AlchemyMaterials.AbyssGel, 5)],
            EconomySinks.AlchemyLureCraftCost + 800,
            "深渊回廊专用 · 目标：深渊巡礼者（神话）"),

        new("lure_star_tide", "星潮碎片饵", "神话·星潮巨兽", "星潮海沟", 8,
            [new FishRequirement("星潮传说鳕", 1, FishRarity.Legendary, "星潮海沟")],
            [new MaterialRequirement(AlchemyMaterials.StarTideShard, 5)],
            EconomySinks.AlchemyLureCraftCost + 850,
            "星潮海沟专用 · 目标：星潮巨兽（神话）"),

        new("lure_void_master", "虚空裂隙饵", "神话·虚空钓主", "虚空钓域", 10,
            [new FishRequirement("终焉传说鱼", 1, FishRarity.Legendary, "虚空钓域")],
            [new MaterialRequirement(AlchemyMaterials.VoidFiber, 4)],
            EconomySinks.AlchemyLureCraftCost + 1200,
            "虚空钓域专用 · 目标：虚空钓主（神话）"),

        new("lure_void_whale", "终焉鲸歌饵", "神话·终焉鲸歌", "虚空钓域", 10,
            [new FishRequirement("虚空鲸影", 1, FishRarity.Legendary, "虚空钓域")],
            [new MaterialRequirement(AlchemyMaterials.VoidMote, 6)],
            EconomySinks.AlchemyLureCraftCost + 1500,
            "虚空钓域专用 · 目标：终焉鲸歌（神话）"),
    ];

    public static GemAlchemyRecipe? FindGem(string id) =>
        GemRecipes.FirstOrDefault(r => r.Id == id);

    public static TargetLureRecipe? FindTargetLure(string id) =>
        TargetLures.FirstOrDefault(r => r.Id == id);

    public static LineAlchemyRecipe? FindLine(string id) =>
        LineRecipes.FirstOrDefault(r => r.Id == id);

    public static string? TargetLureSpot(string recipeId) =>
        FindTargetLure(recipeId)?.SpotName;

    public static string GemTypeLabel(GemType t) => t switch
    {
        GemType.Hook => "抓口",
        GemType.Drag => "卸力",
        GemType.Luck => "幸运",
        GemType.Weight => "钓重",
        GemType.Line => "丝导",
        _ => "?"
    };

    public static string SlotLabel(GearGemSlot s) => s switch
    {
        GearGemSlot.Rod => "鱼竿",
        GearGemSlot.Reel => "渔轮",
        GearGemSlot.Line => "鱼线",
        GearGemSlot.Lure => "拟饵",
        _ => "?"
    };

    /// <summary>宝石加成加算后 soft cap（单项与总和均不超过 15%）。</summary>
    public static GemBonusSet CapGemBonuses(GemBonusSet raw) =>
        new(
            Math.Min(GemBonusSet.MaxTotal, raw.HookBonus),
            Math.Min(GemBonusSet.MaxTotal, raw.DragBonus),
            Math.Min(GemBonusSet.MaxTotal, raw.LuckBonus),
            Math.Min(GemBonusSet.MaxTotal, raw.WeightBonus),
            Math.Min(GemBonusSet.MaxTotal, raw.LineBonus));

    /// <summary>Roll 3~8% 宝石数值。</summary>
    public static double RollGemBonus(Random? rng = null)
    {
        rng ??= Random.Shared;
        return Math.Round(0.03 + rng.NextDouble() * 0.05, 4);
    }
}

/// <summary>三槽宝石加成快照（用于 FishingLoadout）。</summary>
public record GemBonusSet(double HookBonus, double DragBonus, double LuckBonus, double WeightBonus, double LineBonus = 0)
{
    public const double MaxTotal = 0.15;

    public static GemBonusSet Empty => new(0, 0, 0, 0, 0);

    public static GemBonusSet FromGems(IEnumerable<PlayerGem> gems)
    {
        double hook = 0, drag = 0, luck = 0, weight = 0, line = 0;
        foreach (var g in gems.Where(x => x.IsSocketed))
        {
            switch (g.GemType)
            {
                case GemType.Hook: hook += g.BonusValue; break;
                case GemType.Drag: drag += g.BonusValue; break;
                case GemType.Luck: luck += g.BonusValue; break;
                case GemType.Weight: weight += g.BonusValue; break;
                case GemType.Line: line += g.BonusValue; break;
            }
        }
        var capped = AlchemyRecipes.CapGemBonuses(new GemBonusSet(hook, drag, luck, weight, line));
        return capped with { LineBonus = Math.Min(MaxTotal, line) };
    }
}

public record FishingRollContext(
    double RarityBonus = 0,
    string? ActiveTargetLureRecipeId = null,
    string? SpotName = null,
    int LureGearTier = 1,
    double LureMythicBonus = 0);
