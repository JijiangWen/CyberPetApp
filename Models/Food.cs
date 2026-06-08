namespace CyberPetApp.Models;

public class Food : Item
{
    public int HungerRestore { get; set; } = 10;
    public int EnergyRestore { get; set; } = 10;
    public int HappinessRestore { get; set; } = 10;

    public Food(string name, int hungerRestore, int energyRestore, int happinessRestore)
        : base(ItemType.Food, name, 0, $"回复饱腹感 {hungerRestore}")
    {
        HungerRestore = hungerRestore;
        EnergyRestore = energyRestore;
        HappinessRestore = happinessRestore;
    }
}
