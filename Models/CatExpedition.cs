namespace CyberPetApp.Models;

/// <summary>派遣区域定义（静态目录）。</summary>
public record ExpeditionZone(
    string Id,
    string Name,
    string Flavor,
    int DurationMinutes,
    int EnergyCost,
    int MinCatHappiness,
    int GoldMin,
    int GoldMax,
    string PrimaryLoot,
    int LootMin,
    int LootMax);

public static class ExpeditionCatalog
{
    public const string ScrapItem = "旧零件废料";
    public const string DataChipItem = "破损硬磁盘";
    public const string DecorTokenItem = "老旧流通券";

    public static readonly List<ExpeditionZone> Zones =
    [
        new("ruins", "荒地旧废墟", "拾荒旧服务器残骸", 15, 200, 400, 20, 80, ScrapItem, 1, 3),
        // 生锈齿轮组：ruins 副产（PrimaryLoot 仍为废料，Claim 时额外 roll）
        new("datalab", "报废服务器旧址", "清洗损坏日志簇", 30, 350, 500, 50, 150, DataChipItem, 1, 2),
        new("black_alley", "老城区黑市暗巷", "替 NPC 猫跑腿", 45, 500, 600, 100, 250, DecorTokenItem, 1, 1),
        new("coral_outpost", "珊瑚前哨", "回收红珊瑚碎屑与荧光粉", 60, 600, 700, 180, 420, AlchemyMaterials.CoralShard, 2, 4),
        new("wreck_salvage", "沉船打捞", "打捞铁锈与深渊巨兽粘液", 90, 800, 750, 280, 650, AlchemyMaterials.WreckRust, 1, 3),
    ];

    public static ExpeditionZone? Find(string? id) =>
        string.IsNullOrEmpty(id) ? null : Zones.FirstOrDefault(z => z.Id == id);
}

public record ExpeditionStatus(
    bool IsActive,
    ExpeditionZone? Zone,
    DateTime? EndsAt,
    TimeSpan Remaining,
    bool CanClaim);

public record ExpeditionResult(bool Success, string Message, int Gold = 0, string? LootName = null, int LootQty = 0);
