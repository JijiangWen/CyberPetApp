# 第 3 章：依赖注入 (Dependency Injection) 原理 💉

### 3.1 什么是依赖注入 (DI)
在面向对象编程中，如果类 A 需要用到类 B 的功能，传统的写法是在 A 的内部直接实例化 B（例如：`var b = new B()`）。但这种紧密耦合的写法在现代项目开发中会引发几个大问题：
1. **强耦合风险**：如果 B 的构造函数参数发生了改变（比如需要传入配置文件），所有 `new B()` 的类 A 都必须随之修改代码。
2. **测试极难进行**：在对 A 进行单元测试时，你无法把真实的数据库操作类 B 替换为模拟的测试桩（Mock），导致单元测试必须依赖真实运行环境。

**依赖注入（DI）** 彻底解决了这个问题。类 A 不再自己创建类 B，而是声明：“我需要一个类 B（或一个实现了特定接口的类），请在创建我的时候，把这个实例注入传给我。” 这种设计思想被称为**控制反转（IoC, Inversion of Control）**。

在 Blazor / ASP.NET Core 框架中，内置了一个极其强大的 IoC 容器，用来统一管理所有服务实例的生命周期。

---

### 3.2 服务的生命周期注册
在 [Program.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Program.cs) 中，你可以看到许多类似 `builder.Services.AddScoped<Service>()` 的代码。框架提供了三种生命周期模式，代表服务实例在内存中何时创建、何时销毁：

| 生命周期 | 注册方法 | 说明 | 本项目使用场景 |
| :--- | :--- | :--- | :--- |
| **Transient** (瞬态) | `AddTransient` | 每次向容器请求该服务时，都会创建一个**全新的**实例。使用完毕后即随 GC 回收。 | 轻量级、无内部状态的公共算法或字符串工具类。 |
| **Scoped** (作用域) | `AddScoped` | 在**同一次连接**（或 Blazor 的一次 WebSocket 页面会话）中共享同一个实例。 | 绝大部分业务服务（如 `FishingService`, `CyberCatService`），保证用户在当前页面交互期间共享同一内存上下文，且能安全持有 Scope 级的数据库连接。 |
| **Singleton** (单例) | `AddSingleton` | 整个应用程序从启动到关闭期间，内存中**仅存在一个**共享的实例，所有用户共用它。 | 跨用户联机的房间管理器（如 `BoatSessionManager`）或者全局配置类。 |

```csharp
// 在 Program.cs 中注册不同生命周期的服务
builder.Services.AddSingleton<BoatSessionManager>(); // 联机房间全局唯一
builder.Services.AddScoped<FishingService>();        // 每个浏览会话独享一个实例
builder.Services.AddTransient<IDevTools>();          // 每次需要时全新创建一个
```

---

### 3.3 页面中的注入与构造函数注入

在应用程序中获取已注册服务实例有两种主要方式：

#### 1. 在 Razor 页面/组件中使用 `@inject`
在前端 `.razor` 视图中，我们使用声明式的 `@inject` 指令，Blazor 运行时会自动在渲染树实例化该组件时将对应的单例或 Scoped 服务对象塞给组件：
```razor
@page "/game"
@inject FishingService FishingService
@inject PlayerService PlayerService

<button @onclick="CastRod">抛竿</button>

@code {
    private async Task CastRod()
    {
        // 直接使用注入的服务实例，不需要你自己 new 它
        await FishingService.CastRodAsync(PlayerService.CurrentPlayerId);
    }
}
```

#### 2. 在普通的 C# 业务类中使用 **构造函数注入**
在业务层服务类中，我们不能使用 `@inject` 语法。正确的做法是声明 `private readonly` 字段，并在类的构造函数中声明需要依赖的服务接口。IoC 容器在创建当前服务时，会自动解析并把依赖项传递进来。

以 [FishingService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/FishingService.cs) 为例：
```csharp
namespace CyberPetApp.Services;

public class FishingService
{
    private readonly AppDbContext _context;
    private readonly GearMaterialService _materials;

    // 当其他类（或页面）需要 FishingService 时，IoC 容器会自动解析并实例化 AppDbContext 
    // 和 GearMaterialService，并在调用 FishingService 的构造函数时作为参数传进来。
    public FishingService(AppDbContext context, GearMaterialService materials)
    {
        _context = context;
        _materials = materials;
    }
}
```
通过这种构造函数注入的模式，类与类之间完全解耦。如果以后 `GearMaterialService` 内部增加了其他依赖项，调用 `FishingService` 的地方也完全不需要做任何代码修改，极大地提高了代码的可维护性和健壮性！
