# 第 15 章：联机船钓、跨 Circuits 广播与 DB 厂模式 🚢

除了个人在岸边单机钓鱼，CyberPetApp 还允许玩家在大厅创建联机房间、组队邀请在线好友，登上一艘船前往远海开启**联机船钓模式**。这一章深入解析联机底层如何在内存中维持并发安全、如何在不同的浏览器会话间实时同步消息，以及高并发下如何安全地进行数据库读写。

---

### 15.1 联机房间与内存并发安全（ConcurrentDictionary）

船钓房间并非保存在持久数据库中，而是以内存会话的形式存放在单例服务 [BoatSessionManager.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/BoatSessionManager.cs) 中。由于整台服务器的所有玩家连接会并发访问这个单例服务，传统的 `Dictionary` 在并发读写时会直接抛出空指针或键重复异常。

我们使用 .NET 线程安全集合 **`ConcurrentDictionary`** 来确保内存安全：

```csharp
public class BoatSessionManager
{
    // 所有活跃的船只房间 (SessionId -> Session)
    private readonly ConcurrentDictionary<Guid, BoatSession> _activeSessions = new();
    
    // 当前在线玩家与其关联的房间映射 (PlayerId -> SessionId)
    private readonly ConcurrentDictionary<Guid, Guid> _playerSessionMap = new();

    // 活跃的组队邀请列表 (InvitationId -> Invitation)
    private readonly ConcurrentDictionary<Guid, BoatInvitation> _pendingInvitations = new();
}
```

所有的增删改查动作都使用原子性的安全方法（如 `TryAdd`、`TryRemove` 或 `TryGetValue`）进行，从底层杜绝了内存死锁和多线程读写破坏集合结构的问题。

---

### 15.2 跨 Circuit 会话的事件派发与实时广播

在 Blazor Server 中，每个用户的连接都是一个独立的 **Circuit（电路会话）**，拥有自己专属的线程范围和生命周期。如果玩家 A 接受了玩家 B 的船钓邀请上了船，我们要如何通知正在看屏幕的玩家 B，并在其屏幕上立刻画出“玩家 A 已登船”的头像呢？

我们采用 **C# 事件委托（Event Action）** 机制，在单例服务中声明订阅事件：

```csharp
public class BoatSessionManager
{
    // 实时更新事件：当房间成员变动、状态改变或船只移动时，通知相关客户端刷新 UI
    public event Action<Guid>? OnSessionUpdated;
    
    // 实时通知事件：当有新的邀请发送过来时，通知对应的目标玩家
    public event Action<Guid, BoatInvitation>? OnInvitationReceived;

    private void NotifyUpdate(Guid sessionId)
    {
        // 触发委托事件，所有订阅了此事件的 Circuit 都会收到 sessionId 通知
        OnSessionUpdated?.Invoke(sessionId);
    }
}
```

#### 在前端页面组件中订阅与注销事件：
在 `BoatLobby.razor` 联机页面中，我们必须在页面加载时订阅这两个全局事件，并在页面销毁时注销它们，否则会导致内存中不断积压垃圾 Circuit 的引用：

```razor
@implements IDisposable
@inject BoatSessionManager SessionManager

@code {
    protected override void OnInitialized()
    {
        // 订阅事件：当此房间发生变动时，执行本地刷新方法
        SessionManager.OnSessionUpdated += HandleSessionUpdated;
    }

    private async void HandleSessionUpdated(Guid changedSessionId)
    {
        if (mySession?.SessionId == changedSessionId)
        {
            // 跨线程调度：安全地将渲染事件扔回本 Circuit 的 UI 线程执行
            await InvokeAsync(() =>
            {
                // 重新加载房间状态并强制重绘
                RefreshSessionState();
                StateHasChanged();
            });
        }
    }

    public void Dispose()
    {
        // ⚠️ 极其重要：离开页面时必须解除订阅，否则会造成严重的内存泄漏！
        SessionManager.OnSessionUpdated -= HandleSessionUpdated;
    }
}
```

---

### 15.3 线程安全数据库操作：DB 厂模式（`IDbContextFactory`）

在前几章中，我们提到 `AppDbContext` 被注册为 `Scoped` 作用域。这在单用户的 UI 点击交互下运行良好。但是，在联机船钓中，后台常驻任务在持续运行，并且需要将整条船上各个玩家钓起鱼的纪录随时保存进数据库（通过 `SaveBoatCatchAsync`）。

如果在 Scoped 模式下，多个线程在同一时间点共享并使用同一个 `DbContext` 写入数据，就会引起并发崩溃异常。

为了彻底解决长连接多线程下的 `DbContext` 并发访问冲突，我们不向构造函数注入 `AppDbContext`，而是注入它的制造工厂 **`IDbContextFactory<AppDbContext>`**：

```csharp
namespace CyberPetApp.Services;

public class BoatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    // 注入数据库上下文工厂
    public BoatService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task SaveBoatCatchAsync(Guid playerId, string playerName, string boatName, Fish fish)
    {
        // 每次执行写入时，向工厂申请创建一个全新的、临时临时的 DbContext 实例
        using var db = await _dbFactory.CreateDbContextAsync();
        
        var record = new BoatCatchRecord
        {
            PlayerId = playerId,
            PlayerName = playerName,
            BoatName = boatName,
            FishName = fish.Name,
            Rarity = fish.Rarity,
            Weight = fish.ActualWeight,
            CaughtAt = DateTime.UtcNow
        };

        db.BoatCatchRecords.Add(record);
        
        // 保存完毕后，由于 using 块结束，该临时 db 实例会被立即关闭并释放资源
        await db.SaveChangesAsync();
    }
}
```

#### 💡 工厂模式的黄金法则：
通过使用 `IDbContextFactory`，我们使得每次数据库查询和写入都变成了**短寿命（Short-Lived）**的事务。每个方法独立拥有一条数据库连接，执行完立即关闭，不仅实现了 $100\%$ 的线程安全，还极大减轻了数据库连接池的长连接占用压力，让高并发多用户联机平稳丝滑！
