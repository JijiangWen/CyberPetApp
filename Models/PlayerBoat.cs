using System;

namespace CyberPetApp.Models;

public class PlayerBoat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>所有者玩家 ID</summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>小船类型（例如: wood_boat, cyber_yacht）</summary>
    public string BoatType { get; set; } = "wood_boat";
    
    /// <summary>小船自定义名称</summary>
    public string CustomName { get; set; } = "快乐木舢板";
    
    /// <summary>最大乘员数量</summary>
    public int MaxCapacity { get; set; } = 4;
    
    /// <summary>购买金币价格</summary>
    public int PurchasePrice { get; set; }
    
    /// <summary>购买时间</summary>
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
