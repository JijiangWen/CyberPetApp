using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public record AlchemyResult(bool Success, string Message, int CatLevelUps = 0);

public enum AlchemyMaterialKind { Fish, Backpack, Gold }

public record AlchemyMaterialStatus(AlchemyMaterialKind Kind, string Label, int Have, int Need)
{
    public bool IsSufficient => Have >= Need;
}

/// <summary>炼金：合成镶嵌宝石与特殊路亚饵，镶嵌到装备槽。</summary>
public class AlchemyService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    private static readonly Dictionary<string, FishingSpot> SpotCache = FishingSpotCatalog.BuildAll();

    public AlchemyService(AppDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<GemAlchemyRecipe> GetGemRecipes() => AlchemyRecipes.GemRecipes;
    public IReadOnlyList<TargetLureRecipe> GetTargetLureRecipes() => AlchemyRecipes.TargetLures;
    public IReadOnlyList<LineAlchemyRecipe> GetLineRecipes() => AlchemyRecipes.LineRecipes;
    public IReadOnlyList<GearCraftRecipe> GetGearCraftRecipes() => GearProgressionCatalog.GearCraftRecipes;

    public Task<List<PlayerGem>> GetGemsAsync(Guid playerId) =>
        _context.PlayerGems.Where(g => g.PlayerId == playerId)
            .OrderByDescending(g => g.IsSocketed).ThenBy(g => g.GemType).ToListAsync();

    public Task<List<PlayerTargetLure>> GetTargetLuresAsync(Guid playerId) =>
        _context.PlayerTargetLures.Where(l => l.PlayerId == playerId)
            .OrderByDescending(l => l.IsEquipped).ThenBy(l => l.RecipeId).ToListAsync();

    public async Task<AlchemyResult> CraftGemAsync(Player player, CyberCat cat, string recipeId)
    {
        var recipe = AlchemyRecipes.FindGem(recipeId);
        if (recipe is null) return new AlchemyResult(false, "未知宝石配方");

        var check = await ValidateAndConsumeMaterialsAsync(player, recipe.Fish, recipe.Materials, recipe.GoldCost);
        if (!check.Success) return check;

        double bonus = AlchemyRecipes.RollGemBonus(_random);
        var gem = new PlayerGem
        {
            PlayerId = player.Id,
            GemType = recipe.GemType,
            BonusValue = bonus
        };
        _context.PlayerGems.Add(gem);
        int catUps = await ApplyCraftXpAsync(player, cat, 15);
        await _context.SaveChangesAsync();

        return new AlchemyResult(true,
            $"炼成【{recipe.DisplayName}】{AlchemyRecipes.GemTypeLabel(recipe.GemType)} +{bonus:P1}（待镶嵌）",
            catUps);
    }

    public async Task<AlchemyResult> CraftGearAsync(
        Player player,
        CyberCat cat,
        string recipeId,
        EquipmentService equipment,
        IReadOnlyList<FishDexEntry> dex,
        Func<string, bool> hasLicense)
    {
        var recipe = GearProgressionCatalog.FindCraftRecipe(recipeId);
        if (recipe is null) return new AlchemyResult(false, "未知装备配方");

        if (!GearProgressionCatalog.MeetsCraftUnlock(recipe, player.FishingLevel, cat.CatLevel, dex, hasLicense))
        {
            string need = GearProgressionCatalog.UnlockShortfall(
                recipe.RequiredFishingLevel, recipe.RequiredCatLevel,
                recipe.RequiredDexSpot, recipe.RequiredDexPercent,
                recipe.RequiredLicenseSpot, recipe.RequiredOverallDexPercent,
                recipe.RequiredMythicCaught, recipe.RequiredAllMythic,
                player.FishingLevel, cat.CatLevel, dex, hasLicense);
            return new AlchemyResult(false, $"未达锻造门槛：{need}");
        }

        if (await equipment.OwnsGearAsync(player.Id, recipe.OutputGearName, recipe.Slot))
            return new AlchemyResult(false, $"已拥有【{recipe.OutputGearName}】");

        var check = await ValidateAndConsumeMaterialsAsync(player, recipe.Fish, recipe.Materials, recipe.GoldCost);
        if (!check.Success) return check;

        bool added = recipe.Slot switch
        {
            GearCraftSlot.Rod => GearCatalog.Rods.FirstOrDefault(r => r.Name == recipe.OutputGearName) is { } rod
                && await equipment.AddCraftedRodAsync(player.Id, rod),
            GearCraftSlot.Reel => GearCatalog.Reels.FirstOrDefault(r => r.Name == recipe.OutputGearName) is { } reel
                && await equipment.AddCraftedReelAsync(player.Id, reel),
            GearCraftSlot.Line => GearCatalog.Lines.FirstOrDefault(l => l.Name == recipe.OutputGearName) is { } line
                && await equipment.AddCraftedLineAsync(player.Id, line),
            GearCraftSlot.Lure => GearCatalog.Lures.FirstOrDefault(l => l.Name == recipe.OutputGearName) is { } lure
                && await equipment.AddCraftedLureAsync(player.Id, lure, Math.Max(1, lure.PackSize)),
            _ => false
        };
        if (!added) return new AlchemyResult(false, "锻造失败：装备目录不匹配");

        int xp = recipe.Tier switch
        {
            GearTier.T10 => 55,
            GearTier.T9 => 48,
            GearTier.T8 => 42,
            GearTier.T7 => 36,
            GearTier.T6 => 32,
            GearTier.T5 => 35,
            GearTier.T4 => 28,
            _ => 22
        };
        int catUps = await ApplyCraftXpAsync(player, cat, xp);
        await _context.SaveChangesAsync();
        return new AlchemyResult(true, $"锻造完成【{recipe.OutputGearName}】· {recipe.Description}", catUps);
    }

    public async Task<AlchemyResult> CraftLineAsync(Player player, CyberCat cat, string recipeId, EquipmentService equipment)
    {
        var recipe = AlchemyRecipes.FindLine(recipeId);
        if (recipe is null) return new AlchemyResult(false, "未知鱼线配方");

        if (await _context.FishingLines.AnyAsync(l => l.PlayerId == player.Id && l.Name == recipe.OutputLineName))
            return new AlchemyResult(false, $"已拥有【{recipe.OutputLineName}】");

        var check = await ValidateAndConsumeMaterialsAsync(player, recipe.Fish, recipe.Materials, recipe.GoldCost);
        if (!check.Success) return check;

        await equipment.AddCraftedLineAsync(player.Id, recipe);
        int catUps = await ApplyCraftXpAsync(player, cat, 20);
        await _context.SaveChangesAsync();

        return new AlchemyResult(true,
            $"炼成【{recipe.OutputLineName}】强度{recipe.LineStrength}kg · 敏锐{recipe.LineSensitivity:0.#} · 隐蔽{recipe.LineStealth:P0}（待装备）",
            catUps);
    }

    public async Task<AlchemyResult> CraftTargetLureAsync(Player player, CyberCat cat, string recipeId)
    {
        var recipe = AlchemyRecipes.FindTargetLure(recipeId);
        if (recipe is null) return new AlchemyResult(false, "未知特殊饵配方");

        var check = await ValidateAndConsumeMaterialsAsync(player, recipe.Fish, recipe.Materials, recipe.GoldCost);
        if (!check.Success) return check;

        _context.PlayerTargetLures.Add(new PlayerTargetLure
        {
            PlayerId = player.Id,
            RecipeId = recipe.Id,
            RemainingUses = recipe.MaxUses
        });
        int catUps = await ApplyCraftXpAsync(player, cat, 25);
        await _context.SaveChangesAsync();

        return new AlchemyResult(true,
            $"炼成【{recipe.DisplayName}】×{recipe.MaxUses} 次 · 仅 [{recipe.SpotName}] 有效 · 目标：{recipe.TargetFishName}",
            catUps);
    }

    public async Task<AlchemyResult> SocketGemAsync(Player player, Guid gemId, GearGemSlot slot)
    {
        var gem = await _context.PlayerGems
            .FirstOrDefaultAsync(g => g.Id == gemId && g.PlayerId == player.Id);
        if (gem is null) return new AlchemyResult(false, "宝石不存在");
        if (gem.IsSocketed) return new AlchemyResult(false, "宝石已镶嵌");

        var recipe = AlchemyRecipes.GemRecipes.FirstOrDefault(r => r.GemType == gem.GemType);
        if (recipe is not null && recipe.SocketSlot != slot)
            return new AlchemyResult(false, $"{AlchemyRecipes.GemTypeLabel(gem.GemType)}宝石只能镶在{AlchemyRecipes.SlotLabel(recipe.SocketSlot)}");

        if (player.Money < EconomySinks.AlchemySocketFee)
            return new AlchemyResult(false, $"镶嵌需 {EconomySinks.AlchemySocketFee}g");

        var (gearId, ok, msg) = await ResolveEquippedGearAsync(player.Id, slot);
        if (!ok) return new AlchemyResult(false, msg);

        player.Money -= EconomySinks.AlchemySocketFee;
        await PersistMoneyAsync(player);

        // 旧宝石销毁
        var old = await _context.PlayerGems
            .FirstOrDefaultAsync(g => g.PlayerId == player.Id && g.IsSocketed && g.SocketedSlot == slot);
        if (old is not null)
            _context.PlayerGems.Remove(old);

        gem.IsSocketed = true;
        gem.SocketedSlot = slot;
        gem.SocketedGearId = gearId;
        await _context.SaveChangesAsync();

        return new AlchemyResult(true,
            $"已镶嵌至{AlchemyRecipes.SlotLabel(slot)} · {AlchemyRecipes.GemTypeLabel(gem.GemType)} +{gem.BonusValue:P1}（扣 {EconomySinks.AlchemySocketFee}g）");
    }

    public async Task<AlchemyResult> EquipTargetLureAsync(Guid playerId, Guid lureId)
    {
        var lures = await _context.PlayerTargetLures.Where(l => l.PlayerId == playerId).ToListAsync();
        var target = lures.FirstOrDefault(l => l.Id == lureId);
        if (target is null) return new AlchemyResult(false, "特殊饵不存在");
        if (target.RemainingUses <= 0) return new AlchemyResult(false, "特殊饵已用尽");

        foreach (var l in lures) l.IsEquipped = l.Id == lureId;
        await _context.SaveChangesAsync();

        var recipe = AlchemyRecipes.FindTargetLure(target.RecipeId);
        return new AlchemyResult(true, $"已装备【{recipe?.DisplayName ?? target.RecipeId}】剩余 {target.RemainingUses} 次");
    }

    public async Task<AlchemyResult> UnequipTargetLureAsync(Guid playerId)
    {
        var lures = await _context.PlayerTargetLures.Where(l => l.PlayerId == playerId && l.IsEquipped).ToListAsync();
        foreach (var l in lures) l.IsEquipped = false;
        await _context.SaveChangesAsync();
        return new AlchemyResult(true, "已卸下特殊饵");
    }

    /// <summary>成功钓获后扣特殊饵耐久（由 FishingManager 回调）。</summary>
    public async Task ConsumeTargetLureUseAsync(Guid playerId, Guid lureId)
    {
        var lure = await _context.PlayerTargetLures
            .FirstOrDefaultAsync(l => l.Id == lureId && l.PlayerId == playerId);
        if (lure is null || lure.RemainingUses <= 0) return;

        lure.RemainingUses--;
        if (lure.RemainingUses <= 0)
            lure.IsEquipped = false;
        await _context.SaveChangesAsync();
    }

    public int CountFish(Player player, FishRequirement req)
    {
        return player.FishBackpack.Count(f =>
            MatchesRequirement(f, req));
    }

    public int CountMaterial(Player player, string itemName) =>
        player.Backpack.GetValueOrDefault(itemName);

    public IReadOnlyList<AlchemyMaterialStatus> GetMaterialStatus(
        Player player,
        IReadOnlyList<FishRequirement> fishReqs,
        IReadOnlyList<MaterialRequirement> matReqs,
        int goldCost)
    {
        var lines = new List<AlchemyMaterialStatus>();
        foreach (var req in fishReqs)
            lines.Add(new AlchemyMaterialStatus(AlchemyMaterialKind.Fish, FormatFishRequirementLabel(req),
                CountFish(player, req), req.Count));
        foreach (var req in matReqs)
            lines.Add(new AlchemyMaterialStatus(AlchemyMaterialKind.Backpack, req.ItemName,
                CountMaterial(player, req.ItemName), req.Count));
        lines.Add(new AlchemyMaterialStatus(AlchemyMaterialKind.Gold, "金币", player.Money, goldCost));
        return lines;
    }

    public string? GetCraftShortfall(
        Player player,
        IReadOnlyList<FishRequirement> fishReqs,
        IReadOnlyList<MaterialRequirement> matReqs,
        int goldCost)
    {
        foreach (var req in fishReqs)
        {
            int have = CountFish(player, req);
            if (have < req.Count)
                return $"鱼材料不足：{FormatFishRequirementLabel(req)}（{have}/{req.Count}）";
        }

        foreach (var req in matReqs)
        {
            int have = CountMaterial(player, req.ItemName);
            if (have < req.Count)
                return $"背包材料不足：{req.ItemName}（{have}/{req.Count}）";
        }

        if (player.Money < goldCost)
            return $"金币不足（{player.Money}/{goldCost}g）";

        return null;
    }

    public static string FormatFishRequirementLabel(FishRequirement req)
    {
        var spot = req.SpotName is null ? "" : $"[{req.SpotName}] ";
        var fishLabel = req.FishName == "*" ? $"{RarityLabel(req.MinRarity)}任意鱼" : req.FishName;
        return $"{spot}{fishLabel}".Trim();
    }

    private static string RarityLabel(FishRarity? rarity) => rarity switch
    {
        FishRarity.Rare => "稀有",
        FishRarity.Epic => "史诗",
        FishRarity.Legendary => "传说",
        _ => ""
    };

    private async Task<AlchemyResult> ValidateAndConsumeMaterialsAsync(
        Player player,
        IReadOnlyList<FishRequirement> fishReqs,
        IReadOnlyList<MaterialRequirement> matReqs,
        int goldCost)
    {
        if (player.Money < goldCost)
            return new AlchemyResult(false, $"金币不足，需 {goldCost}g");

        foreach (var req in fishReqs)
        {
            if (CountFish(player, req) < req.Count)
                return new AlchemyResult(false, $"鱼材料不足：{FormatFishReq(req)}（当前 {CountFish(player, req)}/{req.Count}）");
        }

        foreach (var req in matReqs)
        {
            if (CountMaterial(player, req.ItemName) < req.Count)
                return new AlchemyResult(false, $"材料不足：{req.ItemName} ×{req.Count}");
        }

        foreach (var req in fishReqs)
            RemoveFishFromPlayer(player, req);

        foreach (var req in matReqs)
            RemoveBackpackItem(player, req.ItemName, req.Count);

        player.Money -= goldCost;
        await PersistMoneyAsync(player);
        return new AlchemyResult(true, "");
    }

    private void RemoveFishFromPlayer(Player player, FishRequirement req)
    {
        int left = req.Count;
        foreach (var fish in player.FishBackpack.ToList())
        {
            if (left <= 0) break;
            if (!MatchesRequirement(fish, req)) continue;

            var dbFish = _context.Fishes.Local.FirstOrDefault(f => f.Id == fish.Id)
                ?? _context.Fishes.FirstOrDefault(f => f.Id == fish.Id && f.PlayerId == player.Id);
            if (dbFish is not null) _context.Fishes.Remove(dbFish);
            player.FishBackpack.Remove(fish);
            left--;
        }
    }

    private void RemoveBackpackItem(Player player, string itemName, int count)
    {
        var item = _context.BackpackItems.Local
                       .FirstOrDefault(b => b.PlayerId == player.Id && b.ItemName == itemName)
                   ?? _context.BackpackItems.FirstOrDefault(b => b.PlayerId == player.Id && b.ItemName == itemName);
        if (item is null) return;

        item.Quantity -= count;
        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);

        int left = player.Backpack.GetValueOrDefault(itemName) - count;
        if (left <= 0) player.Backpack.Remove(itemName);
        else player.Backpack[itemName] = left;
    }

    private static bool MatchesRequirement(Fish fish, FishRequirement req)
    {
        if (!string.IsNullOrEmpty(req.FishName) && req.FishName != "*" && BaseFishName(fish.Name) != req.FishName)
            return false;
        if (req.MinRarity.HasValue && fish.Rarity < req.MinRarity.Value) return false;
        if (req.SpotName is not null && !FishBelongsToSpot(fish.Name, req.SpotName)) return false;
        return true;
    }

    private static string BaseFishName(string name) =>
        name.StartsWith("超规格·", StringComparison.Ordinal) ? name["超规格·".Length..] : name;

    private static bool FishBelongsToSpot(string fishName, string spotName)
    {
        if (!SpotCache.TryGetValue(spotName, out var spot)) return false;
        var baseName = BaseFishName(fishName);
        return spot.FishTable.Any(t => t.Name == baseName && t.TargetLureRecipeId is null);
    }

    private static string FormatFishReq(FishRequirement req)
    {
        var spot = req.SpotName is null ? "" : $"[{req.SpotName}] ";
        var rarity = req.MinRarity?.ToString() ?? "";
        var fishLabel = req.FishName == "*" ? "任意鱼" : req.FishName;
        return $"{spot}{rarity}{fishLabel} ×{req.Count}";
    }

    private async Task<(Guid? GearId, bool Ok, string Message)> ResolveEquippedGearAsync(Guid playerId, GearGemSlot slot)
    {
        switch (slot)
        {
            case GearGemSlot.Rod:
                var rod = await _context.FishingRods.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
                return rod is null ? (null, false, "请先装备鱼竿") : (rod.Id, true, "");
            case GearGemSlot.Reel:
                var reel = await _context.FishingReels.FirstOrDefaultAsync(r => r.PlayerId == playerId && r.IsEquipped);
                return reel is null ? (null, false, "请先装备渔轮") : (reel.Id, true, "");
            case GearGemSlot.Line:
                var line = await _context.FishingLines.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
                return line is null ? (null, false, "请先装备鱼线") : (line.Id, true, "");
            case GearGemSlot.Lure:
                var lure = await _context.FishingLures.FirstOrDefaultAsync(l => l.PlayerId == playerId && l.IsEquipped);
                return lure is null ? (null, false, "请先装备拟饵") : (lure.Id, true, "");
            default:
                return (null, false, "未知槽位");
        }
    }

    private async Task<int> ApplyCraftXpAsync(Player player, CyberCat cat, int xp)
    {
        cat.ApplyActivityCost(CatActivityType.Cooking, default); // 复用轻度消耗
        await _context.SaveChangesAsync();
        return 0; // 猫 XP 由 UI 层 CatProgressionService 可选追加
    }

    private async Task PersistMoneyAsync(Player player)
    {
        var tracked = await _context.Players.FindAsync(player.Id);
        if (tracked is not null)
            tracked.Money = player.Money;
    }
}
