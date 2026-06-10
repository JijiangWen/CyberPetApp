namespace CyberPetApp.Models;

/// <summary>图鉴条目：静态鱼表 + 玩家钓获记录合并。</summary>
public record FishDexEntry(
    string FishName,
    string SpotName,
    FishRarity Rarity,
    string? TargetLureRecipeId,
    string? TargetLureDisplayName,
    bool IsCaught,
    FishCatchRecord? Record);

/// <summary>全服鱼种图鉴目录（来自钓点静态表）。</summary>
public static class FishDexCatalog
{
    private static List<FishDexEntry>? _cache;

    public static IReadOnlyList<FishDexEntry> BuildAll(IReadOnlyList<FishCatchRecord> records)
    {
        var byName = records.ToDictionary(r => r.FishName, r => r, StringComparer.Ordinal);
        var entries = new List<FishDexEntry>();

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (spotName, spot) in FishingSpotCatalog.BuildAll())
        {
            foreach (var fish in spot.FishTable)
            {
                if (!seen.Add(fish.Name)) continue;

                string? lureName = fish.TargetLureRecipeId is null
                    ? null
                    : AlchemyRecipes.FindTargetLure(fish.TargetLureRecipeId)?.DisplayName;

                byName.TryGetValue(fish.Name, out var rec);
                entries.Add(new FishDexEntry(
                    fish.Name,
                    spotName,
                    fish.Rarity,
                    fish.TargetLureRecipeId,
                    lureName,
                    rec is not null,
                    rec));
            }
        }

        return entries
            .OrderBy(e => e.SpotName)
            .ThenByDescending(e => (int)e.Rarity)
            .ThenBy(e => e.FishName)
            .ToList();
    }

    public static int TotalSpecies
    {
        get
        {
            _cache ??= BuildAll([]).ToList();
            return _cache.Count;
        }
    }
}
