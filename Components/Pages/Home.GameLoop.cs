using System.Security.Claims;
using CyberPetApp.Models;
using CyberPetApp.Services;
using Microsoft.AspNetCore.Components.Authorization;

namespace CyberPetApp.Components.Pages;

// 这个类使用 partial 关键字，表示它是主页面 Home 类的“一部分”。
// 这里专门存放游戏心跳定时器（Timer）和核心游戏循环逻辑。
public partial class Home
{
    // 定时器对象，用来在后台规律地触发“游戏Tick”（比如每2秒让时间流逝一次）
    private System.Timers.Timer? gameTimer;

    // 当用户打开这个网页、页面初始化完成时，Blazor 会自动自动执行这个方法。
    // 这相当于游戏的“开机自检与数据加载”过程。
    protected override async Task OnInitializedAsync()
    {
        // 1. 获取当前登录用户的网络安全凭证（判断是否已登录）
        var auth = await _authStateProvider.GetAuthenticationStateAsync();
        username = auth.User.Identity?.Name ?? "";

        // 2. 从用户的登录凭证里，提取出玩家在数据库中的“唯一身份证号”（Player ID）
        var idClaim = auth.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var playerId))
        {
            // 如果解析身份证失败，说明用户没登录或身份不对，直接踢回登录页面
            _navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        // 3. 从数据库中加载这个玩家的个人属性（金币、等级、背包等）
        player = await _playerService.LoadPlayerAsync(playerId);
        if (player is null)
        {
            // 如果数据库里根本没有这个玩家，强制登出并跳转回登录页面
            await _authService.LogoutAsync();
            _navigation.NavigateTo("/login", forceLoad: true);
            return;
        }

        // 4. 将玩家登记到全局会话注册表中，防止同账号多开网页作弊
        GameSessionRegistry.Register(playerId, CircuitId);

        // 5. 记录当前的玩家会话状态，用于局域网或多线程判断
        _circuitSession.PlayerId = player.Id;
        _circuitSession.Username = username;
        _onlineTracker.RegisterOnline(CircuitId, player.Id, username);

        // 6. 标记“数据加载中”为 false，这样前端网页的“加载中...”动画就会消失，显示真实游戏界面
        isLoading = false;

        // 7. 加载或为新玩家创建一只赛博猫咪
        cat = await _cyberCatService.GetOrCreateAsync(playerId);

        // 8. 加载或创建自动喂食器、自动饮水器的数据库记录
        feeder = await _feederService.LoadOrCreateAsync(playerId);
        waterer = await _watererService.LoadOrCreateAsync(playerId);

        // 9. 初始化玩家当前选择的挂机打工岗位与累计打工时长
        workingPlace.Job = player.SelectedWorkJob;
        workingPlace.WorkTickCount = player.WorkTickCount;

        // 10. 加载玩家的小屋和家具摆件状态
        playerHouse = await _houseService.LoadHouseAsync(playerId);
        ApplyFeederCapacity(); // 根据家具加成调整喂食器的最大装粮容量

        // 11. 执行“离线收益补偿计算”（核心功能！）
        // 使用WithDbLock防止在保存离线结果时，有其他线程同时写数据库导致冲突。
        await WithDbLock(async () =>
        {
            // 传入玩家、猫咪、打工岗位和房屋加成，计算从上次下线到这次上线期间，
            // 扣了多少猫咪饱食度、赚了多少金币、产出了多少素材
            var offline = await _offlineCompensationService.ApplyAsync(
                player, cat, workingPlace, playerHouse, GetHouseBuffs());
            
            // 如果离线期间有事件发生，在界面上方弹出一个“离线结算小字条”提示玩家
            if (!string.IsNullOrEmpty(offline.Summary))
                offlineMessage = offline.Summary;
            
            // 同步更新打工进度
            player.WorkTickCount = workingPlace.WorkTickCount;

            // 确保生成了今天的每日求购任务（如果玩家今天还没刷过任务，自动生成一个）
            await _dailyBountyService.EnsureTodayBountyAsync(player, player.FishingLevel);

            // 从数据库拉取玩家所有的历史鱼获记录（用于图鉴和成就判定）
            fishRecords = await _fishRecordService.GetRecordsAsync(playerId);
            RefreshFishDexCache(); // 重新整理鱼图鉴的缓存数据
            await ReloadMarketAsync(); // 重新加载鱼市的摊位和出价列表
        });

        // 12. 确保给新玩家发放了默认的新手钓鱼装备（新手竹竿、普通渔轮等）
        await _equipmentService.EnsureDefaultGearAsync(player.Id);

        // 13. 加载成就奖励与里程碑状态
        await WithDbLock(async () =>
        {
            // 获取里程碑兑换商品给玩家提供的被动属性加成
            milestoneBuffs = await _achievementService.GetBuffsAsync(player.Id);
            // 记录哪些里程碑商品是已经被兑换过的
            milestoneUnlockIds = (await _achievementService.GetUnlockedItemIdsAsync(player.Id)).ToHashSet();
        });

        // 14. 重新计算并缓存玩家当前的房屋家具总加成（比如精力消耗减少X%、金币加成Y%）
        InvalidateHouseBuffs();

        // 15. 刷新玩家拥有的钓点许可证、装备数据以及里程碑数据
        await ReloadSpotLicensesAsync(acquireLock: true);
        await ReloadGearAsync(acquireLock: true);
        await ReloadMilestonesAsync(acquireLock: true);

        // 16. 绑定游戏事件监听器
        // 这样当后台的钓鱼管理器（FishingManager）发生状态变化时，前端能立刻收到广播
        BindGameSession();
        _titlebarFishingOn = fishingManager.IsFishing;
        fishingManager.Changed += OnFishingChangedForChrome;
        
        // 绑定钓鱼各个事件的执行逻辑（事件驱动设计）
        fishingManager.OnCycleComplete = OnFishingCycleComplete; // 这一竿钓完了（成功或失败）
        fishingManager.OnFishCaught = OnFishCaught;             // 成功钓起了一条鱼
        fishingManager.OnLureConsumed = OnLureConsumed;         // 消耗了 1 个拟饵的耐久度
        fishingManager.OnTargetLureConsumed = OnTargetLureConsumed; // 消耗了 1 次神话锁定饵的次数
        fishingManager.OnGearWear = OnGearWear;                 // 扣减了鱼竿/渔轮的耐久磨损

        // 17. 异步加载排行榜数据
        await ReloadLeaderboardAsync();

        // 18. 从数据库读取当前猫咪身上挂着的所有食物Buff（比如吃了刺身，等口时间缩短）
        activeFoodBuffs = await _catBuffService.LoadActiveAsync(playerId);

        // 19. 拍下当前状态的“快照”，保存当前饱食度、金币等，用于前端显示差值动画
        _tickCounters.LastTickRenderSnapshot = new TickRenderSnapshot(
            cat.Hunger, cat.Thirst, cat.Energy, cat.Happiness, cat.Health,
            cat.CatXp, cat.CatLevel, player.Money, feeder.FoodCount,
            fishingManager.IsFishing, player.IsWorking);

        // 20. 刷新侧边栏的猫咪五维数值，同步游戏会话
        TryRefreshSidebarVitals();
        BindGameSession();

        // 21. 启动核心心跳定时器！
        // 设定每 2000 毫秒（2秒）触发一次 OnTimerElapsed 事件
        gameTimer = new System.Timers.Timer(GameTickOrchestrator.TickIntervalMs);
        gameTimer.Elapsed += OnTimerElapsed;
        gameTimer.Start();
    }

    // 当一竿钓鱼流程彻底结束时（不管鱼跑了还是起鱼成功）执行
    private void OnFishingCycleComplete()
    {
        // 锁定猫咪属性，防止多线程同时修改猫咪属性造成混乱
        lock (catStateLock)
        {
            // 聚合猫咪自身属性与吃了食物带来的 Buff 乘数
            var stats = CatBuffService.MergeStats(
                CatFishingStatsHelper.Compute(cat),
                GetFoodBuffSnapshot());
            // 扣除猫咪本次钓鱼所消耗的精力和水分等
            cat.ApplyActivityCost(CatActivityType.FishingCycle, GetHouseBuffs(), stats.EnergyCostMultiplier);
        }
        tickGeneration++; // 提升版本号，通知网页重绘相关局部变量

        _fishingCycleCount++;
        var spot = fishingManager.CurrentSpot;
        if (spot is not null)
        {
            // 钓鱼不是免费的，高阶钓点每钓 10 竿会扣减一次抛竿门票费
            int castFee = EconomySinks.CastFeeForSpot(spot.Name);
            if (castFee > 0 && _fishingCycleCount % 10 == 0)
            {
                // 后台扣减金币
                _ = ChargeCastFeeDuringFishingAsync(spot, castFee);
            }
        }

        // 刷新侧栏，同步状态，重绘页面
        TryRefreshSidebarVitals();
        BindGameSession();
        _ = InvokeAsync(StateHasChanged);
    }

    // 阀门标记：如果上一轮的 2 秒心跳计算还没跑完（比如网络延迟），
    // 这一轮心跳直接跳过，防止服务器队列积压死锁
    private bool _isTickProcessing = false;

    // 核心心跳事件：每过2秒，服务器会自动派发一个线程来执行这个方法。
    // 这相当于游戏世界的“时间流逝齿轮”。
    private async void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (player is null) return;

        // 1. 安全检查：如果该账号在别处开网页登录了，直接踢掉本页面的连接并停止定时器
        if (!GameSessionRegistry.IsSessionValid(player.Id, CircuitId))
        {
            isSessionKicked = true;
            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Elapsed -= OnTimerElapsed;
            }
            await InvokeAsync(StateHasChanged);
            return;
        }

        // 2. 阀门拦截，防止 Tick 重叠并发
        if (_isTickProcessing)
        {
            Logger.LogWarning("Previous game tick is still processing, skipping this tick.");
            return;
        }

        _isTickProcessing = true;
        try
        {
            // 3. 自动喂食检查：如果玩家解锁了自动喂食器，且开启了自动装填
            if (HasAutoFeederUnit() && CanAutoRefillFeeder())
            {
                await WithDbLock(async () =>
                {
                    // 自动从玩家的背包里，消耗金币和食物，填满喂食机器
                    await _feederService.BatchAddFoodFromBackpackAsync(player!, feeder);
                });
            }

            // 4. 自动补水检查：同上，自动消耗纯净水装填饮水机
            if (HasWaterDispenser() && CanAutoRefillWaterer())
            {
                await WithDbLock(async () =>
                {
                    await _watererService.BatchAddWaterFromBackpackAsync(player!, waterer);
                });
            }

            // 5. 执行游戏 Tick 精密调度算法！
            // 这里会统一计算这 2 秒内：猫咪属性扣减多少、打工赚了多少金币、自动吃了几口猫粮等。
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

            // 6. 异步将本次心跳的最新数据保存进数据库存档（防止断电丢进度）
            await SaveGameTickAsync();
            
            // 7. 处理 Tick 运行的附加事件结果：
            // A. 如果打工触发了“摊位券”奖励，往玩家背包里塞入一张摊位券
            if (result.EarnedTicket)
                await GrantStallTicketAsync();
            
            // B. 如果猫咪自动吃了东西，同步更新数据库喂食槽的余量和顺序
            if (result.FeederFoodChanged)
                await SyncFeederAfterFeedAsync();
            
            // C. 如果猫咪喝了水，同步更新饮水机水位
            if (result.ConsumedWater)
                await SyncWatererAfterWaterAsync();
            
            // D. 如果跨天了，扣除房屋每日家具折旧维护费
            if (result.RunMaintenance)
                await TryMaintenanceAsync();
            
            // E. 鱼市出价刷新：每过一段时间，NPC 会对玩家上架的鱼获产生新的随机出价
            if (result.RunMarketNpcOffers)
                await TryMarketNpcOffersAsync();

            // 8. 如果数据有任何变动（IsDirty 为 true），通知浏览器重新渲染页面
            if (result.IsDirty)
            {
                tickGeneration++;
                TryRefreshSidebarVitals();
                BindGameSession();
                // 跨线程调度：回到 UI 线程执行 StateHasChanged 局部重绘页面
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred during OnTimerElapsed tick");
        }
        finally
        {
            // 释放阀门锁，允许下一轮心跳进来
            _isTickProcessing = false;
        }
    }

    // 关闭鱼获大弹窗的提示
    private void DismissCatchBroadcast()
    {
        catchBroadcast = null;
        catchBroadcastMyth = false;
    }

    // 打工获得摊位券的数据库操作
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

    // 定时驱动鱼市 NPC 出价的数据库保存
    private async Task TryMarketNpcOffersAsync()
    {
        try
        {
            await WithDbLock(async () =>
            {
                await _marketService.TryGenerateNpcOffersAsync(player!.Id, cat.Happiness, GetHouseBuffs().NpcOfferChanceMultiplier, cat.Chm);
                await ReloadMarketAsync(); // 重新查询最新的出价并刷新页面
            });
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "TryMarketNpcOffersAsync 失败");
        }
    }

    // 每日折旧扣费
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
                feedMessage = msg; // 如果扣费失败导致欠费，在页面提醒玩家
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "TryMaintenanceAsync 失败");
        }
    }

    // 同步喂处理器槽位
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

    // 心跳中的数据库存档（合并了 Buff 计时和玩家数据更新，降低读写频率）
    private async Task SaveGameTickAsync()
    {
        if (player is null) return;
        try
        {
            List<ActiveCatBuff> updated = [];
            // 获取数据库排它锁，保存数据
            await WithDbLock(async () =>
            {
                updated = await _gamePersistenceService.SaveTickAsync(
                    player, cat, workingPlace.WorkTickCount);
            });
            lock (catStateLock)
            {
                activeFoodBuffs = updated; // 同步更新内存中的 Buff 时长
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SaveGameTickAsync 存档失败");
        }
    }

    // 强力存档：在退出、关网页或断开连接时，必须执行一次完整存档
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

    // 导航跳转到排行榜视图
    private async Task GoToLeaderboardAsync()
    {
        await SelectSectionAsync("fishing");
        await InvokeAsync(StateHasChanged);
    }

    // 垃圾回收清理（极其重要！）：当用户关闭此网页、退出游戏时，
    // Blazor 会自动调用此方法。我们必须在这里销毁所有的定时器，否则服务器内存会直接爆满崩溃。
    public async ValueTask DisposeAsync()
    {
        _leaderboardDebounceCts?.Cancel();
        _leaderboardDebounceCts?.Dispose();
        
        if (gameTimer != null)
        {
            gameTimer.Stop(); // 停止时钟
            gameTimer.Elapsed -= OnTimerElapsed; // 移除心跳绑定，解除强引用
            gameTimer.Dispose(); // 销毁定时器对象
        }
        
        fishingManager.Changed -= OnFishingChangedForChrome;
        
        if (player is not null && fishingManager.IsFishing)
            FishingSessionRegistry.Stop(player.Id, CircuitId);
            
        fishingManager.Dispose();
        
        if (player is not null)
        {
            GameSessionRegistry.Remove(player.Id, CircuitId);
            await PersistGameStateAsync(); // 离线前存一下档
        }
    }

    // 辅助计算：判断背包里是否还有能喂给自动喂食器的食物/水，且钱够不够加工费
    private bool CanAutoRefillFeeder()
    {
        if (player is null || feeder is null) return false;
        if (!player.AutoRefillUnlocked || !player.AutoRefillEnabled) return false; // 没解锁或没开自动功能
        if (!HasAutoFeederUnit()) return false; // 家里没装自动喂食器摆件
        if (feeder.FoodCount >= feeder.MaxFoodCount) return false; // 喂食器本身已经装满了
        if (player.Money < EconomySinks.FeederProcessingFee) return false; // 金币不够装填加工费

        var shop = new Shop();
        // 遍历背包，看看是否有符合自动装填资格的食物
        return player.Backpack.Keys.Any(name => FeederService.TryGetFeederFood(name, shop, out _));
    }

    // 辅助计算：同上，判断是否能自动给饮水器补水
    private bool CanAutoRefillWaterer()
    {
        if (player is null || waterer is null) return false;
        if (!player.AutoRefillUnlocked || !player.AutoRefillEnabled) return false;
        if (!HasWaterDispenser()) return false;
        if (waterer.WaterCount >= waterer.MaxWaterCount) return false;
        if (player.Money < EconomySinks.WatererProcessingFee) return false;

        // 背包里必须拥有“凉白开水（Purified）”且数量大于 0
        return player.Backpack.ContainsKey(WaterCatalog.Purified.Name) && player.Backpack[WaterCatalog.Purified.Name] > 0;
    }
}
