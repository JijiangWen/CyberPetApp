namespace CyberPetApp.Models;

public class AutoWaterer
{
    private readonly object _lock = new();

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public List<WaterItem> Waters { get; set; } = [];
    public int WaterCount
    {
        get
        {
            lock (_lock)
            {
                return Waters.Count;
            }
        }
    }
    public int MaxWaterCount { get; set; } = 10;

    public List<WaterItem> GetWatersSnapshot()
    {
        lock (_lock)
        {
            return new List<WaterItem>(Waters);
        }
    }

    public bool AddWater(WaterItem water)
    {
        lock (_lock)
        {
            if (Waters.Count >= MaxWaterCount) return false;
            Waters.Add(water);
            return true;
        }
    }

    public WaterItem? RemoveLastWater()
    {
        lock (_lock)
        {
            if (Waters.Count <= 0) return null;
            var last = Waters[^1];
            Waters.RemoveAt(Waters.Count - 1);
            return last;
        }
    }

    /// <summary>口渴&lt;500 时自动补水：优先槽内凉白开水，否则免费自来水 +12。</summary>
    public bool CheckAndWater(CyberCat cat, bool dispenserUnlocked)
    {
        if (!dispenserUnlocked) return false;

        lock (_lock)
        {
            if (cat.Thirst >= 500) return false;

            if (Waters.Count > 0)
            {
                var water = Waters[0];
                cat.DrinkWater(water.ThirstRestore);
                Waters.RemoveAt(0);
                return true;
            }
        }

        cat.DrinkWater(WaterCatalog.Tap.ThirstRestore);
        return false;
    }
}
