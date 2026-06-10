namespace CyberPetApp.Models;



/// <summary>家具被动养猫：每 3s（或自定义间隔）恢复四维，仅低于阈值时生效。</summary>

public static class PassiveCatCare

{

    public const int DefaultIntervalMs = 3000;



    public record PassiveDef(

        int Happiness,

        int Energy,

        int IntervalMs = DefaultIntervalMs,

        int HappinessBelow = 1000,

        int EnergyBelow = 1000,

        string ActiveLabel = "");



    private static readonly Dictionary<string, PassiveDef> Map = new(StringComparer.Ordinal)

    {

        ["CatToy"] = new(8, 3, DefaultIntervalMs, 700, 1000, "逗猫棒 +8♥/3s +3⚡/3s（♥&lt;700）"),

        ["JoyPad"] = new(10, 0, DefaultIntervalMs, 700, 1000, "爬架 +10♥/3s（♥&lt;700）"),

        ["CozyBed"] = new(0, 8, DefaultIntervalMs, 1000, 800, "猫窝 +8⚡/3s（⚡&lt;800）"),

        ["AromaDiffuser"] = new(12, 0, DefaultIntervalMs, 1000, 1000, "香薰 +12♥/3s"),

        ["FishTank"] = new(6, 0, DefaultIntervalMs, 1000, 1000, "鱼缸 +6♥/3s"),

        ["SunLamp"] = new(4, 6, DefaultIntervalMs, 1000, 1000, "日照灯 +4♥ +6⚡/3s"),

        ["LuxuryTower"] = new(15, 10, DefaultIntervalMs, 1000, 1000, "豪华爬架 +15♥ +10⚡/3s"),

    };



    /// <summary>解锁自动饮水器的家具 ID（兼容旧档 WaterDispenser）。</summary>

    public static readonly HashSet<string> AutoWatererFurnitureIds = new(StringComparer.Ordinal)

    {

        "WaterDispenser", "WaterFountain"

    };



    public static bool TryGet(string furnitureId, out PassiveDef def) =>

        Map.TryGetValue(furnitureId, out def!);



    public static bool UnlocksAutoWaterer(string furnitureId) =>

        AutoWatererFurnitureIds.Contains(furnitureId);



    public static IEnumerable<string> HintLines(IEnumerable<string> unlockedFurnitureIds)

    {

        foreach (var id in unlockedFurnitureIds)

        {

            if (TryGet(id, out var def))

                yield return $"passive: {def.ActiveLabel}";

            else if (UnlocksAutoWaterer(id))

                yield return "passive: 饮水泉 · 口渴&lt;500 自动补水";

        }

    }



    /// <summary>侧边栏 passive.care 汇总行。</summary>

    public static IReadOnlyList<string> SummarizeRates(IEnumerable<string> unlockedFurnitureIds)

    {

        int happy = 0, energy = 0;

        var lines = new List<string>();

        foreach (var id in unlockedFurnitureIds)

        {

            if (TryGet(id, out var def))

            {

                happy += def.Happiness;

                energy += def.Energy;

                lines.Add(def.ActiveLabel);

            }

            else if (UnlocksAutoWaterer(id))

                lines.Add("饮水泉 · 自动补水");

        }



        if (lines.Count == 0)

            return ["暂无被动恢复（购买逗猫棒/猫窝等家具）"];



        if (happy > 0 || energy > 0)

            lines.Insert(0, $"合计约 +{happy}♥ +{energy}⚡ /3s（阈值内）");

        return lines;

    }



    /// <summary>累加 intervalMs，满 3s 则应用已解锁家具的被动恢复。</summary>

    public static void Tick(CyberCat cat, IEnumerable<string> unlockedFurnitureIds, int intervalMs, ref int accumulatedMs)

    {

        accumulatedMs += intervalMs;

        if (accumulatedMs < DefaultIntervalMs) return;

        accumulatedMs -= DefaultIntervalMs;



        foreach (var id in unlockedFurnitureIds)

        {

            if (!TryGet(id, out var def)) continue;



            if (def.Happiness > 0 && cat.Happiness < def.HappinessBelow)

                cat.Happiness = Math.Min(CyberCat.StatMax, cat.Happiness + def.Happiness);

            if (def.Energy > 0 && cat.Energy < def.EnergyBelow)

                cat.Energy = Math.Min(CyberCat.StatMax, cat.Energy + def.Energy);

        }

    }

}


