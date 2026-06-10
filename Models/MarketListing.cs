namespace CyberPetApp.Models;

/// <summary>鱼市挂单：鱼从背包移除后快照存于此，等待 NPC 出价。</summary>
public class MarketListing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string FishName { get; set; } = "";
    public FishRarity Rarity { get; set; }
    public double ActualWeight { get; set; }
    public double SizePercentage { get; set; }
    public int HungerRestore { get; set; }
    public int EnergyRestore { get; set; }
    public int HappinessRestore { get; set; }
    /// <summary>鱼的原 SellPrice（图鉴估价）。</summary>
    public int BaseSellPrice { get; set; }
    /// <summary>上架底价 = BaseSellPrice × 0.90。</summary>
    public int ListingFloorPrice { get; set; }
    public DateTime ListedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
