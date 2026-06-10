using CyberPetApp.Models;

namespace CyberPetApp.Services;

/// <summary>挂机钓鱼状态机阶段。</summary>
public enum FishingState
{
    Idle,     // 未钓鱼
    Waiting,  // 等口：抛竿后等鱼咬钩
    Biting,   // 咬钩：抓口判定窗口
    Reeling   // 遛鱼：与大物拉锯
}

public class FishingLogEntry
{
    public DateTime Time { get; init; } = DateTime.Now;
    public string Text { get; init; } = "";
    /// <summary>UI 配色：info / good / bad / rare</summary>
    public string Kind { get; init; } = "info";
}

/// <summary>
/// 挂机钓鱼核心：三阶段状态机（Waiting → Biting → Reeling），异步循环驱动。
/// 成长性来自 FishingLoadout（装备数值 + 钓鱼等级）碾压鱼的精明度与爆发力。
/// </summary>
public class FishingManager : IDisposable
{
    private readonly Random _random = new();
    private CancellationTokenSource? _cts;

    public FishingState State { get; private set; } = FishingState.Idle;
    public bool IsFishing { get; private set; }
    public string StatusText { get; private set; } = "";
    public double PhaseTotalSeconds { get; private set; }
    public double PhaseRemainingSeconds { get; private set; }
    public FishingSpot? CurrentSpot { get; private set; }
    public Fish? LastCaughtFish { get; private set; }
    public FishingLoadout Loadout { get; private set; } = new();
    private Func<CatFishingBuff>? _getCatBuff;
    private Func<CatFishingStats>? _getCatStats;

    public List<FishingLogEntry> EventLog { get; } = [];

    public event Action? Changed;
    /// <summary>每轮钓鱼循环结束（成功/脱钩/切线）时触发，用于扣猫活动消耗。</summary>
    public Action? OnCycleComplete { get; set; }
    public Action<Fish>? OnFishCaught { get; set; }
    /// <summary>拟饵耐久耗尽或切线：页面持久化扣耐久/数量。</summary>
    public Action? OnLureConsumed { get; set; }
    /// <summary>特殊饵成功钓获后扣耐久。</summary>
    public Action? OnTargetLureConsumed { get; set; }
    public Action<bool>? OnGearWear { get; set; }

    public Dictionary<string, FishingSpot> FishingSpots { get; set; } = FishingSpotCatalog.BuildAll();

    /// <summary>开始挂机钓鱼；校验钓点等级与三件装备等级门槛。</summary>
    public void StartFishing(FishingSpot spot, FishingLoadout loadout,
        Func<CatFishingBuff>? getCatBuff = null, Func<CatFishingStats>? getCatStats = null)
    {
        if (IsFishing) return;
        if (loadout.FishingLevel < spot.RequiredLevel) return;
        if (!loadout.MeetsGearLevel) return;

        IsFishing = true;
        CurrentSpot = spot;
        loadout.SpotGearEffectiveness = GearProgressionCatalog.SpotGearEffectiveness(loadout.RodGearTier, spot.Name);
        Loadout = loadout;
        _getCatBuff = getCatBuff;
        _getCatStats = getCatStats;
        _cts = new CancellationTokenSource();
        Log($"开始在 [{spot.Name}] 挂机钓鱼", "info");
        _ = RunLoopAsync(spot, _cts.Token);
    }

    public void StopFishing()
    {
        if (!IsFishing) return;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsFishing = false;
        State = FishingState.Idle;
        CurrentSpot = null;
        _getCatBuff = null;
        _getCatStats = null;
        StatusText = "";
        Log("收竿，停止挂机", "info");
        Changed?.Invoke();
    }

    private async Task RunLoopAsync(FishingSpot spot, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var catBuff = _getCatBuff?.Invoke()
                    ?? new CatFishingBuff(0, 1, 0, 1, []);
                var catStats = _getCatStats?.Invoke()
                    ?? new CatFishingStats(1, 10, 10, 10, 10, 10, 10, 0, 0, 1, 1, 1, 0, 1, []);

                var (fish, template) = spot.RollCatch(new FishingRollContext(
                    Loadout.EffectiveRarityBonus + catBuff.RarityBonus + catStats.RarityBonus
                        + Loadout.GemBonuses.LuckBonus,
                    Loadout.TargetLureMatchesSpot(spot.Name) ? Loadout.ActiveTargetLureRecipeId : null,
                    spot.Name,
                    Loadout.LureGearTier,
                    Loadout.LureMythicBonus));

                // ── 阶段一：等口 ──
                // 等口 = base × (1-吸引-玩家Lv×0.3%-抛投×2%) × 猫AGI减免 × 状态乘算，下限 2s
                double baseWait = 5 + _random.NextDouble() * 20;
                double waitFactor = 1.0 - Loadout.EffectiveAttraction
                    - Loadout.FishingLevel * 0.003
                    - Loadout.CastRange * 0.02;
                waitFactor *= catStats.WaitMultiplier;
                double waitSeconds = Math.Max(2.0, baseWait * Math.Max(0.1, waitFactor));
                waitSeconds *= spot.FishingTime / 3.0;
                waitSeconds *= catBuff.WaitTimeMultiplier * catBuff.WaitTimePenalty;
                await RunPhaseAsync(FishingState.Waiting, waitSeconds, "正在抛竿... 观察水面中... 🌊", ct);

                // ── 阶段二：咬钩抓口 ──
                // 咬钩窗口 × 猫AGI延长
                double biteWindow = template.BiteWindowSeconds * catStats.BiteWindowMultiplier;
                await RunPhaseAsync(FishingState.Biting, biteWindow, "浮漂下沉！自动扬竿抓口中... ❗", ct);

                double depthBonus = Loadout.DepthMatchBonus(template, spot.PrimaryDepth);
                // 抓口 = 70% + 装备加算 + 猫属性加算 - 精明×40% - 状态惩罚（soft cap 5%~98%）
                double hookChance = Math.Clamp(
                    0.70 + Loadout.EffectiveSensitivity * 0.1 + Loadout.CastRange * 0.03
                    + Loadout.EffectiveLineSensitivity * 0.05
                    + depthBonus + Loadout.FishingLevel * 0.005
                    + Loadout.GemBonuses.HookBonus + Loadout.GemBonuses.LineBonus
                    + catStats.HookBonus
                    - template.Wariness * 0.40 * (1 - Loadout.LineStealth) - catBuff.SuccessPenalty,
                    0.05, 0.98);
                if (_random.NextDouble() >= hookChance)
                {
                    Log($"鱼儿脱钩溜走了... (抓口率 {hookChance:P0})", "bad");
                    OnGearWear?.Invoke(false);
                    CompleteCycle();
                    continue;
                }

                // ── 阶段三：遛鱼（Rare+） ──
                if (fish.Rarity != FishRarity.Common)
                {
                    double reelSeconds = Math.Max(1.5, (3 + _random.NextDouble() * 2) * (5.0 / Math.Max(1.0, Loadout.GearRatio)));
                    await RunPhaseAsync(FishingState.Reeling, reelSeconds, $"鱼线紧绷！与 {fish.Rarity} 级大物拉锯中... 🎣", ct);

                    // 超重：鱼重超过 min(竿钓重, 轮线杯, 线强度) -45%；否则按占比 -0~15%；猫STR减免
                    double weightLimit = Loadout.EffectiveWeightLimit;
                    double weightPenalty = fish.ActualWeight > weightLimit
                        ? 0.45
                        : fish.ActualWeight / Math.Max(0.1, weightLimit) * 0.15;
                    weightPenalty *= catStats.WeightPenaltyReduction;

                    // 起鱼 = 60% + 装备加算 + 猫属性加算 - 超重 - 爆发×20% - 状态惩罚（soft cap 5%~95%）
                    double landChance = Math.Clamp(
                        0.60 + Loadout.EffectiveDragPower * 0.05 + Loadout.EffectiveSmoothness * 0.08
                        + Loadout.FishingLevel * 0.005
                        + Loadout.GemBonuses.DragBonus
                        + catStats.LandBonus
                        - weightPenalty - template.Power * 0.20 - catBuff.SuccessPenalty,
                        0.05, 0.95);
                    if (_random.NextDouble() >= landChance)
                    {
                        Log($"大物发力！不幸切线/爆轮... 😭 ({fish.Name} {fish.ActualWeight}kg 跑了)", "bad");
                        ConsumeLineDurability();
                        ConsumeLureDurability();
                        OnGearWear?.Invoke(true);
                        CompleteCycle();
                        continue;
                    }
                }

                OnGearWear?.Invoke(false);
                LastCaughtFish = fish;
                if (Loadout.TargetLureMatchesSpot(spot.Name) && template.TargetLureRecipeId == Loadout.ActiveTargetLureRecipeId)
                    OnTargetLureConsumed?.Invoke();
                string logKind = template.TargetLureRecipeId is not null ? "legendary"
                    : fish.Rarity == FishRarity.Legendary || fish.SizePercentage > 100 ? "legendary"
                    : fish.Rarity >= FishRarity.Epic ? "rare" : "good";
                Log($"捕获 {fish.Name}！{fish.Rarity} · {fish.ActualWeight}kg · {fish.SellPrice}g", logKind);
                OnFishCaught?.Invoke(fish);
                CompleteCycle();
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            State = FishingState.Idle;
            StatusText = "";
            Changed?.Invoke();
        }
    }

    private async Task RunPhaseAsync(FishingState state, double seconds, string text, CancellationToken ct)
    {
        State = state;
        StatusText = text;
        PhaseTotalSeconds = seconds;
        PhaseRemainingSeconds = seconds;
        Changed?.Invoke();

        var end = DateTime.UtcNow.AddSeconds(seconds);
        while (DateTime.UtcNow < end)
        {
            await Task.Delay(250, ct);
            PhaseRemainingSeconds = Math.Max(0, (end - DateTime.UtcNow).TotalSeconds);
            Changed?.Invoke();
        }
    }

    /// <summary>切线：鱼线耐久按耐磨减免后扣除。</summary>
    private void ConsumeLineDurability()
    {
        int wear = Math.Max(1, (int)Math.Round(8 * (1 - Loadout.AbrasionResistance)));
        Loadout.LineDurability = Math.Max(0, Loadout.LineDurability - wear);
        if (Loadout.LineDurability < EconomySinks.DurabilityLowThreshold)
            Log($"鱼线 [{Loadout.LineName}] 耐久偏低，顺滑度下降", "bad");
    }

    /// <summary>切线：拟饵耐久 -1；归零则扣库存并通知页面。</summary>
    private void ConsumeLureDurability()
    {
        if (!Loadout.HasLure) return;
        Loadout.LureDurabilityRemaining--;
        if (Loadout.LureDurabilityRemaining <= 0)
        {
            Loadout.LureQuantity--;
            var spec = GearCatalog.FindLure(Loadout.LureName);
            if (Loadout.LureQuantity > 0 && spec is not null)
            {
                Loadout.LureDurabilityRemaining = spec.MaxDurability;
                Log($"拟饵 [{Loadout.LureName}] 一枚用尽，自动换上新的 (剩余×{Loadout.LureQuantity})", "info");
            }
            else
            {
                Loadout.LureDurabilityRemaining = 0;
                Log($"拟饵 [{Loadout.LureName}] 已耗尽！等口变慢、品质加成失效", "bad");
            }
        }
        OnLureConsumed?.Invoke();
    }

    private void CompleteCycle()
    {
        OnCycleComplete?.Invoke();
        Changed?.Invoke();
    }

    private void Log(string text, string kind)
    {
        lock (EventLog)
        {
            EventLog.Insert(0, new FishingLogEntry { Text = text, Kind = kind });
            if (EventLog.Count > 10) EventLog.RemoveAt(EventLog.Count - 1);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        IsFishing = false;
    }
}
