using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>
/// 局外装备服务：默认装备发放、查询、购买、切换装备、拟饵耐久消耗。
/// </summary>
public class EquipmentService
{
    private readonly AppDbContext _context;

    public EquipmentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EnsureDefaultGearAsync(Guid playerId)
    {
        bool changed = false;

        if (!await _context.FishingRods.AnyAsync(r => r.PlayerId == playerId))
        {
            var spec = GearCatalog.DefaultRod;
            _context.FishingRods.Add(new FishingRod
            {
                PlayerId = playerId,
                Name = spec.Name,
                Sensitivity = spec.Sensitivity,
                MaxStrength = spec.MaxStrength,
                CastRange = spec.CastRange,
                RequiredLevel = spec.RequiredLevel,
                Durability = 100,
                IsEquipped = true
            });
            changed = true;
        }

        if (!await _context.FishingReels.AnyAsync(r => r.PlayerId == playerId))
        {
            var spec = GearCatalog.DefaultReel;
            _context.FishingReels.Add(new FishingReel
            {
                PlayerId = playerId,
                Name = spec.Name,
                DragPower = spec.DragPower,
                GearRatio = spec.GearRatio,
                LineCapacity = spec.LineCapacity,
                Smoothness = spec.Smoothness,
                RequiredLevel = spec.RequiredLevel,
                Durability = 100,
                IsEquipped = true
            });
            changed = true;
        }

        if (!await _context.FishingLines.AnyAsync(l => l.PlayerId == playerId))
        {
            var spec = GearCatalog.DefaultLine;
            _context.FishingLines.Add(new FishingLine
            {
                PlayerId = playerId,
                Name = spec.Name,
                LineStrength = spec.LineStrength,
                LineSensitivity = spec.LineSensitivity,
                LineStealth = spec.LineStealth,
                AbrasionResistance = spec.AbrasionResistance,
                TargetDepth = spec.TargetDepth,
                RequiredLevel = spec.RequiredLevel,
                Durability = 100,
                IsEquipped = true
            });
            changed = true;
        }

        if (!await _context.FishingLures.AnyAsync(l => l.PlayerId == playerId))
        {
            var spec = GearCatalog.DefaultLure;
            _context.FishingLures.Add(new FishingLure
            {
                PlayerId = playerId,
                Name = spec.Name,
                Attraction = spec.Attraction,
                RarityBonus = spec.RarityBonus,
                TargetDepth = spec.TargetDepth,
                DurabilityRemaining = spec.MaxDurability,
                RequiredLevel = spec.RequiredLevel,
                Quantity = GearCatalog.DefaultLureQuantity,
                IsEquipped = true
            });
            changed = true;
        }

        if (changed) await _context.SaveChangesAsync();
    }

    public Task<List<FishingRod>> GetRodsAsync(Guid playerId) =>
        _context.FishingRods.Where(r => r.PlayerId == playerId).OrderBy(r => r.Name).ToListAsync();

    public Task<List<FishingReel>> GetReelsAsync(Guid playerId) =>
        _context.FishingReels.Where(r => r.PlayerId == playerId).OrderBy(r => r.Name).ToListAsync();

    public Task<List<FishingLine>> GetLinesAsync(Guid playerId) =>
        _context.FishingLines.Where(l => l.PlayerId == playerId).OrderBy(l => l.Name).ToListAsync();

    public Task<List<FishingLure>> GetLuresAsync(Guid playerId) =>
        _context.FishingLures.Where(l => l.PlayerId == playerId).OrderBy(l => l.Name).ToListAsync();

    public async Task<bool> BuyRodAsync(Player player, RodSpec spec,
        int catLevel = 1, IReadOnlyList<FishDexEntry>? dex = null, Func<string, bool>? hasLicense = null)
    {
        if (spec.CraftOnly || !spec.ShopAvailable) return false;
        dex ??= [];
        hasLicense ??= _ => true;
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
            return false;
        if (player.Money < spec.Price) return false;
        if (await _context.FishingRods.AnyAsync(r => r.PlayerId == player.Id && r.Name == spec.Name)) return false;

        player.Money -= spec.Price;
        await PersistMoneyAsync(player);
        _context.FishingRods.Add(new FishingRod
        {
            PlayerId = player.Id,
            Name = spec.Name,
            Sensitivity = spec.Sensitivity,
            MaxStrength = spec.MaxStrength,
            CastRange = spec.CastRange,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BuyReelAsync(Player player, ReelSpec spec,
        int catLevel = 1, IReadOnlyList<FishDexEntry>? dex = null, Func<string, bool>? hasLicense = null)
    {
        if (spec.CraftOnly || !spec.ShopAvailable) return false;
        dex ??= [];
        hasLicense ??= _ => true;
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
            return false;
        if (player.Money < spec.Price) return false;
        if (await _context.FishingReels.AnyAsync(r => r.PlayerId == player.Id && r.Name == spec.Name)) return false;

        player.Money -= spec.Price;
        await PersistMoneyAsync(player);
        _context.FishingReels.Add(new FishingReel
        {
            PlayerId = player.Id,
            Name = spec.Name,
            DragPower = spec.DragPower,
            GearRatio = spec.GearRatio,
            LineCapacity = spec.LineCapacity,
            Smoothness = spec.Smoothness,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BuyLineAsync(Player player, LineSpec spec,
        int catLevel = 1, IReadOnlyList<FishDexEntry>? dex = null, Func<string, bool>? hasLicense = null)
    {
        if (spec.CraftOnly || !spec.ShopAvailable) return false;
        dex ??= [];
        hasLicense ??= _ => true;
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
            return false;
        if (player.Money < spec.Price) return false;
        if (await _context.FishingLines.AnyAsync(l => l.PlayerId == player.Id && l.Name == spec.Name)) return false;

        player.Money -= spec.Price;
        await PersistMoneyAsync(player);
        _context.FishingLines.Add(new FishingLine
        {
            PlayerId = player.Id,
            Name = spec.Name,
            LineStrength = spec.LineStrength,
            LineSensitivity = spec.LineSensitivity,
            LineStealth = spec.LineStealth,
            AbrasionResistance = spec.AbrasionResistance,
            TargetDepth = spec.TargetDepth,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCraftedRodAsync(Guid playerId, RodSpec spec)
    {
        if (await _context.FishingRods.AnyAsync(r => r.PlayerId == playerId && r.Name == spec.Name))
            return false;
        _context.FishingRods.Add(new FishingRod
        {
            PlayerId = playerId,
            Name = spec.Name,
            Sensitivity = spec.Sensitivity,
            MaxStrength = spec.MaxStrength,
            CastRange = spec.CastRange,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100,
            IsCrafted = true
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCraftedReelAsync(Guid playerId, ReelSpec spec)
    {
        if (await _context.FishingReels.AnyAsync(r => r.PlayerId == playerId && r.Name == spec.Name))
            return false;
        _context.FishingReels.Add(new FishingReel
        {
            PlayerId = playerId,
            Name = spec.Name,
            DragPower = spec.DragPower,
            GearRatio = spec.GearRatio,
            LineCapacity = spec.LineCapacity,
            Smoothness = spec.Smoothness,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100,
            IsCrafted = true
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCraftedLureAsync(Guid playerId, LureSpec spec, int packSize)
    {
        var existing = await _context.FishingLures
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.Name == spec.Name);
        if (existing is not null)
        {
            existing.Quantity += packSize;
            if (existing.DurabilityRemaining <= 0 && existing.Quantity > 0)
                existing.DurabilityRemaining = spec.MaxDurability;
            existing.IsCrafted = true;
        }
        else
        {
            _context.FishingLures.Add(new FishingLure
            {
                PlayerId = playerId,
                Name = spec.Name,
                Attraction = spec.Attraction,
                RarityBonus = spec.RarityBonus,
                TargetDepth = spec.TargetDepth,
                DurabilityRemaining = spec.MaxDurability,
                RequiredLevel = spec.RequiredLevel,
                Quantity = packSize,
                IsCrafted = true
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCraftedLineAsync(Guid playerId, LineAlchemyRecipe recipe)
    {
        if (await _context.FishingLines.AnyAsync(l => l.PlayerId == playerId && l.Name == recipe.OutputLineName))
            return false;

        _context.FishingLines.Add(new FishingLine
        {
            PlayerId = playerId,
            Name = recipe.OutputLineName,
            LineStrength = recipe.LineStrength,
            LineSensitivity = recipe.LineSensitivity,
            LineStealth = recipe.LineStealth,
            AbrasionResistance = recipe.AbrasionResistance,
            TargetDepth = recipe.TargetDepth,
            RequiredLevel = 1,
            Durability = 100,
            IsCrafted = true
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCraftedLineAsync(Guid playerId, LineSpec spec)
    {
        if (await _context.FishingLines.AnyAsync(l => l.PlayerId == playerId && l.Name == spec.Name))
            return false;
        _context.FishingLines.Add(new FishingLine
        {
            PlayerId = playerId,
            Name = spec.Name,
            LineStrength = spec.LineStrength,
            LineSensitivity = spec.LineSensitivity,
            LineStealth = spec.LineStealth,
            AbrasionResistance = spec.AbrasionResistance,
            TargetDepth = spec.TargetDepth,
            RequiredLevel = spec.RequiredLevel,
            Durability = 100,
            IsCrafted = true
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> OwnsGearAsync(Guid playerId, string gearName, GearCraftSlot slot) => slot switch
    {
        GearCraftSlot.Rod => await _context.FishingRods.AnyAsync(r => r.PlayerId == playerId && r.Name == gearName),
        GearCraftSlot.Reel => await _context.FishingReels.AnyAsync(r => r.PlayerId == playerId && r.Name == gearName),
        GearCraftSlot.Line => await _context.FishingLines.AnyAsync(l => l.PlayerId == playerId && l.Name == gearName),
        GearCraftSlot.Lure => await _context.FishingLures.AnyAsync(l => l.PlayerId == playerId && l.Name == gearName),
        _ => false
    };

    public async Task<bool> BuyLureAsync(Player player, LureSpec spec,
        int catLevel = 1, IReadOnlyList<FishDexEntry>? dex = null, Func<string, bool>? hasLicense = null)
    {
        if (spec.CraftOnly || !spec.ShopAvailable) return false;
        dex ??= [];
        hasLicense ??= _ => true;
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
            return false;
        if (player.Money < spec.PackPrice) return false;

        player.Money -= spec.PackPrice;
        await PersistMoneyAsync(player);

        var existing = await _context.FishingLures
            .FirstOrDefaultAsync(l => l.PlayerId == player.Id && l.Name == spec.Name);
        if (existing is not null)
        {
            existing.Quantity += spec.PackSize;
            if (existing.DurabilityRemaining <= 0 && existing.Quantity > 0)
                existing.DurabilityRemaining = spec.MaxDurability;
        }
        else
        {
            _context.FishingLures.Add(new FishingLure
            {
                PlayerId = player.Id,
                Name = spec.Name,
                Attraction = spec.Attraction,
                RarityBonus = spec.RarityBonus,
                TargetDepth = spec.TargetDepth,
                DurabilityRemaining = spec.MaxDurability,
                RequiredLevel = spec.RequiredLevel,
                Quantity = spec.PackSize
            });
        }
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task EquipRodAsync(Guid playerId, Guid rodId)
    {
        var rods = await _context.FishingRods.Where(r => r.PlayerId == playerId).ToListAsync();
        foreach (var r in rods) r.IsEquipped = r.Id == rodId;
        await _context.SaveChangesAsync();
    }

    public async Task EquipReelAsync(Guid playerId, Guid reelId)
    {
        var reels = await _context.FishingReels.Where(r => r.PlayerId == playerId).ToListAsync();
        foreach (var r in reels) r.IsEquipped = r.Id == reelId;
        await _context.SaveChangesAsync();
    }

    public async Task EquipLineAsync(Guid playerId, Guid lineId)
    {
        var lines = await _context.FishingLines.Where(l => l.PlayerId == playerId).ToListAsync();
        foreach (var l in lines) l.IsEquipped = l.Id == lineId;
        await _context.SaveChangesAsync();
    }

    public async Task EquipLureAsync(Guid playerId, Guid lureId)
    {
        var lures = await _context.FishingLures.Where(l => l.PlayerId == playerId).ToListAsync();
        foreach (var l in lures) l.IsEquipped = l.Id == lureId;
        var equipped = lures.FirstOrDefault(l => l.Id == lureId);
        if (equipped is not null && equipped.DurabilityRemaining <= 0 && equipped.Quantity > 0)
        {
            var spec = GearCatalog.FindLure(equipped.Name);
            equipped.DurabilityRemaining = spec?.MaxDurability ?? 10;
        }
        await _context.SaveChangesAsync();
    }

    /// <summary>切线后同步拟饵耐久/库存（与内存 Loadout 一致）。</summary>
    public async Task SyncEquippedLureAsync(Guid playerId, int durabilityRemaining, int quantity)
    {
        var lure = await _context.FishingLures
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        if (lure is null) return;
        lure.DurabilityRemaining = durabilityRemaining;
        lure.Quantity = quantity;
        await _context.SaveChangesAsync();
    }

    public async Task<FishingLoadout> BuildLoadoutAsync(Guid playerId, int fishingLevel, double milestoneRarityBonus = 0, string? activeSpotName = null)
    {
        var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var reel = await _context.FishingReels.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var line = await _context.FishingLines.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        var lure = await _context.FishingLures.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        var gems = await _context.PlayerGems.Where(g => g.PlayerId == playerId && g.IsSocketed).ToListAsync();
        var targetLure = await _context.PlayerTargetLures
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped && l.RemainingUses > 0);

        int rodTier = GearProgressionCatalog.GetRodTier(rod?.Name);
        var lureSpec = lure?.Name is not null ? GearCatalog.FindLure(lure.Name) : null;
        int lureTier = lureSpec is not null ? (int)lureSpec.Tier : 1;
        double lureMythic = lureSpec?.MythicBonus ?? 0;
        double spotEff = activeSpotName is null ? 1.0
            : GearProgressionCatalog.SpotGearEffectiveness(rodTier, activeSpotName);

        return new FishingLoadout
        {
            RodName = rod?.Name ?? "赤手空拳",
            Sensitivity = rod?.Sensitivity ?? 0,
            CastRange = rod?.CastRange ?? 1,
            MaxStrength = rod?.MaxStrength ?? 1,
            RodRequiredLevel = rod?.RequiredLevel ?? 1,
            RodDurability = rod?.Durability ?? 100,
            RodGearTier = rodTier,
            SpotGearEffectiveness = spotEff,
            ReelName = reel?.Name ?? "无",
            DragPower = reel?.DragPower ?? 0,
            GearRatio = reel?.GearRatio ?? 5.0,
            LineCapacity = reel?.LineCapacity ?? 8,
            Smoothness = reel?.Smoothness ?? 0.3,
            ReelRequiredLevel = reel?.RequiredLevel ?? 1,
            ReelDurability = reel?.Durability ?? 100,
            LineName = line?.Name ?? "无",
            LineStrength = line?.LineStrength ?? 1,
            LineSensitivity = line?.LineSensitivity ?? 0,
            LineStealth = line?.LineStealth ?? 0,
            AbrasionResistance = line?.AbrasionResistance ?? 0,
            LineDepth = line?.TargetDepth ?? WaterDepth.Middle,
            LineRequiredLevel = line?.RequiredLevel ?? 1,
            LineDurability = line?.Durability ?? 100,
            LureName = lure?.Name ?? "无",
            Attraction = lure?.Attraction ?? 0,
            RarityBonus = lure?.RarityBonus ?? 0,
            LureDepth = lure?.TargetDepth ?? WaterDepth.Middle,
            LureDurabilityRemaining = lure?.DurabilityRemaining ?? 0,
            LureRequiredLevel = lure?.RequiredLevel ?? 1,
            LureQuantity = lure?.Quantity ?? 0,
            LureGearTier = lureTier,
            LureMythicBonus = lureMythic,
            FishingLevel = fishingLevel,
            MilestoneRarityBonus = milestoneRarityBonus,
            GemBonuses = GemBonusSet.FromGems(gems),
            ActiveTargetLureRecipeId = targetLure?.RecipeId,
            ActiveTargetLureUses = targetLure?.RemainingUses ?? 0,
            ActiveTargetLureId = targetLure?.Id
        };
    }

    public async Task<List<string>> WearEquippedGearAsync(Guid playerId, bool lineBreakExtra, string? spotName = null, bool isEscape = false)
    {
        var logs = new List<string>();
        if (isEscape && Random.Shared.NextDouble() >= 0.5)
            return logs; // 脱钩：50% 概率完全免磨损

        bool escapeLineOnly = isEscape && !lineBreakExtra;

        var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var reel = await _context.FishingReels.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var fishingLine = await _context.FishingLines.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        var rng = Random.Shared;
        int wear = rng.Next(1, 4) + GearProgressionCatalog.SpotExtraWear(spotName);
        if (lineBreakExtra) wear += 5;

        if (!escapeLineOnly && rod is not null)
        {
            var rodAffix = GearCatalog.FindRod(rod.Name)?.Affix ?? GearAffix.Balanced;
            int rodWear = (int)Math.Ceiling(wear * GearAffixHelper.WearMultiplier(rodAffix));
            if (rodWear > 0 && rod.Durability > 0)
            {
                rod.Durability = Math.Max(0, rod.Durability - rodWear);
                logs.Add($"鱼竿耐久 -{rodWear}");
            }
        }
        if (!escapeLineOnly && reel is not null)
        {
            var reelAffix = GearCatalog.FindReel(reel.Name)?.Affix ?? GearAffix.Balanced;
            int reelWear = (int)Math.Ceiling(wear * GearAffixHelper.WearMultiplier(reelAffix));
            if (reelWear > 0 && reel.Durability > 0)
            {
                reel.Durability = Math.Max(0, reel.Durability - reelWear);
                logs.Add($"渔轮耐久 -{reelWear}");
            }
        }
        if (fishingLine is not null)
        {
            int lineWear = lineBreakExtra
                ? Math.Max(1, (int)Math.Round(wear * (1 - fishingLine.AbrasionResistance)))
                : escapeLineOnly ? Math.Max(1, wear / 2) : wear;
            if (lineWear > 0 && fishingLine.Durability > 0)
            {
                fishingLine.Durability = Math.Max(0, fishingLine.Durability - lineWear);
                logs.Add($"鱼线耐久 -{lineWear}");
            }
        }
        await _context.SaveChangesAsync();
        return logs;
    }

    public async Task<(bool Ok, string Message)> RepairRodAsync(Player player, Guid rodId, bool fullRepair)
    {
        var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.Id == rodId && r.PlayerId == player.Id);
        if (rod is null) return (false, "鱼竿不存在");
        if (rod.Durability >= 100) return (false, "耐久已满");

        int tier = GearProgressionCatalog.GetGearTier(rod.Name);
        int cost;
        if (fullRepair)
        {
            int rawFullCost = EconomySinks.GearFullRepairCost(rod.MaxStrength, tier);
            cost = (int)Math.Ceiling(rawFullCost * (100.0 - rod.Durability) / 100.0);
        }
        else
        {
            cost = EconomySinks.GearRepairPartialCost(tier);
        }
        if (!await TrySpendAsync(player, cost)) return (false, $"金币不足，修理需 {cost}g");

        rod.Durability = fullRepair ? 100 : Math.Min(100, rod.Durability + EconomySinks.GearRepairPartialAmount);
        await _context.SaveChangesAsync();
        return (true, fullRepair
            ? $"[{rod.Name}] 全修至 100，扣 {cost}g"
            : $"[{rod.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久，扣 {cost}g");
    }

    public async Task<(bool Ok, string Message)> RepairReelAsync(Player player, Guid reelId, bool fullRepair)
    {
        var reel = await _context.FishingReels.FirstOrDefaultAsync(r => r.Id == reelId && r.PlayerId == player.Id);
        if (reel is null) return (false, "卷线器不存在");
        if (reel.Durability >= 100) return (false, "耐久已满");

        int tier = GearProgressionCatalog.GetGearTier(reel.Name);
        int cost;
        if (fullRepair)
        {
            int rawFullCost = EconomySinks.GearFullRepairCost(reel.DragPower, tier);
            cost = (int)Math.Ceiling(rawFullCost * (100.0 - reel.Durability) / 100.0);
        }
        else
        {
            cost = EconomySinks.GearRepairPartialCost(tier);
        }
        if (!await TrySpendAsync(player, cost)) return (false, $"金币不足，修理需 {cost}g");

        reel.Durability = fullRepair ? 100 : Math.Min(100, reel.Durability + EconomySinks.GearRepairPartialAmount);
        await _context.SaveChangesAsync();
        return (true, fullRepair
            ? $"[{reel.Name}] 全修至 100，扣 {cost}g"
            : $"[{reel.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久，扣 {cost}g");
    }

    public async Task<(bool Ok, string Message)> RepairLineAsync(Player player, Guid lineId, bool fullRepair)
    {
        var line = await _context.FishingLines.FirstOrDefaultAsync(l => l.Id == lineId && l.PlayerId == player.Id);
        if (line is null) return (false, "鱼线不存在");
        if (line.Durability >= 100) return (false, "耐久已满");

        int tier = GearProgressionCatalog.GetGearTier(line.Name);
        int cost;
        if (fullRepair)
        {
            int rawFullCost = EconomySinks.GearFullRepairCost(line.LineStrength, tier);
            cost = (int)Math.Ceiling(rawFullCost * (100.0 - line.Durability) / 100.0);
        }
        else
        {
            cost = EconomySinks.GearRepairPartialCost(tier);
        }
        if (!await TrySpendAsync(player, cost)) return (false, $"金币不足，修理需 {cost}g");

        line.Durability = fullRepair ? 100 : Math.Min(100, line.Durability + EconomySinks.GearRepairPartialAmount);
        await _context.SaveChangesAsync();
        return (true, fullRepair
            ? $"[{line.Name}] 全修至 100，扣 {cost}g"
            : $"[{line.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久，扣 {cost}g");
    }

    public async Task SyncEquippedLineDurabilityAsync(Guid playerId, int durability)
    {
        var line = await _context.FishingLines
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        if (line is null) return;
        line.Durability = durability;
        await _context.SaveChangesAsync();
    }

    private async Task<bool> TrySpendAsync(Player player, int amount)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is null || tracked.Money < amount) return false;
        tracked.Money -= amount;
        player.Money = tracked.Money;
        return true;
    }

    private async Task PersistMoneyAsync(Player player)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is not null && !ReferenceEquals(tracked, player))
            tracked.Money = player.Money;
    }

    // ── 商店乐观 UI（校验 / 内存更新 / 后台 Commit） ──

    public static bool TryPrepareBuyRod(
        Player player, IReadOnlyList<FishingRod> rods, RodSpec spec,
        int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense,
        out FishingRod rod, out string error)
    {
        rod = null!;
        error = "";
        if (spec.CraftOnly || !spec.ShopAvailable) { error = "需炼金锻造"; return false; }
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
        {
            error = $"未满足解锁：{GearProgressionCatalog.UnlockShortfall(
                spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
                spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
                spec.RequiredMythicCaught, spec.RequiredAllMythic,
                player.FishingLevel, catLevel, dex, hasLicense)}";
            return false;
        }
        if (player.Money < spec.Price) { error = "金币不足或已拥有同类装备"; return false; }
        if (rods.Any(r => r.Name == spec.Name)) { error = "已拥有该装备"; return false; }
        rod = CreateRodFromSpec(spec, player.Id);
        return true;
    }

    public static bool TryPrepareBuyReel(
        Player player, IReadOnlyList<FishingReel> reels, ReelSpec spec,
        int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense,
        out FishingReel reel, out string error)
    {
        reel = null!;
        error = "";
        if (spec.CraftOnly || !spec.ShopAvailable) { error = "需炼金锻造"; return false; }
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
        {
            error = $"未满足解锁：{GearProgressionCatalog.UnlockShortfall(
                spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
                spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
                spec.RequiredMythicCaught, spec.RequiredAllMythic,
                player.FishingLevel, catLevel, dex, hasLicense)}";
            return false;
        }
        if (player.Money < spec.Price) { error = "金币不足或已拥有同类装备"; return false; }
        if (reels.Any(r => r.Name == spec.Name)) { error = "已拥有该装备"; return false; }
        reel = CreateReelFromSpec(spec, player.Id);
        return true;
    }

    public static bool TryPrepareBuyLine(
        Player player, IReadOnlyList<FishingLine> lines, LineSpec spec,
        int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense,
        out FishingLine line, out string error)
    {
        line = null!;
        error = "";
        if (spec.CraftOnly || !spec.ShopAvailable) { error = "需炼金锻造"; return false; }
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
        {
            error = $"未满足解锁：{GearProgressionCatalog.UnlockShortfall(
                spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
                spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
                spec.RequiredMythicCaught, spec.RequiredAllMythic,
                player.FishingLevel, catLevel, dex, hasLicense)}";
            return false;
        }
        if (player.Money < spec.Price) { error = "金币不足或已拥有同类装备"; return false; }
        if (lines.Any(l => l.Name == spec.Name)) { error = "已拥有该装备"; return false; }
        line = CreateLineFromSpec(spec, player.Id);
        return true;
    }

    public static bool TryPrepareBuyLure(
        Player player, LureSpec spec,
        int catLevel, IReadOnlyList<FishDexEntry> dex, Func<string, bool> hasLicense,
        out string error)
    {
        error = "";
        if (spec.CraftOnly || !spec.ShopAvailable)
        {
            error = "需炼金锻造";
            return false;
        }

        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player.FishingLevel, catLevel, dex, hasLicense))
        {
            error = $"未满足解锁：{GearProgressionCatalog.UnlockShortfall(
                spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent,
                spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent,
                spec.RequiredMythicCaught, spec.RequiredAllMythic,
                player.FishingLevel, catLevel, dex, hasLicense)}";
            return false;
        }

        if (player.Money < spec.PackPrice)
        {
            error = "金币不足";
            return false;
        }

        return true;
    }

    public static void ApplyBuyRodOptimistic(Player player, List<FishingRod> rods, RodSpec spec, FishingRod rod)
    {
        player.Money -= spec.Price;
        rods.Add(rod);
        rods.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    public static void ApplyBuyReelOptimistic(Player player, List<FishingReel> reels, ReelSpec spec, FishingReel reel)
    {
        player.Money -= spec.Price;
        reels.Add(reel);
        reels.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    public static void ApplyBuyLineOptimistic(Player player, List<FishingLine> lines, LineSpec spec, FishingLine line)
    {
        player.Money -= spec.Price;
        lines.Add(line);
        lines.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
    }

    public static FishingLure ApplyBuyLureOptimistic(Player player, List<FishingLure> lures, LureSpec spec)
    {
        player.Money -= spec.PackPrice;
        var existing = lures.FirstOrDefault(l => l.Name == spec.Name);
        if (existing is not null)
        {
            existing.Quantity += spec.PackSize;
            if (existing.DurabilityRemaining <= 0 && existing.Quantity > 0)
                existing.DurabilityRemaining = spec.MaxDurability;
            return existing;
        }

        var lure = CreateLureFromSpec(spec, player.Id);
        lures.Add(lure);
        lures.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return lure;
    }

    public static void RollbackBuyRod(Player player, List<FishingRod> rods, RodSpec spec, FishingRod rod)
    {
        player.Money += spec.Price;
        rods.RemoveAll(r => r.Id == rod.Id);
    }

    public static void RollbackBuyReel(Player player, List<FishingReel> reels, ReelSpec spec, FishingReel reel)
    {
        player.Money += spec.Price;
        reels.RemoveAll(r => r.Id == reel.Id);
    }

    public static void RollbackBuyLine(Player player, List<FishingLine> lines, LineSpec spec, FishingLine line)
    {
        player.Money += spec.Price;
        lines.RemoveAll(l => l.Id == line.Id);
    }

    public static void RollbackBuyLure(Player player, List<FishingLure> lures, LureSpec spec)
    {
        player.Money += spec.PackPrice;
        var existing = lures.FirstOrDefault(l => l.Name == spec.Name);
        if (existing is null) return;
        existing.Quantity -= spec.PackSize;
        if (existing.Quantity <= 0)
            lures.RemoveAll(l => l.Id == existing.Id);
    }

    public static void ApplyEquipRodOptimistic(List<FishingRod> rods, FishingRod rod, FishingLoadout loadout)
    {
        foreach (var r in rods) r.IsEquipped = r.Id == rod.Id;
        loadout.RodName = rod.Name;
        loadout.Sensitivity = rod.Sensitivity;
        loadout.CastRange = rod.CastRange;
        loadout.MaxStrength = rod.MaxStrength;
        loadout.RodRequiredLevel = rod.RequiredLevel;
        loadout.RodDurability = rod.Durability;
        loadout.RodGearTier = GearProgressionCatalog.GetRodTier(rod.Name);
    }

    public static void ApplyEquipReelOptimistic(List<FishingReel> reels, FishingReel reel, FishingLoadout loadout)
    {
        foreach (var r in reels) r.IsEquipped = r.Id == reel.Id;
        loadout.ReelName = reel.Name;
        loadout.DragPower = reel.DragPower;
        loadout.GearRatio = reel.GearRatio;
        loadout.LineCapacity = reel.LineCapacity;
        loadout.Smoothness = reel.Smoothness;
        loadout.ReelRequiredLevel = reel.RequiredLevel;
        loadout.ReelDurability = reel.Durability;
    }

    public static void ApplyEquipLineOptimistic(List<FishingLine> lines, FishingLine line, FishingLoadout loadout)
    {
        foreach (var l in lines) l.IsEquipped = l.Id == line.Id;
        loadout.LineName = line.Name;
        loadout.LineStrength = line.LineStrength;
        loadout.LineSensitivity = line.LineSensitivity;
        loadout.LineStealth = line.LineStealth;
        loadout.AbrasionResistance = line.AbrasionResistance;
        loadout.LineDepth = line.TargetDepth;
        loadout.LineRequiredLevel = line.RequiredLevel;
        loadout.LineDurability = line.Durability;
    }

    public static void ApplyEquipLureOptimistic(List<FishingLure> lures, FishingLure lure, FishingLoadout loadout)
    {
        foreach (var l in lures) l.IsEquipped = l.Id == lure.Id;
        if (lure.DurabilityRemaining <= 0 && lure.Quantity > 0)
        {
            var spec = GearCatalog.FindLure(lure.Name);
            lure.DurabilityRemaining = spec?.MaxDurability ?? 10;
        }

        var lureSpec = GearCatalog.FindLure(lure.Name);
        loadout.LureName = lure.Name;
        loadout.Attraction = lure.Attraction;
        loadout.RarityBonus = lure.RarityBonus;
        loadout.LureDepth = lure.TargetDepth;
        loadout.LureDurabilityRemaining = lure.DurabilityRemaining;
        loadout.LureRequiredLevel = lure.RequiredLevel;
        loadout.LureQuantity = lure.Quantity;
        loadout.LureGearTier = lureSpec is not null ? (int)lureSpec.Tier : 1;
        loadout.LureMythicBonus = lureSpec?.MythicBonus ?? 0;
    }

    public static bool TryPrepareRepairRod(Player player, FishingRod rod, bool fullRepair, out int cost, out string error)
    {
        cost = 0;
        error = "";
        if (rod.Durability >= 100)
        {
            error = "耐久已满";
            return false;
        }

        int tier = GearProgressionCatalog.GetGearTier(rod.Name);
        cost = fullRepair
            ? EconomySinks.GearFullRepairCost(rod.MaxStrength, tier)
            : EconomySinks.GearRepairPartialCost(tier);
        if (player.Money < cost)
        {
            error = $"金币不足，修理需 {cost}g";
            return false;
        }

        return true;
    }

    public static bool TryPrepareRepairReel(Player player, FishingReel reel, bool fullRepair, out int cost, out string error)
    {
        cost = 0;
        error = "";
        if (reel.Durability >= 100)
        {
            error = "耐久已满";
            return false;
        }

        int tier = GearProgressionCatalog.GetGearTier(reel.Name);
        cost = fullRepair
            ? EconomySinks.GearFullRepairCost(reel.DragPower, tier)
            : EconomySinks.GearRepairPartialCost(tier);
        if (player.Money < cost)
        {
            error = $"金币不足，修理需 {cost}g";
            return false;
        }

        return true;
    }

    public static bool TryPrepareRepairLine(Player player, FishingLine line, bool fullRepair, out int cost, out string error)
    {
        cost = 0;
        error = "";
        if (line.Durability >= 100)
        {
            error = "耐久已满";
            return false;
        }

        int tier = GearProgressionCatalog.GetGearTier(line.Name);
        cost = fullRepair
            ? EconomySinks.GearFullRepairCost(line.LineStrength, tier)
            : EconomySinks.GearRepairPartialCost(tier);
        if (player.Money < cost)
        {
            error = $"金币不足，修理需 {cost}g";
            return false;
        }

        return true;
    }

    public static void ApplyRepairRodOptimistic(Player player, FishingRod rod, bool fullRepair, int cost, FishingLoadout loadout)
    {
        player.Money -= cost;
        rod.Durability = fullRepair ? 100 : Math.Min(100, rod.Durability + EconomySinks.GearRepairPartialAmount);
        if (rod.IsEquipped)
            loadout.RodDurability = rod.Durability;
    }

    public static void ApplyRepairReelOptimistic(Player player, FishingReel reel, bool fullRepair, int cost, FishingLoadout loadout)
    {
        player.Money -= cost;
        reel.Durability = fullRepair ? 100 : Math.Min(100, reel.Durability + EconomySinks.GearRepairPartialAmount);
        if (reel.IsEquipped)
            loadout.ReelDurability = reel.Durability;
    }

    public static void ApplyRepairLineOptimistic(Player player, FishingLine line, bool fullRepair, int cost, FishingLoadout loadout)
    {
        player.Money -= cost;
        line.Durability = fullRepair ? 100 : Math.Min(100, line.Durability + EconomySinks.GearRepairPartialAmount);
        if (line.IsEquipped)
            loadout.LineDurability = line.Durability;
    }

    public async Task<(bool Ok, string Message)> CommitBuyRodAsync(Player player, FishingRod rod)
    {
        if (await _context.FishingRods.AnyAsync(r => r.PlayerId == player.Id && r.Name == rod.Name))
            return (false, "已拥有该鱼竿");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        if (!await _context.FishingRods.AnyAsync(r => r.Id == rod.Id))
            _context.FishingRods.Add(CloneRod(rod));

        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitBuyReelAsync(Player player, FishingReel reel)
    {
        if (await _context.FishingReels.AnyAsync(r => r.PlayerId == player.Id && r.Name == reel.Name))
            return (false, "已拥有该卷线器");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        if (!await _context.FishingReels.AnyAsync(r => r.Id == reel.Id))
            _context.FishingReels.Add(CloneReel(reel));

        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitBuyLineAsync(Player player, FishingLine line)
    {
        if (await _context.FishingLines.AnyAsync(l => l.PlayerId == player.Id && l.Name == line.Name))
            return (false, "已拥有该鱼线");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        if (!await _context.FishingLines.AnyAsync(l => l.Id == line.Id))
            _context.FishingLines.Add(CloneLine(line));

        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitBuyLureAsync(Player player, LureSpec spec, FishingLure memoryLure)
    {
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        var existing = await _context.FishingLures
            .FirstOrDefaultAsync(l => l.PlayerId == player.Id && l.Name == spec.Name);
        if (existing is not null)
        {
            existing.Quantity = memoryLure.Quantity;
            existing.DurabilityRemaining = memoryLure.DurabilityRemaining;
        }
        else if (!await _context.FishingLures.AnyAsync(l => l.Id == memoryLure.Id))
        {
            _context.FishingLures.Add(CloneLure(memoryLure));
        }

        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitRepairRodAsync(Player player, FishingRod rod)
    {
        var dbRod = await _context.FishingRods.FirstOrDefaultAsync(r => r.Id == rod.Id && r.PlayerId == player.Id);
        if (dbRod is null) return (false, "鱼竿不存在");
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null) return (false, "玩家不存在");
        dbPlayer.Money = player.Money;
        dbRod.Durability = rod.Durability;
        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitRepairReelAsync(Player player, FishingReel reel)
    {
        var dbReel = await _context.FishingReels.FirstOrDefaultAsync(r => r.Id == reel.Id && r.PlayerId == player.Id);
        if (dbReel is null) return (false, "卷线器不存在");
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null) return (false, "玩家不存在");
        dbPlayer.Money = player.Money;
        dbReel.Durability = reel.Durability;
        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitRepairLineAsync(Player player, FishingLine line)
    {
        var dbLine = await _context.FishingLines.FirstOrDefaultAsync(l => l.Id == line.Id && l.PlayerId == player.Id);
        if (dbLine is null) return (false, "鱼线不存在");
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null) return (false, "玩家不存在");
        dbPlayer.Money = player.Money;
        dbLine.Durability = line.Durability;
        await _context.SaveChangesAsync();
        return (true, "");
    }

    private static FishingRod CreateRodFromSpec(RodSpec spec, Guid playerId) => new()
    {
        PlayerId = playerId,
        Name = spec.Name,
        Sensitivity = spec.Sensitivity,
        MaxStrength = spec.MaxStrength,
        CastRange = spec.CastRange,
        RequiredLevel = spec.RequiredLevel,
        Durability = 100
    };

    private static FishingReel CreateReelFromSpec(ReelSpec spec, Guid playerId) => new()
    {
        PlayerId = playerId,
        Name = spec.Name,
        DragPower = spec.DragPower,
        GearRatio = spec.GearRatio,
        LineCapacity = spec.LineCapacity,
        Smoothness = spec.Smoothness,
        RequiredLevel = spec.RequiredLevel,
        Durability = 100
    };

    private static FishingLine CreateLineFromSpec(LineSpec spec, Guid playerId) => new()
    {
        PlayerId = playerId,
        Name = spec.Name,
        LineStrength = spec.LineStrength,
        LineSensitivity = spec.LineSensitivity,
        LineStealth = spec.LineStealth,
        AbrasionResistance = spec.AbrasionResistance,
        TargetDepth = spec.TargetDepth,
        RequiredLevel = spec.RequiredLevel,
        Durability = 100
    };

    private static FishingLure CreateLureFromSpec(LureSpec spec, Guid playerId) => new()
    {
        PlayerId = playerId,
        Name = spec.Name,
        Attraction = spec.Attraction,
        RarityBonus = spec.RarityBonus,
        TargetDepth = spec.TargetDepth,
        DurabilityRemaining = spec.MaxDurability,
        RequiredLevel = spec.RequiredLevel,
        Quantity = spec.PackSize
    };

    private static FishingRod CloneRod(FishingRod rod) => new()
    {
        Id = rod.Id,
        PlayerId = rod.PlayerId,
        Name = rod.Name,
        Sensitivity = rod.Sensitivity,
        MaxStrength = rod.MaxStrength,
        CastRange = rod.CastRange,
        RequiredLevel = rod.RequiredLevel,
        Durability = rod.Durability,
        IsEquipped = rod.IsEquipped,
        IsCrafted = rod.IsCrafted
    };

    private static FishingReel CloneReel(FishingReel reel) => new()
    {
        Id = reel.Id,
        PlayerId = reel.PlayerId,
        Name = reel.Name,
        DragPower = reel.DragPower,
        GearRatio = reel.GearRatio,
        LineCapacity = reel.LineCapacity,
        Smoothness = reel.Smoothness,
        RequiredLevel = reel.RequiredLevel,
        Durability = reel.Durability,
        IsEquipped = reel.IsEquipped,
        IsCrafted = reel.IsCrafted
    };

    private static FishingLine CloneLine(FishingLine line) => new()
    {
        Id = line.Id,
        PlayerId = line.PlayerId,
        Name = line.Name,
        LineStrength = line.LineStrength,
        LineSensitivity = line.LineSensitivity,
        LineStealth = line.LineStealth,
        AbrasionResistance = line.AbrasionResistance,
        TargetDepth = line.TargetDepth,
        RequiredLevel = line.RequiredLevel,
        Durability = line.Durability,
        IsEquipped = line.IsEquipped,
        IsCrafted = line.IsCrafted
    };

    private static FishingLure CloneLure(FishingLure lure) => new()
    {
        Id = lure.Id,
        PlayerId = lure.PlayerId,
        Name = lure.Name,
        Attraction = lure.Attraction,
        RarityBonus = lure.RarityBonus,
        TargetDepth = lure.TargetDepth,
        DurabilityRemaining = lure.DurabilityRemaining,
        RequiredLevel = lure.RequiredLevel,
        Quantity = lure.Quantity,
        IsEquipped = lure.IsEquipped,
        IsCrafted = lure.IsCrafted
    };

    public async Task<FishingRod?> GetEquippedRodAsync(Guid playerId)
    {
        return await _context.FishingRods.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
    }

    public async Task<FishingReel?> GetEquippedReelAsync(Guid playerId)
    {
        return await _context.FishingReels.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
    }

    public async Task<FishingLine?> GetEquippedLineAsync(Guid playerId)
    {
        return await _context.FishingLines.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
    }
}

public sealed class GearBuyRodEventArgs
{
    public required RodSpec Spec { get; init; }
    public required FishingRod Rod { get; init; }
}

public sealed class GearBuyReelEventArgs
{
    public required ReelSpec Spec { get; init; }
    public required FishingReel Reel { get; init; }
}

public sealed class GearBuyLineEventArgs
{
    public required LineSpec Spec { get; init; }
    public required FishingLine Line { get; init; }
}

public sealed class GearBuyLureEventArgs
{
    public required LureSpec Spec { get; init; }
    public required FishingLure Lure { get; init; }
}

