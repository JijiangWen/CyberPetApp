namespace CyberPetApp.Models;

public class Player
{
    public Guid Id { get; set; }
    public int Money { get; set; } = 100;

    // ── 多技能等级系统 ──
    public int FishingLevel { get; set; } = 1;
    public int FishingXp { get; set; }
    public int CookingLevel { get; set; } = 1;
    public int CookingXp { get; set; }

    public Dictionary<string, int> Backpack { get; set; } = [];
    public List<Fish> FishBackpack { get; set; } = [];
    public List<ShopItem> Items { get; set; } = [];
    public bool IsWorking { get; set; }
    public WorkJobType SelectedWorkJob { get; set; } = WorkJobType.Construction;

    /// <summary>上次房屋维护费扣款时间（UTC）。</summary>
    public DateTime? LastMaintenanceAt { get; set; }

    /// <summary>维护费拖欠：猫幸福 -50 且钓鱼 -8%，维护费×2。</summary>
    public bool MaintenanceOverdue { get; set; }

    /// <summary>上次活跃时间（UTC），用于离线 tick 补偿。</summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>打工摊位券进度 tick（持久化）。</summary>
    public int WorkTickCount { get; set; }

    /// <summary>每日高价求购任务日期（UTC 日期部分）。</summary>
    public DateTime? DailyBountyDate { get; set; }

    /// <summary>今日求购目标鱼种名。</summary>
    public string? DailyBountyFishName { get; set; }

    /// <summary>今日求购奖励金币。</summary>
    public int DailyBountyReward { get; set; }

    /// <summary>今日求购是否已领取。</summary>
    public bool DailyBountyClaimed { get; set; }

    /// <summary>今日手动刷新求购次数（首次免费，之后每次 50g）。</summary>
    public int DailyBountyManualRefreshCount { get; set; }

    /// <summary>已解锁工种位掩码（bit0=工地，bit1=猫咖，bit2=鱼市搬运）。</summary>
    public int UnlockedWorkJobMask { get; set; } = 1;

    /// <summary>里程碑成就点数（已领取奖励累计，用于商店兑换）。</summary>
    public int MilestonePoints { get; set; }

    /// <summary>累计手动喂食次数（成就统计）。</summary>
    public int LifetimeFeedCount { get; set; }

    /// <summary>累计卖鱼/市场成交金币（成就统计）。</summary>
    public int TotalFishGoldEarned { get; set; }

    /// <summary>市场成交次数。</summary>
    public int MarketSalesCount { get; set; }

    /// <summary>累计钓获 Rare 及以上。</summary>
    public int RareCatchCount { get; set; }

    /// <summary>累计钓获传说鱼。</summary>
    public int LegendaryCatchCount { get; set; }

    /// <summary>累计烹饪次数。</summary>
    public int CookCount { get; set; }

    /// <summary>进行中的派遣区域 Id（null = 无）。</summary>
    public string? ExpeditionZoneId { get; set; }

    /// <summary>派遣预计结束时间（UTC）。</summary>
    public DateTime? ExpeditionEndsAt { get; set; }

    public bool IsWorkJobUnlocked(WorkJobType job) =>
        (UnlockedWorkJobMask & (1 << (int)job)) != 0;

    public void UnlockWorkJob(WorkJobType job) =>
        UnlockedWorkJobMask |= 1 << (int)job;

    /// <summary>升级所需经验：Lv1~39 为 100×level^1.5；Lv40+ 指数放缓（500h 毕业曲线）。</summary>
    public static int XpToNext(int level)
    {
        double exp = level >= 40 ? 1.68 : 1.5;
        double xp = 100 * Math.Pow(level, exp);
        if (level >= 40)
            xp *= Math.Pow(1.07, level - 40);
        return Math.Max(100, (int)xp);
    }

    /// <summary>增加钓鱼经验，返回升级次数（可能连升）。</summary>
    public int AddFishingXp(int xp)
    {
        FishingXp += xp;
        int ups = 0;
        while (FishingXp >= XpToNext(FishingLevel))
        {
            FishingXp -= XpToNext(FishingLevel);
            FishingLevel++;
            ups++;
        }
        return ups;
    }

    /// <summary>增加烹饪经验，返回升级次数。</summary>
    public int AddCookingXp(int xp)
    {
        CookingXp += xp;
        int ups = 0;
        while (CookingXp >= XpToNext(CookingLevel))
        {
            CookingXp -= XpToNext(CookingLevel);
            CookingLevel++;
            ups++;
        }
        return ups;
    }

    public void BuyItem(ShopItem item)
    {
        if (Money < item.Price)
        {
            Console.WriteLine("金币不足！");
            return;
        }

        Money -= item.Price;

        string name = item.Food.Name;
        if (Backpack.TryGetValue(name, out int value))
            Backpack[name] = ++value;
        else
            Backpack[name] = 1;
    }
}
