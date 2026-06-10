namespace CyberPetApp.Models;

/// <summary>玩家拥有的镶嵌宝石（未镶嵌在背包，已镶嵌绑定装备槽）。</summary>
public class PlayerGem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public GemType GemType { get; set; }
    /// <summary>加成比例 0.03~0.08</summary>
    public double BonusValue { get; set; }
    public bool IsSocketed { get; set; }
    public GearGemSlot? SocketedSlot { get; set; }
    /// <summary>镶嵌到的装备 Id（竿/轮/饵）。</summary>
    public Guid? SocketedGearId { get; set; }
}
