# 多人联机船钓系统——从零到一的开发与教学笔记

本项目基于 **Blazor Server** 与 **Entity Framework Core (PostgreSQL)** 架构。由于所有用户的浏览器会话（Circuit）都直接托管在服务器的同一个进程中，我们能够利用这一特性实现一种高响应、零网络延迟的**原生内存同步联机架构**。

本教学笔记将从最基础的联机原理开始，逐步拆解如何从零构建一个包含“购买小船”、“在线组队邀请”、“实时共享船钓甲板”与“跨玩家鱼货资料卡查看”的完整系统。

---

## 1. 基础概念：Blazor Server 联机同步原理

在传统的 Web 开发（如 React/Vue）中，要实现多人实时联机，必须在后端架设 **Websocket (SignalR/Socket.IO)** 服务器，并且前端需要通过 Socket 客户端进行复杂的网络握手、网络状态管理与心跳包重连。

但在 **Blazor Server** 中，原理完全不同：
1. **同一进程运行**：玩家A 和 玩家B 访问网站时，服务器会为每个人启动一个独立的 UI 渲染线程（称为 **Circuit**）。但它们底层共享同一个 .NET 运行时内存空间！
2. **单例（Singleton）服务共享**：如果我们在 DI 容器中注册一个生命周期为 `Singleton` 的服务，那么这个服务在服务器里**只有唯一的一个内存实例**。所有的玩家 Circuit 都可以同时读写这个实例。
3. **C# 事件分发**：当玩家A 在单例服务中修改了状态，可以通过标准的 **C# 事件 (Action/EventHandler)** 通知其他订阅了该事件的玩家 Circuit。其他玩家在监听到事件后，只需在各自的 UI 线程执行 `StateHasChanged()` 刷新，即可在毫秒级内完成状态的物理同步，无需任何额外的网络往返！

```
[ 玩家A 浏览器 ] ◄── SignalR (UI Diff) ──► [ 玩家A Circuit 线程 ] ──┐
                                                                 ▼
                                                  [ 单例内存服务 BoatSessionManager ]
                                                                 ▲
[ 玩家B 浏览器 ] ◄── SignalR (UI Diff) ──► [ 玩家B Circuit 线程 ] ──┘
```

---

## 2. 第一步：设计数据库表与 EF Core 映射 (Models)

虽然联机状态存在于内存中，但“玩家拥有的小船”和“船钓捕获的历史鱼货”必须持久化进数据库。

### 2.1 小船所有权模型：`PlayerBoat.cs`
在 `Models/PlayerBoat.cs` 中，我们定义了小船的所有权结构：
```csharp
using System;

namespace CyberPetApp.Models;

public class PlayerBoat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>所有者玩家 ID</summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>小船类型代号（例如: wood_boat, cyber_yacht）</summary>
    public string BoatType { get; set; } = "wood_boat";
    
    /// <summary>小船的自定义名称</summary>
    public string CustomName { get; set; } = "快乐木舢板";
    
    /// <summary>最大乘员数量</summary>
    public int MaxCapacity { get; set; } = 4;
    
    /// <summary>购买金币价格</summary>
    public int PurchasePrice { get; set; }
    
    /// <summary>购买时间</summary>
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
```

### 2.2 船钓专属鱼货模型：`BoatCatchRecord.cs`
用于记录多人出海时每条鱼是被谁在哪条船上钓起的。在 `Models/BoatCatchRecord.cs` 中：
```csharp
using System;

namespace CyberPetApp.Models;

public class BoatCatchRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>钓起者玩家 ID</summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>钓起者当时的玩家名字（反范式冗余，方便快速读取展示）</summary>
    public string PlayerName { get; set; } = "";
    
    /// <summary>捕获当时所在的小船名称</summary>
    public string BoatName { get; set; } = "";
    
    /// <summary>鱼类名称</summary>
    public string FishName { get; set; } = "";
    
    /// <summary>稀有度（Common, Rare, Epic, Legendary）</summary>
    public FishRarity Rarity { get; set; } = FishRarity.Common;
    
    /// <summary>体重 (kg)</summary>
    public double Weight { get; set; }
    
    /// <summary>尺寸百分比 (%)</summary>
    public double SizePercentage { get; set; }
    
    /// <summary>捕获时间 (UTC)</summary>
    public DateTime CaughtAt { get; set; } = DateTime.UtcNow;
}
```

### 2.3 配置 EF Core 映射：`AppDbContext.cs`
修改 `Data/AppDbContext.cs` 将它们声明为 `DbSet` 并在 `OnModelCreating` 中配置级联删除：
```csharp
public DbSet<PlayerBoat> PlayerBoats { get; set; } = null!;
public DbSet<BoatCatchRecord> BoatCatchRecords { get; set; } = null!;

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... 原有配置 ...

    modelBuilder.Entity<PlayerBoat>(entity =>
    {
        entity.HasKey(b => b.Id);
        entity.HasIndex(b => new { b.PlayerId, b.BoatType }).IsUnique();
        entity.HasOne<Player>()
            .WithMany()
            .HasForeignKey(b => b.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    modelBuilder.Entity<BoatCatchRecord>(entity =>
    {
        entity.HasKey(r => r.Id);
        entity.HasIndex(r => r.PlayerId);
        entity.HasOne<Player>()
            .WithMany()
            .HasForeignKey(r => r.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    });
}
```

> **基础教学命令**：每次修改数据库表后，需要通过控制台命令生成迁移文件并写入物理数据库：
> ```bash
> dotnet ef migrations add AddMultiplayerBoatSystem
> dotnet ef database update
> ```

---

## 3. 第二步：编写内存组队管理器 (Services)

我们利用进程内的单例服务维护当前正在活动的联机房间（Session）和玩家发出的联机邀请（Invitation）。

### 3.1 联机单例服务：`BoatSessionManager.cs`
该管理器支持线程安全的字典操作，并在联机房间更新时触发事件广播。

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CyberPetApp.Models;

namespace CyberPetApp.Services;

public enum BoatSessionStatus { Lobby, Fishing }

public class BoatSession
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }
    public string OwnerName { get; set; } = "";
    public string BoatType { get; set; } = "wood_boat";
    public string BoatName { get; set; } = "";
    public int MaxCapacity { get; set; } = 4;
    public BoatSessionStatus Status { get; set; } = BoatSessionStatus.Lobby;
    public ConcurrentDictionary<Guid, string> Members { get; set; } = new();
    public List<string> ActionLogs { get; set; } = new();
}

public class BoatInvitation
{
    public Guid InvitationId { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public Guid HostId { get; set; }
    public string HostName { get; set; } = "";
    public Guid InviteeId { get; set; }
    public string InviteeName { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalSeconds > 60;
}

public class BoatSessionManager
{
    private readonly ConcurrentDictionary<Guid, BoatSession> _activeSessions = new();
    private readonly ConcurrentDictionary<Guid, Guid> _playerSessionMap = new();
    private readonly ConcurrentDictionary<Guid, BoatInvitation> _pendingInvitations = new();

    public event Action<Guid>? OnSessionUpdated;
    public event Action<Guid, BoatInvitation>? OnInvitationReceived;

    public BoatSession? GetSessionForPlayer(Guid playerId)
    {
        if (_playerSessionMap.TryGetValue(playerId, out var sessionId))
            return _activeSessions.TryGetValue(sessionId, out var s) ? s : null;
        return null;
    }

    public List<(Guid Id, string Name)> GetAvailablePlayers(Guid excludePlayerId, Dictionary<Guid, string> onlinePlayers)
    {
        return onlinePlayers
            .Where(kv => kv.Key != excludePlayerId && !_playerSessionMap.ContainsKey(kv.Key))
            .Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public BoatSession CreateSession(Guid ownerId, string ownerName, PlayerBoat boat)
    {
        LeaveSession(ownerId);
        var session = new BoatSession
        {
            OwnerId = ownerId, OwnerName = ownerName,
            BoatType = boat.BoatType, BoatName = boat.CustomName,
            MaxCapacity = boat.MaxCapacity
        };
        session.Members.TryAdd(ownerId, ownerName);
        _activeSessions[session.SessionId] = session;
        _playerSessionMap[ownerId] = session.SessionId;
        session.ActionLogs.Add($"[系统] 船长 {ownerName} 创建了小船 {session.BoatName}！");
        NotifyUpdate(session.SessionId);
        return session;
    }

    public bool SendInvitation(Guid sessionId, Guid hostId, string hostName, Guid inviteeId, string inviteeName)
    {
        if (!_activeSessions.TryGetValue(sessionId, out var session) || session.Members.Count >= session.MaxCapacity)
            return false;
        var invite = new BoatInvitation { SessionId = sessionId, HostId = hostId, HostName = hostName, InviteeId = inviteeId, InviteeName = inviteeName };
        _pendingInvitations[invite.InvitationId] = invite;
        OnInvitationReceived?.Invoke(inviteeId, invite);
        return true;
    }

    public List<BoatInvitation> GetInvitationsForPlayer(Guid playerId)
    {
        return _pendingInvitations.Values.Where(i => i.InviteeId == playerId && !i.IsExpired).ToList();
    }

    public bool AcceptInvitation(Guid inviteId, Guid playerId, string playerName)
    {
        if (!_pendingInvitations.TryRemove(inviteId, out var invite) || invite.IsExpired) return false;
        if (!_activeSessions.TryGetValue(invite.SessionId, out var session) || session.Members.Count >= session.MaxCapacity)
            return false;
        LeaveSession(playerId);
        session.Members.TryAdd(playerId, playerName);
        _playerSessionMap[playerId] = session.SessionId;
        session.ActionLogs.Add($"[系统] 玩家 {playerName} 登上了小船！");
        NotifyUpdate(session.SessionId);
        return true;
    }

    public void DeclineInvitation(Guid inviteId) => _pendingInvitations.TryRemove(inviteId, out _);

    public void StartFishing(Guid sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Status = BoatSessionStatus.Fishing;
            session.ActionLogs.Add($"[系统] 船钓正式开始！");
            NotifyUpdate(sessionId);
        }
    }

    public void StopFishing(Guid sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Status = BoatSessionStatus.Lobby;
            session.ActionLogs.Add($"[系统] 船钓结束，返回港湾。");
            NotifyUpdate(sessionId);
        }
    }

    public void LeaveSession(Guid playerId)
    {
        if (!_playerSessionMap.TryRemove(playerId, out var sessionId)) return;
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.Members.TryRemove(playerId, out var name);
            session.ActionLogs.Add($"[系统] {name} 离开了小船。");
            if (session.OwnerId == playerId)
            {
                session.ActionLogs.Add($"[系统] 船长离开了房间，小船解散。");
                foreach (var memberId in session.Members.Keys) _playerSessionMap.TryRemove(memberId, out _);
                _activeSessions.TryRemove(sessionId, out _);
            }
            NotifyUpdate(sessionId);
        }
    }

    public void BroadcastCatch(Guid sessionId, string playerName, string fishName, FishRarity rarity, double weight)
    {
        if (_activeSessions.TryGetValue(sessionId, out var session))
        {
            session.ActionLogs.Add($"[鱼获] 玩家 {playerName} 成功钓起 {fishName} ({weight:F2} kg)！");
            NotifyUpdate(sessionId);
        }
    }

    private void NotifyUpdate(Guid sessionId) => OnSessionUpdated?.Invoke(sessionId);
}
```

---

## 4. 第三步：在线会话追踪与下线自动清理机制

当玩家刷新页面、关闭浏览器标签页或者掉线时，我们需要实时检测到这种生命周期变化，并自动将其从小船和在线列表中剔除，防止出现“幽灵成员”。

### 4.1 会话上下文：`CircuitSessionContext.cs`
用于将当前的 Circuit ID 与玩家个人信息在 DI 作用域内关联：
```csharp
using System;

namespace CyberPetApp.Services;

public class CircuitSessionContext
{
    public string? CircuitId { get; set; }
    public Guid PlayerId { get; set; }
    public string Username { get; set; } = "";
}
```

### 4.2 连接监听器：`FishingCircuitHandler.cs`
通过重写 Blazor 的 `CircuitHandler`，当连接断开（`OnCircuitClosedAsync`）时，清理内存状态：
```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace CyberPetApp.Services;

public sealed class FishingCircuitHandler : CircuitHandler
{
    private readonly CircuitSessionContext _context;
    private readonly OnlineTracker _tracker;
    private readonly BoatSessionManager _boatSessionManager;

    public FishingCircuitHandler(CircuitSessionContext context, OnlineTracker tracker, BoatSessionManager boatSessionManager)
    {
        _context = context;
        _tracker = tracker;
        _boatSessionManager = boatSessionManager;
    }

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        _context.CircuitId = circuit.Id;
        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        if (_context.PlayerId != Guid.Empty)
        {
            // 下线清理：自动退出房间/解散船只
            _boatSessionManager.LeaveSession(_context.PlayerId);
        }
        _tracker.UnregisterOnline(circuit.Id);
        FishingSessionRegistry.StopByCircuit(circuit.Id);
        return Task.CompletedTask;
    }
}
```

---

## 5. 第四步：编写防冲突的数据持久化服务 (BoatService)

在 Blazor Server 中，频繁操作数据库时，如果多个 Circuit 线程在同一个 Scoped `DbContext` 实例上并发调用 `SaveChangesAsync`，就会发生数据库锁冲突死机。
所以 `BoatService` 内部使用 `IDbContextFactory<AppDbContext>`，每次操作都创建一次临时的短生命周期 Context 实例，确保绝对的并发安全：

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class BoatService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public BoatService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task<List<PlayerBoat>> GetPlayerBoatsAsync(Guid playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.PlayerBoats.Where(b => b.PlayerId == playerId).ToListAsync();
    }

    public async Task<(bool Success, string Message)> PurchaseBoatAsync(Guid playerId, string boatType, string boatName, int price, int capacity)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var player = await db.Players.FindAsync(playerId);
        if (player == null) return (false, "玩家不存在");
        if (player.Money < price) return (false, $"金币不足，需要 {price}g");

        var owns = await db.PlayerBoats.AnyAsync(b => b.PlayerId == playerId && b.BoatType == boatType);
        if (owns) return (false, "您已拥有此类型的小船！");

        player.Money -= price;
        db.PlayerBoats.Add(new PlayerBoat { PlayerId = playerId, BoatType = boatType, CustomName = boatName, MaxCapacity = capacity, PurchasePrice = price });
        await db.SaveChangesAsync();
        return (true, $"成功购买 {boatName}！");
    }

    public async Task SaveBoatCatchAsync(Guid playerId, string playerName, string boatName, Fish fish)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.BoatCatchRecords.Add(new BoatCatchRecord
        {
            PlayerId = playerId, PlayerName = playerName, BoatName = boatName,
            FishName = fish.Name, Rarity = fish.Rarity, Weight = fish.ActualWeight,
            SizePercentage = fish.SizePercentage
        });
        await db.SaveChangesAsync();
    }

    public async Task<List<BoatCatchRecord>> GetBoatCatchHistoryAsync(Guid playerId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.BoatCatchRecords
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.CaughtAt).ToListAsync();
    }
}
```

---

## 6. 第五步：注册依赖 (`Program.cs`)

在 `Program.cs` 中，我们需要做两件事：
1. **替换为 DbContext 工厂**：原先的 `builder.Services.AddDbContext<AppDbContext>` 会导致 `BoatService` 无法解析 `IDbContextFactory<AppDbContext>`。我们需要修改为 `AddDbContextFactory`（它会同时注册 `IDbContextFactory` 和 Scoped 的 `DbContext` 以保持对旧服务的兼容）：
```csharp
// 替换为 DbContextFactory
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

2. **注册联机与数据库服务**：联机状态管理必须使用 Singleton，保证所有在线用户的 Circuit 读写同一处内存；数据库交互服务使用 Scoped 即可：
```csharp
// 联机状态管理必须使用 Singleton，保证所有在线用户的 Circuit 读写同一处内存
builder.Services.AddSingleton<BoatSessionManager>();
builder.Services.AddSingleton<OnlineTracker>();

// 数据库交互服务使用 Scoped，与玩家的 Circuit 连接生命周期绑定即可
builder.Services.AddScoped<BoatService>();
```

---

## 7. 第六步：页面 UI 与交互逻辑实现

### 7.1 联机船钓主面板：`HomeBoatTab.razor`
在这里，我们通过监听 `SessionManager.OnSessionUpdated` 事件，在联机房间日志、船员加入或状态切换时调用 `InvokeAsync(StateHasChanged)`，使全船玩家界面毫秒级同步。

**关键事件订阅逻辑：**
```csharp
protected override async Task OnInitializedAsync()
{
    playerId = SessionContext.PlayerId;
    username = SessionContext.Username;

    if (playerId != Guid.Empty)
    {
        // 1. 注册进入全局在线 Tracker
        Tracker.RegisterOnline(SessionContext.CircuitId ?? "", playerId, username);
    }

    // 2. 加载玩家的小船列表与最新房间状态
    await LoadMyBoatsAsync();
    RefreshSessionData();

    // 3. 订阅事件以进行实时状态同步刷新
    SessionManager.OnSessionUpdated += HandleSessionUpdated;
    SessionManager.OnInvitationReceived += HandleInvitationReceived;
    Tracker.OnOnlineListChanged += HandleOnlineListChanged;
}

private void HandleSessionUpdated(Guid sessionId)
{
    // 如果是玩家当前所在的小船更新，强制重新刷新当前 Circuit 的组件
    if (currentSession?.SessionId == sessionId || currentSession == null)
    {
        RefreshSessionData();
    }
}

// 4. 注意：组件销毁时，必须及时退订，防止事件内存泄漏！
public void Dispose()
{
    SessionManager.OnSessionUpdated -= HandleSessionUpdated;
    SessionManager.OnInvitationReceived -= HandleInvitationReceived;
    Tracker.OnOnlineListChanged -= HandleOnlineListChanged;
}
```

### 7.2 玩家资料与船钓鱼货弹窗：`PlayerProfileModal.razor`
当在船员列表里点击任何人的名字，将触发此弹窗。该弹窗会根据玩家 ID 从 `BoatService` 实时提取其数据库中所有的船钓鱼货记录。

```razor
@code {
    [Parameter, EditorRequired] public Guid PlayerId { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private Player? targetPlayer;
    private string targetUsername = "";
    private List<BoatCatchRecord> catchHistory = new();
    private bool isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        isLoading = true;
        try
        {
            // 从数据库拉取其个人等级与基础信息
            targetPlayer = await PlayerDb.GetPlayerAsync(PlayerId);
            targetUsername = await PlayerDb.GetUsernameAsync(PlayerId);

            // 从专属联机捕获记录表拉取船钓鱼货
            catchHistory = await BoatDb.GetBoatCatchHistoryAsync(PlayerId);
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

---

## 8. 第七步：联机开发环境下的多端测试方法

当功能完成后，我们需要模拟多位玩家同时在线，以验证组队和起鱼的同步响应。

1. **第一端 (主标签页)**：打开普通的 Chrome / Edge 窗口登录 **玩家A**，进入“联机小船”面板。
2. **第二端 (无痕窗口)**：按下 `Ctrl + Shift + N` 打开一个全新的**无痕浏览窗口 (Incognito Mode)** 登录 **玩家B**。
   > **关键知识点**：无痕模式具有独立的 Cookie 上下文，因此服务器会将它视作一个完全不同的新 Circuit 客户端，从而能同时登入两个不一样的玩家账号。
3. **流程验证**：
   - 玩家A 购买游艇后，点击“作为船长起航”。
   - 玩家A 的“可邀请列表”中会实时呈现玩家B，点击“邀请”。
   - 观察玩家B 的无痕窗口，顶端应瞬时跳出“📩 收到来自 玩家A 的登船邀请”通知。
   - 玩家B 点击接受，双方列表同时呈现两名玩家。
   - 船长开启船钓，抛竿钓鱼，观察两个浏览器中的状态日志，起鱼记录应以亚秒级延迟双向广播同步！
