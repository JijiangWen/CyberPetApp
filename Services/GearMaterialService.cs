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

    /// <summary>成功钓获后随机掉落素材（写入背包）。</summary>
    public async Task<string?> TryGrantCatchMaterialAsync(Player player, Fish fish, string spotName)
    {
        var drops = RollCatchDrops(fish, spotName);
        if (drops.Count == 0) return null;

        var parts = new List<string>();
        foreach (var (name, qty) in drops)
        {
            await _playerService.GrantBackpackItemAsync(player, name, qty);
            parts.Add($"{name}×{qty}");
        }
        await _context.SaveChangesAsync();
        return string.Join(" · ", parts);
    }

    /// <summary>分解鱼获换取炼金素材（鱼从背包移除）。</summary>
    public async Task<(bool Ok, string Message)> DisassembleFishAsync(Player player, Fish fish)
    {
        string spot = ResolveSpot(fish.Name);
        var drops = RollDisassembleDrops(fish, spot);
        if (drops.Count == 0)
            return (false, "该鱼无法分解");

        var dbFish = await _context.Fishes.FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is null) return (false, "鱼不存在");
        _context.Fishes.Remove(dbFish);
        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);

        var parts = new List<string>();
        foreach (var (name, qty) in drops)
        {
            await _playerService.GrantBackpackItemAsync(player, name, qty);
            parts.Add($"{name}×{qty}");
        }
        await _context.SaveChangesAsync();
        return (true, $"分解【{fish.Name}】→ {string.Join(" · ", parts)}");
    }

    /// <summary>直售/市场上架时额外返还少量素材。</summary>
    public async Task GrantRecycleBonusAsync(Player player, Fish fish)
    {
        if (_random.NextDouble() > 0.35) return;
        string spot = ResolveSpot(fish.Name);
        string? bonus = fish.Rarity switch
        {
            FishRarity.Legendary when fish.Name.StartsWith("神话·", StringComparison.Ordinal)
                => AlchemyMaterials.MythScalePowder,
            FishRarity.Legendary => AlchemyMaterials.ScalePowder,
            FishRarity.Epic => SpotEpicRecycleMaterial(spot),
            FishRarity.Rare when spot is "雾海深渊" or "芦苇湾" => AlchemyMaterials.CarbonFiber,
            FishRarity.Rare when spot is "暗涌裂谷" => AlchemyMaterials.RiftSlag,
            FishRarity.Common when spot is "静溪" or "浅塘" => _random.NextDouble() < 0.5
                ? AlchemyMaterials.WaterWeed : AlchemyMaterials.BambooStrip,
            FishRarity.Common when spot == "芦苇湾" => AlchemyMaterials.ReedFiber,
            _ => AlchemyMaterials.FishBone
        };
        if (bonus is null) return;
        int qty = fish.Rarity >= FishRarity.Epic ? 1 : _random.Next(1, 3);
        await _playerService.GrantBackpackItemAsync(player, bonus, qty);
        await _context.SaveChangesAsync();
    }

    private static string SpotEpicRecycleMaterial(string spot) => spot switch
    {
        "远礁外海" or "星潮海沟" => AlchemyMaterials.OpenSeaStarCore,
        "深渊回廊" => AlchemyMaterials.AbyssGel,
        "极光冰湾" => AlchemyMaterials.AuroraIceCrystal,
        "夜光引渠" => AlchemyMaterials.CanalGlowPowder,
        "珊瑚暗流" => AlchemyMaterials.CoralShard,
        "沉船墓场" => AlchemyMaterials.WreckRust,
        "暗涌裂谷" => AlchemyMaterials.RiftSlag,
        "虚空钓域" => AlchemyMaterials.VoidMote,
        "雾海深渊" => AlchemyMaterials.CarbonFiber,
        _ => AlchemyMaterials.DeepSeaCrystal
    };

    private List<(string Name, int Qty)> RollCatchDrops(Fish fish, string spotName)
    {
        var list = new List<(string, int)>();
        if (_random.NextDouble() > 0.22) return list;

        if (fish.Rarity == FishRarity.Common && spotName is "静溪" or "浅塘" && _random.NextDouble() < 0.4)
            list.Add((AlchemyMaterials.WaterWeed, 1));
        if (fish.Rarity == FishRarity.Common && spotName == "芦苇湾" && _random.NextDouble() < 0.35)
            list.Add((AlchemyMaterials.ReedFiber, 1));
        if (fish.Rarity <= FishRarity.Rare)
            list.Add((AlchemyMaterials.FishScale, _random.Next(1, 3)));
        if (fish.Rarity >= FishRarity.Rare && spotName is "雾海深渊" or "芦苇湾")
            list.Add((AlchemyMaterials.CarbonFiber, 1));
        if (fish.Rarity >= FishRarity.Rare && spotName == "夜光引渠")
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

        if (spot is "静溪" or "浅塘" && fish.Rarity == FishRarity.Common)
        {
            list.Add((AlchemyMaterials.BambooStrip, Random.Shared.Next(1, 4)));
            list.Add((AlchemyMaterials.WaterWeed, 1));
        }
        if (spot == "芦苇湾" && fish.Rarity == FishRarity.Common)
            list.Add((AlchemyMaterials.ReedFiber, Random.Shared.Next(1, 3)));
        if (spot is "雾海深渊" or "芦苇湾" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.CarbonFiber, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "夜光引渠" && fish.Rarity >= FishRarity.Rare)
            list.Add((AlchemyMaterials.CanalGlowPowder, fish.Rarity >= FishRarity.Epic ? 2 : 1));
        if (spot == "暗涌裂谷" && fish.Rarity >= FishRarity.Rare)
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
        return SpotByFish.GetValueOrDefault(baseName, "静溪");
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
