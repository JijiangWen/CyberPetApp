namespace CyberPetApp.Models;

/// <summary>猫咪状态对钓鱼的加成快照（每轮循环重新计算）。</summary>
public record CatFishingBuff(
    double RarityBonus,
    double WaitTimeMultiplier,
    double SuccessPenalty,
    double WaitTimePenalty,
    IReadOnlyList<string> StatusLines);

/// <summary>
/// 根据猫咪四维属性计算钓鱼 buff/debuff。
/// Happiness≥800 → 非 Common 稀有度权重 +5%；Energy≥800 → 等口 -8%；
/// Hunger&lt;300 或 Energy&lt;250 → 抓口/遛鱼各 -10%、等口 +15%；
/// 维护费拖欠 → 抓口/遛鱼再 -8%。
/// </summary>
public static class CatBuffHelper
{
    public static CatFishingBuff Compute(CyberCat cat, bool maintenanceOverdue = false, HouseBuffs houseBuffs = default)
    {
        double rarityBonus = 0;
        double waitMult = 1.0;
        double successPenalty = 0;
        double waitPenalty = 1.0;
        var lines = new List<string>();
        int happyThreshold = houseBuffs.FishingHappinessThreshold;

        if (cat.Happiness >= happyThreshold)
        {
            rarityBonus = 0.05;
            lines.Add(happyThreshold < 800
                ? $"开心≥{happyThreshold}（家具）：稀有鱼权重 +5%"
                : "开心≥800：稀有鱼权重 +5%");
        }

        if (cat.Energy >= 800)
        {
            waitMult = 0.92; // 等口时间 -8%
            lines.Add("精力≥800：等口时间 -8%");
        }

        if (cat.Hunger < 300 || cat.Energy < 250)
        {
            successPenalty = 0.10;
            waitPenalty = 1.15;
            lines.Add("饥饿/疲劳：抓口·遛鱼 -10%，等口 +15%");
        }

        if (maintenanceOverdue)
        {
            successPenalty += 0.08;
            lines.Add("维护费拖欠：抓口·遛鱼 -8%");
        }

        if (lines.Count == 0)
            lines.Add("无钓鱼加成/惩罚");

        return new CatFishingBuff(rarityBonus, waitMult, successPenalty, waitPenalty, lines);
    }
}
