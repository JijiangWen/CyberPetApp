namespace CyberPetApp.Models;

/// <summary>钓点区域（UI 分组 / 解锁阶段）。</summary>
public enum FishingSpotRegion
{
  前期,
  T2T3,
  T4T5,
  T6T7,
  T8T9,
  T10终极
}

public record FishingSpotRegionInfo(
    FishingSpotRegion Region,
    string Label,
    string UnlockHint,
    int MinRodTier);

/// <summary>静态钓点配置：鱼表、稀有度权重、水层与收益系数。</summary>
public static partial class FishingSpotCatalog
{
    private static readonly Dictionary<string, FishingSpotRegionInfo> RegionBySpot = new()
    {
        ["镇外溪流"] = new(FishingSpotRegion.前期, "前期流域", "Lv1 · T1竿", 1),
        ["废弃鱼塘"] = new(FishingSpotRegion.前期, "前期流域", "Lv1 · T1竿", 1),
        ["近海礁石"] = new(FishingSpotRegion.T2T3, "雾海/芦苇", "Lv8 · T2竿", 2),
        ["芦苇湿地"] = new(FishingSpotRegion.T2T3, "雾海/芦苇", "Lv12 · T3竿", 3),
        ["地下暗河"] = new(FishingSpotRegion.T4T5, "引渠/裂谷", "许可+Lv18 · T4竿", 4),
        ["深水海湾"] = new(FishingSpotRegion.T4T5, "引渠/裂谷", "许可+Lv18 · T5竿", 5),
        ["极光冰湾"] = new(FishingSpotRegion.T6T7, "冰湾/墓场/珊瑚", "许可+Lv32 · T6竿", 6),
        ["沉船墓场"] = new(FishingSpotRegion.T6T7, "冰湾/墓场/珊瑚", "许可+Lv32 · T7竿", 7),
        ["珊瑚暗流"] = new(FishingSpotRegion.T6T7, "冰湾/墓场/珊瑚", "许可+Lv32 · T6竿", 6),
        ["远礁外海"] = new(FishingSpotRegion.T8T9, "外海/深渊/星潮", "许可+Lv45 · T8竿", 8),
        ["深渊回廊"] = new(FishingSpotRegion.T8T9, "外海/深渊/星潮", "许可+Lv45 · T9竿", 9),
        ["星潮海沟"] = new(FishingSpotRegion.T8T9, "外海/深渊/星潮", "许可+Lv45 · T9竿", 9),
        ["虚空钓域"] = new(FishingSpotRegion.T10终极, "虚空终局", "全神话+Lv60 · T10竿", 10),
    };

    public static FishingSpotRegionInfo RegionFor(string spotName) =>
        RegionBySpot.GetValueOrDefault(spotName,
            new(FishingSpotRegion.前期, "?", "?", 1));

    public static IReadOnlyList<(FishingSpotRegion Region, string Label, IReadOnlyList<string> SpotNames)> RegionGroups()
    {
        var order = new[]
        {
            FishingSpotRegion.前期, FishingSpotRegion.T2T3, FishingSpotRegion.T4T5,
            FishingSpotRegion.T6T7, FishingSpotRegion.T8T9, FishingSpotRegion.T10终极
        };
        var spots = BuildAll();
        return order.Select(r =>
        {
            var names = spots.Keys
                .Where(k => RegionFor(k).Region == r)
                .OrderBy(k => spots[k].RequiredLevel)
                .ThenBy(k => k)
                .ToList();
            var label = names.Count > 0 ? RegionFor(names[0]).Label : r.ToString();
            return (Region: r, Label: label, SpotNames: (IReadOnlyList<string>)names);
        }).Where(g => g.SpotNames.Count > 0).ToList();
    }

    public static Dictionary<string, FishingSpot> BuildAll()
    {
        var spots = new Dictionary<string, FishingSpot>();
        BuildAllSpots(spots);
        return spots;
    }

    /// <summary>钓点代表鱼种预览（UI 用，3 条）。</summary>
    public static IReadOnlyList<string> PreviewFish(string spotName) =>
        PreviewFishMap.TryGetValue(spotName, out var p) ? p : [];

    public static int FishSpeciesCount(string spotName) =>
        BuildAll().TryGetValue(spotName, out var s) ? s.FishTable.Count : 0;

    public static int TotalSpeciesCount
    {
        get
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var spot in BuildAll().Values)
                foreach (var f in spot.FishTable)
                    seen.Add(f.Name);
            return seen.Count;
        }
    }
}
