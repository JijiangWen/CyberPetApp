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

    public async Task WearEquippedGearAsync(Guid playerId, bool lineBreakExtra, string? spotName = null)
    {
        var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var reel = await _context.FishingReels.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
        var fishingLine = await _context.FishingLines.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
        var rng = Random.Shared;
        int wear = rng.Next(1, 4) + GearProgressionCatalog.SpotExtraWear(spotName);
        if (lineBreakExtra) wear += 5;

        if (rod is not null)
        {
            var rodAffix = GearCatalog.FindRod(rod.Name)?.Affix ?? GearAffix.Balanced;
            rod.Durability = Math.Max(0, rod.Durability - (int)Math.Ceiling(wear * GearAffixHelper.WearMultiplier(rodAffix)));
        }
        if (reel is not null)
        {
            var reelAffix = GearCatalog.FindReel(reel.Name)?.Affix ?? GearAffix.Balanced;
            reel.Durability = Math.Max(0, reel.Durability - (int)Math.Ceiling(wear * GearAffixHelper.WearMultiplier(reelAffix)));
        }
        if (fishingLine is not null)
        {
            int lineWear = lineBreakExtra
                ? Math.Max(1, (int)Math.Round(wear * (1 - fishingLine.AbrasionResistance)))
                : wear;
            fishingLine.Durability = Math.Max(0, fishingLine.Durability - lineWear);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<(bool Ok, string Message)> RepairRodAsync(Player player, Guid rodId, bool fullRepair)
    {
        var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.Id == rodId && r.PlayerId == player.Id);
        if (rod is null) return (false, "鱼竿不存在");
        if (rod.Durability >= 100) return (false, "耐久已满");

        int tier = GearProgressionCatalog.GetGearTier(rod.Name);
        int cost = fullRepair
            ? EconomySinks.GearFullRepairCost(rod.MaxStrength, tier)
            : EconomySinks.GearRepairPartialCost(tier);
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
        int cost = fullRepair
            ? EconomySinks.GearFullRepairCost(reel.DragPower, tier)
            : EconomySinks.GearRepairPartialCost(tier);
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
        int cost = fullRepair
            ? EconomySinks.GearFullRepairCost(line.LineStrength, tier)
            : EconomySinks.GearRepairPartialCost(tier);
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
}

