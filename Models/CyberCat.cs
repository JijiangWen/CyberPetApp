namespace CyberPetApp.Models;

public class CyberCat
{
    public const int StatMax = 1000;

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }

    public string Name { get; set; } = "猫";
    public int CatLevel { get; set; } = 1;
    public int CatXp { get; set; }

    // 战斗属性（1~100 显示刻度，与四维饱食等 0~1000 分离）
    public int Str { get; set; } = CatFishingStatsHelper.BaseStat;
    public int Agi { get; set; } = CatFishingStatsHelper.BaseStat;
    public int Sen { get; set; } = CatFishingStatsHelper.BaseStat;
    public int Sta { get; set; } = CatFishingStatsHelper.BaseStat;
    public int Chm { get; set; } = CatFishingStatsHelper.BaseStat;
    public int Luk { get; set; } = CatFishingStatsHelper.BaseStat;

    public int Health { get; set; } = StatMax;
    public int Happiness { get; set; } = StatMax;
    public int Energy { get; set; } = StatMax;
    public int Hunger { get; set; } = StatMax;
    public int Thirst { get; set; } = StatMax;

    /// <summary>旧存档四维上限 100；Health 不衰减，≤100 即视为旧刻度并 ×10 迁移。</summary>
    public bool MigrateLegacyStats()
    {
        if (Health > 100) return false;

        Health = Math.Min(StatMax, Health * 10);
        Hunger = Math.Min(StatMax, Hunger * 10);
        Energy = Math.Min(StatMax, Energy * 10);
        Happiness = Math.Min(StatMax, Happiness * 10);
        Thirst = Math.Min(StatMax, Thirst * 10);
        return true;
    }

    public string GetMood() =>
        $"The cat is {Happiness} happy and {Energy} energy.";

    public void FeedFood(Food food)
    {
        Console.WriteLine($"{Name} 吃了 {food.Name}.");
        Hunger += food.HungerRestore;
        Energy += food.EnergyRestore;
        Happiness += food.HappinessRestore;

        if (Hunger > StatMax) Hunger = StatMax;
        if (Energy > StatMax) Energy = StatMax;
        if (Happiness > StatMax) Happiness = StatMax;
    }

    public void DrinkWater(int amount)
    {
        Thirst += amount;
        if (Thirst > StatMax) Thirst = StatMax;
    }

    public void Stroke()
    {
        Happiness += 5;
        Energy += 5;

        if (Happiness > StatMax) Happiness = StatMax;
        if (Energy > StatMax) Energy = StatMax;
    }

    public void Sleep(HouseBuffs buffs = default)
    {
        Hunger -= 10;
        int energyGain = (int)Math.Round(10 * buffs.SleepEnergyMultiplier);
        Energy += energyGain;

        if (Hunger < 0) Hunger = 0;
        if (Energy > StatMax) Energy = StatMax;
    }

    /// <summary>背景 tick 计数（2s/次）；满 300 次 ≈10 分钟四维各 -1，仅作挂机/离线缓慢衰减。</summary>
    public int BackgroundTickCount { get; set; }

    public const int BackgroundTicksPerDecay = 300;

    /// <summary>极慢被动衰减：每 ≈10 分钟四维各 -1（家具倍率仍生效）。</summary>
    public void Tick(HouseBuffs buffs = default)
    {
        BackgroundTickCount++;
        if (BackgroundTickCount < BackgroundTicksPerDecay) return;
        BackgroundTickCount = 0;

        Hunger--;
        Energy -= ScaledCost(1, buffs.EnergyDecayMultiplier);
        Happiness -= ScaledCost(1, buffs.HappinessDecayMultiplier);
        Thirst -= ScaledCost(1, buffs.ThirstDecayMultiplier);
        ClampStats();
    }

    /// <summary>活动驱动消耗：钓鱼/打工/派遣/烹饪/市场等触发时调用。</summary>
    public void ApplyActivityCost(CatActivityType type, HouseBuffs buffs = default, double energyCostMultiplier = 1.0)
    {
        var delta = CatActivityCost.Get(type);
        Hunger += delta.Hunger;
        int energyDelta = delta.Energy >= 0
            ? delta.Energy
            : -(int)Math.Round(-delta.Energy * Math.Clamp(energyCostMultiplier, 0.5, 1.0));
        Energy += ScaledDelta(energyDelta, buffs.EnergyDecayMultiplier);
        Happiness += ScaledDelta(delta.Happiness, buffs.HappinessDecayMultiplier);
        Thirst += ScaledDelta(delta.Thirst, buffs.ThirstDecayMultiplier);
        Health += delta.Health;
        ClampStats();
    }

    private void ClampStats()
    {
        if (Hunger < 0) Hunger = 0;
        if (Energy < 0) Energy = 0;
        if (Happiness < 0) Happiness = 0;
        if (Thirst < 0) Thirst = 0;
        if (Health < 0) Health = 0;
        if (Hunger > StatMax) Hunger = StatMax;
        if (Energy > StatMax) Energy = StatMax;
        if (Happiness > StatMax) Happiness = StatMax;
        if (Thirst > StatMax) Thirst = StatMax;
        if (Health > StatMax) Health = StatMax;
    }

    /// <summary>正向恢复不缩放；负向消耗按家具倍率（越小扣越少）。</summary>
    private static int ScaledDelta(int amount, double multiplier) =>
        amount >= 0 ? amount : -ScaledCost(-amount, multiplier);

    private static int ScaledCost(int baseAmount, double multiplier) =>
        Math.Max(0, (int)Math.Round(baseAmount * multiplier));
}
