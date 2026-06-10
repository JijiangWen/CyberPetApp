using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>烹饪食物持续 Buff：喂食施加、定时 Tick 缓回、钓鱼公式读取。</summary>
public class CatBuffService
{
    private readonly AppDbContext _context;

    public CatBuffService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ActiveCatBuff>> LoadActiveAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        var rows = await _context.PlayerCatBuffs
            .Where(b => b.PlayerId == playerId && b.ExpiresAt > now)
            .OrderBy(b => b.ExpiresAt)
            .ToListAsync();
        return rows.Select(ToActive).ToList();
    }

    /// <summary>喂食后施加食谱内全部 Buff（同类型取更强 Value 并刷新时长）。</summary>
    public async Task ApplyRecipeBuffsAsync(Guid playerId, CookingRecipe recipe)
    {
        var defs = recipe.AllBuffs().ToList();
        if (defs.Count == 0) return;

        var now = DateTime.UtcNow;
        var existing = await _context.PlayerCatBuffs
            .Where(b => b.PlayerId == playerId && b.ExpiresAt > now)
            .ToListAsync();

        foreach (var def in defs)
        {
            var row = existing.FirstOrDefault(b => b.BuffType == def.BuffType);
            int ticks = def.TotalTicks;
            var expires = now.AddMinutes(def.DurationMinutes);

            if (row is null)
            {
                _context.PlayerCatBuffs.Add(new PlayerCatBuff
                {
                    PlayerId = playerId,
                    BuffType = def.BuffType,
                    Value = def.Value,
                    TickIntervalMinutes = def.TickIntervalMinutes,
                    RemainingTicks = ticks,
                    StartedAt = now,
                    NextTickAt = IsRegen(def.BuffType)
                        ? now.AddMinutes(def.TickIntervalMinutes)
                        : expires,
                    ExpiresAt = expires,
                    SourceFoodName = recipe.FoodName
                });
            }
            else
            {
                row.Value = Math.Max(row.Value, def.Value);
                row.TickIntervalMinutes = def.TickIntervalMinutes;
                row.RemainingTicks = Math.Max(row.RemainingTicks, ticks);
                row.StartedAt = now;
                row.ExpiresAt = expires;
                row.NextTickAt = IsRegen(def.BuffType)
                    ? now.AddMinutes(def.TickIntervalMinutes)
                    : expires;
                row.SourceFoodName = recipe.FoodName;
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>游戏定时器（≈2s）调用：处理缓回 Tick 并清理过期 Buff。</summary>
    public async Task<List<ActiveCatBuff>> TickBuffsAsync(Guid playerId, CyberCat cat, bool saveChanges = true)
    {
        var now = DateTime.UtcNow;

        var expired = await _context.PlayerCatBuffs
            .Where(b => b.PlayerId == playerId && b.ExpiresAt <= now)
            .ToListAsync();
        if (expired.Count > 0)
            _context.PlayerCatBuffs.RemoveRange(expired);

        var rows = await _context.PlayerCatBuffs
            .Where(b => b.PlayerId == playerId && b.ExpiresAt > now)
            .ToListAsync();

        bool changed = expired.Count > 0;
        foreach (var row in rows)
        {

            if (!IsRegen(row.BuffType) || row.RemainingTicks <= 0)
                continue;

            if (now < row.NextTickAt)
                continue;

            ApplyRegenTick(cat, row.BuffType, row.Value);
            row.RemainingTicks--;
            row.NextTickAt = now.AddMinutes(row.TickIntervalMinutes);
            if (row.RemainingTicks <= 0)
                row.ExpiresAt = Math.Min(row.ExpiresAt.Ticks, now.Ticks) == row.ExpiresAt.Ticks
                    ? now
                    : row.ExpiresAt;
            changed = true;
        }

        if (changed && saveChanges)
            await _context.SaveChangesAsync();

        return rows
            .Where(r => r.ExpiresAt > now)
            .Select(ToActive)
            .ToList();
    }

    public static FoodBuffSnapshot BuildSnapshot(IReadOnlyList<ActiveCatBuff> buffs)
    {
        if (buffs.Count == 0)
            return FoodBuffSnapshot.Empty;

        double hook = 0, rarity = 0, energyMult = 1.0;
        var lines = new List<string>();

        foreach (var group in buffs.GroupBy(b => b.BuffType))
        {
            var best = group.OrderByDescending(b => b.Value).First();
            switch (best.BuffType)
            {
                case CatFoodBuffType.HookBonus:
                    hook = Math.Max(hook, best.Value);
                    break;
                case CatFoodBuffType.RareWeightBonus:
                    rarity = Math.Max(rarity, best.Value);
                    break;
                case CatFoodBuffType.FishingEnergyDiscount:
                    energyMult = Math.Min(energyMult, best.Value);
                    break;
            }
        }

        foreach (var b in buffs.OrderBy(b => b.ExpiresAt))
            lines.Add($"[{b.SourceFoodName}] {b.DisplayLabel}");

        if (hook > 0)
            lines.Add($"食物 Buff：抓口 +{hook:P0}");
        if (rarity > 0)
            lines.Add($"食物 Buff：Rare+ 权重 +{rarity:P0}");
        if (energyMult < 1.0)
            lines.Add($"食物 Buff：钓鱼精力 ×{energyMult:0.##}");

        return new FoodBuffSnapshot(hook, rarity, energyMult, buffs, lines);
    }

    public static CatFishingStats MergeStats(CatFishingStats baseStats, FoodBuffSnapshot food)
    {
        if (food.ActiveBuffs.Count == 0)
            return baseStats;

        var lines = baseStats.StatusLines.ToList();
        if (food.HookBonus > 0)
            lines.Add($"[料理] 抓口 +{food.HookBonus:P0}");
        if (food.RarityBonus > 0)
            lines.Add($"[料理] 稀有权重 +{food.RarityBonus:P0}");
        if (food.FishingEnergyDiscount < 1.0)
            lines.Add($"[料理] 钓鱼精力 ×{food.FishingEnergyDiscount:0.##}");

        return baseStats with
        {
            HookBonus = baseStats.HookBonus + food.HookBonus,
            RarityBonus = baseStats.RarityBonus + food.RarityBonus,
            EnergyCostMultiplier = baseStats.EnergyCostMultiplier * food.FishingEnergyDiscount,
            StatusLines = lines
        };
    }

    public static CatFishingBuff MergeStateBuff(CatFishingBuff baseBuff, FoodBuffSnapshot food)
    {
        if (food.RarityBonus <= 0)
            return baseBuff;

        var lines = baseBuff.StatusLines.ToList();
        lines.Add($"[料理] 稀有鱼权重 +{food.RarityBonus:P0}");
        return baseBuff with
        {
            RarityBonus = baseBuff.RarityBonus + food.RarityBonus,
            StatusLines = lines
        };
    }

    private static void ApplyRegenTick(CyberCat cat, CatFoodBuffType type, double value)
    {
        int amount = (int)Math.Round(value);
        switch (type)
        {
            case CatFoodBuffType.HungerRegenOverTime:
                cat.Hunger = Math.Min(CyberCat.StatMax, cat.Hunger + amount);
                break;
            case CatFoodBuffType.EnergyRegenOverTime:
                cat.Energy = Math.Min(CyberCat.StatMax, cat.Energy + amount);
                break;
            case CatFoodBuffType.HappinessRegenOverTime:
                cat.Happiness = Math.Min(CyberCat.StatMax, cat.Happiness + amount);
                break;
        }
    }

    private static bool IsRegen(CatFoodBuffType type) => type switch
    {
        CatFoodBuffType.HungerRegenOverTime => true,
        CatFoodBuffType.EnergyRegenOverTime => true,
        CatFoodBuffType.HappinessRegenOverTime => true,
        _ => false
    };

    private static ActiveCatBuff ToActive(PlayerCatBuff row)
    {
        string label = row.BuffType switch
        {
            CatFoodBuffType.HungerRegenOverTime => $"饱腹缓回 +{(int)row.Value}/{row.TickIntervalMinutes}min · 剩{row.RemainingTicks}次",
            CatFoodBuffType.EnergyRegenOverTime => $"精力缓回 +{(int)row.Value}/{row.TickIntervalMinutes}min · 剩{row.RemainingTicks}次",
            CatFoodBuffType.HappinessRegenOverTime => $"心情缓回 +{(int)row.Value}/{row.TickIntervalMinutes}min · 剩{row.RemainingTicks}次",
            CatFoodBuffType.FishingEnergyDiscount => $"钓鱼精力 ×{row.Value:0.##}",
            CatFoodBuffType.HookBonus => $"抓口 +{row.Value:P0}",
            CatFoodBuffType.RareWeightBonus => $"Rare+ +{row.Value:P0}",
            _ => row.BuffType.ToString()
        };

        return new ActiveCatBuff(
            row.BuffType, row.Value, row.ExpiresAt,
            row.RemainingTicks, row.TickIntervalMinutes,
            row.SourceFoodName, label);
    }
}
