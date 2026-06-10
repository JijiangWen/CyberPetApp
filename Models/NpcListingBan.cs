namespace CyberPetApp.Models;

/// <summary>还价失败后，某 NPC 类型在指定挂单上的 24h 出价禁令。</summary>
public class NpcListingBan
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public NpcBuyerType BuyerType { get; set; }
    public DateTime BannedUntil { get; set; }
}
