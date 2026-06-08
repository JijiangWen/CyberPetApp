namespace CyberPetApp.Models;

public enum FishRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public class Fish : Food
{
    // ✨ 将 Type 改名为 Rarity，避免和基类的 Type 属性冲突
    public FishRarity Rarity { get; set; }
    public double MinWeight { get; set; }
    public double MaxWeight { get; set; }
    public double ActualWeight { get; set; }
    public int SellPrice { get; set; }

    public double SizePercentage =>
        MaxWeight <= MinWeight ? 0 :
        Math.Round((ActualWeight - MinWeight) / (MaxWeight - MinWeight) * 100, 1);

    // 🔨 修正后的构造函数
    public Fish(string name, int hungerRestore, int energyRestore, int happinessRestore,
                int sellPrice, FishRarity rarity, double minWeight, double maxWeight, double actualWeight)
        : base(name, hungerRestore, energyRestore, happinessRestore)
    {
        Rarity = rarity;
        MinWeight = minWeight;
        MaxWeight = maxWeight;
        SellPrice = sellPrice;
        ActualWeight = actualWeight;
        // ✨ 记得在构造函数里把爷爷辈的 ItemType 指定为 Food
        Type = ItemType.Food;
    }
}