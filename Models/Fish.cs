namespace CyberPetApp.Models;

public enum FishRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public class Fish
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public string Name { get; set; }
    public FishRarity Rarity { get; set; }
    public int HungerRestore { get; set; }
    public int EnergyRestore { get; set; }
    public int HappinessRestore { get; set; }
    public double ActualWeight { get; set; }
    public double SizePercentage { get; set; }
    public int SellPrice { get; set; }

    public Fish() => Name = "";

    public Fish(string name, int hungerRestore, int energyRestore, int happinessRestore, int sellPrice, FishRarity rarity, double actualWeight)
    {
        Id = Guid.NewGuid();
        Name = name;
        Rarity = rarity;
        HungerRestore = hungerRestore;
        EnergyRestore = energyRestore;
        HappinessRestore = happinessRestore;
        ActualWeight = actualWeight;
        SellPrice = sellPrice;
    }
}
