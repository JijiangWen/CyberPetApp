using CyberPetApp.Models;

namespace CyberPetApp.Services;

/// <summary>2s 心跳渲染比较快照（P1-2 条件刷新）。</summary>
public readonly record struct TickRenderSnapshot(
    int Hunger, int Thirst, int Energy, int Happiness, int Health,
    int CatXp, int CatLevel, int PlayerMoney, int FeederFoodCount,
    bool IsFishing, bool IsWorking);

/// <summary>单次 2s tick 模拟结果；UI 与存档副作用由 Home 根据此结果调度。</summary>
public sealed class TickResult
{
    public bool IsDirty { get; init; }
    public TickRenderSnapshot Snapshot { get; init; }
    public bool EarnedTicket { get; init; }
    public bool FeederFoodChanged { get; init; }
    public bool ConsumedWater { get; init; }
    public bool RunMaintenance { get; init; }
    public bool RunMarketNpcOffers { get; init; }
}

/// <summary>跨 tick 累加计数器（由 Home 持有）。</summary>
public sealed class GameTickCounters
{
    public int PassiveCareAccumulatedMs;
    public int MaintenanceTickCounter;
    public int MarketTickCounter;
    public TickRenderSnapshot LastTickRenderSnapshot;
}

/// <summary>单次 tick 只读输入。</summary>
public sealed class GameTickInput
{
    public required CyberCat Cat { get; init; }
    public required Player Player { get; init; }
    public required WorkingPlace WorkingPlace { get; init; }
    public required AutoFeeder Feeder { get; init; }
    public required AutoWaterer Waterer { get; init; }
    public required HouseBuffs HouseBuffs { get; init; }
    public required IEnumerable<string> UnlockedFurnitureIds { get; init; }
    public required bool HasWaterDispenser { get; init; }
    public required bool IsFishing { get; init; }
    public required int MarketOfferInterval { get; init; }
    public required int MaintenanceTicksPerDay { get; init; }
}

/// <summary>
/// P2-2 轻量版：封装 2s 游戏模拟逻辑，与 Blazor UI / DB 解耦。
/// Home 的 Timer 仅调用 <see cref="RunTick"/> 并按 <see cref="TickResult"/> 决定是否刷新。
/// </summary>
public class GameTickOrchestrator
{
    public const int TickIntervalMs = 2000;

    public TickResult RunTick(GameTickInput input, GameTickCounters counters, object catStateLock)
    {
        bool earnedTicket;
        int foodBefore;
        bool consumedWater;

        lock (catStateLock)
        {
            input.Cat.Tick(input.HouseBuffs);
            earnedTicket = false;
            input.WorkingPlace.Tick(input.Player, input.Cat, out earnedTicket, input.HouseBuffs);
            foodBefore = input.Feeder.FoodCount;
            input.Feeder.CheckAndFeed(input.Cat);
            consumedWater = input.Waterer.CheckAndWater(input.Cat, input.HasWaterDispenser);
            PassiveCatCare.Tick(
                input.Cat, input.UnlockedFurnitureIds, TickIntervalMs, ref counters.PassiveCareAccumulatedMs);
        }

        input.Player.WorkTickCount = input.WorkingPlace.WorkTickCount;

        var after = BuildSnapshot(input);
        var before = counters.LastTickRenderSnapshot;
        counters.LastTickRenderSnapshot = after;

        bool runMaintenance = false;
        counters.MaintenanceTickCounter++;
        if (counters.MaintenanceTickCounter >= input.MaintenanceTicksPerDay)
        {
            counters.MaintenanceTickCounter = 0;
            runMaintenance = true;
        }

        bool runMarket = false;
        counters.MarketTickCounter++;
        if (counters.MarketTickCounter >= input.MarketOfferInterval)
        {
            counters.MarketTickCounter = 0;
            runMarket = true;
        }

        return new TickResult
        {
            IsDirty = !after.Equals(before),
            Snapshot = after,
            EarnedTicket = earnedTicket,
            FeederFoodChanged = input.Feeder.FoodCount != foodBefore,
            ConsumedWater = consumedWater,
            RunMaintenance = runMaintenance,
            RunMarketNpcOffers = runMarket
        };
    }

    private static TickRenderSnapshot BuildSnapshot(GameTickInput input) => new(
        input.Cat.Hunger, input.Cat.Thirst, input.Cat.Energy, input.Cat.Happiness, input.Cat.Health,
        input.Cat.CatXp, input.Cat.CatLevel, input.Player.Money, input.Feeder.FoodCount,
        input.IsFishing, input.Player.IsWorking);
}
