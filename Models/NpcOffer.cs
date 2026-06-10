namespace CyberPetApp.Models;

public enum NpcBuyerType
{
    /// <summary>饕客：Epic+ 溢价收购。</summary>
    Gourmet,
    /// <summary>流浪猫：Common 批量低价。</summary>
    StrayCat,
    /// <summary>收藏家：Legendary / 超规格高溢价。</summary>
    Collector,
    /// <summary>厨师猫：偏好 Rare/Epic 食材鱼。</summary>
    ChefCat,
    /// <summary>心情 NPC：收购价随卖家猫心情浮动。</summary>
    MoodNpc
}

/// <summary>NPC 自动出价，30 分钟过期，玩家一键接受成交。</summary>
public class NpcOffer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ListingId { get; set; }
    public Guid PlayerId { get; set; }
    public NpcBuyerType BuyerType { get; set; }
    public int OfferPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
}
