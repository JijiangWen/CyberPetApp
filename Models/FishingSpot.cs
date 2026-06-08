namespace CyberPetApp.Models;

public class FishingSpot
{
    public string Name { get; set; }
    public int RequiredLevel { get; set; }
    public int FishingTime { get; set; }

    public List<Fish> FishTable { get; set; } = [];

    public Dictionary<FishRarity, int> FishRarityTable { get; set; } = new()
    {
        {FishRarity.Common, 70},
        {FishRarity.Rare, 20},
        {FishRarity.Epic, 8},
        {FishRarity.Legendary, 2}
    };

    private readonly Random _random = new();

    public FishingSpot(string name) => Name = name;

    public Fish FishRoll()
    {
        if (FishTable.Count == 0) throw new Exception("鱼塘里没有鱼！");
        //random fish
        Fish baseFish = FishTable[_random.Next(FishTable.Count)];
        //random fish weight
        double actualWeight = RollFishWeight(baseFish);
        double sizePercentage = (actualWeight - baseFish.MinWeight) / (baseFish.MaxWeight - baseFish.MinWeight);
        //rarity is according to the size percentage
        FishRarity finalRarity;
        if (sizePercentage >= 0.98)      finalRarity = FishRarity.Legendary; // 前 2% 的体型是传说
        else if (sizePercentage >= 0.92) finalRarity = FishRarity.Epic;      // 前 8% 的体型是史诗
        else if (sizePercentage >= 0.70) finalRarity = FishRarity.Rare;      // 前 30% 的体型是稀有
        else                             finalRarity = FishRarity.Common;    // 剩下的都是普通

        int sellPrice = (int)(actualWeight * 10);

        Fish caughtFish = new Fish(
        baseFish.Name,
        baseFish.HungerRestore,
        baseFish.EnergyRestore,
        baseFish.HappinessRestore,
        sellPrice,
        finalRarity, // 塞入刚刚判定好的稀有度
        baseFish.MinWeight,
        baseFish.MaxWeight,
        actualWeight // 塞入刚刚锁死不变量的重量
    );

        return caughtFish;
    }

    private double RollFishWeight(Fish fish)
    {
        double randomWeight = fish.MinWeight + (_random.NextDouble() * (fish.MaxWeight - fish.MinWeight));
        return Math.Round(randomWeight, 1);
    }
}
