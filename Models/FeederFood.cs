namespace CyberPetApp.Models;

public class FeederFood
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AutoFeederId { get; set; }
    public string Name { get; set; } = "";
    public int HungerRestore { get; set; }
    public int EnergyRestore { get; set; }
    public int HappinessRestore { get; set; }
    public int SlotIndex { get; set; }
}
