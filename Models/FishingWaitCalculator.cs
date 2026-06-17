namespace CyberPetApp.Models;

/// <summary>
/// 钓鱼等口判定：每轮间隔基础 15 秒观察水面，再掷概率是否咬钩；咬中后才进入抓口/起鱼。
/// 间隔可由装备/饵缩短；咬钩率由饵吸引、装备与猫状态提升。
/// </summary>
public static class FishingWaitCalculator
{
    /// <summary>单次观察间隔基准（秒）。</summary>
    public const double BaseWaitSeconds = 15.0;

    /// <summary>观察间隔下限（秒）。</summary>
    public const double MinWaitSeconds = 10.0;

    /// <summary>装备+饵可提供的最大间隔缩短比例。</summary>
    public const double MaxGearReduction = 0.55;

    /// <summary>裸装单次观察咬钩基准概率（非必定上钩）。</summary>
    public const double BaseBiteChance = 0.12;

    public const double MinBiteChance = 0.06;
    public const double MaxBiteChance = 0.55;

    public static double ComputeWaitSeconds(
        FishingLoadout loadout,
        CatFishingStats catStats,
        CatFishingBuff catBuff,
        FishingSpot spot)
    {
        // 所有地图的平均期望时间完全一致（统一 5 分钟）
        double baseWait = 300.0;

        // 装备与饵缩减加成
        double reduction = loadout.EffectiveAttraction * 0.5
            + loadout.FishingLevel * 0.003
            + loadout.CastRange * 0.02
            + Math.Max(0, loadout.RodGearTier - 1) * 0.015
            + Math.Max(0, loadout.LureGearTier - 1) * 0.01;
        reduction = Math.Min(MaxGearReduction, reduction);

        double factor = 1.0 - reduction;
        factor *= catStats.WaitMultiplier;
        factor *= catBuff.WaitTimeMultiplier * catBuff.WaitTimePenalty;

        double expectedWait = baseWait * factor;

        // 根据地图等级，决定“命运扭曲系数”
        // 等级越高，power 越小，使得 ln(u) 的两极分化越严重
        double levelFactor = (Math.Clamp(spot.RequiredLevel, 1, 60) - 1) / 59.0; // 映射到 0.0 ~ 1.0
        double power = Math.Clamp(1.5 - levelFactor, 0.5, 1.5); 

        // 扭曲随机数
        var rand = new Random();
        double u = 1.0 - rand.NextDouble(); // 随机区间 (0, 1]
        double distortedU = Math.Pow(u, power);

        // 带入泊松指数公式
        double exponentialRandom = -Math.Log(distortedU);
        double finalWait = expectedWait * exponentialRandom;

        // 终极限幅（高级图的上限允许更长，但下限允许极短）
        double minWait = 120.0 - (levelFactor * 90.0); // 1级图最快2分钟，60级图最快30秒！
        double maxWait = 900.0 + (levelFactor * 600.0); // 1级图最慢15分钟，60级图最慢25分钟

        return Math.Clamp(finalWait, minWait, maxWait);
    }

    /// <summary>单次观察结束后，是否咬钩的概率（0~1）。目前为 100% 必定咬钩，由单次大等待期完全替代频繁小检查循环。</summary>
    public static double ComputeBiteChance(
        FishingLoadout loadout,
        CatFishingStats catStats,
        CatFishingBuff catBuff,
        FishingSpot spot)
    {
        return 1.0;
    }

    private static double SpotDepthBiteBonus(FishingLoadout loadout, FishingSpot spot)
    {
        double bonus = 0;
        if (loadout.HasLure && loadout.LureDepth == spot.PrimaryDepth)
            bonus += 0.04;
        if (loadout.LineDepth == spot.PrimaryDepth)
            bonus += 0.03;
        return bonus;
    }
}
