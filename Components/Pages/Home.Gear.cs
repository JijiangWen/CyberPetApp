using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

// 这个类使用 partial 关键字，表示它是主页面 Home 类的“一部分”。
// 这里专门处理“钓鱼装备（鱼竿、渔轮、鱼线、拟饵）的购买、换装、修理及自动修复工具”的后台保存和前端回显逻辑。
public partial class Home
{
    // 辅助方法：每次装备状态变化（购买/装备/修理）后，刷新界面版本，通知页面进行重新渲染。
    private void NotifyGearChrome()
    {
        tickGeneration++; // 递增版本号，迫使界面感知数据刷新
        BindGameSession(); // 同步当前的页面会话状态
        _ = InvokeAsync(StateHasChanged); // 跨线程调度，刷新 HTML 界面
    }

    // ── 乐观 UI 的收尾方法 ──
    // 当玩家在前端商店面板上点击“购买鱼竿”并成功瞬间，会触发这个方法。
    // 这时内存里的钱已经扣了，鱼竿也已经塞进背包了，我们只需在后台启动数据库保存即可。
    private Task FinalizeBuyRod(GearBuyRodEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}"; // 在界面上提示购买成功
        _ = PersistBuyRodAsync(args.Spec, args.Rod); // 异步启动后台写库存档
        return Task.CompletedTask;
    }

    // 购买渔轮的乐观收尾
    private Task FinalizeBuyReel(GearBuyReelEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}";
        _ = PersistBuyReelAsync(args.Spec, args.Reel);
        return Task.CompletedTask;
    }

    // 购买鱼线的乐观收尾
    private Task FinalizeBuyLine(GearBuyLineEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name}";
        _ = PersistBuyLineAsync(args.Spec, args.Line);
        return Task.CompletedTask;
    }

    // 购买拟饵的乐观收尾（拟饵是消耗品，按包卖，有数量 PackSize）
    private Task FinalizeBuyLure(GearBuyLureEventArgs args)
    {
        gearMessage = $"已购入 {args.Spec.Name} ×{args.Spec.PackSize}";
        _ = PersistBuyLureAsync(args.Spec, args.Lure);
        return Task.CompletedTask;
    }

    // 装备鱼竿的乐观收尾
    private Task FinalizeEquipRod(FishingRod rod)
    {
        gearMessage = $"已装备 {rod.Name}";
        _ = PersistEquipRodAsync(rod);
        return Task.CompletedTask;
    }

    // 装备渔轮的乐观收尾
    private Task FinalizeEquipReel(FishingReel reel)
    {
        gearMessage = $"已装备 {reel.Name}";
        _ = PersistEquipReelAsync(reel);
        return Task.CompletedTask;
    }

    // 装备鱼线的乐观收尾
    private Task FinalizeEquipLine(FishingLine line)
    {
        gearMessage = $"已装备 {line.Name}";
        _ = PersistEquipLineAsync(line);
        return Task.CompletedTask;
    }

    // 装备拟饵的乐观收尾
    private Task FinalizeEquipLure(FishingLure lure)
    {
        gearMessage = $"已装备 {lure.Name}";
        _ = PersistEquipLureAsync(lure);
        return Task.CompletedTask;
    }

    // 修理鱼竿的乐观收尾（支持全修和只修20点耐久）
    private Task FinalizeRepairRod((FishingRod Rod, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Rod.Name}] 全修至 100"
            : $"[{args.Rod.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairRodAsync(args.Rod);
        return Task.CompletedTask;
    }

    // 修理渔轮的乐观收尾
    private Task FinalizeRepairReel((FishingReel Reel, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Reel.Name}] 全修至 100"
            : $"[{args.Reel.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairReelAsync(args.Reel);
        return Task.CompletedTask;
    }

    // 修理鱼线的乐观收尾
    private Task FinalizeRepairLine((FishingLine Line, bool Full) args)
    {
        gearMessage = args.Full
            ? $"[{args.Line.Name}] 全修至 100"
            : $"[{args.Line.Name}] +{EconomySinks.GearRepairPartialAmount} 耐久";
        _ = PersistRepairLineAsync(args.Line);
        return Task.CompletedTask;
    }

    // ── 数据库真实持久化存档逻辑（含乐观 UI 失败回滚机制） ──

    // 将购买鱼竿的事实真正保存进数据库。如果在写库时发现失败（例如钱不够、连接超时），则自动回滚内存！
    private async Task PersistBuyRodAsync(RodSpec spec, FishingRod rod)
    {
        try
        {
            // 获取数据库异步互斥锁，确保同一时间只有一个线程在操作 EF Core
            await WithDbLock(async () =>
            {
                // 提交写库请求
                var (ok, msg) = await _equipmentService.CommitBuyRodAsync(player!, rod);
                if (!ok)
                {
                    // 核心逻辑：写库失败！执行 RollbackBuyRod 回滚！
                    // 退回玩家刚才扣减的金币，并把内存列表里刚刚乐观添加的鱼竿删掉！
                    EquipmentService.RollbackBuyRod(player!, myRods, spec, rod);
                    gearMessage = msg; // 页面显示报错信息
                    
                    // 重新从数据库拉取最真实最安全的装备列表，防止界面数据不一致
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome(); // 重新渲染页面
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyRod 失败，正在强制拉取真实装备数据进行回滚: {RodName}", spec.Name);
            // 发生异常时，直接无视内存，强行重新读数据库进行数据校正
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 将购买渔轮保存到数据库，带回滚
    private async Task PersistBuyReelAsync(ReelSpec spec, FishingReel reel)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyReelAsync(player!, reel);
                if (!ok)
                {
                    // 失败回滚内存
                    EquipmentService.RollbackBuyReel(player!, myReels, spec, reel);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyReel 失败: {ReelName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 将购买鱼线保存到数据库，带回滚
    private async Task PersistBuyLineAsync(LineSpec spec, FishingLine line)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyLineAsync(player!, line);
                if (!ok)
                {
                    // 失败回滚内存
                    EquipmentService.RollbackBuyLine(player!, myLines, spec, line);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyLine 失败: {LineName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 将购买拟饵保存到数据库，带回滚
    private async Task PersistBuyLureAsync(LureSpec spec, FishingLure lure)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _equipmentService.CommitBuyLureAsync(player!, spec, lure);
                if (!ok)
                {
                    // 失败回滚内存
                    EquipmentService.RollbackBuyLure(player!, myLures, spec);
                    gearMessage = msg;
                    await ReloadGearAsync(acquireLock: false);
                }
            });
            NotifyGearChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyLure 失败: {LureName}", spec.Name);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存“装备上这根鱼竿”的状态
    private async Task PersistEquipRodAsync(FishingRod rod)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipRodAsync(player!.Id, rod.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipRod 失败: {RodId}", rod.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存“装备上这台渔轮”的状态
    private async Task PersistEquipReelAsync(FishingReel reel)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipReelAsync(player!.Id, reel.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipReel 失败: {ReelId}", reel.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存“装备上这根鱼线”的状态
    private async Task PersistEquipLineAsync(FishingLine line)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipLineAsync(player!.Id, line.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipLine 失败: {LineId}", line.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存“装备上这枚拟饵”的状态
    private async Task PersistEquipLureAsync(FishingLure lure)
    {
        try
        {
            await WithDbLock(() => _equipmentService.EquipLureAsync(player!.Id, lure.Id));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistEquipLure 失败: {LureId}", lure.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存鱼竿修理后的最新耐久值
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
            Logger.LogWarning(ex, "PersistRepairRod 失败: {RodId}", rod.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存渔轮修理后的最新耐久值
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
            Logger.LogWarning(ex, "PersistRepairReel 失败: {ReelId}", reel.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // 持久化保存鱼线修理后的最新耐久值
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
            Logger.LogWarning(ex, "PersistRepairLine 失败: {LineId}", line.Id);
            await ReloadGearAsync(acquireLock: true);
            NotifyGearChrome();
        }
    }

    // ── 自动修复工具的购买与设置逻辑 ──

    // 玩家花费 3000 金币解锁“自动修复工具”挂饰。
    // 解锁后，只要玩家身上钱够，当鱼竿/轮/线耐久低于设定阈值时，心跳后台会自动扣钱修复，防止耐久归零腰斩性能。
    private Task FinalizeBuyAutoRepair()
    {
        if (player is null || player.AutoRepairUnlocked) return Task.CompletedTask;
        if (player.Money < 3000)
        {
            gearMessage = "金币不足，购买自动修复工具需要 3000g";
            return Task.CompletedTask;
        }

        player.Money -= 3000; // 扣减内存金币
        player.AutoRepairUnlocked = true; // 标记已解锁
        player.AutoRepairEnabled = true;  // 购买后默认自动开启
        gearMessage = "已购入自动修复工具！";
        _ = SaveAutoRepairSettingsAsync(); // 异步存盘
        return Task.CompletedTask;
    }

    // 玩家开启或关闭自动修复功能
    private Task FinalizeToggleAutoRepair(bool enabled)
    {
        if (player is null || !player.AutoRepairUnlocked) return Task.CompletedTask;
        player.AutoRepairEnabled = enabled;
        gearMessage = enabled ? "自动修复已开启" : "自动修复已关闭";
        _ = SaveAutoRepairSettingsAsync();
        return Task.CompletedTask;
    }

    // 玩家在界面上拖动滑块，调整自动修复的耐久度临界点阈值（例如低于 20% 时触发修复）
    private Task FinalizeChangeAutoRepairThreshold(int threshold)
    {
        if (player is null || !player.AutoRepairUnlocked) return Task.CompletedTask;
        player.AutoRepairThreshold = threshold;
        gearMessage = $"已设置自动修复阈值为 {threshold}%";
        _ = SaveAutoRepairSettingsAsync();
        return Task.CompletedTask;
    }

    // 将自动修复工具的参数更改保存到数据库中
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
            Logger.LogWarning(ex, "SaveAutoRepairSettings 失败");
        }
    }
}
