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
        double reduction = loadout.EffectiveAttraction * 0.5
            + loadout.FishingLevel * 0.003
            + loadout.CastRange * 0.02
            + Math.Max(0, loadout.RodGearTier - 1) * 0.015
            + Math.Max(0, loadout.LureGearTier - 1) * 0.01;
        reduction = Math.Min(MaxGearReduction, reduction);

        double factor = 1.0 - reduction;
        factor *= catStats.WaitMultiplier;
        factor *= spot.FishingTime / 3.0;
        factor *= catBuff.WaitTimeMultiplier * catBuff.WaitTimePenalty;

        return Math.Max(MinWaitSeconds, BaseWaitSeconds * factor);
    }

    /// <summary>单次观察结束后，是否咬钩的概率（0~1）。</summary>
    public static double ComputeBiteChance(
        FishingLoadout loadout,
        CatFishingStats catStats,
        CatFishingBuff catBuff,
        FishingSpot spot)
    {
        double chance = BaseBiteChance
            + loadout.EffectiveAttraction
            + loadout.FishingLevel * 0.002
            + loadout.CastRange * 0.012
            + SpotDepthBiteBonus(loadout, spot)
            + catStats.RarityBonus * 0.5;

        chance *= 1.0 - catBuff.SuccessPenalty;

        if (!loadout.HasLure)
            chance *= 0.65;

        return Math.Clamp(chance, MinBiteChance, MaxBiteChance);
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
