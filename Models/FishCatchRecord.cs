namespace CyberPetApp.Models;

/// <summary>鱼类图鉴：记录钓获次数与最大体重。</summary>
public class FishCatchRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    /// <summary>基础鱼种名（去掉「超规格·」前缀）。</summary>
    public string FishName { get; set; } = "";
    public int CatchCount { get; set; }
    public double MaxWeight { get; set; }
    public FishRarity BestRarity { get; set; } = FishRarity.Common;
    /// <summary>是否为指定饵专属神话鱼。</summary>
    public bool IsTargetExclusive { get; set; }
}
