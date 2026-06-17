using System;

namespace CyberPetApp.Models;

/// <summary>
/// 专属船钓鱼货历史记录，供点开玩家资料时加载展示。
/// </summary>
public class BoatCatchRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>钓鱼的玩家 ID</summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>钓鱼当时的玩家名字</summary>
    public string PlayerName { get; set; } = "";
    
    /// <summary>当时所在的小船名称</summary>
    public string BoatName { get; set; } = "";
    
    /// <summary>鱼类名称</summary>
    public string FishName { get; set; } = "";
    
    /// <summary>稀有度</summary>
    public FishRarity Rarity { get; set; } = FishRarity.Common;
    
    /// <summary>体重 (kg)</summary>
    public double Weight { get; set; }
    
    /// <summary>尺寸百分比 (%)</summary>
    public double SizePercentage { get; set; }
    
    /// <summary>捕获时间 (UTC)</summary>
    public DateTime CaughtAt { get; set; } = DateTime.UtcNow;
}
