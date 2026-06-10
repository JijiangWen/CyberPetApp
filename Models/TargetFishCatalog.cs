namespace CyberPetApp.Models;

/// <summary>指定饵专属鱼（神话种）静态查询，供图鉴 / UI / 成就使用。</summary>
public static class TargetFishCatalog
{
    private static readonly Lazy<Dictionary<string, FishingSpot>> Spots =
        new(() => FishingSpotCatalog.BuildAll());

    public static string NormalizeFishName(string name) =>
        name.StartsWith("超规格·", StringComparison.Ordinal) ? name["超规格·".Length..] : name;

    public static bool IsTargetExclusive(string fishName) =>
        AllTemplates().Any(t => t.Name == NormalizeFishName(fishName));

    public static FishTemplate? FindTemplate(string fishName)
    {
        var key = NormalizeFishName(fishName);
        return AllTemplates().FirstOrDefault(t => t.Name == key);
    }

    public static TargetLureRecipe? RequiredLure(string fishName)
    {
        var tpl = FindTemplate(fishName);
        return tpl?.TargetLureRecipeId is null
            ? null
            : AlchemyRecipes.FindTargetLure(tpl.TargetLureRecipeId);
    }

    public static IReadOnlyList<FishTemplate> HiddenFishForSpot(string spotName)
    {
        if (!Spots.Value.TryGetValue(spotName, out var spot)) return [];
        return spot.FishTable.Where(f => f.TargetLureRecipeId is not null).ToList();
    }

    public static IReadOnlyList<(FishTemplate Fish, TargetLureRecipe Lure)> AllEntries()
    {
        var list = new List<(FishTemplate, TargetLureRecipe)>();
        foreach (var spot in Spots.Value.Values)
        {
            foreach (var fish in spot.FishTable.Where(f => f.TargetLureRecipeId is not null))
            {
                var lure = AlchemyRecipes.FindTargetLure(fish.TargetLureRecipeId!);
                if (lure is not null) list.Add((fish, lure));
            }
        }
        return list;
    }

    private static IEnumerable<FishTemplate> AllTemplates() =>
        Spots.Value.Values.SelectMany(s => s.FishTable).Where(f => f.TargetLureRecipeId is not null);
}
