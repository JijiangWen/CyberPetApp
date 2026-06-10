# Blazor & C# 技术总结

本文档按技术点组织，每项附 **一段说明** 与 **本项目中的真实代码引用**。所有示例均来自 CyberPetApp 仓库，非虚构功能。

---

## 1. Blazor Server 与 InteractiveServer 渲染模式

Blazor Server 在服务端运行组件逻辑，通过 SignalR 将 UI 差异推送到浏览器。本项目在 `Program.cs` 注册交互式服务端组件，并在主游戏页 `Home.razor` 声明 `@rendermode InteractiveServer`，使按钮点击、Tab 切换、钓鱼进度条等需要即时响应的交互在服务端处理后再同步到客户端。

```99:101:Program.cs
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

```139:141:Program.cs
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

```30:31:Components/Pages/Home.razor
@rendermode InteractiveServer
@implements IAsyncDisposable
```

---

## 2. Razor 组件、@code、Parameter、EventCallback

Razor 文件混合 HTML 标记与 C# 逻辑。子组件通过 `[Parameter]` 接收父组件数据，通过 `EventCallback` 向父组件回传用户操作，实现单向数据流 + 事件上行。

`CatVitalsPanel` 接收猫咪实体与 debuff 文案，在 `@code` 中计算进度条样式：

```50:61:Components/CatVitalsPanel.razor
@code {
    [Parameter, EditorRequired] public CyberCat Cat { get; set; } = default!;
    [Parameter] public bool ShowLowStateAlert { get; set; } = true;
    [Parameter] public IEnumerable<string>? DebuffLines { get; set; }

    private static string FillStyle(int value)
    {
        double pct = Math.Clamp(value * 100.0 / CyberCat.StatMax, 0, 100);
        int hue = (int)Math.Round(pct * 1.2); // 0=红 → 120=绿
        return FormattableString.Invariant(
            $"width:{pct:0.#}%;background:linear-gradient(90deg,hsl({hue},72%,38%),hsl({hue},82%,52%))");
    }
}
```

`CatCareSidebar` 将喂食、抚摸等操作定义为 `EventCallback`，由 `Home.razor` 注入具体处理方法：

```173:181:Components/CatCareSidebar.razor
    [Parameter] public EventCallback<Food> OnFeed { get; set; }
    [Parameter] public EventCallback OnDrink { get; set; }
    [Parameter] public EventCallback OnStroke { get; set; }
    [Parameter] public EventCallback OnSleep { get; set; }
    [Parameter] public EventCallback OnTreat { get; set; }
```

---

## 3. 组件拆分

`Home.razor` 超过两千行，将 UI 按功能域拆成独立 Razor 组件，父页面只负责状态注入与编排。典型拆分：

| 组件 | 职责 |
|------|------|
| `CatCareSidebar` | 猫咪立绘、操作按钮、被动恢复摘要 |
| `CatVitalsPanel` | 四维进度条与低状态警告 |
| `GearLoadoutPanel` | 四槽装备（竿/轮/线/饵）与宝石槽 |
| `FishDexPanel` | 图鉴进度 |
| `ExpeditionPanel` | 派遣 |
| `FurnitureShopPanel` / `LifeShopPanel` | 商店 |

父页面组装示例：

```88:102:Components/Pages/Home.razor
            <CatCareSidebar Cat="cat"
                            Player="player"
                            CatState="@CatState()"
                            CatMoodText="@CatMoodText()"
                            DebuffLines="sidebarDebuffLines"
                            PassiveSummary="PassiveCareSummaryLines()"
                            ActiveFoodBuffs="activeFoodBuffs"
                            CanTreat="sidebarCanTreat"
                            Message="feedMessage"
                            HasFood="HasBackpackFood"
                            OnFeed="FeedShopFood"
                            OnDrink="ManualDrinkWater"
                            OnStroke="ManualStroke"
                            OnSleep="ManualSleep"
                            OnTreat="TreatCat" />
```

---

## 4. 依赖注入（DI）

`Program.cs` 使用 `AddScoped` 注册所有业务服务，每个 HTTP 请求（或 Blazor 电路）获得独立实例，可安全注入 `AppDbContext`。页面通过 `@inject` 声明依赖。

```51:57:Program.cs
builder.Services.AddScoped<FishingService>();
builder.Services.AddScoped<CyberCatService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<EquipmentService>();
builder.Services.AddScoped<CookingService>();
```

```6:9:Components/Pages/Home.razor
@inject FishingService FishingService
@inject CyberCatService CyberCatService
@inject AuthService AuthService
@inject PlayerService PlayerService
```

`FishingService` 构造函数展示服务间依赖链：`FishingService` → `AppDbContext` + `GearMaterialService`。

```12:20:Services/FishingService.cs
public class FishingService
{
    private readonly AppDbContext _context;
    private readonly GearMaterialService _materials;

    public FishingService(AppDbContext context, GearMaterialService materials)
    {
        _context = context;
        _materials = materials;
    }
```

---

## 5. EF Core + PostgreSQL 与 Migration

数据访问集中在 `AppDbContext`，映射玩家、猫咪、装备、鱼获、市场挂单等实体。`OnModelCreating` 配置主键、默认值、外键与导航属性忽略（如 `Player.Backpack` 运行时字典不入库）。

```19:23:Program.cs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

```12:22:Data/AppDbContext.cs
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<CyberCat> CyberCats { get; set; } = null!;
    public DbSet<BackpackItem> BackpackItems { get; set; } = null!;
    public DbSet<Fish> Fishes { get; set; } = null!;
    public DbSet<PlayerHouse> PlayerHouses { get; set; } = null!;
    public DbSet<Room> Rooms { get; set; } = null!;
    public DbSet<Furniture> Furnitures { get; set; } = null!;
    public DbSet<AutoFeeder> AutoFeeders { get; set; } = null!;
    public DbSet<FeederFood> FeederFoods { get; set; } = null!;
    public DbSet<AutoWaterer> AutoWaterers { get; set; } = null!;
    public DbSet<WatererWater> WatererWaters { get; set; } = null!;
    public DbSet<GameAccount> GameAccounts { get; set; } = null!;
```

模型变更通过 `Migrations/` 目录下的 EF 迁移管理，例如 `20260609045835_GuidIds.cs`、`20260610061206_ExpandGearAlchemyProgression.cs` 等，由 `dotnet ef migrations add` 生成、`dotnet ef database update` 应用。

---

## 6. Background Timer 与游戏 Tick

主循环使用 `System.Timers.Timer`，间隔 **2000ms**，在 `OnTimerElapsed` 中驱动：猫背景衰减、打工、喂食器/饮水器、被动家具恢复、维护费、NPC 市场报价，并触发 `StateHasChanged` 刷新 UI。

```2723:2782:Components/Pages/Home.razor
        gameTimer = new System.Timers.Timer(2000);
        gameTimer.Elapsed += OnTimerElapsed;
        gameTimer.Start();
    // ...
    private void OnTimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        var buffs = GetHouseBuffs();
        bool earnedTicket;
        int foodBefore;
        bool consumedWater;
        lock (catStateLock)
        {
            cat.Tick(buffs);
            earnedTicket = false;
            if (workingPlace.Tick(player!, cat, out earnedTicket, buffs))
            { /* 券在锁外发放 */ }
            foodBefore = feeder.FoodCount;
            feeder.CheckAndFeed(cat);
            consumedWater = waterer.CheckAndWater(cat, HasWaterDispenser());
            PassiveCatCare.Tick(cat, UnlockedFurnitureIds(), 2000, ref passiveCareAccumulatedMs);
        }
        // ... 维护费、市场报价、持久化 ...
        InvokeAsync(StateHasChanged);
    }
```

钓鱼逻辑独立于页面定时器：`FishingManager` 用 `async/await` + `CancellationToken` 自驱三阶段循环（见游戏设计文档）。

---

## 7. Minimal API 与 AuthEndpoints

登录/注册/登出不走 Blazor 表单 POST 到 Minimal API，避免在交互电路中处理 Cookie 写入的边界情况。`AuthEndpoints` 映射三个 POST 路由，成功后 `Redirect`。

```7:33:Services/AuthEndpoints.cs
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", async ([FromForm] string username, [FromForm] string password, [FromForm] string? confirmPassword, AuthService auth) =>
        {
            if (password != confirmPassword)
                return Results.Redirect("/register?error=" + Uri.EscapeDataString("两次密码不一致"));

            var result = await auth.RegisterAsync(username, password);
            return result.Success
                ? Results.Redirect("/")
                : Results.Redirect("/register?error=" + Uri.EscapeDataString(result.Error!));
        }).DisableAntiforgery();
        // ... login、logout ...
    }
```

`Program.cs` 中：`app.MapAuthEndpoints();`

---

## 8. Cookie 认证与授权

`AuthService` 使用 `PasswordHasher<GameAccount>` 哈希密码，登录成功后写入 Cookie（30 天滑动过期）。`Routes.razor` 包裹 `CascadingAuthenticationState` + `AuthorizeRouteView`，未登录用户重定向到登录页。

```31:45:Program.cs
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "CyberPet.Auth";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
```

```25:57:Services/AuthService.cs
    public async Task<AuthResult> RegisterAsync(string username, string password)
    {
        // ... 校验、创建 Player + CyberCat + GameAccount ...
        await SignInAsync(player.Id, username);
        return AuthResult.Ok();
    }
```

```1:18:Components/Routes.razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" NotFoundPage="typeof(Pages.NotFound)">
        <Found Context="routeData">
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    @if (context.User.Identity?.IsAuthenticated != true)
                    {
                        <RedirectToLogin />
                    }
```

---

## 9. CSS 隔离与 wwwroot 静态资源

Blazor 为每个 `.razor` 组件生成隔离 CSS（`Component.razor.css`），编译进 `CyberPetApp.styles.css`。全局主题与雪碧图规则放在 `wwwroot/cyberpet-theme.css`、`wwwroot/app.css`，在 `App.razor` 中引用。

```9:12:Components/App.razor
    <link rel="stylesheet" href="@Assets["app.css"]" />
    <link rel="stylesheet" href="@Assets["cyberpet-theme.css"]" />
    <link rel="stylesheet" href="@Assets["CyberPetApp.styles.css"]" />
```

`Program.cs` 使用 `app.MapStaticAssets()` 提供带指纹的静态资源。

---

## 10. 雪碧图与 CSS background-position

像素风资源不逐张请求 HTTP，而是合成雪碧图。`SpriteCatalog` 将家具 ID、鱼名映射为 CSS 类名；`cyberpet-theme.css` 用 `background-image` + `background-position` 切片显示。

```6:26:Models/SpriteCatalog.cs
    public static string Furniture(string furnitureId) => furnitureId switch
    {
        "Sofa" => "furn-sofa",
        "TV" => "furn-tv",
        "CatToy" => "furn-cattoy",
        // ...
        _ => "furn-sofa"
    };
```

```287:305:wwwroot/cyberpet-theme.css
/* ═══ Fish sprites (fish-set.png · 6 cols × 4 rows · 64px/cell) ═══ */
.fish-sprite {
    background-image: url('/assets/fish-set.png?v=4');
    /* ... */
}
.fish-01 { background-position: 0%    0%; }
.fish-02 { background-position: 20%   0%; }
```

资产由 `tools/build_assets.py` 绘制单帧 → `strip_bg.py` 去背景 → 拼合到 `wwwroot/assets/*.png`。

---

## 11. async/await 服务模式

业务逻辑封装在 Scoped 服务中，方法返回 `Task`/`Task<T>`，页面 `await` 调用。`PlayerService.LoadPlayerAsync` 聚合多表查询并填充运行时集合：

```16:44:Services/PlayerService.cs
    public async Task<Player?> LoadPlayerAsync(Guid playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player is null) return null;

        player.FishBackpack = await _context.Fishes
            .Where(f => f.PlayerId == playerId)
            .ToListAsync();

        var backpackItems = await _context.BackpackItems
            .Where(b => b.PlayerId == playerId)
            .ToListAsync();

        player.Backpack = backpackItems.ToDictionary(b => b.ItemName, b => b.Quantity);
        // ...
        return player;
    }
```

`FishingManager.RunLoopAsync` 在后台异步执行钓鱼阶段，每 250ms 更新剩余秒数并回调 `OnChanged`。

---

## 12. 线程锁与并发控制

Blazor Server 定时器回调与 UI 事件可能在不同线程触发。`Home.razor` 用 `catStateLock` 保护猫咪状态读写，用 `SemaphoreSlim dbLock` 串行化数据库写入，避免竞态。

```1492:1500:Components/Pages/Home.razor
    private readonly SemaphoreSlim dbLock = new(1, 1);
    private readonly object catStateLock = new();

    private async Task WithDbLock(Func<Task> action)
    {
        await dbLock.WaitAsync();
        try { await action(); }
        finally { dbLock.Release(); }
    }
```

`FishingManager.Log` 对 `EventLog` 列表使用 `lock`，防止并发插入：

```251:257:Services/FishingManager.cs
    private void Log(string text, string kind)
    {
        lock (EventLog)
        {
            EventLog.Insert(0, new FishingLogEntry { Text = text, Kind = kind });
            if (EventLog.Count > 10) EventLog.RemoveAt(EventLog.Count - 1);
        }
    }
```

---

## 13. C# record、pattern matching、static catalog

项目大量使用现代 C# 特性表达不可变 DTO 与静态游戏数据：

**record** — 炼金结果、猫钓鱼快照、装备配方：

```7:7:Services/AlchemyService.cs
public record AlchemyResult(bool Success, string Message, int CatLevelUps = 0);
```

```7:29:Models/CatFishingStats.cs
public record CatFishingStats(
    int CatLevel,
    int Str,
    // ...
    IReadOnlyList<string> StatusLines);
```

**pattern matching（switch 表达式）** — 活动消耗、素材来源提示、精灵类名：

```34:51:Models/CatActivityCost.cs
    public static Delta Get(CatActivityType type) => type switch
    {
        CatActivityType.FishingCycle => new(-7, -10, -3, -5, 0),
        CatActivityType.WorkTick => new(0, -2, -1, -1, 0),
        // ...
        _ => default
    };
```

```42:58:Models/GearProgressionCatalog.cs
    public static string SourceHint(string name) => name switch
    {
        BambooStrip => "静溪普通鱼分解",
        CarbonFiber => "雾海稀有+鱼分解/副产",
        // ...
        _ => "?"
    };
```

**static catalog** — `GearProgressionCatalog`、`FishingSpotCatalog`、`AlchemyRecipes` 等以静态只读列表/字典集中配置数值，便于策划调参而不改逻辑代码。

**init 属性** — `FishingLogEntry`、`AuthResult` 等：

```14:20:Services/FishingManager.cs
public class FishingLogEntry
{
    public DateTime Time { get; init; } = DateTime.Now;
    public string Text { get; init; } = "";
    public string Kind { get; init; } = "info";
}
```

---

## 14. 职责分离：FishingManager vs FishingService

- **`FishingManager`**（页面级实例）：内存状态机、随机判定、阶段计时，不直接访问数据库。
- **`FishingService`**：EF Core 持久化鱼获、直售结算。

钓到鱼后 `Home.razor` 在 `OnFishCaught` 回调中调用 `FishingService.SaveCaughtFishAsync`，清晰分离「玩法模拟」与「存档」。

---

## 15. 资产构建脚本

`tools/build_assets.py` 调用 `draw_sprites` 绘制 64×64 像素图，经 `strip_bg.py` 洪水填充去除灰底，再合成 `furniture-set.png`、`fish-set.png` 等雪碧图到 `wwwroot/assets/`。

```13:19:tools/build_assets.py
Pipeline per sprite:
  1. Draw on solid BG (#b8b8b8) — 64×64, 4–6px safe margin, visually centered
  2. python tools/strip_bg.py <file>  (flood-fill transparent)
  3. Composite into wwwroot/assets/*.png
```
