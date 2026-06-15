# 自动修复工具功能设计与实现思路（学习笔记）

本项目在渔具商店中新增了**“自动修复工具”**功能。当玩家解锁并开启此功能后，可以在装备（鱼竿、卷线器、鱼线）耐久度下降到设定阈值时自动执行全修扣费。

以下是关于该功能的架构设计思路、开发做法、关键代码及核心设计模式的总结。

---

## 一、 核心设计思路（Architecture & Rationale）

### 1. 数据存储与生命周期管理：为什么直接存在 `Player` 表？
* **方案选择**：在 `Player` 模型中增加 `AutoRepairUnlocked`、`AutoRepairEnabled` 和 `AutoRepairThreshold` 属性，而不是建立独立的实体关联表。
* **考量与优点**：
  - **减少数据库 IO**：如果使用独立的表，每次加载游戏或校验余额时都需要进行 `JOIN` 查询。将简单配置存在 `Player` 实体中，可以让它在加载玩家数据时被“一次性读出”，零成本获取。
  - **存盘一致性**：可以直接利用原有的 `_playerService.SaveProgressAsync(player)` 统一保存，避免维护多套实体的变更保存逻辑。

### 2. 触发时机：事件驱动 vs 定时轮询
* **事件驱动（采用）**：将自动修复的判定切入点放置在 **“磨损计算事件”**（`HandleGearWearAsync`）中。
* **考量与优点**：
  - 如果使用后台 `Timer` 轮询检查耐久度并扣款，会引入很多不必要的 CPU/DB 轮询损耗，还可能在钓起大鱼耐久为零的瞬间与轮询器发生并发踩踏冲突。
  - 只有在耐久度发生实际下降（即发生磨损）后才在同一个数据库并发锁（`WithDbLock`）中运行自动修复校验，是最经济、也是最线程安全的做法。

### 3. Blazor 页面架构：事件冒泡与状态拥有者模式
* **组件层次**：`Home.razor`（状态拥有者） $\rightarrow$ `HomeGearTab.razor`（装备Tab） $\rightarrow$ `GearShopPanel.razor`（商店设置面板）。
* **考量与优点**：
  - 遵循**单向数据流**原则。`GearShopPanel` 只负责将交互（如点击购买、拖动阈值、切换开关）转换为 `EventCallback` 冒泡传回给主页，再由主页在 DB 线程锁下修改数据库并刷新内存快照。
  - 这保证了主菜单标题栏显示的“金币数”、左侧边栏的“状态监控”和装备面板的显示耐久能够时刻保持严格一致。

---

## 二、 关键代码分析

### 1. 实体定义与数据库配置
在 `Models/Player.cs` 中增加玩家的配置属性：

```csharp
// Models/Player.cs
public class Player
{
    // ... 原有属性
    
    // ── 自动修复设置 ──
    public bool AutoRepairUnlocked { get; set; } = false; // 是否购买了自动修复工具
    public bool AutoRepairEnabled { get; set; } = false;  // 是否开启了自动修复功能
    public int AutoRepairThreshold { get; set; } = 20;    // 自动修复触发的耐久度百分比阈值
}
```

在 `Data/AppDbContext.cs` 中为旧数据做向后兼容默认值配置：

```csharp
// Data/AppDbContext.cs
entity.Property(p => p.AutoRepairUnlocked).HasDefaultValue(false);
entity.Property(p => p.AutoRepairEnabled).HasDefaultValue(false);
entity.Property(p => p.AutoRepairThreshold).HasDefaultValue(20);
```

---

### 2. 持久化层同步
在 `Services/PlayerService.cs` 中，由于实体可能不是引用的同一个实例，我们需要将内存对象的状态手动同步给 EF Core 跟踪的本地实体：

```csharp
// Services/PlayerService.cs
public void SyncProgressToTracked(Player player)
{
    var tracked = _context.Players.Local.FirstOrDefault(p => p.Id == player.Id)
        ?? _context.Players.Find(player.Id);
    if (tracked is null) return;

    if (!ReferenceEquals(tracked, player))
    {
        // ... 其他属性同步
        tracked.AutoRepairUnlocked = player.AutoRepairUnlocked;
        tracked.AutoRepairEnabled = player.AutoRepairEnabled;
        tracked.AutoRepairThreshold = player.AutoRepairThreshold;
    }
}
```

---

### 3. 耐久度变化时的自动修复逻辑（核心切入点）
在 `Components/Pages/Home.Fishing.cs` 中的磨损处理中，增加自动修复判定。在有锁的事务内完成检测、扣款、保存以及记录终端日志：

```csharp
// Components/Pages/Home.Fishing.cs
private async Task HandleGearWearAsync(bool lineBreak, bool isEscape)
{
    try
    {
        List<string> wearLogs = [];
        List<string> autoRepairLogs = [];
        
        await WithDbLock(async () =>
        {
            // 1. 先结算本次造成的装备磨损
            wearLogs = await _equipmentService.WearEquippedGearAsync(player!.Id, lineBreak, fishingManager.CurrentSpot?.Name, isEscape);
            
            // 2. 判定自动修复条件
            if (player.AutoRepairUnlocked && player.AutoRepairEnabled)
            {
                var rod = await _equipmentService.GetEquippedRodAsync(player.Id);
                var reel = await _equipmentService.GetEquippedReelAsync(player.Id);
                var line = await _equipmentService.GetEquippedLineAsync(player.Id);

                // 鱼竿自动修复
                if (rod is not null && rod.Durability <= player.AutoRepairThreshold)
                {
                    var (ok, msg) = await _equipmentService.RepairRodAsync(player, rod.Id, fullRepair: true);
                    if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                    else autoRepairLogs.Add($"自动修复失败: {msg}");
                }
                // 卷线器自动修复
                if (reel is not null && reel.Durability <= player.AutoRepairThreshold)
                {
                    var (ok, msg) = await _equipmentService.RepairReelAsync(player, reel.Id, fullRepair: true);
                    if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                    else autoRepairLogs.Add($"自动修复失败: {msg}");
                }
                // 鱼线自动修复
                if (line is not null && line.Durability <= player.AutoRepairThreshold)
                {
                    var (ok, msg) = await _equipmentService.RepairLineAsync(player, line.Id, fullRepair: true);
                    if (ok) autoRepairLogs.Add($"自动修复: {msg}");
                    else autoRepairLogs.Add($"自动修复失败: {msg}");
                }
            }

            // 3. 重新获取内存 Loadout 信息（金币数、耐久度等）
            await ReloadGearAsync(acquireLock: false);
        });

        // 4. 打印钓鱼终端输出日志
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
```

---

### 4. 前端界面控制与交互

在 `Components/GearShopPanel.razor` 中定义前端操作触发点，若无此工具显示售价 `3000g` 的卡片，若已有则展示开关与阈值：

```razor
<!-- Components/GearShopPanel.razor -->
@if (_category == "autorepair")
{
    <div class="auto-repair-tab-content">
        <div class="auto-repair-header">
            <h3>🔧 智能自动修复系统</h3>
            <p class="hint">// 购买自动修复工具后，在挂机或手动钓鱼时，当装备耐久度低于设定阈值时会自动扣款进行全修。</p>
        </div>
        
        @if (!Player.AutoRepairUnlocked)
        {
            <div class="auto-repair-buy-card">
                <div class="card-details">
                    <span class="tool-title">自动修复工具 (Auto-Repair Tool)</span>
                    <p class="tool-desc">高科技纳米修复液与内置检测芯片。开启后，在钓鱼期间检测到装备耐久度低于阈值时，自动全修（扣除金币与手动全修一致）。</p>
                    <span class="tool-price">售价：<strong>3000 金币</strong></span>
                </div>
                <button type="button" class="g-btn primary" disabled="@(Player.Money < 3000)" @onclick="BuyAutoRepairTool">
                    购买 (3000g)
                </button>
            </div>
        }
        else
        {
            <div class="auto-repair-settings-card">
                <div class="settings-header">
                    <span class="status-badge @(Player.AutoRepairEnabled ? "active" : "inactive")">
                        @(Player.AutoRepairEnabled ? "● 自动修复已启用" : "○ 自动修复已禁用")
                    </span>
                </div>
                <div class="settings-body">
                    <div class="setting-item">
                        <label class="switch-container">
                            <input type="checkbox" checked="@Player.AutoRepairEnabled" @onchange="ToggleAutoRepair" />
                            <span class="slider round"></span>
                            <span class="label-text">启用自动修复</span>
                        </label>
                    </div>
                    <div class="setting-item">
                        <label for="repair-threshold">自动修复触发阈值 (当前: @Player.AutoRepairThreshold% 耐久):</label>
                        <div class="slider-wrapper">
                            <input type="range" id="repair-threshold" min="5" max="50" step="5" value="@Player.AutoRepairThreshold" @onchange="ChangeThreshold" />
                            <span class="range-val">@Player.AutoRepairThreshold%</span>
                        </div>
                    </div>
                </div>
            </div>
        }
    </div>
}
```

---

## 三、 值得学习与沉淀的工程设计模式

1. **单向事件冒泡（Event Bubbling）**
   在编写复杂的 Blazor 组件树时，应尽量让子组件（如 `GearShopPanel`）作为无状态/纯展现的“木偶组件”，通过 `EventCallback` 触发事件冒泡；而核心的状态修改、业务调用和线程锁应统一交给顶层页面持有者（如 `Home.razor`）来调度。这样可以确保状态的高一致性。

2. **多线程并发安全锁模式（Concurrency Lock Pattern）**
   此游戏中挂机钓鱼和打工是运行在后台异步 Tick 线程中的，如果用户在页面上频繁点按或修改属性，极容易触发 EF Core 的 `DbContext DbConcurrencyException`。
   我们使用了本地信号量锁 `WithDbLock(async () => { ... })` 控制了所有可能对 `Player` 数据库上下文做写的操作，这是异步状态管理必不可少的安全机制。

3. **优雅的 UI 细节控制**
   在本功能切换到 `autorepair` 栏时，顶部的“搜索工具栏”和“筛选过滤区”是不符合交互逻辑的。因此使用条件渲染 `@if (_category != "autorepair")` 动态隐藏它们，避免了给用户传递歧义的操作行为。
