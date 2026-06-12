namespace CyberPetApp.Models;

public class FishTemplate
{
    public string Name { get; set; }
    public int HungerRestore { get; set; }
    public int EnergyRestore { get; set; }
    public int HappinessRestore { get; set; }
    public FishRarity Rarity { get; set; }
    /// <summary>同稀有度档位内的出现权重，越大越容易钓到。</summary>
    public int SpawnWeight { get; set; }
    public double MinWeight { get; set; }
    public double MaxWeight { get; set; }
    public string Description { get; set; } = "";

    /// <summary>精明度 0~1：越高咬钩窗口越短、越容易脱钩（降低抓口成功率）。</summary>
    public double Wariness { get; set; }

    /// <summary>爆发力 0~1：越高遛鱼阶段越容易切线/爆轮（降低起鱼成功率）。</summary>
    public double Power { get; set; }

    /// <summary>偏好水层：与拟饵 TargetDepth 匹配时抓口 +5%。</summary>
    public WaterDepth PreferredDepth { get; set; } = WaterDepth.Middle;

    /// <summary>若非空，仅装备对应特殊饵且钓点匹配时进入鱼池（神话鱼）。</summary>
    public string? TargetLureRecipeId { get; set; }

    /// <summary>咬钩窗口秒数：笨鱼(0精明度) 3.0s → 精明鱼(1精明度) 1.5s。</summary>
    public double BiteWindowSeconds => 3.0 - 1.5 * Wariness;

    public FishTemplate(string name, int hungerRestore, int energyRestore, int happinessRestore,
        FishRarity rarity, double minWeight, double maxWeight, int spawnWeight = 100,
        double wariness = -1, double power = -1)
    {
        Name = name;
        HungerRestore = hungerRestore;
        EnergyRestore = energyRestore;
        HappinessRestore = happinessRestore;
        Rarity = rarity;
        SpawnWeight = spawnWeight;
        MinWeight = minWeight;
        MaxWeight = maxWeight;

        // 未显式指定时，按稀有度给默认值（稀有度越高越精明、越能挣扎）
        Wariness = wariness >= 0 ? wariness : rarity switch
        {
            FishRarity.Legendary => 0.75,
            FishRarity.Epic => 0.55,
            FishRarity.Rare => 0.35,
            _ => 0.10
        };
        Power = power >= 0 ? power : rarity switch
        {
            FishRarity.Legendary => 0.80,
            FishRarity.Epic => 0.55,
            FishRarity.Rare => 0.30,
            _ => 0.10
        };
    }
}
