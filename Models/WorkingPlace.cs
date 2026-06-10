namespace CyberPetApp.Models;

public class WorkingPlace
{
    public WorkJobType Job { get; set; } = WorkJobType.Construction;

    /// <summary>打工 tick 计数（2s/tick），满阈值发 1 张摊位券。</summary>
    public int WorkTickCount { get; set; }

    public const int TicksPerStallTicketDefault = 150;

    /// <summary>当前工种每 tick 金币产出（QA：工地 1→0.7）。</summary>
    public double GoldPerTick => Job switch
    {
        WorkJobType.Construction => 0.7,
        _ => 0
    };

    /// <summary>当前工种发摊位券所需 tick 数（鱼市搬运更快）。</summary>
    public int TicksPerStallTicket => Job switch
    {
        WorkJobType.Construction => 150,
        WorkJobType.CatCafe => 180,
        WorkJobType.FishMarketPorter => 120,
        _ => TicksPerStallTicketDefault
    };

    /// <summary>猫咖兼职：每 tick 给猫增加的幸福值。</summary>
    public int HappinessPerTick => Job == WorkJobType.CatCafe ? 3 : 0;

    public string Name => Job switch
    {
        WorkJobType.Construction => "工地",
        WorkJobType.CatCafe => "猫咖兼职",
        WorkJobType.FishMarketPorter => "鱼市搬运",
        _ => "工地"
    };

    public static string JobLabel(WorkJobType job) => job switch
    {
        WorkJobType.Construction => "工地",
        WorkJobType.CatCafe => "猫咖兼职",
        WorkJobType.FishMarketPorter => "鱼市搬运",
        _ => "工地"
    };

    /// <summary>是否满足切换工种条件（属性门槛）。</summary>
    public static bool CanSelectJob(WorkJobType job, Player player, CyberCat cat) => job switch
    {
        WorkJobType.Construction => true,
        WorkJobType.CatCafe => cat.Happiness >= 600,
        WorkJobType.FishMarketPorter => player.FishingLevel >= 3,
        _ => false
    };

    /// <summary>切换前是否需支付培训费（未解锁且非默认工种）。</summary>
    public static bool NeedsUnlockFee(WorkJobType job, Player player) =>
        job != WorkJobType.Construction && !player.IsWorkJobUnlocked(job);

    public static string JobLockReason(WorkJobType job) => job switch
    {
        WorkJobType.CatCafe => "需猫幸福 ≥600",
        WorkJobType.FishMarketPorter => "需钓鱼 Lv.3",
        _ => ""
    };

    public void ToggleWork(Player player) => player.IsWorking = !player.IsWorking;

    /// <summary>打工一次 tick：按工种加金币/幸福，累计满阈值产出摊位券。</summary>
    public bool Tick(Player player, CyberCat cat, out bool earnedStallTicket, HouseBuffs houseBuffs = default)
    {
        earnedStallTicket = false;
        if (!player.IsWorking) return false;

        int gold = Math.Max(0, (int)Math.Round(GoldPerTick * houseBuffs.WorkGoldMultiplier));
        player.Money += gold;
        cat.ApplyActivityCost(CatActivityType.WorkTick, houseBuffs);
        if (HappinessPerTick > 0)
            cat.Happiness = Math.Min(CyberCat.StatMax, cat.Happiness + HappinessPerTick);

        WorkTickCount += Math.Max(1, (int)Math.Round(1 * houseBuffs.StallTicketProgressMultiplier));
        int effectiveTicks = EffectiveTicksPerStallTicket(houseBuffs);
        if (WorkTickCount < effectiveTicks) return true;

        WorkTickCount -= effectiveTicks;
        earnedStallTicket = true;
        return true;
    }

    private int EffectiveTicksPerStallTicket(HouseBuffs houseBuffs)
    {
        if (houseBuffs.StallTicketProgressMultiplier <= 1.0)
            return TicksPerStallTicket;
        return Math.Max(60, (int)Math.Round(TicksPerStallTicket / houseBuffs.StallTicketProgressMultiplier));
    }

    public int EffectiveTicksPerStallTicketForDisplay(HouseBuffs houseBuffs) =>
        EffectiveTicksPerStallTicket(houseBuffs);

    public int StallTicketProgressPercent() =>
        TicksPerStallTicket <= 0 ? 0 : (int)Math.Round(100.0 * WorkTickCount / TicksPerStallTicket);
}
