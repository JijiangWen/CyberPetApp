using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    private async Task StartFishingAsync(FishingSpot spot)
    {
        if (!FishingSessionRegistry.TryStart(player!.Id, CircuitId))
        {
            fishingBlockMessage = "?????????????????????";
            return;
        }

        if (player.FishingLevel < spot.RequiredLevel)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"[{spot.Name}] ???? Lv.{spot.RequiredLevel}";
            return;
        }

        if (!loadout.MeetsGearLevel)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"??????????? Lv.{loadout.MinGearRequiredLevel}";
            return;
        }

        if (SpotLicenseCatalog.RequiresLicense(spot.Name) && !HasSpotAccess(spot.Name))
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"[{spot.Name}] ?????????????";
            return;
        }

        int castFee = EconomySinks.CastFeeForSpot(spot.Name);
        bool paid = false;
        await WithDbLock(async () => paid = await _playerService.TrySpendGoldAsync(player!, castFee));
        if (!paid)
        {
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            fishingBlockMessage = $"????????? {castFee}g";
            return;
        }

        fishingBlockMessage = null;
        feedMessage = $"????? {castFee}g";
        await ReloadGearAsync();
        loadout.SpotGearEffectiveness = GearProgressionCatalog.SpotGearEffectiveness(loadout.RodGearTier, spot.Name);
        fishingManager.StartFishing(spot, loadout,
            () => CatBuffService.MergeStateBuff(
                CatBuffHelper.Compute(cat, player!.MaintenanceOverdue, GetHouseBuffs()),
                GetFoodBuffSnapshot()),
            () => CatBuffService.MergeStats(
                CatFishingStatsHelper.Compute(cat),
                GetFoodBuffSnapshot()));
    }

    private void StopFishing()
    {
        fishingManager.StopFishing();
        FishingSessionRegistry.Stop(player!.Id, CircuitId);
    }

    private void OnFishCaught(Fish fish) => _ = HandleFishCaughtAsync(fish);

    private async Task HandleFishCaughtAsync(Fish fish)
    {
        try
        {
            player!.FishBackpack.Add(fish);

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
                string? spotName = fishingManager.CurrentSpot?.Name ?? "??";
                var matDrop = await _gearMaterialService.TryGrantCatchMaterialAsync(player, fish, spotName);
                if (matDrop is not null)
                    feedMessage = string.IsNullOrEmpty(feedMessage) ? $"???{matDrop}" : $"{feedMessage} � ?? {matDrop}";
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
                await _playerService.SaveProgressAsync(player);
                await _catProgressionService.SaveAsync(cat);
            });

            ScheduleLeaderboardRefresh();

            if (catLevelMsg is not null)
                feedMessage = catLevelMsg;

            bool isMyth = TargetFishCatalog.IsTargetExclusive(fish.Name);
            catchBroadcastMyth = isMyth;
            if (isMyth)
            {
                catchBroadcastLegendary = false;
                var lure = TargetFishCatalog.RequiredLure(fish.Name);
                catchBroadcast = $"??????{fish.Name} � {fish.ActualWeight}kg � ?? {fish.SellPrice}g � ??? {(lure?.DisplayName ?? "?")} -1";
            }
            else if (fish.Rarity == FishRarity.Legendary || fish.SizePercentage > 100)
            {
                catchBroadcastLegendary = true;
                catchBroadcast = $"?????{fish.Name} � {fish.ActualWeight}kg � ?? {fish.SellPrice}g";
            }
            else if (fish.Rarity == FishRarity.Epic)
            {
                catchBroadcastLegendary = false;
                catchBroadcast = $"???? {fish.Name} � {fish.ActualWeight}kg � {fish.SellPrice}g";
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
            feedMessage = "????????????";
        }
    }

    private void OnLureConsumed() => _ = HandleLureConsumedAsync();

    private void OnGearWear(bool lineBreak) => _ = HandleGearWearAsync(lineBreak);

    private async Task HandleGearWearAsync(bool lineBreak)
    {
        try
        {
            await WithDbLock(async () =>
            {
                await _equipmentService.WearEquippedGearAsync(player!.Id, lineBreak, fishingManager.CurrentSpot?.Name);
            });
            var eqRod = myRods.FirstOrDefault(r => r.IsEquipped);
            var eqReel = myReels.FirstOrDefault(r => r.IsEquipped);
            var eqLine = myLines.FirstOrDefault(l => l.IsEquipped);
            if (eqRod is not null || eqReel is not null || eqLine is not null)
            {
                myRods = await _equipmentService.GetRodsAsync(player!.Id);
                myReels = await _equipmentService.GetReelsAsync(player.Id);
                myLines = await _equipmentService.GetLinesAsync(player.Id);
                eqRod = myRods.FirstOrDefault(r => r.IsEquipped);
                eqReel = myReels.FirstOrDefault(r => r.IsEquipped);
                eqLine = myLines.FirstOrDefault(l => l.IsEquipped);
                if (eqRod is not null)
                {
                    loadout.RodDurability = eqRod.Durability;
                    loadout.Sensitivity = eqRod.Sensitivity;
                    loadout.CastRange = eqRod.CastRange;
                }
                if (eqReel is not null)
                {
                    loadout.ReelDurability = eqReel.Durability;
                    loadout.DragPower = eqReel.DragPower;
                    loadout.Smoothness = eqReel.Smoothness;
                    loadout.LineCapacity = eqReel.LineCapacity;
                }
                if (eqLine is not null)
                {
                    loadout.LineDurability = eqLine.Durability;
                    loadout.LineStrength = eqLine.LineStrength;
                    loadout.LineSensitivity = eqLine.LineSensitivity;
                    loadout.LineStealth = eqLine.LineStealth;
                    loadout.AbrasionResistance = eqLine.AbrasionResistance;
                }
            }
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "HandleGearWearAsync ??");
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
                    feedMessage = $"?????? {EconomySinks.LineRepairFee}g � ???? {loadout.LineDurability} � ?? {loadout.LureDurabilityRemaining}";
                else
                    feedMessage = $"????????? {EconomySinks.LineRepairFee}g";
            });
            myLures = await _equipmentService.GetLuresAsync(player!.Id);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "HandleLureConsumedAsync ??");
        }
    }
}
