# 第 6 章：定时器与后台心跳循环 ⏱️

### 6.1 基于 Timer 的 2 秒心跳

在网页挂机养成类游戏中，时间的流逝和属性的变化需要由一个持续的“心跳机制”来驱动。在 CyberPetApp 中，每个玩家打开游戏页面后，服务器都会在内存中开启一个专有的定时器：
```csharp
private System.Timers.Timer gameTimer;
```

#### 1. 为什么用 `System.Timers.Timer` 而不用 `System.Threading.Thread.Sleep`？
使用 `Thread.Sleep` 会强行让当前执行线程进入睡眠状态，完全霸占并浪费服务器的宝贵线程资源；而 `System.Timers.Timer` 是基于**系统时钟事件和线程池调度**的。它到了设定的时间后，会自动从线程池挑一个空闲的协程/线程来执行事件回调，不会产生任何无意义的线程占用。

#### 2. 定时器注册与心跳：
在 [Home.GameLoop.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/Pages/Home.GameLoop.cs) 中：
```csharp
private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
{
    if (player is null) return;

    // 运行游戏 Tick 逻辑并获取结果快照
    var result = _tickOrchestrator.RunTick(
        new GameTickInput { ... },
        _tickCounters,
        catStateLock);

    // 异步保存当前的进度存档
    // _ = 表示放弃同步等待，在后台线程中慢慢写入，不要阻塞心跳的后续逻辑
    _ = SaveGameTickAsync();
    
    // 如果数值（如饱食度降了，或者钱加了）发生了变动，刷新 UI 界面
    if (result.IsDirty)
    {
        InvokeAsync(StateHasChanged);
    }
}
```

---

### 6.2 优雅的离线补偿逻辑

当玩家退出登录或关闭浏览器后，服务端对应的 Circuit 实例会被销毁，对应的定时器也就停止运行了。但是，作为一款赛博猫咪养成游戏，我们必须让猫咪在玩家不在线的时候**继续演化**（例如离线期间饥饿度继续降低，或者家具被动继续产出少量金币）。

我们采用的方案是：**在重新登录时，一次性计算并追补所有的离线 Tick**。
[OfflineCompensationService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/OfflineCompensationService.cs) 实现了这个被动数值补算算法：

```csharp
public async Task<OfflineResult> ApplyAsync(Player player, CyberCat cat, ...)
{
    var now = DateTime.UtcNow;
    // 1. 计算自上次活跃到现在的总时间跨度
    var elapsed = now - player.LastActiveAt;
    
    // 2. 将离线毫秒数折算为 Tick 次数（本项目中每 2000 毫秒为一个 Tick）
    int rawTicks = (int)(elapsed.TotalMilliseconds / TickIntervalMs);
    
    // 3. 限制最高补偿额度（防止玩家下线半年后上线，导致数值无限透支或挂机产出直接让游戏经济崩盘）
    // 我们设定最大补偿 30 分钟 (900 次 Ticks)
    int ticks = Math.Min(rawTicks, MaxOfflineTicks);
    
    for (int i = 0; i < ticks; i++)
    {
        // 循环模拟每一次 tick 中产生的：
        cat.Tick(buffs); // 属性扣减与衰减
        workingPlace.Tick(player, cat, out bool earnedTicket, buffs); // 挂机打工判定
        // ...
    }

    // 4. 将玩家最后活动时间更新为当前时间
    player.LastActiveAt = now;
    
    return new OfflineResult 
    { 
        Summary = $"您已离线 {(int)elapsed.TotalMinutes} 分钟，获得了 {ticks} 次后台数值补偿..." 
    };
}
```

---

### 6.3 异步控制与 `CancellationToken` 取消流

除了常规的 2 秒心跳，游戏中还有一种更复杂的局部循环——**钓鱼的循环流程**。
钓鱼循环有等待、咬钩、遛鱼等不同时长阶段，且可以一直挂机下去。我们不能在 2s 定时器中塞入钓鱼状态机，而是使用一个独立的 `async/await` 异步循环在后台执行。

在这个长时间运行的后台任务中，如果玩家点击了“收竿”按钮，或者退出了页面，我们要如何瞬间叫停正在 `Task.Delay()` 等待中的后台任务呢？

我们使用 .NET 标准的 **`CancellationToken`（取消令牌）** 机制。

#### 核心代码实现解析：
分析 [FishingManager.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/FishingManager.cs)：

```csharp
public class FishingManager
{
    private CancellationTokenSource? _cts;

    public void StartFishing(...)
    {
        // 1. 创建令牌源
        _cts = new CancellationTokenSource();
        
        // 2. 开启异步死循环，并将令牌传入
        _ = RunLoopAsync(spot, _cts.Token);
    }

    public void StopFishing()
    {
        // 当玩家手动点击“收竿”按钮时，发出取消信号
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private async Task RunLoopAsync(FishingSpot spot, CancellationToken ct)
    {
        try
        {
            // 在循环头和每一次 Task.Delay 时都传入取消令牌
            while (!ct.IsCancellationRequested)
            {
                // 等待鱼口 (5~12秒)：如果在此期间 _cts.Cancel() 被调用，
                // Task.Delay 会瞬间抛出 OperationCanceledException 异常退出，杜绝多余等待。
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);

                // 抓口判定窗口 (2~4秒)
                await Task.Delay(TimeSpan.FromSeconds(biteWindow), ct);
            }
        }
        catch (OperationCanceledException)
        {
            // 捕获到取消异常，说明玩家收竿了，安静地退出流程即可
        }
    }
}
```
通过 `CancellationToken`，系统内的各个异步轮询和网络请求都具备了“召之即来，挥之即去”的控制能力，保障了服务器运行的高效与干净。
