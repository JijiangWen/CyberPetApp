using System.Security.Claims;
using CyberPetApp.Models;
using CyberPetApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    private System.Timers.Timer? gameTimer;

    protected override async Task OnInitializedAsync()
    {
        var auth = await _authStateProvider.GetAuthenticationStateAsync();
        username = auth.User.Identity?.Name ?? "";

        var idClaim = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var playerId))
        {
            _navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        player = await _playerService.LoadPlayerAsync(playerId);
        if (player is null)
        {
            await _authService.LogoutAsync();
            _navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        isLoading = false;
        cat = await _cyberCatService.GetOrCreateAsync(playerId);
        feeder = await _feederService.LoadOrCreateAsync(playerId);
        waterer = await _watererService.LoadOrCreateAsync(playerId);
        workingPlace.Job = player.SelectedWorkJob;
        workingPlace.WorkTickCount = player.WorkTickCount;
        playerHouse = await _houseService.LoadHouseAsync(playerId);
        ApplyFeederCapacity();

        await WithDbLock(async () =>
        {
            var offline = await _offlineCompensationService.ApplyAsync(
                player, cat, workingPlace, playerHouse, GetHouseBuffs());
            if (!string.IsNullOrEmpty(offline.Summary))
                offlineMessage = offline.Summary;
            player.WorkTickCount = workingPlace.WorkTickCount;
            await _dailyBountyService.EnsureTodayBountyAsync(player, player.FishingLevel);
            fishRecords = await _fishRecordService.GetRecordsAsync(playerId);
            RefreshFishDexCache();
            await ReloadMarketAsync();
        });

        // 首次进入发放默认装备并构建运行时快照
        await _equipmentService.EnsureDefaultGearAsync(player.Id);
        milestoneBuffs = await _achievementService.GetBuffsAsync(player.Id);
        milestoneUnlockIds = (await _achievementService.GetUnlockedItemIdsAsync(player.Id)).ToHashSet();
        InvalidateHouseBuffs();
        await ReloadSpotLicensesAsync();
        await ReloadGearAsync();
        await ReloadMilestonesAsync();

        BindGameSession();
        _titlebarFishingOn = fishingManager.IsFishing;
        fishingManager.Changed += OnFishingChangedForChrome;
        // P0-2：钓鱼进度由 FishingStatusPanel 局部订阅 Changed，此处仅标题栏响应 IsFishing 变化
        fishingManager.OnCycleComplete = OnFishingCycleComplete;
        fishingManager.OnFishCaught = OnFishCaught;
        fishingManager.OnLureConsumed = OnLureConsumed;
        fishingManager.OnTargetLureConsumed = OnTargetLureConsumed;
        fishingManager.OnGearWear = OnGearWear;

        await ReloadLeaderboardAsync();
        activeFoodBuffs = await _catBuffService.LoadActiveAsync(playerId);
        _tickCounters.LastTickRenderSnapshot = new TickRenderSnapshot(
            cat.Hunger, cat.Thirst, cat.Energy, cat.Happiness, cat.Health,
            cat.CatXp, cat.CatLevel, player.Money, feeder.FoodCount,
            fishingManager.IsFishing, player.IsWorking);
        TryRefreshSidebarVitals();
        BindGameSession();

        gameTimer = new System.Timers.Timer(GameTickOrchestrator.TickIntervalMs);
        gameTimer.Elapsed += OnTimerElapsed;
        gameTimer.Start();
    }

    private void OnFishingCycleComplete()
    {
        lock (catStateLock)
        {
            var stats = CatBuffService.MergeStats(
                CatFishingStatsHelper.Compute(cat),
                GetFoodBuffSnapshot());
            cat.ApplyActivityCost(CatActivityType.FishingCycle, GetHouseBuffs(), stats.EnergyCostMultiplier);
        }
        tickGeneration++;
        TryRefreshSidebarVitals();
        BindGameSession();
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (player is null) return;

        var result = _tickOrchestrator.RunTick(
            new GameTickInput
            {
                Cat = cat,
                Player = player,
                WorkingPlace = workingPlace,
                Feeder = feeder,
                Waterer = waterer,
                HouseBuffs = GetHouseBuffs(),
                UnlockedFurnitureIds = UnlockedFurnitureIds(),
                HasWaterDispenser = HasWaterDispenser(),
                IsFishing = fishingManager.IsFishing,
                MarketOfferInterval = MarketService.NpcOfferIntervalForJob(workingPlace.Job),
                MaintenanceTicksPerDay = MaintenanceService.TicksPerGameDay
            },
            _tickCounters,
            catStateLock);

        _ = SaveGameTickAsync();
        if (result.EarnedTicket)
            _ = GrantStallTicketAsync();
        if (result.FeederFoodChanged)
            _ = SyncFeederAfterFeedAsync();
        if (result.ConsumedWater)
            _ = SyncWatererAfterWaterAsync();
        if (result.RunMaintenance)
            _ = TryMaintenanceAsync();
        if (result.RunMarketNpcOffers)
            _ = TryMarketNpcOffersAsync();

        if (!result.IsDirty) return;

        tickGeneration++;
        TryRefreshSidebarVitals();
        BindGameSession();
        InvokeAsync(StateHasChanged);
    }

    private void DismissCatchBroadcast()
    {
        catchBroadcast = null;
        catchBroadcastMyth = false;
    }

    private async Task GrantStallTicketAsync()
    {
        try
        {
            await WithDbLock(async () =>
                await _playerService.GrantBackpackItemAsync(player!, MarketService.StallTicketItemName));
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "GrantStallTicketAsync 失败");
        }
    }

    private async Task TryMarketNpcOffersAsync()
    {
        try
        {
            await WithDbLock(async () =>
            {
                await _marketService.TryGenerateNpcOffersAsync(player!.Id, cat.Happiness, GetHouseBuffs().NpcOfferChanceMultiplier);
                await ReloadMarketAsync();
            });
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "TryMarketNpcOffersAsync 失败");
        }
    }

    private async Task TryMaintenanceAsync()
    {
        try
        {
            string? msg = null;
            await WithDbLock(async () =>
            {
                var (_, _, message) = await _maintenanceService.TryChargeAsync(player!, playerHouse, cat);
                msg = message;
            });
            if (msg is not null && player!.MaintenanceOverdue)
                feedMessage = msg;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "TryMaintenanceAsync 失败");
        }
    }

    private async Task SyncFeederAfterFeedAsync()
    {
        try
        {
            await WithDbLock(async () => await _feederService.SyncSlotsAfterFeedAsync(feeder));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SyncFeederAfterFeedAsync 失败");
        }
    }

    /// <summary>P0-1：合并 Buff Tick + 猫 + 玩家（含 LastActiveAt）为单次 SaveChanges。</summary>
    private async Task SaveGameTickAsync()
    {
        if (player is null) return;
        try
        {
            List<ActiveCatBuff> updated = [];
            await WithDbLock(async () =>
            {
                updated = await _gamePersistenceService.SaveTickAsync(
                    player, cat, workingPlace.WorkTickCount);
            });
            lock (catStateLock)
            {
                activeFoodBuffs = updated;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SaveGameTickAsync 存档失败");
        }
    }

    /// <summary>非心跳场景（Dispose、远征返回等）会话存档。</summary>
    private async Task PersistGameStateAsync()
    {
        if (player is null) return;
        try
        {
            await WithDbLock(async () =>
            {
                await _gamePersistenceService.SaveSessionAsync(
                    player, cat, workingPlace.WorkTickCount);
            });
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistGameStateAsync 存档失败");
        }
    }

    private async Task GoToLeaderboardAsync()
    {
        await SelectSectionAsync("fishing");
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _leaderboardDebounceCts?.Cancel();
        _leaderboardDebounceCts?.Dispose();
        if (gameTimer != null)
        {
            gameTimer.Stop();
            gameTimer.Elapsed -= OnTimerElapsed;
            gameTimer.Dispose();
        }
        fishingManager.Changed -= OnFishingChangedForChrome;
        if (player is not null && fishingManager.IsFishing)
            FishingSessionRegistry.Stop(player.Id, CircuitId);
        fishingManager.Dispose();
        if (player is not null)
            await PersistGameStateAsync();
    }
}
