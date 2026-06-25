# 第 1 章：Blazor Server 基础与生命周期 🚀

### 1.1 什么是 Blazor Server？
传统的 Web 开发模式通常是**前后端分离**：前端使用 Vue、React 或 Angular 编写，运行在用户浏览器中；后端使用 Java、Go 或 C# 提供 API 接口。前端通过异步网络请求（Ajax/Fetch）与后端通信并获取 JSON 数据。

而在 **Blazor Server** 架构中：
* 所有的 C# 代码和组件逻辑都在**服务端**运行。
* 浏览器端与服务器之间建立了一条持久的 **SignalR (WebSocket)** 双向通信通道。
* 当用户在页面上点击按钮、输入文本或触发事件时，这些事件会立即通过 WebSocket 发送到服务端，服务端在内存中重新计算组件的 UI，并将计算得到的 **HTML 差异（DOM Diff）** 推送回浏览器，浏览器前端运行时（轻量级 JS 脚本）只做局部更新渲染。

```
┌─────────────────┐  User Click   ┌──────────────────┐
│ 浏览器 (Client)  ├─────────────>│ Blazor Server    │
│                 │  (WebSocket) │                  │
│ [局部 DOM 更新]  │<─────────────┤ [计算 DOM Diff]  │
└─────────────────┘  DOM Diff     └──────────────────┘
```

> [!NOTE]
> **Blazor Server 的优势**：你不需要编写多余的 Web API 控制器（Controller）来做前后端数据同步，也不需要处理复杂的 JS 状态管理（如 Pinia/Redux）。直接在 C# 中查询数据库并修改变量，UI 就会自动响应式更新！

---

### 1.2 深入入口文件 Program.cs
[Program.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Program.cs) 是整个 ASP.NET Core 应用程序的起点，负责**容器服务注册**和**中间件管道配置**。

下面是 CyberPetApp 中最核心的入口配置解析：

```csharp
using CyberPetApp.Components;
using CyberPetApp.Data;
using CyberPetApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册 Razor 组件服务与交互式 Server 组件支持
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. 注册数据库上下文 (EF Core) 并使用 PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 3. 注册自定义的业务逻辑服务 (DI 容器)
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<CyberCatService>();
builder.Services.AddScoped<FishingService>();

// 4. 构建应用实例
var app = builder.Build();

// 5. 配置 HTTP 请求管道 (中间件)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication(); // 启用认证（判断是谁在访问）
app.UseAuthorization();  // 启用授权（判断这个人能做什么）
app.UseAntiforgery();    // 启用防伪标记（防止跨站请求伪造）

// 6. 映射路由端点
app.MapStaticAssets();   // 映射静态资源
app.MapAuthEndpoints();  // 映射自定义登录/注册 API 端点（用于写入 Cookie）

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // 全局开启 InteractiveServer 交互渲染模式

app.Run();
```

---

### 1.3 什么是 `@rendermode InteractiveServer`
在 Blazor 中，默认渲染模式是**静态服务器渲染（Static SSR）**。在这种模式下，组件仅在服务器端渲染一次 HTML 字符串并发送给浏览器，之后任何按钮点击或变量修改都不会引起 UI 改变（因为 WebSocket 通道并没有建立）。

为了实现猫咪状态的实时倒计时、钓鱼进度条的动画、或者商店侧边栏的交互，我们在页面或组件的顶部必须添加渲染模式指令：
```razor
@rendermode InteractiveServer
```
这会告诉 Blazor 运行时：“该组件需要启用 WebSocket 双向连接，支持动态交互与部分页面重绘。”

---

### 1.4 组件生命周期详解
Blazor 组件生命周期包含一系列随着组件加载、更新、销毁而自动触发的方法。在 CyberPetApp 中，我们的主页面逻辑分散在多个 partial 类中，其中 [Home.GameLoop.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/Pages/Home.GameLoop.cs) 完美展示了生命周期函数的用法：

```
     ┌──────────────────────────┐
     │       组件被创建         │
     └────────────┬─────────────┘
                  ▼
     ┌──────────────────────────┐
     │   OnInitializedAsync()   │ <── 异步执行初始化逻辑（加载玩家数据、启动游戏 Timer）
     └────────────┬─────────────┘
                  ▼
     ┌──────────────────────────┐
     │   OnParametersSetAsync() │ <── 父组件传入的属性更新时触发
     └────────────┬─────────────┘
                  │ 🔄 发生交互 / Timer 触发
                  ▼
     ┌──────────────────────────┐
     │     StateHasChanged()    │ <── 强制触发 UI 重绘
     └────────────┬─────────────┘
                  │ 🚪 用户离开页面 / 销毁
                  ▼
     ┌──────────────────────────┐
     │      DisposeAsync()      │ <── 资源释放（注销 Timer 事件、保存最终进度）
     └──────────────────────────┘
```

#### 1. 初始化阶段 `OnInitializedAsync`
当组件完成实例化并准备好渲染时触发。在此处我们加载玩家存档，计算离线补偿，并开启主心跳定时器：
```csharp
protected override async Task OnInitializedAsync()
{
    // 1. 加载当前登录玩家的存档数据
    player = await _playerService.LoadPlayerAsync(playerId);
    
    // 2. 加载玩家底层的赛博猫咪属性
    cat = await _catService.GetCatByPlayerIdAsync(playerId);

    // 3. 计算并应用玩家下线期间的离线收益与数值衰减
    var offlineResult = await _offlineCompensationService.ApplyAsync(player, cat);

    // 4. 实例化并启动主游戏循环定时器 (2秒一次 Tick)
    gameTimer = new System.Timers.Timer(GameTickOrchestrator.TickIntervalMs);
    gameTimer.Elapsed += OnTimerElapsed;
    gameTimer.Start();
}
```

#### 2. 交互与渲染阶段 `StateHasChanged`
在普通的按钮事件处理器中，Blazor 会在事件处理完成后自动调用 `StateHasChanged()` 刷新 UI。但是，如果是被后台多线程或者外部 Timer 触发的回调（例如每 2 秒的心跳 Tick），我们必须显式地调用 `InvokeAsync(StateHasChanged)` 来通知 UI 线程进行局部重绘：
```csharp
private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
{
    // 执行后台数值计算逻辑 (在非 UI 线程运行)
    TickResult result = RunGameTick();

    if (result.IsDirty)
    {
        // 跨线程调度：回到 UI 线程执行页面重绘
        InvokeAsync(StateHasChanged);
    }
}
```

#### 3. 销毁阶段 `DisposeAsync`
当用户主动注销、切换到其他页面或者关闭浏览器导致 Circuit 断开时触发。我们**必须**在此处关闭定时器并注销事件，否则会导致内存中驻留大量游离的定时器，造成严重的**内存泄漏**：
```csharp
public async ValueTask DisposeAsync()
{
    if (gameTimer != null)
    {
        // 1. 停止定时器
        gameTimer.Stop();
        // 2. 注销心跳事件，解除强引用
        gameTimer.Elapsed -= OnTimerElapsed;
        // 3. 释放定时器资源
        gameTimer.Dispose();
    }
}
```
