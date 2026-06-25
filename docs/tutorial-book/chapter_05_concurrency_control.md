# 第 5 章：并发控制与多线程安全 (核心难点) 🔒

### 5.1 为什么 Blazor Server 存在线程安全隐患？

在典型的 Blazor Server 架构下，所有的逻辑都在服务器上运行。当用户在浏览器上发生操作（例如点击“喂食”按钮）时，会调用 UI 线程执行对应的 C# 方法；与此同时，后台的定时器（如每 2 秒一次的数值衰减、打工收益 Tick）也运行在独立的服务端后台线程中。

如果玩家在频繁操作 UI 界面（如连续点击喂食、切换背包）的同时，后台心跳线程也在运行，两者就会**并发**读写内存中的同一实例（如猫咪实体 `cat.Hunger`），从而发生**竞态条件（Race Condition）**。

更致命的是，**EF Core 的 `DbContext` 在设计上绝对是非线程安全的**。如果主 UI 线程与后台心跳线程同时调用 `SaveChangesAsync()`，就会引起数据冲突并直接在后台爆出致命崩溃：
```text
System.InvalidOperationException: A second operation was started on this context instance before a previous operation completed. This is usually caused by different threads concurrently using the same instance of DbContext.
```

因此，我们必须为应用套上合理的“同步锁”来保护临界区资源。

---

### 5.2 内存对象同步锁：`lock` 关键字

为了防止多线程同时修改内存中猫咪的五维指标，我们在当前 Circuit 会话中定义了一个只读的“哨兵锁对象”：
```csharp
private readonly object catStateLock = new();
```

任何线程在读写猫咪属性之前，必须通过 `lock` 关键字夺取这个“哨兵锁”。如果锁已经被另一个线程占用了，当前线程将会在外面排队等待，直到锁被释放：

```csharp
public void UpdateCatState()
{
    lock (catStateLock)
    {
        // 临界区代码：同一时间有且仅有一个线程能进入此块
        cat.Hunger = Math.Max(0, cat.Hunger - 2);
        cat.Energy = Math.Min(1000, cat.Energy + 1);
    }
}
```

> [!WARNING]
> **锁防区限制**：`lock` 是操作系统线程级的同步原语。在 `lock` 作用域的大括号内，**绝不允许**出现 `await` 关键字。因为当线程执行到 `await` 挂起时，可能会被切换到其他线程恢复执行，这会导致 `lock` 的锁状态发生混乱，编译期也会直接报错。

---

### 5.3 数据库异步锁：`SemaphoreSlim` 与 `WithDbLock`

既然 `lock` 内不能使用 `await`，而数据库保存数据 `await _context.SaveChangesAsync()` 又必须是异步的，我们要如何锁定数据库操作呢？

答案是使用 .NET 提供的异步轻量信号量 **`SemaphoreSlim`**。

我们在 [Home.razor.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/Pages/Home.razor.cs) 中声明并包装了 `WithDbLock` 工具方法，用于集中管控所有的数据库读写：

```csharp
// 初始化信号量：最大允许同时通过的人数为 1 (相当于一个互斥锁)
private readonly SemaphoreSlim dbLock = new(1, 1);

private async Task WithDbLock(Func<Task> action)
{
    // 异步排队等待锁。如果有另一个线程正在写库，这里会优雅挂起，让出 CPU
    await dbLock.WaitAsync();
    try
    {
        // 独占执行传入的数据库操作任务
        await action();
    }
    finally
    {
        // 无论里面代码是否抛出异常，都必须在 finally 里释放锁，防止造成死锁
        dbLock.Release();
    }
}
```

#### 使用示例：
```csharp
await WithDbLock(async () =>
{
    player.Money += 100;
    _context.Players.Update(player);
    await _context.SaveChangesAsync(); // 安全写库，免受冲突
});
```

---

### 5.4 锁重入死锁风险（Deadlock Warning）

`SemaphoreSlim` 是**不可重入**的。
如果一个正在持锁运行的方法内部，又调用了另一个也试图获取相同锁的方法，程序就会陷入**自我等待**的死局，页面瞬间永久卡死：

```csharp
// ❌ 错误示范：自我死锁
await WithDbLock(async () =>
{
    await SavePlayerStatsAsync(); // 假设 SavePlayerStatsAsync 内部也调用了 WithDbLock
});
```

---

### 5.5 终极防护解决方案：有条件锁定模式 (Conditional Locking)

为了在杜绝 `DbContext` 并发冲突的同时，彻底避免死锁，CyberPetApp 引入了**有条件锁定模式**。

#### 1. 方法签名参数化
将所有“既可以作为外层独立调用，又可以嵌套在其他大事务内部”的数据刷新方法（如 `ReloadGearAsync`, `ReloadMilestonesAsync` 等），重构为接收一个可选的 `bool acquireLock = true` 参数：

```csharp
private async Task ReloadGearAsync(bool acquireLock = true)
{
    // 提取真正的业务数据读取体
    Func<Task> body = async () =>
    {
        myRods = await _equipmentService.GetRodsAsync(player!.Id);
        // ... 其他数据库加载
    };

    if (acquireLock)
    {
        // 外层没有加锁时调用：加锁执行，确保线程安全
        await WithDbLock(body);
    }
    else
    {
        // 外层已经持有了 dbLock 时调用：直接执行，避免锁重入引起死锁！
        await body();
    }
}
```

#### 2. 写读合并原子操作
在执行 UI 点击或者钓鱼心跳收尾时，我们决不能在“锁里写数据，锁外直接 Reload”。而是把它们合并在同一个锁段里，通过传递 `acquireLock: false` 规避重入风险：

```csharp
private async Task HandleGearWearAsync(bool lineBreak)
{
    try
    {
        await WithDbLock(async () =>
        {
            // 1. 写操作：扣减耐久
            await _equipmentService.WearEquippedGearAsync(player!.Id, lineBreak);
            
            // 2. 读操作：刷新页面装备状态（传递 false 避免自我死锁）
            await ReloadGearAsync(acquireLock: false);
        });
        
        await InvokeAsync(StateHasChanged);
    }
    catch (Exception ex)
    {
        Logger.LogWarning(ex, "HandleGearWearAsync 失败");
    }
}
```

#### 3. 心跳定时器防重叠标志
在心跳触发的方法中，如果上一轮心跳的异步写库事务由于网络卡顿还没完成，决不能让下一轮心跳并发进来。我们在心跳入口处加锁或布尔阀门：
```csharp
private bool _isTickProcessing = false;

private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
{
    if (_isTickProcessing) return; // 上一轮还没处理完，直接跳过本轮，防止请求积压
    _isTickProcessing = true;
    try
    {
        // 执行 Tick
    }
    finally
    {
        _isTickProcessing = false;
    }
}
```
通过这套机制，整个游戏完美地对 UI 交互与定时器心跳的 `DbContext` 访问进行了线程间串行化，同时兼顾了高并发与零死锁。
