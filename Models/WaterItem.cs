namespace CyberPetApp.Models;

public record WaterItem(string Name, int ThirstRestore);

public static class WaterCatalog
{
    public static readonly WaterItem Purified = new("凉白开水", 20);
    public static readonly WaterItem Tap = new("自来水", 12);

    public static WaterItem? ByName(string name) =>
        name == Purified.Name ? Purified : null;
}
