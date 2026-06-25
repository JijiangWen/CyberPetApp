# CyberPetApp 从零学代码 · 究极教学书 📚

欢迎阅读 **《CyberPetApp 从零学代码 · 究极教学书》**。本书基于项目实际源码，专为 C# / Blazor 网页游戏开发初学者设计。书中采用循序渐进的教学方式，深入剖析游戏各模块的底层机制与编码实践，帮助你从零起步，全面掌握现代 Web 应用程序开发的核心技术。

为了让学习体验更流畅，我们将教学书拆分为结构清晰的**专题章节**，每个章节都包含极度详尽的数学公式、业务逻辑设计和完整的代码解析。

> [!TIP]
> **🚀 初学必看**：如果您是刚接触项目或零基础的开发者，建议优先阅读我们专门为您定制的 [**【零基础学习导航与开发路线图】**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/learning-roadmap.md)，它会指导您在不同阶段应该看哪些文件，以及如何开始第一步。

---

## 🗺️ 架构总览与数据流图

在编写代码之前，先了解 **CyberPetApp** 的整体系统架构与数据流向是非常重要的。整个系统遵循现代 Web 应用的“分层设计模式”，分为三层：

```
┌────────────────────────────────────────────────────────┐
│               演示层 (Razor Components)                 │
│  [Home.razor]  [CatCareSidebar]  [GearLoadoutPanel]    │
└───────────┬────────────────────────────▲───────────────┘
            │ 1. 用户操作 / 事件触发       │ 4. StateHasChanged
            ▼                            │    重新渲染 UI
┌────────────────────────────────────────┴───────────────┐
│                 业务逻辑层 (Services)                   │
│  [CyberCatService] [FishingService] [PlayerService]     │
└───────────┬────────────────────────────▲───────────────┘
            │ 2. 执行业务逻辑 /           │ 3. 返回处理结果 /
            │    调用持久层接口          │    内存状态快照
            ▼                            │
┌────────────────────────────────────────┴───────────────┐
│               数据持久层 (EF Core & DB)                 │
│  [AppDbContext.cs] ---------> [PostgreSQL 数据库]       │
└────────────────────────────────────────────────────────┘
```

---

## 📚 目录与章节指南

点击下方链接即可跳转到对应章节进行深入学习，建议按照顺序循序渐进地阅读：

### 🧱 第一部分：Blazor 框架与 C# 编程基石
* [**第 0 章：C# 与 Razor 基础语法超详细入门 🚀**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_00_csharp_razor_basics.md)
  - 变量与常用数据类型（`int`, `double`, `string`, `Guid` 等）
  - 分支判断（`if-else`）与循环控制（`foreach`）
  - 类与对象实例化，Razor 模板语法与动态 CSS/HTML 数据流双向绑定
* [**第 1 章：Blazor Server 基础与生命周期 🚀**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_01_blazor_lifecycle.md)
  - WebSocket (SignalR) 双向通信模型优势
  - 入口文件 `Program.cs` 容器与管道详解
  - 交互模式 `@rendermode InteractiveServer`
  - 组件生命周期钩子（`OnInitializedAsync`、`StateHasChanged` 与资源注销 `DisposeAsync`）
* [**第 2 章：Razor 组件的数据流与组件拆分 🧱**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_02_components_dataflow.md)
  - 父子组件传递：属性下行（Parameters Down），事件上行（Events Up）
  - 属性参数绑定 `[Parameter]` 与事件回调 `EventCallback`
  - 使用 C# `partial` 特性进行物理文件模块拆分
* [**第 3 章：依赖注入 (Dependency Injection) 原理 💉**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_03_dependency_injection.md)
  - 控制反转（IoC）与解耦设计
  - 服务生命周期注册：瞬态（Transient）、作用域（Scoped）与单例（Singleton）
  - Razor 视图声明注入 `@inject` 与普通的构造函数注入模式

### 💾 第二部分：持久化、多线程并发与底层安全
* [**第 4 章：数据持久化与 EF Core 核心设计 💾**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_04_efcore_persistence.md)
  - 对象关系映射 (ORM) 与 DbContext 基础
  - 联合主键、外键级联删除配置与内存辅助 `[NotMapped]` 特性
  - 数据库迁移（Migrations）工作流
  - 异步磁盘 I/O 读写与 `async`/`await` 原理
* [**第 5 章：并发控制与多线程安全 🔒**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_05_concurrency_control.md)
  - Blazor Server 并发隐患与竞态条件（Race Condition）
  - 内存线程同步锁 `lock` 机制与锁防区限制
  - 数据库异步信号量锁 `SemaphoreSlim` 串行封装
  - 锁重入死锁防御与 **“有条件锁定模式 (Conditional Locking)”**
* [**第 6 章：定时器与后台心跳循环 ⏱️**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_06_timers_game_loop.md)
  - `System.Timers.Timer` 驱动的每 2 秒心跳 Tick 回调
  - 离线补偿逻辑（Offline Compensation）时间跨度模拟算法
  - 异步阻塞轮询与 `CancellationToken` 取消流控制
* [**第 7 章：Cookie 认证与 Minimal API 混合架构 🔑**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_07_auth_cookie_endpoints.md)
  - 为什么 WebSocket 无法写入 Cookie 的底层解释
  - Minimal API 表单提交混合架构设计与凭证签发
  - 声明对象 `ClaimsPrincipal` 加密 Cookie
  - 路由守卫 `CascadingAuthenticationState` 与权限认证拦截

### 🎨 第三部分：游戏 UI 呈现与经典高级特性
* [**第 8 章：游戏 UI 呈现与像素风雪碧图 (Sprites) 🎨**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_08_graphics_sprites.md)
  - 什么是 CSS 雪碧图（Sprite Sheets）及高并发网络性能优化
  - `image-rendering: pixelated` 锯齿不模糊技术
  - 后端映射翻译器 `SpriteCatalog` 静态映射
* [**第 9 章：现代 C# 高级特性实战 🛠️**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_09_csharp_advanced.md)
  - 不可变只读记录 `record` 及其值相等性
  - 仅在初始化可写 `init` 属性修饰符
  - Switch 表达式函数式模式匹配
  - 数据与逻辑彻底解耦的 Catalog 静态配置模式

### 🎮 第四部分：游戏业务模块究极深挖（核心实战）
* [**第 10 章：房屋、房间与家具被动养成系统 🏠**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_10_housing_furniture.md)
  - 2x2 网格平面房间结构与解锁金币扣减
  - 被动加成家具 Catalog 映射与同类去重最优算法（Category Flattening）
  - 房屋自动补水与喂食心跳托管，家具日常折旧维护费平衡
* [**第 11 章：钓鱼系统底层数学与概率演算 🎣**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_11_fishing_deepdive.md)
  - 泊松分布扭曲概率等口时间公式与指数随机分布
  - 截口（Drop Bite）触发与渐进假口波纹涟漪计算
  - 扬竿抓口率（Hook Chance）和遛鱼起鱼率（Land Chance）最大拉力超重惩罚
  - 鱼获随机体型（Oversized）幂分布与价格超线性溢价公式
* [**第 12 章：装备成长、磨损公式与商店乐观 UI 🛠️**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_12_equipment_progression.md)
  - 四件套渔具敏感度、卸力、拉力与吸引力配置
  - Durability 低于 30 数值腰斩惩罚，钓点额外磨损与断线惩罚
  - 阶梯式金币全额及微量修理算法，商店零延迟“乐观 UI”回滚闭环设计
* [**第 13 章：炼金与烹饪双加工系统 🧪🍳**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_13_alchemy_cooking.md)
  - 炼金合成：3%~8% 随机加成宝石，全局 15% 镶嵌限幅，旧宝石爆毁替换
  - 特定神话鱼 22% 命中特殊锁定饵制作与消耗
  - 烹饪鱼肉料理：经验加成缩放，批量制作逻辑，喂食 Buff 持续状态存盘
* [**第 14 章：鱼市交易、NPC 偏好与砍价博弈 ⚖️**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_14_market_trading.md)
  - 摊位数量限制（摊位券扩充）与不可退摊位手续费
  - 饕客、流浪猫、收藏家、心情 NPC 动态偏好与保底报价公式
  - 砍价博弈算法：根据魅力与快乐度计算抬价率，砍价失败 24 小时封单黑名单
* [**第 15 章：联机船钓、跨 Circuits 广播与 DB 厂模式 🚢**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_15_boat_multiplayer.md)
  - 线程安全内存房间 `ConcurrentDictionary` 高并发保护
  - 跨 Circuits 会话事件订阅派发广播，UI 渲染线程调度与生命期注销
  - 用 `IDbContextFactory<AppDbContext>` 短寿命上下文杜绝并发冲突
* [**第 16 章：里程碑成就与每日高价求购板 🏆📆**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_16_daily_bounties_achievements.md)
  - 多类型游玩成就监测、自动增量同步与点数兑换 shop 专属 Buff
  - 每日求购：跨天时区判断，钓鱼等级可用池区间，交纳奖励与高阶素材赠予
* [**第 17 章：喂食器、饮水泉与猫咪状态自然衰减 🍲💧**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_17_feeder_waterer_autocare.md)
  - 猫咪饥饿与口渴 Tick 自然衰减，健康度归零濒死医治
  - 自动喂食/补水托管装填加工费，先进先吃 FIFO 队列重排，离线补偿托管计算

### 🛠️ 实操与实战专栏
* [**实操专栏：如何从零开发一个游戏新功能 🚀**](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/practical-feature-guide.md)
  - 从零开发“猫咪玩具与亲密度系统”的完整构思与代码编写实操指南。

---

祝你学习愉快！本书会带你揭开一个完整赛博猫咪钓鱼游戏背后的全部神秘代码面纱。如果你在阅读或动手开发过程中有任何疑问，请随时向我提问，我们一同探讨！
