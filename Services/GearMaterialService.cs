using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>炼金素材：钓获副产、鱼分解、鱼市回收加成。</summary>
public class GearMaterialService
{
    private readonly AppDbContext _context;
    private readonly PlayerService _playerService;
    private readonly Random _random = new();

    private static readonly Dictionary<string, string> SpotByFish = BuildFishSpotMap();

    public GearMaterialService(AppDbContext context, PlayerService playerService)
    {
        _context = context;
        _playerService = playerService;
    }

    /// <summary>成功钓获后随机掉落素材（写入背包）。
    /// NOTE: this method no longer calls SaveChanges; caller should persist once.
    /// </summary>
    public Task<string?> TryGrantCatchMaterialAsync(Player player, Fish fish, string spotName)
    {
        var drops = RollCatchDrops(fish, spotName);
        if (drops.Count == 0) return Task.FromResult<string?>(null);

        var parts = new List<string>();
        foreach (var (name, qty) in drops)
        {
            UpsertBackpackItem(player, name, qty);
            parts.Add($"{name}×{qty}");
        }

        // caller is responsible for calling SaveChangesAsync once
        return Task.FromResult<string?>(string.Join(" · ", parts));
    }

    /// <summary>分解鱼获：仅更新内存，供乐观 UI 立即反馈。</summary>
    public (bool Ok, string Message, List<(string Name, int Qty)> Drops) ApplyDisassembleInMemory(Player player, Fish fish)
    {
        string spot = ResolveSpot(fish.Name);
        var drops = RollDisassembleDrops(fish, spot);
        if (drops.Count == 0)
            return (false, "该鱼无法分解", []);

        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);
        var parts = new List<string>();
        foreach (var (name, qty) in drops)
        {
            player.Backpack[name] = player.Backpack.GetValueOrDefault(name) + qty;
            parts.Add($"{name}×{qty}");
        }

        return (true, $"分解【{fish.Name}】→ {string.Join(" · ", parts)}", drops);
    }

    /// <summary>将分解结果写入 EF 跟踪并提交（内存已由页面更新）。</summary>
    public async Task PersistDisassembleAsync(Player player, Fish fish, IReadOnlyList<(string Name, int Qty)> drops)
    {
        var dbFish = _context.Fishes.Local.FirstOrDefault(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is not null)
            _context.Fishes.Remove(dbFish);
        else
            _context.Entry(new Fish { Id = fish.Id, PlayerId = player.Id }).State = EntityState.Deleted;

        foreach (var (name, qty) in drops)
            ApplyBackpackDeltaToContext(player, name, qty);

        await _context.SaveChangesAsync();
    }

    /// <summary>仅同步 EF 跟踪（内存已由调用方更新）。</summary>
    private void ApplyBackpackDeltaToContext(Player player, string name, int delta)
    {
        var item = _context.BackpackItems.Local
            .FirstOrDefault(b => b.PlayerId == player.Id && b.ItemName == name);
        if (item is null)
        {
            _context.BackpackItems.Add(new BackpackItem
            {
                PlayerId = player.Id,
                ItemName = name,
                Quantity = player.Backpack.GetValueOrDefault(name)
            });
        }
        else
        {
            item.Quantity += delta;
        }
    }

    /// <summary>直售/市场上架时额外返还少量素材。</summary>
    public Task GrantRecycleBonusAsync(Player player, Fish fish)
    {
        if (_random.NextDouble() > 0.35) return Task.CompletedTask;
        string spot = ResolveSpot(fish.Name);
        string? bonus = fish.Rarity switch
        {
            FishRarity.Legendary when fish.Name.StartsWith("神话·", StringComparison.Ordinal)
                => AlchemyMaterials.MythScalePowder,
            FishRarity.Legendary => AlchemyMaterials.ScalePowder,
            FishRarity.Epic => SpotEpicRecycleMaterial(spot),
            FishRarity.Rare when spot is "近海礁石" or "芦苇湿地" => AlchemyMaterials.CarbonFiber,
            FishRarity.Rare when spot is "深水海湾" => AlchemyMaterials.RiftSlag,
            FishRarity.Common when spot is "镇外溪流" or "废弃鱼塘" => _random.NextDouble() < 0.5
                ? AlchemyMaterials.WaterWeed : AlchemyMaterials.BambooStrip,
            FishRarity.Common when spot == "芦苇湿地" => AlchemyMaterials.ReedFiber,
            _ => AlchemyMaterials.FishBone
        };
        if (bonus is null) return Task.CompletedTask;
        int qty = fish.Rarity >= FishRarity.Epic ? 1 : _random.Next(1, 3);

        UpsertBackpackItem(player, bonus, qty);
        return Task.CompletedTask;
    }

    private static string SpotEpicRecycleMaterial(string spot) => spot switch
    {
        "远礁外海" or "星潮海沟" => AlchemyMaterials.OpenSeaStarCore,
        "深渊回廊" => AlchemyMaterials.AbyssGel,
        "极光冰湾" => AlchemyMaterials.AuroraIceCrystal,
        "地下暗河" => AlchemyMaterials.CanalGlowPowder,
        "珊瑚暗流" => AlchemyMaterials.CoralShard,
        "沉船墓场" => AlchemyMaterials.WreckRust,
        "深水海湾" => AlchemyMaterials.RiftSlag,
        "虚空钓域" => AlchemyMaterials.VoidMote,
        "近海礁石" => AlchemyMaterials.CarbonFiber,
        _ => AlchemyMaterials.DeepSeaCrystal
    };

    private List<(string Name, int Qty)> RollCatchDrops(Fish fish, string spotName)
    {
        var list = new List<(string, int)>();
        if (_random.NextDouble() > 0.22) return list;

        if (fish.Rarity == FishRarity.Common && spotName is "镇外溪流" or "废弃鱼塘" && _random.NextDouble() < 0.4)
            list.Add((AlchemyMaterials.WaterWeed, 1));
        if (fish.Rarity == FishRarity.Common && spotName == "芦苇湿地" && _random.NextDouble() < 0.35)
            list.Add((AlchemyMaterials.ReedFiber, 1));
        if (fish.Rarity <= FishRarity.Rare)
            list.Add((AlchemyMaterials.FishScale, _random.Next(1, 3)));
        if (fish.Rarity >= FishRarity.Rare && spotName is "近海礁石" or "芦苇湿地")
            list.Add((AlchemyMaterials.CarbonFiber, 1));
        if (fish.Rarity >= FishRarity.Rare && spotName == "地下暗河")
            list.Add((AlchemyMaterials.CanalGlowPowder, 1));
        if (fish.Rarity >= FishRarity.Epic)
            list.Add((SpotEpicRecycleMaterial(spotName), 1));
        if (fish.Name.StartsWith("神话·", StringComparison.Ordinal) && _random.NextDouble() < 0.35)
            list.Add((AlchemyMaterials.MythScalePowder, 1));
        if (list.Count == 0 && _random.NextDouble() < 0.5)
            list.Add((AlchemyMaterials.FishBone, 1));
        return list;
    }

    private static List<(string Name, int Qty)> RollDisassembleDrops(Fish fish, string spot)
    {
        var list = new List<(string, int)>();
        list.Add((AlchemyMaterials.FishBone, fish.Rarity switch
        {
            FishRarity.Legendary => 3,
            FishRarity.Epic => 2,
            FishRarity.Rare => 2,
            _ => 1
        }));
        list.Add((AlchemyMaterials.FishScale, fish.Rarity switch
        {
            FishRarity.Legendary => 4,
            FishRarity.Epic => 3,
            FishRarity.Rare => 2,
            _ => 1
        }));
        if (fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.ScalePowder, 1));

        // 允许在“地下暗河”（夜光引渠）、“极光冰湾”、“沉船墓场”等高阶钓点中，分解 Rare 以上的废弃机械类鱼种以 20% 概率掉落生锈齿轮组
        if ((spot == "地下暗河" || spot == "极光冰湾" || spot == "沉船墓场") &&
            fish.Rarity >= FishRarity.Rare &&
            (fish.Name.Contains("齿轮") || fish.Name.Contains("发条") || fish.Name.Contains("机械") || fish.Name.Contains("金属") || fish.Name.Contains("铁皮") || fish.Name.Contains("零件") || fish.Name.Contains("锈")))
        {
            if (Random.Shared.NextDouble() < 0.20)
            {
                list.Add((AlchemyMaterials.GearSet, 1));
            }
        }

        if (spot is "镇外溪流" or "废弃鱼塘" && fish.Rarity == FishRarity.Common)
        {
            list.Add((AlchemyMaterials.BambooStrip, Random.Shared.Next(1, 4)));
            list.Add((AlchemyMaterials.WaterWeed, 1));
        }
        if (spot == "芦苇湿地" && fish.Rarity == FishRarity.Common)
            list.Add((AlchemyMaterials.ReedFiber, Random.Shared.Next(1, 3)));
        if (spot is "近海礁石" or "芦苇湿地" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.CarbonFiber, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "地下暗河" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.CanalGlowPowder, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "深水海湾" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.RiftSlag, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "珊瑚暗流" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.CoralShard, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "极光冰湾" && fish.Rarity >= FishRarity.Epic)
            list.Add((AlchemyMaterials.AuroraIceCrystal, fish.Rarity == FishRarity.Legendary ? 2 : 1));
        if (spot == "沉船墓场" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.WreckRust, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot is "远礁外海" or "星潮海沟" && fish.Rarity >= FishRarity.Epic)
            list.Add((AlchemyMaterials.OpenSeaStarCore, fish.Rarity == FishRarity.Legendary ? 2 : 1));
        if (spot == "深渊回廊" && fish.Rarity >= FishRarity.Epic)
            list.Add((AlchemyMaterials.AbyssGel, fish.Rarity == FishRarity.Legendary ? 2 : 1));
        if (spot == "虚空钓域" && fish.Rarity >= FishRarity.Legendary)
            list.Add((AlchemyMaterials.VoidMote, fish.Name.StartsWith("神话·", StringComparison.Ordinal) ? 2 : 1));
        if (spot == "远礁外海" && fish.Rarity >= FishRarity.Epic)
            list.Add((AlchemyMaterials.DeepSeaCrystal, fish.Rarity == FishRarity.Legendary ? 2 : 1));
        if (fish.Name.StartsWith("神话·", StringComparison.Ordinal))
            list.Add((AlchemyMaterials.MythScalePowder, 2));

        return list.Where(x => x.Item2 > 0).ToList();
    }

    private static string ResolveSpot(string fishName)
    {
        string baseName = fishName.StartsWith("超规格·", StringComparison.Ordinal)
            ? fishName["超规格·".Length..] : fishName;
        return SpotByFish.GetValueOrDefault(baseName, "镇外溪流");
    }

    /// <summary>仅操作 EF Local 缓存 + 内存字典，避免同步查库阻塞 UI 线程。</summary>
    private void UpsertBackpackItem(Player player, string name, int delta)
    {
        var item = _context.BackpackItems.Local
            .FirstOrDefault(b => b.PlayerId == player.Id && b.ItemName == name);
        if (item is null)
        {
            int baseQty = player.Backpack.GetValueOrDefault(name);
            _context.BackpackItems.Add(new BackpackItem
            {
                PlayerId = player.Id,
                ItemName = name,
                Quantity = baseQty + delta
            });
        }
        else
        {
            item.Quantity += delta;
        }

        player.Backpack[name] = player.Backpack.GetValueOrDefault(name) + delta;
    }

    private static Dictionary<string, string> BuildFishSpotMap()
    {
        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (spotName, spot) in FishingSpotCatalog.BuildAll())
            foreach (var t in spot.FishTable)
                map.TryAdd(t.Name, spotName);
        return map;
    }
}
