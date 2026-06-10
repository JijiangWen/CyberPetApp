namespace CyberPetApp.Models;

/// <summary>玩家炼制的特殊路亚饵（消耗耐久，仅指定钓点有效）。</summary>
public class PlayerTargetLure
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string RecipeId { get; set; } = "";
    public int RemainingUses { get; set; }
    public bool IsEquipped { get; set; }
}
