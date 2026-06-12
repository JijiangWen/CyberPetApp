using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    public sealed record CookFishCommitArgs(
        Fish Fish, string RecipeId, CookingRecipe Recipe, int Fee, int XpGain, int LevelUps,
        CookingService.CookFishSnapshot Snapshot, string Message);

    public sealed record CookAllCommitArgs(
        bool CommonOnlyOnly, CookingService.CookAllSnapshot Snapshot,
        int TotalFee, int TotalXp, int CookedCount, int LevelUps, string Message);

    public sealed record FeedCookedCommitArgs(string FoodName, CookingRecipe Recipe, int PrevQty, string? CatLevelMsg, string Message);

    private Task FinalizeCookFish(CookFishCommitArgs args)
    {
        cookMessage = args.Message;
        tickGeneration++;
        BindGameSession();
        TryRefreshSidebarVitals();
        _ = PersistCookFishAsync(args);
        return Task.CompletedTask;
    }

    private Task FinalizeCookAll(CookAllCommitArgs args)
    {
        cookMessage = args.Message;
        tickGeneration++;
        BindGameSession();
        TryRefreshSidebarVitals();
        _ = PersistCookAllAsync(args);
        return Task.CompletedTask;
    }

    private Task FinalizeFeedCookedFood(FeedCookedCommitArgs args)
    {
        feedMessage = args.Message;
        tickGeneration++;
        BindGameSession();
        TryRefreshSidebarVitals();
        _ = PersistFeedCookedFoodAsync(args);
        return Task.CompletedTask;
    }

    private async Task PersistCookFishAsync(CookFishCommitArgs args)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _cookingService.CommitCookFishAsync(player!, args.Fish, args.Recipe);
                if (!ok)
                {
                    CookingService.RollbackCookFishOptimistic(player!, cat, args.Fish, args.Recipe, args.Snapshot);
                    cookMessage = msg;
                }
                else
                {
                    await _cyberCatService.SaveAsync(cat);
                    await _playerService.SaveProgressAsync(player!);
                    await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());
                }
            });
            tickGeneration++;
            BindGameSession();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistCookFish failed for {FishName}", args.Fish.Name);
        }
    }

    private async Task PersistCookAllAsync(CookAllCommitArgs args)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _cookingService.CommitCookAllAsync(player!, args.Snapshot);
                if (!ok)
                {
                    CookingService.RollbackCookAllOptimistic(player!, cat, args.Snapshot);
                    cookMessage = msg;
                }
                else
                {
                    await _cyberCatService.SaveAsync(cat);
                    await _playerService.SaveProgressAsync(player!);
                    await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());
                }
            });
            tickGeneration++;
            BindGameSession();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistCookAll failed");
        }
    }

    private async Task PersistFeedCookedFoodAsync(FeedCookedCommitArgs args)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _cookingService.CommitFeedCookedAsync(player!, args.FoodName, args.Recipe);
                if (!ok)
                {
                    CookingService.RollbackFeedCookedOptimistic(player!, args.FoodName, args.PrevQty);
                    feedMessage = msg;
                }
                else
                {
                    activeFoodBuffs = await _catBuffService.LoadActiveAsync(player!.Id);
                    await _catProgressionService.SaveAsync(cat);
                    await _playerService.SaveProgressAsync(player);
                    await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
                }
            });
            tickGeneration++;
            BindGameSession();
            TryRefreshSidebarVitals();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistFeedCookedFood failed for {FoodName}", args.FoodName);
        }
    }

    private Task FeedCookedFood(string foodName)
    {
        if (!CookingService.TryPrepareFeedCooked(player!, foodName, out var recipe, out var food, out var error))
        {
            feedMessage = error;
            return InvokeAsync(StateHasChanged);
        }

        int prevQty = CookingService.ApplyFeedCookedOptimistic(player!, foodName);
        string? catLevelMsg = null;
        lock (catStateLock)
        {
            cat.FeedFood(food!);
            int catXp = CatFishingStatsHelper.XpFromFood(foodName);
            (_, catLevelMsg) = _catProgressionService.AddXp(cat, catXp);
        }

        player!.LifetimeFeedCount++;
        string buffHint = recipe!.AllBuffs().Any() ? $" · 施加 Buff：{recipe.BuffEffectLabel}" : "";
        string msg = string.IsNullOrEmpty(catLevelMsg)
            ? $"喂食 [{foodName}]{buffHint}"
            : $"喂食 [{foodName}]{buffHint} · {catLevelMsg}";

        return FinalizeFeedCookedFood(new FeedCookedCommitArgs(foodName, recipe, prevQty, catLevelMsg, msg));
    }

    private Task FeedShopFood(Food food)
    {
        if (!PlayerService.TryPrepareConsumeBackpack(player!, food.Name, out var error))
        {
            feedMessage = error;
            return InvokeAsync(StateHasChanged);
        }

        PlayerService.ApplyConsumeBackpackOptimistic(player!, food.Name);
        string? catLevelMsg;
        lock (catStateLock)
        {
            cat.FeedFood(food);
            int catXp = CatFishingStatsHelper.XpFromFood(food.Name);
            (_, catLevelMsg) = _catProgressionService.AddXp(cat, catXp);
        }

        player!.LifetimeFeedCount++;
        feedMessage = string.IsNullOrEmpty(catLevelMsg) ? $"已喂食 {food.Name}" : catLevelMsg;
        tickGeneration++;
        BindGameSession();
        TryRefreshSidebarVitals();
        _ = PersistFeedShopFoodAsync(food);
        return Task.CompletedTask;
    }

    private async Task PersistFeedShopFoodAsync(Food food)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var ok = await _playerService.CommitConsumeBackpackAsync(player!, food.Name);
                if (!ok)
                    PlayerService.RollbackConsumeBackpackOptimistic(player!, food.Name);
                await _playerService.SaveProgressAsync(player!);
                await _catProgressionService.SaveAsync(cat);
                await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());
            });
            tickGeneration++;
            BindGameSession();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistFeedShopFood failed for {FoodName}", food.Name);
        }
    }
}
