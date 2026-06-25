# 实操教学书：如何从零开发一个游戏新功能 🚀

在挂机养成游戏 **CyberPetApp** 中，如果你想要增加一个全新的游戏功能（例如：**猫咪玩具与亲密度系统**），你需要遵循规范的开发流程。Blazor Server 应用的开发是一个**“纵向贯穿”**的过程——从最底层的数据库设计，到中间的业务逻辑服务，再到最顶层的 Razor 前端组件呈现。

本指南将以开发 **“猫咪玩具与亲密度系统 (Cat Toy & Intimacy System)”** 为例，手把手教你如何从头到尾完成一个新功能的完整编写，提供全部思路、架构设计以及每一行代码的详细解读。

---

## 🗺️ 新功能设计方案：猫咪玩具与亲密度系统

### 1. 玩法设定与规则
* **亲密度 (Closeness)**：猫咪新增一个属性值（范围 `0 ~ 1000`）。亲密度越高，未来钓鱼时获得的收益加成或经验加成就越高。
* **猫咪玩具 (Cat Toy)**：玩家可以从商店购买玩具。玩具具有**耐用度 (Durability)**，每次陪猫咪玩耍会消耗玩具耐用度，并消耗猫咪的**精力值 (Energy)**，但会大幅提升猫咪的**亲密度**。
* **玩具种类**：
  1. **逗猫棒 (Feather Wand)**：价格 100 金币，每次玩耍消耗 10 精力，增加 15 亲密度，消耗 10 耐用度。
  2. **发条小鼠 (Wind-up Mouse)**：价格 250 金币，每次玩耍消耗 20 精力，增加 35 亲密度，消耗 15 耐用度。
  3. **豪华猫爬架 (Cat Tree)**：价格 800 金币，每次玩耍消耗 40 精力，增加 90 亲密度，消耗 5 耐用度。

### 2. 三层架构开发流向
我们将按照**自底向上**的逻辑来实现：
```
┌──────────────────────────────────────────────────────────┐
│ 1. 数据库建模 (Model)   │ 创建 CatToy 实体，在 AppDbContext 注册映射关系  │
├─────────────────────────┼────────────────────────────────────────┤
│ 2. 数据库迁移 (Migration)│ 执行 dotnet ef 命令，生成并更新数据库表结构    │
├─────────────────────────┼────────────────────────────────────────┤
│ 3. 业务服务 (Service)   │ 编写 CatToyService 处理购买和玩耍的事务与锁逻辑  │
├─────────────────────────┼────────────────────────────────────────┤
│ 4. 前端组件 (Component) │ 编写 CatToyPanel.razor 渲染商店和玩具背包交互   │
├─────────────────────────┼────────────────────────────────────────┤
│ 5. 主页集成 (Integration)│ 在 Home.razor 引入组件，实现数据刷新与心跳交互  │
└──────────────────────────────────────────────────────────┘
```

---

## 🧱 第一步：底层数据建模与数据库配置

开发任何新功能，第一步永远是**“存数据”**。我们需要确定哪些数据需要持久化写入数据库。

### 1. 修改已有的猫咪实体 `CyberCat.cs`
我们需要在猫咪模型中添加一个“亲密度”字段。

打开 [CyberCat.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/CyberCat.cs)，在类中添加 `Closeness` 属性：

```csharp
// Models/CyberCat.cs
// 增加亲密度属性，默认初始值为 0
public int Closeness { get; set; } = 0;
```

### 2. 创建全新的玩具实体 `CatToy.cs`
在 `Models` 文件夹下新建一个文件 `CatToy.cs`，代表玩家拥有的玩具。

> [!NOTE]
> 这里的每一个属性都将对应数据库表里的一列。

```csharp
// Models/CatToy.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace CyberPetApp.Models;

/// <summary>
/// 玩具类型的枚举定义（用不同的代号区分玩具种类）
/// </summary>
public enum ToyType
{
    FeatherWand = 0, // 逗猫棒
    WindUpMouse = 1, // 发条小鼠
    CatTree = 2      // 豪华猫爬架
}

/// <summary>
/// 猫咪玩具的数据模型。
/// 它对应数据库中的 "CatToys" 数据表。
/// </summary>
public class CatToy
{
    [Key] // 声明这是一个主键（唯一身份证号）
    public Guid Id { get; set; } = Guid.NewGuid();

    // 拥有这个玩具的玩家的 ID（外键，指向 Players 表）
    public Guid PlayerId { get; set; }

    // 玩具的种类（对应上面的枚举）
    public ToyType Type { get; set; }

    // 玩具的名字，例如“豪华猫爬架”
    public string Name { get; set; } = string.Empty;

    // 当前耐用度（100 代表全新，0 代表已损坏）
    public int Durability { get; set; } = 100;

    // 最大耐用度，默认 100
    public int MaxDurability { get; set; } = 100;

    // 购买时消耗的金币数
    public int Price { get; set; }

    // 购买该玩具的时间记录
    public DateTime PurchasedTime { get; set; } = DateTime.UtcNow;
}
```

### 3. 配置数据库上下文 `AppDbContext.cs`
我们需要让 Entity Framework Core 知道这个新模型，并将它映射成数据库表。

打开 [AppDbContext.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Data/AppDbContext.cs)，进行以下修改：

#### (1) 注册 `DbSet` 数据表集合
在类体顶部的属性声明区域添加：
```csharp
// Data/AppDbContext.cs
public DbSet<CatToy> CatToys { get; set; } = null!;
```

#### (2) 在 `OnModelCreating` 中配置表关系与默认值
在 `OnModelCreating` 方法底部，添加针对 `CatToy` 的流畅配置（Fluent API），并为 `CyberCat` 表配置 `Closeness` 字段的默认值：

```csharp
// Data/AppDbContext.cs -> OnModelCreating 方法内部

// 1. 给已有的 CyberCat 表的 Closeness 属性配置数据库默认值
modelBuilder.Entity<CyberCat>(entity =>
{
    // ... 原有的配置保持不变 ...
    entity.Property(c => c.Closeness).HasDefaultValue(0); // 默认亲密度为 0
});

// 2. 配置全新的 CatToy 表
modelBuilder.Entity<CatToy>(entity =>
{
    entity.HasKey(t => t.Id); // 设置主键为 Id

    // 建立外键关系：一个玩家(Player)可以拥有多个玩具(CatToy)
    // 当玩家账号被删除时，级联删除(DeleteBehavior.Cascade)所有属于他的玩具，防止垃圾数据残留。
    entity.HasOne<Player>()
          .WithMany()
          .HasForeignKey(t => t.PlayerId)
          .OnDelete(DeleteBehavior.Cascade);

    // 字段属性细节配置
    entity.Property(t => t.Name).HasMaxLength(100).IsRequired(); // 玩具名字限长100且不能为空
    entity.Property(t => t.Durability).HasDefaultValue(100);
    entity.Property(t => t.MaxDurability).HasDefaultValue(100);
});
```

---

## 💾 第二步：数据库迁移 (EF Core Migrations)

由于我们修改了 C# 的数据模型类，现在必须让底层的 PostgreSQL 数据库同步更新表结构。我们使用 .NET Core 的命令行工具来做这件事。

> [!IMPORTANT]
> 数据库迁移分两步：**第一步生成迁移代码**，**第二步将代码应用到数据库**。

### 1. 运行迁移命令
打开你的终端（比如 PowerShell），定位到项目根目录 `c:\Users\wen.jijiang\Desktop\blazor_test\CyberPetApp`，执行以下命令：

```powershell
# 1. 创建一个新的迁移版本，命名为 AddCatToysAndCloseness
dotnet ef migrations add AddCatToysAndCloseness

# 2. 将迁移应用到数据库中，使其在数据库内新建 CatToys 表并为 CyberCats 表新增 Closeness 列
dotnet ef database update
```

* **执行原理**：`dotnet ef` 会自动比对 C# 模型与数据库当前状态的差异，自动在项目的 `Migrations` 文件夹下生成一个新的 `.cs` 脚本，然后将其翻译成 `CREATE TABLE` 和 `ALTER TABLE` 的 SQL 语句，发往数据库执行。

---

## 💉 第三步：编写业务逻辑服务 (Service Layer)

业务层是整个游戏的“大脑”，所有的数值计算、金币扣减、精力验证、防作弊检查和数据库存盘，都写在这一层。

在 `Services` 文件夹下，新建 `CatToyService.cs` 文件：

```csharp
// Services/CatToyService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>
/// 猫咪玩具与亲密度逻辑服务
/// </summary>
public class CatToyService
{
    private readonly AppDbContext _context;

    // 构造函数：注入数据库上下文，使我们能增删改查玩具和猫咪数据
    public CatToyService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 加载某个玩家拥有的所有玩具（排除耐用度归零报废的）
    /// </summary>
    public async Task<List<CatToy>> LoadPlayerToysAsync(Guid playerId)
    {
        return await _context.CatToys
            .Where(t => t.PlayerId == playerId && t.Durability > 0)
            .OrderByDescending(t => t.PurchasedTime)
            .ToListAsync();
    }

    /// <summary>
    /// 购买新玩具的业务逻辑
    /// </summary>
    /// <param name="player">当前买玩具的玩家实体（在 EF 跟踪中）</param>
    /// <param name="type">要购买的玩具种类</param>
    public async Task<CatToy> BuyToyAsync(Player player, ToyType type)
    {
        // 1. 根据玩具类型，定义购买的价格 and 数据配置
        int price = type switch
        {
            ToyType.FeatherWand => 100,
            ToyType.WindUpMouse => 250,
            ToyType.CatTree => 800,
            _ => throw new ArgumentException("未知的玩具类型")
        };

        string name = type switch
        {
            ToyType.FeatherWand => "羽毛逗猫棒",
            ToyType.WindUpMouse => "发条机械鼠",
            ToyType.CatTree => "多层豪华猫爬架",
            _ => ""
        };

        // 2. 防作弊检查：检查玩家钱包里金币是否足够
        if (player.Gold < price)
        {
            throw new InvalidOperationException("金币不足，无法购买该玩具！");
        }

        // 3. 扣钱
        player.Gold -= price;

        // 4. 创建玩具并存入数据库
        var newToy = new CatToy
        {
            PlayerId = player.Id,
            Type = type,
            Name = name,
            Price = price,
            Durability = 100,
            MaxDurability = 100,
            PurchasedTime = DateTime.UtcNow
        };

        _context.CatToys.Add(newToy);

        // 5. 保存数据库（EF 会自动在一个事务里提交金币减少和新增玩具的操作）
        await _context.SaveChangesAsync();

        return newToy;
    }

    /// <summary>
    /// 陪猫咪玩玩具的业务逻辑（核心交互）
    /// </summary>
    /// <param name="cat">被陪玩的猫咪实体（EF 跟踪中）</param>
    /// <param name="toy">所使用的玩具对象（EF 跟踪中）</param>
    /// <returns>返回本次互动增加的亲密度数值</returns>
    public async Task<int> PlayWithCatAsync(CyberCat cat, CatToy toy)
    {
        // 1. 安全验证：检查玩具耐用度是否已经彻底报废
        if (toy.Durability <= 0)
        {
            throw new InvalidOperationException("该玩具已坏，无法使用！");
        }

        // 2. 根据玩具定义各自的数值消耗与加成
        int energyCost = toy.Type switch
        {
            ToyType.FeatherWand => 10,
            ToyType.WindUpMouse => 20,
            ToyType.CatTree => 40,
            _ => 10
        };

        int closenessGain = toy.Type switch
        {
            ToyType.FeatherWand => 15,
            ToyType.WindUpMouse => 35,
            ToyType.CatTree => 90,
            _ => 15
        };

        int durabilityCost = toy.Type switch
        {
            ToyType.FeatherWand => 10,
            ToyType.WindUpMouse => 15,
            ToyType.CatTree => 5,
            _ => 10
        };

        // 3. 防作弊检查：猫咪当前的精力是否足够运动？
        if (cat.Energy < energyCost)
        {
            throw new InvalidOperationException("猫咪太累了，先让它睡一觉吧！");
        }

        // 4. 属性更新
        cat.Energy -= energyCost; // 扣减猫咪精力
        cat.Closeness = Math.Min(CyberCat.StatMax, cat.Closeness + closenessGain); // 增加亲密度（封顶 1000）
        toy.Durability = Math.Max(0, toy.Durability - durabilityCost); // 扣减玩具耐久度

        // 5. 假如玩具用坏了，把它从数据库中移除（或者留在库里但耐久为 0）
        if (toy.Durability <= 0)
        {
            _context.CatToys.Remove(toy);
        }

        // 6. 保存并同步到数据库
        await _context.SaveChangesAsync();

        return closenessGain;
    }
}
```

### 🎯 服务注册 (Dependency Injection)
我们需要把这个新写好的服务加入到 ASP.NET Core 的依赖注入容器中，否则 Blazor 页面无法 `@inject` 它。

打开入口文件 [Program.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Program.cs)，在服务注册的区域添加注册代码：

```csharp
// Program.cs
// 注册猫咪玩具服务（使用 Scoped 生命周期，确保每个浏览器 Circuit 独立）
builder.Services.AddScoped<CatToyService>();
```

---

## 🎨 第四步：开发前端交互面板组件 (Blazor UI Component)

接下来，我们要用 HTML/CSS 和 Blazor 独有的语法开发前端界面。我们在 `Components` 文件夹下新建一个组件 `CatToyPanel.razor`。

这个组件将负责：
1. 渲染猫咪当前的亲密度属性值（显示炫酷的进度条）。
2. 显示可购买玩具的“玩具店”。
3. 渲染玩家当前拥有的“玩具背包”，并可以点击玩具与猫咪互动。

```razor
@* Components/CatToyPanel.razor *@
@using CyberPetApp.Models
@using CyberPetApp.Services
@inject CatToyService ToyService

<div class="toy-system-panel">
    <!-- 1. 顶层状态区：猫咪亲密度状态栏 -->
    <div class="closeness-header">
        <h3>❤️ 与猫咪的亲密度</h3>
        <div class="progress-bar-container">
            <!-- 动态宽度进度条 -->
            <div class="progress-fill" style="width: @(GetClosenessPercent())%"></div>
            <span class="progress-text">@Cat.Closeness / @CyberCat.StatMax (@GetClosenessPercent()%)</span>
        </div>
        <p class="desc">多多与猫咪互动，当亲密度满级时可以解锁神秘称号！</p>
    </div>

    <!-- 2. 操作提示与报错条 -->
    @if (!string.IsNullOrEmpty(errorMessage))
    {
        <div class="alert alert-danger">⚠️ @errorMessage</div>
    }
    @if (!string.IsNullOrEmpty(successMessage))
    {
        <div class="alert alert-success">✨ @successMessage</div>
    }

    <div class="toy-sections">
        <!-- 3. 玩具商店板块 -->
        <div class="toy-shop-section">
            <h4>🛒 玩具小店</h4>
            <div class="shop-list">
                <div class="shop-item">
                    <span class="toy-icon">🪶</span>
                    <div class="info">
                        <strong>羽毛逗猫棒</strong>
                        <span>消耗 10 金币 | ⚡ 猫精力 -10 | ❤️ 亲密 +15</span>
                    </div>
                    <button class="btn btn-buy" @onclick="() => BuyToy(ToyType.FeatherWand)" disabled="@(Player.Gold < 100)">
                        100 金币
                    </button>
                </div>

                <div class="shop-item">
                    <span class="toy-icon">🐭</span>
                    <div class="info">
                        <strong>发条机械鼠</strong>
                        <span>消耗 250 金币 | ⚡ 猫精力 -20 | ❤️ 亲密 +35</span>
                    </div>
                    <button class="btn btn-buy" @onclick="() => BuyToy(ToyType.WindUpMouse)" disabled="@(Player.Gold < 250)">
                        250 金币
                    </button>
                </div>

                <div class="shop-item">
                    <span class="toy-icon">🏰</span>
                    <div class="info">
                        <strong>豪华猫爬架</strong>
                        <span>消耗 800 金币 | ⚡ 猫精力 -40 | ❤️ 亲密 +90</span>
                    </div>
                    <button class="btn btn-buy" @onclick="() => BuyToy(ToyType.CatTree)" disabled="@(Player.Gold < 800)">
                        800 金币
                    </button>
                </div>
            </div>
        </div>

        <!-- 4. 玩具背包板块 -->
        <div class="toy-inventory-section">
            <h4>🎒 玩具箱 (@ownedToys.Count)</h4>
            @if (ownedToys.Count == 0)
            {
                <div class="empty-hint">玩具箱空空如也，快去商店买几个吧！</div>
            }
            else
            {
                <div class="toy-grid">
                    @foreach (var toy in ownedToys)
                    {
                        <div class="toy-card">
                            <span class="card-icon">@(GetToyIcon(toy.Type))</span>
                            <h5>@toy.Name</h5>
                            <div class="durability-bar">
                                <div class="durability-fill" style="width: @(toy.Durability)%"></div>
                                <span class="d-text">耐久: @toy.Durability/@toy.MaxDurability</span>
                            </div>
                            <button class="btn btn-play" @onclick="() => PlayWithToy(toy)" disabled="@(Cat.Energy < GetEnergyCost(toy.Type))">
                                🎮 互动 (⚡ @GetEnergyCost(toy.Type))
                            </button>
                        </div>
                    }
                </div>
            }
        </div>
    </div>
</div>

@code {
    // 接收父页面传递进来的玩家实体与猫咪实体
    [Parameter] public Player Player { get; set; } = null!;
    [Parameter] public CyberCat Cat { get; set; } = null!;

    // 自定义的回调事件：当数据发生改变时，通知父页面刷新 UI 并执行 SaveProgress 保存
    [Parameter] public EventCallback OnStateUpdated { get; set; }

    private List<CatToy> ownedToys = new();
    private string errorMessage = "";
    private string successMessage = "";

    // 页面加载时自动拉取玩具箱
    protected override async Task OnInitializedAsync()
    {
        await RefreshToys();
    }

    private async Task RefreshToys()
    {
        ownedToys = await ToyService.LoadPlayerToysAsync(Player.Id);
    }

    private double GetClosenessPercent()
    {
        return Math.Round((double)Cat.Closeness / CyberCat.StatMax * 100, 1);
    }

    private string GetToyIcon(ToyType type) => type switch
    {
        ToyType.FeatherWand => "🪶",
        ToyType.WindUpMouse => "🐭",
        ToyType.CatTree => "🏰",
        _ => "🧸"
    };

    private int GetEnergyCost(ToyType type) => type switch
    {
        ToyType.FeatherWand => 10,
        ToyType.WindUpMouse => 20,
        ToyType.CatTree => 40,
        _ => 10
    };

    // 触发购买玩具
    private async Task BuyToy(ToyType type)
    {
        errorMessage = "";
        successMessage = "";
        try
        {
            var newToy = await ToyService.BuyToyAsync(Player, type);
            successMessage = $"成功购买了 {newToy.Name}！";
            await RefreshToys(); // 重新拉取背包
            await OnStateUpdated.InvokeAsync(); // 通知主界面（扣除金币、刷新UI数据）
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    // 触发玩耍逻辑
    private async Task PlayWithToy(CatToy toy)
    {
        errorMessage = "";
        successMessage = "";
        try
        {
            int gain = await ToyService.PlayWithCatAsync(Cat, toy);
            successMessage = $"你用 {toy.Name} 逗了猫咪！亲密度 +{gain}，猫咪感到非常快乐！";
            await RefreshToys(); // 耐久降低了或玩具报废了，重新整理玩具背包
            await OnStateUpdated.InvokeAsync(); // 通知主页面（扣除精力、增亲密、存档）
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }
}
```

---

## 🏠 第五步：将新功能集成到挂机主页面

最后，我们需要在游戏主页 [Home.razor](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/Pages/Home.razor) 里呈现这个新开发的面板。

### 1. 修改 `Home.razor` 引入组件
打开 `Home.razor`，找到用于放置挂机面板（例如钓鱼、打工、烹饪）的 Tab 内容切换区，加入我们的新 Tab 标签以及组件容器。

#### (1) 添加 Tab 菜单按钮：
```html
<button class="tab-btn @(currentTab == "toy" ? "active" : "")" @onclick='() => currentTab = "toy"'>
    🧸 猫咪玩具
</button>
```

#### (2) 添加 Tab 内容主体：
```razor
@if (currentTab == "toy")
{
    <!-- 引入刚刚写好的组件，把内存中的 player 和 cat 传给它，并绑定更新回调 -->
    <CatToyPanel Player="player" Cat="cat" OnStateUpdated="OnGameProgressUpdated" />
}
```

### 2. 在 `Home.razor.cs` / `Home.GameLoop.cs` 中刷新状态
因为 `OnGameProgressUpdated` 是我们在主页面里定义的用于“玩家金币扣减、精力扣减存盘”的统一事件。
当组件触发 `OnStateUpdated.InvokeAsync()` 时，主页面就会执行相应的落库操作。

由于我们在组件里直接传了处于上下文生命周期跟踪状态的 `player` 和 `cat`，主页面只需要调用：
```csharp
private async Task OnGameProgressUpdated()
{
    // 利用 dbLock 线程安全锁，调用 PersistenceService 将当前改动的 player 与 cat 实体写入数据库存盘
    await WithDbLock(async () =>
    {
        await _playerService.SaveProgressAsync(player);
        await _cyberCatService.SaveAsync(cat, saveChanges: true);
    });
    
    // 强制 Blazor 重新执行渲染树计算，在玩家浏览器上刷新最新的金币、精力数值
    StateHasChanged();
}
```

---

## 🏆 核心知识点与避坑总结

在挂机养成和钓鱼这类高频交互游戏中，编写新功能时请谨记以下设计原则：

### 1. 线程同步锁（重中之重 🔒）
* **原理**：Blazor Server 所有的事件（如按钮点击、挂机心跳 Tick 等）都是**多线程并发触发**的。如果不用锁，就极易出现“主心跳正在扣猫饱食度，玩家点击购买玩具扣金币，两个操作同时访问一个 DbContext”从而引发 `InvalidOperationException: DbContext has concurrent execution` 的经典报错。
* **做法**：在 Razor 页面中使用 **`WithDbLock(...)`** 串行化所有会更改状态、操作数据库的按钮事件。

### 2. 精力与金币的双重验证（防刷 🛡️）
* **前端置灰是不够的**：前端的按钮虽然有 `disabled="@(Player.Gold < 100)"`，但黑客或高玩可以通过网络伪造 SignalR 封包强行点击。
* **后端双保险**：必须像 `BuyToyAsync` and `PlayWithCatAsync` 里那样，在 Service 层写死 `if (player.Gold < price) throw ...` 判定，将恶意刷币截断在数据库之前。

### 3. 外键级联删除 (Cascade) 
* 在 `AppDbContext.cs` 中显式定义 `OnDelete(DeleteBehavior.Cascade)`。当玩家删号时，自动连带清空其下的所有子玩具表。这能保持你的生产数据库在运行数年后依然高度整洁，免于坏账数据干扰。

---

祝你开发顺利！现在你可以按照这套规范，着手为 **CyberPetApp** 编写你自己的专属功能了！如有任何疑问，欢迎在下方输入并与我一同探讨！
