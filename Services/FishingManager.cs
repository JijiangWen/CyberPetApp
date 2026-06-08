using CyberPetApp.Models;

namespace CyberPetApp.Services;

public class FishingManager : IDisposable
{
    public const int TickIntervalMs = 5000;
    public const int TickIntervalSeconds = TickIntervalMs / 1000;

    public bool IsFishing { get; private set; }
    public int RemainingTicks { get; private set; }
    public FishingSpot? CurrentSpot { get; private set; }
    public Fish? LastCaughtFish { get; private set; }
    public Action? OnChanged { get; set; }

    private System.Timers.Timer? _fishingTimer;
    public Dictionary<string, FishingSpot> FishingSpots { get; set; } = new();

    public FishingManager()
    {
        var creek = new FishingSpot("小溪") { FishingTime = 3, RequiredLevel = 1 };
        creek.FishTable.Add(new Fish("野生鲫鱼", 15, 5, 5, 0, FishRarity.Common, 0.5, 3.0, 0));
        creek.FishTable.Add(new Fish("大口鲈鱼", 20, 10, 8, 0, FishRarity.Common, 1.0, 5.0, 0));
        creek.FishTable.Add(new Fish("黄金锦鲤", 50, 20, 30, 0, FishRarity.Legendary, 2.0, 10.0, 0));
        FishingSpots["小溪"] = creek;

        var ocean = new FishingSpot("神秘深海") { FishingTime = 5, RequiredLevel = 3 };
        ocean.FishTable.Add(new Fish("深海鲈鱼", 20, 10, 8, 0, FishRarity.Common, 1.0, 5.0, 0));
        ocean.FishTable.Add(new Fish("深海鲫鱼", 15, 5, 5, 0, FishRarity.Common, 0.5, 3.0, 0));
        ocean.FishTable.Add(new Fish("深海锦鲤", 50, 20, 30, 0, FishRarity.Legendary, 2.0, 10.0, 0));
        FishingSpots["神秘深海"] = ocean;
    }

    public void StartFishing(FishingSpot spot, Player player)
    {
        if (IsFishing) return;

        IsFishing = true;
        CurrentSpot = spot;
        RemainingTicks = spot.FishingTime;

        _fishingTimer = new System.Timers.Timer(TickIntervalMs);
        _fishingTimer.Elapsed += (sender, e) => OnTimerTick(player);
        _fishingTimer.Start();

        OnChanged?.Invoke();
    }

    private void OnTimerTick(Player player)
    {
        RemainingTicks--;

        if (RemainingTicks <= 0)
        {
            _fishingTimer?.Stop();
            _fishingTimer?.Dispose();
            _fishingTimer = null;
            FinishFishing(player);
        }

        OnChanged?.Invoke();
    }

    private void FinishFishing(Player player)
    {
        if (CurrentSpot == null) return;

        Fish caughtFish = CurrentSpot.FishRoll();
        LastCaughtFish = caughtFish;

        IsFishing = false;
        CurrentSpot = null;

        player.FishBackpack.Add(caughtFish);

        OnChanged?.Invoke();
    }

    public void Dispose()
    {
        _fishingTimer?.Stop();
        _fishingTimer?.Dispose();
        _fishingTimer = null;
    }
}