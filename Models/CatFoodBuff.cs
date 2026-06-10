namespace CyberPetApp.Models;

/// <summary>烹饪食物附带的持续 Buff 类型。</summary>
public enum CatFoodBuffType
{
    /// <summary>每 TickIntervalMinutes 增加 Value 点饱腹（上限 StatMax）。</summary>
    HungerRegenOverTime,
    /// <summary>每 TickIntervalMinutes 增加 Value 点精力。</summary>
    EnergyRegenOverTime,
    /// <summary>每 TickIntervalMinutes 增加 Value 点心情。</summary>
    HappinessRegenOverTime,
    /// <summary>钓鱼精力消耗乘算因子（0.9 = -10%）。</summary>
    FishingEnergyDiscount,
    /// <summary>抓口率加算（与 CatFishingStats.HookBonus 叠加）。</summary>
    HookBonus,
    /// <summary>非 Common 稀有度权重加算。</summary>
    RareWeightBonus,
}

/// <summary>食谱静态 Buff 定义（写入 CookBook）。</summary>
public record CatFoodBuffDef(
    CatFoodBuffType BuffType,
    double Value,
    int DurationMinutes,
    int TickIntervalMinutes = 10)
{
    public int TotalTicks => TickIntervalMinutes > 0
        ? Math.Max(1, DurationMinutes / TickIntervalMinutes)
        : 0;

    public string ShortLabel => BuffType switch
    {
        CatFoodBuffType.HungerRegenOverTime => $"饱腹+{(int)Value}/{TickIntervalMinutes}min ×{DurationMinutes}min",
        CatFoodBuffType.EnergyRegenOverTime => $"精力+{(int)Value}/{TickIntervalMinutes}min ×{DurationMinutes}min",
        CatFoodBuffType.HappinessRegenOverTime => $"心情+{(int)Value}/{TickIntervalMinutes}min ×{DurationMinutes}min",
        CatFoodBuffType.FishingEnergyDiscount => $"钓鱼精力 -{(int)((1 - Value) * 100)}% ×{DurationMinutes}min",
        CatFoodBuffType.HookBonus => $"抓口 +{Value:P0} ×{DurationMinutes}min",
        CatFoodBuffType.RareWeightBonus => $"Rare+ 权重 +{Value:P0} ×{DurationMinutes}min",
        _ => ""
    };
}

/// <summary>持久化：玩家当前生效的食物 Buff（PlayerCatBuffs 表）。</summary>
public class PlayerCatBuff
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public CatFoodBuffType BuffType { get; set; }
    public double Value { get; set; }
    public int TickIntervalMinutes { get; set; }
    public int RemainingTicks { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime NextTickAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public string SourceFoodName { get; set; } = "";
}

/// <summary>运行时 Buff 快照（UI / 钓鱼公式）。</summary>
public record ActiveCatBuff(
    CatFoodBuffType BuffType,
    double Value,
    DateTime ExpiresAt,
    int RemainingTicks,
    int TickIntervalMinutes,
    string SourceFoodName,
    string DisplayLabel);

/// <summary>食物 Buff 对钓鱼的汇总修正。</summary>
public record FoodBuffSnapshot(
    double HookBonus,
    double RarityBonus,
    double FishingEnergyDiscount,
    IReadOnlyList<ActiveCatBuff> ActiveBuffs,
    IReadOnlyList<string> StatusLines)
{
    public static FoodBuffSnapshot Empty { get; } = new(0, 0, 1.0, [], []);
}
