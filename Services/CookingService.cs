using CyberPetApp.Data;

using CyberPetApp.Models;

using Microsoft.EntityFrameworkCore;



namespace CyberPetApp.Services;



public record CookResult(bool Success, string Message, int LevelUps = 0);



/// <summary>

/// 烹饪服务：把背包里的鱼按食谱转化为宠物饲料（BackpackItem），扣加工费并给烹饪经验。

/// </summary>

public class CookingService

{

    private readonly AppDbContext _context;

    private readonly CatBuffService _catBuffService;



    public CookingService(AppDbContext context, CatBuffService catBuffService)

    {

        _context = context;

        _catBuffService = catBuffService;

    }



    /// <summary>烹饪一条鱼：校验等级与加工费 → 删鱼 → 产物入背包 → 加烹饪经验。</summary>

    public async Task<CookResult> CookFishAsync(

        Player player, CyberCat cat, Fish fish, HouseBuffs houseBuffs = default, string? recipeId = null)

    {

        var recipe = ResolveRecipe(fish.Rarity, recipeId, player.CookingLevel);

        if (recipe is null)

            return new CookResult(false, "未找到可用食谱（检查烹饪等级）");



        int fee = EconomySinks.CookingProcessingFee(fish.Rarity);

        var dbPlayer = await _context.Players.FindAsync(player.Id);

        if (dbPlayer is null)

            return new CookResult(false, "玩家不存在");

        if (dbPlayer.Money < fee)

            return new CookResult(false, $"金币不足，烹饪加工费需 {fee}g");



        var dbFish = await _context.Fishes.FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);

        if (dbFish is null)

            return new CookResult(false, "这条鱼不存在（可能已卖出/已烹饪）");



        dbPlayer.Money -= fee;

        player.Money = dbPlayer.Money;

        _context.Fishes.Remove(dbFish);

        await UpsertBackpackItemAsync(player.Id, recipe.FoodName, +1);



        int xpGain = ScaleCookingXp(recipe.Xp, houseBuffs);

        int ups = player.AddCookingXp(xpGain);

        player.CookCount += 1;

        cat.ApplyActivityCost(CatActivityType.Cooking, houseBuffs);

        await PersistSkillsAsync(player);

        await _context.SaveChangesAsync();



        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);

        player.Backpack[recipe.FoodName] = player.Backpack.GetValueOrDefault(recipe.FoodName) + 1;



        return new CookResult(true, $"{fish.Name} → {recipe.FoodName} (-{fee}g, +{xpGain} xp)", ups);

    }



    /// <summary>批量烹饪；commonOnlyOnly 时仅把 Common 鱼做成默认生鱼片。</summary>

    public async Task<CookResult> CookAllAsync(

        Player player, CyberCat cat, HouseBuffs houseBuffs = default, bool commonOnlyOnly = false)

    {

        var cookable = player.FishBackpack.Where(f =>

        {

            if (commonOnlyOnly && f.Rarity != FishRarity.Common) return false;

            var recipe = commonOnlyOnly ? CookBook.DefaultFor(f.Rarity) : CookBook.DefaultFor(f.Rarity);

            return player.CookingLevel >= recipe.RequiredCookingLevel;

        }).ToList();



        if (cookable.Count == 0)

            return new CookResult(false, commonOnlyOnly

                ? "没有可批量烹饪的普通鱼（需解锁生鱼片 Lv.1）"

                : "没有可烹饪的鱼（检查食谱解锁等级与金币）");



        int totalFee = cookable.Sum(f => EconomySinks.CookingProcessingFee(f.Rarity));

        var dbPlayer = await _context.Players.FindAsync(player.Id);

        if (dbPlayer is null)

            return new CookResult(false, "玩家不存在");

        if (dbPlayer.Money < totalFee)

            return new CookResult(false, $"金币不足，批量加工费共需 {totalFee}g");



        dbPlayer.Money -= totalFee;

        player.Money = dbPlayer.Money;



        int totalXp = 0, ups = 0, cooked = 0;

        foreach (var fish in cookable)

        {

            var recipe = CookBook.DefaultFor(fish.Rarity);

            var dbFish = await _context.Fishes.FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);

            if (dbFish is null) continue;

            cooked++;



            _context.Fishes.Remove(dbFish);

            await UpsertBackpackItemAsync(player.Id, recipe.FoodName, +1);



            int xpGain = ScaleCookingXp(recipe.Xp, houseBuffs);

            totalXp += xpGain;

            ups += player.AddCookingXp(xpGain);



            player.FishBackpack.RemoveAll(f => f.Id == fish.Id);

            player.Backpack[recipe.FoodName] = player.Backpack.GetValueOrDefault(recipe.FoodName) + 1;

            cat.ApplyActivityCost(CatActivityType.Cooking, houseBuffs);

        }



        player.CookCount += cooked;

        await PersistSkillsAsync(player);

        await _context.SaveChangesAsync();

        string scope = commonOnlyOnly ? "普通鱼→生鱼片" : "全部可烹饪";

        return new CookResult(true, $"批量烹饪 {cooked} 条（{scope}，-{totalFee}g，+{totalXp} xp）", ups);

    }



    /// <summary>喂食：扣背包 → 即时 FeedFood → 施加持续 Buff。</summary>

    /// <summary>消耗烹饪食物并施加 Buff；猫 FeedFood 由调用方在 catStateLock 内执行。</summary>
    public async Task<(Food? Food, string? Message)> FeedCookedFoodAsync(Player player, string foodName)

    {

        var recipe = CookBook.RecipeByName(foodName);

        if (recipe is null)

            return (null, null);



        var food = await ConsumeCookedFoodAsync(player, foodName);

        if (food is null)

            return (null, $"背包里没有 {foodName}");



        await _catBuffService.ApplyRecipeBuffsAsync(player.Id, recipe);

        string buffHint = recipe.AllBuffs().Any()

            ? $" · 施加 Buff：{recipe.BuffEffectLabel}"

            : "";

        return (food, $"喂食 [{foodName}]{buffHint}");

    }



    private static CookingRecipe? ResolveRecipe(FishRarity rarity, string? recipeId, int cookingLevel)

    {

        if (!string.IsNullOrEmpty(recipeId))

        {

            var picked = CookBook.RecipeById(recipeId);

            if (picked is null || picked.Input != rarity)

                return null;

            if (cookingLevel < picked.RequiredCookingLevel)

                return null;

            return picked;

        }



        var def = CookBook.DefaultFor(rarity);

        return cookingLevel >= def.RequiredCookingLevel ? def : null;

    }



    private static int ScaleCookingXp(int baseXp, HouseBuffs houseBuffs) =>

        Math.Max(1, (int)Math.Round(baseXp * houseBuffs.CookingXpMultiplier));



    /// <summary>喂食消耗一份烹饪食物（从 BackpackItem 扣减），返回对应 Food；没有库存返回 null。</summary>

    public async Task<Food?> ConsumeCookedFoodAsync(Player player, string foodName)

    {

        var food = CookBook.FoodByName(foodName);

        if (food is null) return null;



        var item = await _context.BackpackItems

            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == foodName);

        if (item is null || item.Quantity <= 0) return null;



        item.Quantity--;

        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);

        await _context.SaveChangesAsync();



        int left = player.Backpack.GetValueOrDefault(foodName) - 1;

        if (left <= 0) player.Backpack.Remove(foodName);

        else player.Backpack[foodName] = left;



        return food;

    }



    private async Task UpsertBackpackItemAsync(Guid playerId, string itemName, int delta)

    {

        var item = _context.BackpackItems.Local

                       .FirstOrDefault(b => b.PlayerId == playerId && b.ItemName == itemName)

                   ?? await _context.BackpackItems

                       .FirstOrDefaultAsync(b => b.PlayerId == playerId && b.ItemName == itemName);



        if (item is null)

            _context.BackpackItems.Add(new BackpackItem { PlayerId = playerId, ItemName = itemName, Quantity = delta });

        else

            item.Quantity += delta;

    }



    private async Task PersistSkillsAsync(Player player)

    {

        var tracked = await _context.Players.FindAsync(player.Id);

        if (tracked is not null && !ReferenceEquals(tracked, player))

        {

            tracked.CookingLevel = player.CookingLevel;

            tracked.CookingXp = player.CookingXp;

            tracked.CookCount = player.CookCount;

        }

    }

    // ── 乐观 UI ──

    public sealed record CookFishSnapshot(
        int Money, int CookingXp, int CookingLevel, int CookCount, int FoodQty,
        int CatHunger, int CatEnergy, int CatHappiness, int CatThirst, int CatHealth);

    public sealed record CookAllSnapshot(
        int Money, int CookingXp, int CookingLevel, int CookCount,
        Dictionary<string, int> FoodQtyByName,
        int CatHunger, int CatEnergy, int CatHappiness, int CatThirst, int CatHealth,
        List<Fish> CookedFish);

    public static bool TryPrepareCookFish(
        Player player, Fish fish, string? recipeId,
        out CookingRecipe? recipe, out int fee, out string error)
    {
        recipe = ResolveRecipe(fish.Rarity, recipeId, player.CookingLevel);
        error = "";
        fee = 0;
        if (recipe is null)
        {
            error = "未找到可用食谱（检查烹饪等级）";
            return false;
        }

        fee = EconomySinks.CookingProcessingFee(fish.Rarity);
        if (player.Money < fee)
        {
            error = $"金币不足，烹饪加工费需 {fee}g";
            return false;
        }

        if (player.FishBackpack.All(f => f.Id != fish.Id))
        {
            error = "这条鱼不存在（可能已卖出/已烹饪）";
            return false;
        }

        return true;
    }

    public static (int LevelUps, int XpGain, CookFishSnapshot Snapshot) ApplyCookFishOptimistic(
        Player player, CyberCat cat, Fish fish, CookingRecipe recipe, int fee, HouseBuffs houseBuffs)
    {
        var snap = new CookFishSnapshot(
            player.Money, player.CookingXp, player.CookingLevel, player.CookCount,
            player.Backpack.GetValueOrDefault(recipe.FoodName),
            cat.Hunger, cat.Energy, cat.Happiness, cat.Thirst, cat.Health);

        player.Money -= fee;
        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);
        player.Backpack[recipe.FoodName] = player.Backpack.GetValueOrDefault(recipe.FoodName) + 1;
        int xpGain = ScaleCookingXp(recipe.Xp, houseBuffs);
        int ups = player.AddCookingXp(xpGain);
        player.CookCount += 1;
        cat.ApplyActivityCost(CatActivityType.Cooking, houseBuffs);
        return (ups, xpGain, snap);
    }

    public static void RollbackCookFishOptimistic(
        Player player, CyberCat cat, Fish fish, CookingRecipe recipe, CookFishSnapshot snap)
    {
        player.Money = snap.Money;
        player.CookingXp = snap.CookingXp;
        player.CookingLevel = snap.CookingLevel;
        player.CookCount = snap.CookCount;
        if (snap.FoodQty <= 0) player.Backpack.Remove(recipe.FoodName);
        else player.Backpack[recipe.FoodName] = snap.FoodQty;
        if (player.FishBackpack.All(f => f.Id != fish.Id))
            player.FishBackpack.Add(fish);
        cat.Hunger = snap.CatHunger;
        cat.Energy = snap.CatEnergy;
        cat.Happiness = snap.CatHappiness;
        cat.Thirst = snap.CatThirst;
        cat.Health = snap.CatHealth;
    }

    public static bool TryPrepareCookAll(
        Player player, bool commonOnlyOnly,
        out List<Fish> cookable, out int totalFee, out string error)
    {
        error = "";
        totalFee = 0;
        cookable = player.FishBackpack.Where(f =>
        {
            if (commonOnlyOnly && f.Rarity != FishRarity.Common) return false;
            var recipe = CookBook.DefaultFor(f.Rarity);
            return player.CookingLevel >= recipe.RequiredCookingLevel;
        }).ToList();

        if (cookable.Count == 0)
        {
            error = commonOnlyOnly
                ? "没有可批量烹饪的普通鱼（需解锁生鱼片 Lv.1）"
                : "没有可烹饪的鱼（检查食谱解锁等级与金币）";
            return false;
        }

        totalFee = cookable.Sum(f => EconomySinks.CookingProcessingFee(f.Rarity));
        if (player.Money < totalFee)
        {
            error = $"金币不足，批量加工费共需 {totalFee}g";
            return false;
        }

        return true;
    }

    public static (int LevelUps, int TotalXp, int CookedCount, CookAllSnapshot Snapshot) ApplyCookAllOptimistic(
        Player player, CyberCat cat, List<Fish> cookable, int totalFee, HouseBuffs houseBuffs, bool commonOnlyOnly)
    {
        var foodQty = player.Backpack.ToDictionary(kv => kv.Key, kv => kv.Value);
        var snap = new CookAllSnapshot(
            player.Money, player.CookingXp, player.CookingLevel, player.CookCount,
            foodQty,
            cat.Hunger, cat.Energy, cat.Happiness, cat.Thirst, cat.Health,
            []);

        player.Money -= totalFee;
        int totalXp = 0, ups = 0, cooked = 0;
        foreach (var fish in cookable)
        {
            var recipe = CookBook.DefaultFor(fish.Rarity);
            player.FishBackpack.RemoveAll(f => f.Id == fish.Id);
            player.Backpack[recipe.FoodName] = player.Backpack.GetValueOrDefault(recipe.FoodName) + 1;
            int xpGain = ScaleCookingXp(recipe.Xp, houseBuffs);
            totalXp += xpGain;
            ups += player.AddCookingXp(xpGain);
            cat.ApplyActivityCost(CatActivityType.Cooking, houseBuffs);
            snap.CookedFish.Add(fish);
            cooked++;
        }

        player.CookCount += cooked;
        return (ups, totalXp, cooked, snap);
    }

    public static void RollbackCookAllOptimistic(Player player, CyberCat cat, CookAllSnapshot snap)
    {
        player.Money = snap.Money;
        player.CookingXp = snap.CookingXp;
        player.CookingLevel = snap.CookingLevel;
        player.CookCount = snap.CookCount;
        player.Backpack.Clear();
        foreach (var kv in snap.FoodQtyByName)
            player.Backpack[kv.Key] = kv.Value;
        foreach (var fish in snap.CookedFish)
        {
            if (player.FishBackpack.All(f => f.Id != fish.Id))
                player.FishBackpack.Add(fish);
        }

        cat.Hunger = snap.CatHunger;
        cat.Energy = snap.CatEnergy;
        cat.Happiness = snap.CatHappiness;
        cat.Thirst = snap.CatThirst;
        cat.Health = snap.CatHealth;
    }

    public async Task<(bool Ok, string Message)> CommitCookFishAsync(
        Player player, Fish fish, CookingRecipe recipe)
    {
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null) return (false, "玩家不存在");

        var dbFish = await _context.Fishes.FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is null) return (false, "这条鱼不存在（可能已卖出/已烹饪）");

        dbPlayer.Money = player.Money;
        _context.Fishes.Remove(dbFish);
        int targetQty = player.Backpack.GetValueOrDefault(recipe.FoodName);
        var backpackItem = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == recipe.FoodName);
        if (backpackItem is null)
            _context.BackpackItems.Add(new BackpackItem { PlayerId = player.Id, ItemName = recipe.FoodName, Quantity = targetQty });
        else
            backpackItem.Quantity = targetQty;
        await PersistSkillsAsync(player);
        await _context.SaveChangesAsync();
        return (true, "");
    }

    public async Task<(bool Ok, string Message)> CommitCookAllAsync(
        Player player, CookAllSnapshot snap)
    {
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null) return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        foreach (var fish in snap.CookedFish)
        {
            var dbFish = await _context.Fishes.FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);
            if (dbFish is not null) _context.Fishes.Remove(dbFish);
        }

        foreach (var kv in player.Backpack)
        {
            var item = await _context.BackpackItems
                .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == kv.Key);
            if (item is null)
                _context.BackpackItems.Add(new BackpackItem { PlayerId = player.Id, ItemName = kv.Key, Quantity = kv.Value });
            else
                item.Quantity = kv.Value;
        }

        var trackedItems = await _context.BackpackItems.Where(b => b.PlayerId == player.Id).ToListAsync();
        foreach (var item in trackedItems)
        {
            if (!player.Backpack.ContainsKey(item.ItemName))
                _context.BackpackItems.Remove(item);
        }

        await PersistSkillsAsync(player);
        await _context.SaveChangesAsync();
        return (true, "");
    }

    public static bool TryPrepareFeedCooked(
        Player player, string foodName, out CookingRecipe? recipe, out Food? food, out string error)
    {
        recipe = CookBook.RecipeByName(foodName);
        food = CookBook.FoodByName(foodName);
        error = "";
        if (recipe is null || food is null)
        {
            error = "未知料理";
            return false;
        }

        if (!player.Backpack.TryGetValue(foodName, out int qty) || qty <= 0)
        {
            error = $"背包里没有 {foodName}";
            return false;
        }

        return true;
    }

    public static int ApplyFeedCookedOptimistic(Player player, string foodName)
    {
        int prev = player.Backpack.GetValueOrDefault(foodName);
        int left = prev - 1;
        if (left <= 0) player.Backpack.Remove(foodName);
        else player.Backpack[foodName] = left;
        return prev;
    }

    public static void RollbackFeedCookedOptimistic(Player player, string foodName, int prevQty)
    {
        if (prevQty <= 0) player.Backpack.Remove(foodName);
        else player.Backpack[foodName] = prevQty;
    }

    public async Task<(bool Ok, string Message)> CommitFeedCookedAsync(Player player, string foodName, CookingRecipe recipe)
    {
        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == foodName);
        int targetQty = player.Backpack.GetValueOrDefault(foodName);
        if (targetQty <= 0)
        {
            if (item is not null) _context.BackpackItems.Remove(item);
        }
        else
        {
            if (item is null) return (false, "背包同步失败");
            item.Quantity = targetQty;
        }

        await _catBuffService.ApplyRecipeBuffsAsync(player.Id, recipe);
        await _context.SaveChangesAsync();
        return (true, "");
    }

}


