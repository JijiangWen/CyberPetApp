# 第 4 章：数据持久化与 EF Core 核心设计 💾

### 4.1 什么是 EF Core
**Entity Framework Core (EF Core)** 是微软官方提供的现代 **ORM (对象关系映射)** 框架。它将数据库中的**数据表**映射为 C# 中的**类**，将数据表中的**记录（行）**映射为 C# 中的**对象**。

在 **CyberPetApp** 中，我们直接使用 C# 来编写数据库实体的模型，完全不需要手写繁琐、易出错的原始 SQL 查询语句，所有数据库操作都可以通过面向对象的方式优雅完成。

---

### 4.2 AppDbContext 配置

[AppDbContext.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Data/AppDbContext.cs) 是连接应用程序和关系型数据库的核心网关，继承自 `DbContext`。它不仅声明了哪些类需要映射为数据库表，还负责定义表与表之间的关联关系。

```csharp
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Data;

public class AppDbContext : DbContext
{
    // 声明数据库表对应的实体集 (DbSet)
    public DbSet<Player> Players { get; set; } = null!;
    public DbSet<CyberCat> CyberCats { get; set; } = null!;
    public DbSet<BackpackItem> BackpackItems { get; set; } = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // 重写 OnModelCreating 用于手动配置高级表映射关系
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置联合主键 (Composite Key)：背包道具表由 PlayerId 和 ItemName 联合确定唯一性
        modelBuilder.Entity<BackpackItem>()
            .HasKey(b => new { b.PlayerId, b.ItemName });
            
        // 配置外键约束与级联删除：当玩家账号被删除时，其关联的猫咪和背包物品自动一并删除
        modelBuilder.Entity<CyberCat>()
            .HasOne<Player>()
            .WithOne()
            .HasForeignKey<CyberCat>(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### 💡 特殊映射处理：`[NotMapped]` 特性
有些数据结构在 C# 内存中读写极为方便（例如字典 `Dictionary`），但是关系型数据库并不支持直接存储这种结构。
或者有些变量仅作为临时缓冲存放在内存中，不需要持久化进硬盘。

我们可以在这类属性上方打上 `[NotMapped]` 标记，通知 EF Core 引擎：**“忽略此属性，不要在数据库表中为它创建字段。”**

例如，在 `Player` 类中：
```csharp
using System.ComponentModel.DataAnnotations.Schema;

public class Player
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    
    // 数据库中不存此字段，我们仅在服务层加载数据时，将副表 BackpackItems 
    // 的扁平数据提取并转换成这个 Dictionary 缓存，方便页面快速查询。
    [NotMapped]
    public Dictionary<string, int> Backpack { get; set; } = new();
}
```

---

### 4.3 数据库迁移 (Migrations) 与数据库更新
关系数据库的结构（Schema）是死板的，而 C# 代码的模型在开发阶段是频繁变化的（例如给装备表加一个“是否炼金锻造”的布尔值）。
为了将 C# Model 的修改应用到数据库中，我们必须使用 EF Core 的**迁移工具（Migrations）**。

#### 实战迁移步骤：
打开终端，确保定位到项目根目录下，依次执行：

1. **创建迁移脚本**：
   ```bash
   dotnet ef migrations add AddNewGearStats
   ```
   *这会在项目的 `Migrations` 文件夹下生成两个 C# 文件，记录了“从上一个版本变化到最新版本”需要执行的 SQL 更改（如 `AddColumn` 等）。*

2. **将迁移应用到真实的数据库上**：
   ```bash
   dotnet ef database update
   ```
   *这会自动连接数据库，生成对应的 SQL 并执行，更改数据表结构，且不会丢失已有数据。*

---

### 4.4 异步数据操作与 async/await 原理
数据库读写是一项需要穿过网络和读取磁盘的慢速 I/O 操作。如果采用同步调用（例如 `_context.Players.Find()`），当前的线程在等待数据库回复的几毫秒甚至几十毫秒内，会被完全阻塞挂起，无法处理任何其他操作。在 Web 服务中，这会导致系统并发吞吐量剧烈下滑，页面出现卡顿。

C# 通过 `async`（声明异步方法）和 `await`（等待异步任务完成）提供了一套极其优秀的异步编程范式。

#### 异步流程深度解析：
分析 [PlayerService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/PlayerService.cs) 中的数据加载逻辑：

```csharp
public async Task<Player?> LoadPlayerAsync(Guid playerId)
{
    // 1. FindAsync 异步查询主表
    // 线程执行到 await 时，会将控制权归还给服务器线程池，当前线程去处理其他用户的点击事件了。
    // 当 PostgreSQL 数据库把结果数据返回时，线程池会随便调度一个空闲线程，恢复执行本方法后面的代码。
    var player = await _context.Players.FindAsync(playerId);
    
    if (player is null) return null;

    // 2. 异步查询鱼包数据并生成列表
    player.FishBackpack = await _context.Fishes
        .Where(f => f.PlayerId == playerId)
        .ToListAsync();

    // 3. 异步查询背包道具表
    var backpackItems = await _context.BackpackItems
        .Where(b => b.PlayerId == playerId)
        .ToListAsync();

    // 4. 将扁平列表转换为方便业务逻辑查询的 Dict 缓存
    player.Backpack = backpackItems.ToDictionary(b => b.ItemName, b => b.Quantity);
    
    return player;
}
```

#### ✍️ 异步开发铁律：
1. **一路异步到底**：只要方法内调用了带有 `Async` 后缀的 EF Core 方法，当前方法就必须声明为 `async Task` 或 `async Task<T>`。
2. **切忌使用 `.Result` 或 `.Wait()`**：如果在异步流中强行调用同步等待，会导致严重的**线程死锁（Deadlock）**，直接引起应用程序卡死！
