using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    private int _fishingCycleCount = 0;

    private async Task StartFishingAsync(FishingSpot spot)
    {
        if (player!.IsFishBackpackFull)
        {
            fishingBlockMessage = $"鱼背包已满 ({player.FishBackpack.Count}/{player.FishBackpackCapacity})，请先卖/分解鱼或升级背包容量";
            return;
        }

        if (!FishingSessionRegistry.TryStart(player.Id, CircuitId))
        {
            fishingBlockMessage = "正在处理其他钓鱼会话";
            return;
        }

        if (player.FishingLevel < spot.RequiredLevel)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"[{spot.Name}] 需要钓鱼等级达到 Lv.{spot.RequiredLevel}";
            return;
        }

        if (!loadout.MeetsGearLevel)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"装备等级不足，需要钓鱼等级达到 Lv.{loadout.MinGearRequiredLevel}";
            return;
        }

        if (SpotLicenseCatalog.RequiresLicense(spot.Name) && !HasSpotAccess(spot.Name))
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"[{spot.Name}] 未拥有许可证，需购买日租或永久证";
            return;
        }

        int castFee = EconomySinks.CastFeeForSpot(spot.Name);
        if (player.Money < castFee)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"金币不足，需要抛竿费 {castFee}g";
            return;
        }

        _fishingCycleCount = 0;

        // ── 乐观 UI 更新 ──
        player.Money -= castFee;
        fishingBlockMessage = null;
        feedMessage = $"已付抛竿费 {castFee}g";
        TryRefreshSidebarVitals();
        
        loadout.SpotGearEffectiveness = GearProgressionCatalog.SpotGearEffectiveness(loadout.RodGearTier, spot.Name);
        fishingManager.StartFishing(spot, loadout,
            () => CatBuffService.MergeStateBuff(
                CatBuffHelper.Compute(cat, player!.MaintenanceOverdue, GetHouseBuffs()),
                GetFoodBuffSnapshot()),
            () => CatBuffService.MergeStats(
                CatFishingStatsHelper.Compute(cat),
                GetFoodBuffSnapshot()));

        tickGeneration++;
        BindGameSession();
        await InvokeAsync(StateHasChanged);

        // 后台异步进行数据库金币扣除与同步
        _ = PersistStartFishingAsync(spot, castFee);
    }

    private async Task PersistStartFishingAsync(FishingSpot spot, int castFee)
    {
        try
        {
            bool success = false;
            await WithDbLock(async () =>
            {
                success = await _playerService.TrySpendGoldAsync(player!, castFee);
                if (success)
                {
                    // 同步一下装备的最新状态
                    await ReloadGearAsync(acquireLock: false);
                }
            });

            if (!success)
            {
                // 回滚乐观更新
                player!.Money += castFee;
                fishingManager.StopFishing();
                FishingSessionRegistry.Stop(player.Id, CircuitId);
                fishingBlockMessage = "抛竿失败：同步数据库余额不足。";
                feedMessage = "";
                tickGeneration++;
                BindGameSession();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistStartFishingAsync 失败，spot={SpotName}", spot.Name);
        }
    }

    private void StopFishing()
    {
        fishingManager.StopFishing();
        FishingSessionRegistry.Stop(player!.Id, CircuitId);

        // ── 乐观 UI ──
        tickGeneration++;
        BindGameSession();
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnFishCaught(Fish fish) => _ = HandleFishCaughtAsync(fish);

    private async Task HandleFishCaughtAsync(Fish fish)
    {
        try
        {
            if (player!.IsFishBackpackFull)
            {
                feedMessage = $"鱼背包已满 ({player.FishBackpack.Count}/{player.FishBackpackCapacity})，钓鱼已停止";
                StopFishing();
                await InvokeAsync(StateHasChanged);
                return;
            }
            player.FishBackpack.Add(fish);

            int xp = fish.Rarity switch
            {
                FishRarity.Legendary => 160,
                FishRarity.Epic => 65,
                FishRarity.Rare => 26,
                _ => 9
            };
            if (player.FishingLevel >= 40) xp = Math.Max(1, (int)(xp * 0.85));
            if (fish.SizePercentage > 100) xp = (int)(xp * 1.6);

            player.AddFishingXp(xp);
            if (fish.Rarity >= FishRarity.Rare) player.RareCatchCount++;
            if (fish.Rarity == FishRarity.Legendary) player.LegendaryCatchCount++;
            loadout.FishingLevel = player.FishingLevel;

            string? catLevelMsg;
            lock (catStateLock)
            {
                int catXp = CatFishingStatsHelper.XpFromFish(fish);
                (_, catLevelMsg) = _catProgressionService.AddXp(cat, catXp);
            }

            await WithDbLock(async () =>
            {
                await _fishingService.SaveCaughtFishAsync(player.Id, fish);
                await _fishRecordService.RecordCatchAsync(player.Id, fish);
                fishRecords = await _fishRecordService.GetRecordsAsync(player.Id);
                RefreshFishDexCache();
                var (claimed, bountyMsg) = await _dailyBountyService.TryClaimAsync(player, fish);
                if (claimed) marketMessage = bountyMsg;
                string? spotName = fishingManager.CurrentSpot?.Name ?? "未知钓点";
                var matDrop = await _gearMaterialService.TryGrantCatchMaterialAsync(player, fish, spotName);
                if (matDrop is not null)
                    feedMessage = string.IsNullOrEmpty(feedMessage)
                        ? $"获得素材 {matDrop}"
                        : $"{feedMessage} · 素材 {matDrop}";
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
                await _playerService.SaveProgressAsync(player);
                await _catProgressionService.SaveAsync(cat);
            });

            ScheduleLeaderboardRefresh();

            if (catLevelMsg is not null)
                feedMessage = catLevelMsg;

            TryRefreshSidebarVitals();
            bool isMyth = TargetFishCatalog.IsTargetExclusive(fish.Name);
            catchBroadcastMyth = isMyth;
            if (isMyth)
            {
                catchBroadcastLegendary = false;
                var lure = TargetFishCatalog.RequiredLure(fish.Name);
                catchBroadcast = $"神话神兽！{fish.Name} · {fish.ActualWeight}kg · 售价 {fish.SellPrice}g · 拟饵 {(lure?.DisplayName ?? "?")} -1";
            }
            else if (fish.Rarity == FishRarity.Legendary || fish.SizePercentage > 100)
            {
                catchBroadcastLegendary = true;
                catchBroadcast = $"传说巨物！{fish.Name} · {fish.ActualWeight}kg · 售价 {fish.SellPrice}g";
            }
            else if (fish.Rarity == FishRarity.Epic)
            {
                catchBroadcastLegendary = false;
                catchBroadcast = $"史诗大鱼 {fish.Name} · {fish.ActualWeight}kg · {fish.SellPrice}g";
            }
            else
            {
                catchBroadcastLegendary = false;
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "HandleFishCaughtAsync ???{FishName}", fish.Name);
            feedMessage = "收鱼失败，请稍后重试";
        }
    }

    private void OnLureConsumed() => _ = HandleLureConsumedAsync();

    private void OnGearWear(bool lineBreak, bool isEscape) => _ = HandleGearWearAsync(lineBreak, isEscape);

    private async Task HandleGearWearAsync(bool lineBreak, bool isEscape)
    {
        try
        {
            List<string> wearLogs = [];
            List<string> autoRepairLogs = [];
            await WithDbLock(async () =>
            {
                wearLogs = await _equipmentService.WearEquippedGearAsync(player!.Id, lineBreak, fishingManager.CurrentSpot?.Name, isEscape);
                
                // 自动修复工具逻辑
                if (player.AutoRepairUnlocked && player.AutoRepairEnabled)
                {
                    var rod = await _equipmentService.GetEquippedRodAsync(player.Id);
                    var reel = await _equipmentService.GetEquippedReelAsync(player.Id);
                    var line = await _equipmentService.GetEquippedLineAsync(player.Id);

                    if (rod is not null && rod.Durability <= player.AutoRepairThreshold)
                    {
                        var (ok, msg) = await _equipmentService.RepairRodAsync(player, rod.Id, fullRepair: true);
                        if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                        else autoRepairLogs.Add($"自动修复失败: {msg}");
                    }
                    if (reel is not null && reel.Durability <= player.AutoRepairThreshold)
                    {
                        var (ok, msg) = await _equipmentService.RepairReelAsync(player, reel.Id, fullRepair: true);
                        if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                        else autoRepairLogs.Add($"自动修复失败: {msg}");
                    }
                    if (line is not null && line.Durability <= player.AutoRepairThreshold)
                    {
                        var (ok, msg) = await _equipmentService.RepairLineAsync(player, line.Id, fullRepair: true);
                        if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                        else autoRepairLogs.Add($"自动修复失败: {msg}");
                    }
                }

                await ReloadGearAsync(acquireLock: false);
            });

            foreach (var log in wearLogs)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                fishingManager.Log($"[{time}] {log}", "bad");
            }
            foreach (var log in autoRepairLogs)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                fishingManager.Log($"[{time}] {log}", log.Contains("失败") ? "bad" : "good");
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "HandleGearWearAsync 失败");
        }
    }

    private async Task HandleLureConsumedAsync()
    {
        try
        {
            await WithDbLock(async () =>
            {
                await _equipmentService.SyncEquippedLureAsync(player!.Id, loadout.LureDurabilityRemaining, loadout.LureQuantity);
                await _equipmentService.SyncEquippedLineDurabilityAsync(player.Id, loadout.LineDurability);
                if (await _playerService.TrySpendGoldAsync(player, EconomySinks.LineRepairFee))
                    feedMessage = $"切线维修 {EconomySinks.LineRepairFee}g · 线耐久 {loadout.LineDurability} · 饵 {loadout.LureDurabilityRemaining}";
                else
                    feedMessage = $"金币不足，切线维修需 {EconomySinks.LineRepairFee}g";
                
                await ReloadGearAsync(acquireLock: false);
            });
            TryRefreshSidebarVitals();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "HandleLureConsumedAsync 失败");
        }
    }

    private async Task ChargeCastFeeDuringFishingAsync(FishingSpot spot, int castFee)
    {
        try
        {
            if (player!.Money < castFee)
            {
                fishingBlockMessage = $"金币不足，需要抛竿费 {castFee}g，已自动停止钓鱼";
                StopFishing();
                await InvokeAsync(StateHasChanged);
                return;
            }

            // 乐观 UI 更新
            player.Money -= castFee;
            feedMessage = $"已付抛竿费 {castFee}g (挂机每 10 轮扣除)";
            TryRefreshSidebarVitals();
            await InvokeAsync(StateHasChanged);

            bool success = false;
            await WithDbLock(async () =>
            {
                success = await _playerService.TrySpendGoldAsync(player!, castFee);
                if (success)
                {
                    await ReloadGearAsync(acquireLock: false);
                }
            });

            if (!success)
            {
                // 回滚并自动停止钓鱼
                player!.Money += castFee;
                fishingBlockMessage = "抛竿扣费失败，已自动停止钓鱼。";
                StopFishing();
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "ChargeCastFeeDuringFishingAsync 失败，spot={SpotName}", spot.Name);
        }
    }
}
