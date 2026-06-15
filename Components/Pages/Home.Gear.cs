using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    private void NotifyGearChrome()
    {
        tickGeneration++;
        BindGameSession();
        _ = InvokeAsync(StateHasChanged);
    }

    private Task FinalizeBuyRod(GearBuyRodEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}";
        _ = PersistBuyRodAsync(args.Spec, args.Rod);
        return Task.CompletedTask;
    }

    private Task FinalizeBuyReel(GearBuyReelEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}";
        _ = PersistBuyReelAsync(args.Spec, args.Reel);
        return Task.CompletedTask;
    }

    private Task FinalizeBuyLine(GearBuyLineEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}";
        _ = PersistBuyLineAsync(args.Spec, args.Line);
        return Task.CompletedTask;
    }

    private Task FinalizeBuyLure(GearBuyLureEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name} ×{args.Spec.PackSize}";
        _ = PersistBuyLureAsync(args.Spec, args.Lure);
        return Task.CompletedTask;
    }

    private Task FinalizeEquipRod(FishingRod rod)
    {
        gearMessage = $"已装备 {rod.Name}";
        _ = PersistEquipRodAsync(rod);
        return Task.CompletedTask;
    }

    private Task FinalizeEquipReel(FishingReel reel)
    {
        gearMessage = $"已装备 {reel.Name}";
        _ = PersistEquipReelAsync(reel);
        return Task.CompletedTask;
    }

    private Task FinalizeEquipLine(FishingLine line)
    {
        gearMessage = $"已装备 {line.Name}";
        _ = PersistEquipLineAsync(line);
        return Task.CompletedTask;
    }

    private Task FinalizeEquipLure(FishingLure lure)
    {
        gearMessage = $"已装备 {lure.Name}";
        _ = PersistEquipLureAsync(lure);
        return Task.CompletedTask;
    }

    private Task FinalizeRepairRod((FishingRod Rod, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Rod.Name}] 全修至 100"
            : $"[{args.Rod.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairRodAsync(args.Rod);
        return Task.CompletedTask;
    }

    private Task FinalizeRepairReel((FishingReel Reel, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Reel.Name}] 全修至 100"
            : $"[{args.Reel.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairReelAsync(args.Reel);
        return Task.CompletedTask;
    }

    private Task FinalizeRepairLine((FishingLine Line, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Line.Name}] 全修至 100"
            : $"[{args.Line.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairLineAsync(args.Line);
        return Task.CompletedTask;
    }

    private async Task PersistBuyRodAsync(RodSpec spec, FishingRod rod)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyRodAsync(player!, rod);
                if (!ok)
                {
                    EquipmentService.RollbackBuyRod(player!, myRods, spec, rod);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyRod failed for {RodName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistBuyReelAsync(ReelSpec spec, FishingReel reel)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyReelAsync(player!, reel);
                if (!ok)
                {
                    EquipmentService.RollbackBuyReel(player!, myReels, spec, reel);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyReel failed for {ReelName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistBuyLineAsync(LineSpec spec, FishingLine line)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyLineAsync(player!, line);
                if (!ok)
                {
                    EquipmentService.RollbackBuyLine(player!, myLines, spec, line);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyLine failed for {LineName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistBuyLureAsync(LureSpec spec, FishingLure lure)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyLureAsync(player!, spec, lure);
                if (!ok)
                {
                    EquipmentService.RollbackBuyLure(player!, myLures, spec);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyLure failed for {LureName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistEquipRodAsync(FishingRod rod)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipRodAsync(player!.Id, rod.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipRod failed for {RodId}", rod.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistEquipReelAsync(FishingReel reel)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipReelAsync(player!.Id, reel.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipReel failed for {ReelId}", reel.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistEquipLineAsync(FishingLine line)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipLineAsync(player!.Id, line.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipLine failed for {LineId}", line.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistEquipLureAsync(FishingLure lure)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipLureAsync(player!.Id, lure.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipLure failed for {LureId}", lure.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistRepairRodAsync(FishingRod rod)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitRepairRodAsync(player!, rod);
                if (!ok)
                {
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistRepairRod failed for {RodId}", rod.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistRepairReelAsync(FishingReel reel)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitRepairReelAsync(player!, reel);
                if (!ok)
                {
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistRepairReel failed for {ReelId}", reel.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private async Task PersistRepairLineAsync(FishingLine line)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitRepairLineAsync(player!, line);
                if (!ok)
                {
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistRepairLine failed for {LineId}", line.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    private Task FinalizeBuyAutoRepair()
    {
        if (player is null || player.AutoRepairUnlocked) return Task.CompletedTask;
        if (player.Money < 3000)
        {
            gearMessage = "金币不足，购买自动修复工具需要 3000g";
            return Task.CompletedTask;
        }

        player.Money -= 3000;
        player.AutoRepairUnlocked = true;
        player.AutoRepairEnabled = true; // Auto enable on purchase
        gearMessage = "已购入自动修复工具！";
        _ = SaveAutoRepairSettingsAsync();
        return Task.CompletedTask;
    }

    private Task FinalizeToggleAutoRepair(bool enabled)
    {
        if (player is null || !player.AutoRepairUnlocked) return Task.CompletedTask;
        player.AutoRepairEnabled = enabled;
        gearMessage = enabled ? "自动修复已开启" : "自动修复已关闭";
        _ = SaveAutoRepairSettingsAsync();
        return Task.CompletedTask;
    }

    private Task FinalizeChangeAutoRepairThreshold(int threshold)
    {
        if (player is null || !player.AutoRepairUnlocked) return Task.CompletedTask;
        player.AutoRepairThreshold = threshold;
        gearMessage = $"已设置自动修复阈值为 {threshold}%";
        _ = SaveAutoRepairSettingsAsync();
        return Task.CompletedTask;
    }

    private async Task SaveAutoRepairSettingsAsync()
    {
        try
        {
            await WithDbLock(async () =>
            {
                await _playerService.SaveProgressAsync(player!);
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SaveAutoRepairSettings failed");
        }
    }
}
