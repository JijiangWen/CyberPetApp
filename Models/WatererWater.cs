namespace CyberPetApp.Models;

public class WatererWater
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AutoWatererId { get; set; }
    public string Name { get; set; } = "";
    public int ThirstRestore { get; set; }
    public int SlotIndex { get; set; }
}
