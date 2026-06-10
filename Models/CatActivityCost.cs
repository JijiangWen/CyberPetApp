namespace CyberPetApp.Models;

/// <summary>触发猫咪属性消耗的游戏活动类型。</summary>
public enum CatActivityType
{
    /// <summary>完成一竿钓鱼（成功/脱钩/切线均算一轮）。</summary>
    FishingCycle,
    /// <summary>打工 tick（每 2s 一次）。</summary>
    WorkTick,
    /// <summary>派遣出发（不含区域 Energy，Energy 按 ExpeditionZone 单独扣）。</summary>
    ExpeditionStart,
    /// <summary>派遣归来领取奖励。</summary>
    ExpeditionReturn,
    /// <summary>烹饪 1 条鱼。</summary>
    Cooking,
    /// <summary>鱼市上架一条鱼。</summary>
    MarketList,
    /// <summary>对 NPC 报价还价一次。</summary>
    MarketCounter,
    /// <summary>鱼背包直售（快速变现，轻量消耗）。</summary>
    DirectFishSell,
}

/// <summary>
/// 活动驱动属性消耗表（满值 1000）。
/// 设计意图：纯钓鱼约 3~5 轮/分钟 → Energy -30~50/min，20+ 分钟才精力见底；
/// 打工 tick 仅扣精力/幸福/口渴（不扣饥饿），与钓鱼并行约 15+ 分钟才需明显喂食；
/// AutoFeeder 阈值 500 可维持长期挂机。
/// </summary>
public static class CatActivityCost
{
    public readonly record struct Delta(int Hunger, int Energy, int Happiness, int Thirst, int Health);

    public static Delta Get(CatActivityType type) => type switch
    {
        // 一竿完整循环：主要耗精力，轻度饥饿/口渴
        CatActivityType.FishingCycle => new(-7, -10, +3, -5, 0),
        // 打工 tick（2s）：高频但轻量；饥饿由钓鱼/派遣承担，避免并行时 5 分钟饿扁
        CatActivityType.WorkTick => new(0, -2, -1, -1, 0),
        // 派遣出发：Energy 由区域定义，此处仅扣其余三维
        CatActivityType.ExpeditionStart => new(-15, 0, -5, -10, 0),
        // 归来：长途跋涉后饥饿口渴明显
        CatActivityType.ExpeditionReturn => new(-80, 0, 0, -40, 0),
        // 烹饪：略耗体，做饭本身略增幸福
        CatActivityType.Cooking => new(-5, -8, +2, -3, 0),
        // 市场操作：轻量消耗，避免无代价刷报价
        CatActivityType.MarketList => new(-2, -3, 0, -2, 0),
        CatActivityType.MarketCounter => new(-2, -3, 0, -2, 0),
        CatActivityType.DirectFishSell => new(-1, -2, 0, -1, 0),
        _ => default
    };

    /// <summary>UI 提示用：各活动消耗摘要。</summary>
    public static string ActivityCostHint
    {
        get
        {
            var fish = Get(CatActivityType.FishingCycle);
            var work = Get(CatActivityType.WorkTick);
            return $"钓鱼 {fish.Energy}⚡{fish.Hunger}饱快乐+{fish.Happiness}/轮 · 打工 {work.Energy}⚡/tick · 背景≈10min各-1";
        }
    }
}
