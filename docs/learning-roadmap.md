# CyberPetApp 零基础学习导航与开发路线图 🗺️

欢迎来到 **CyberPetApp** 的代码世界！本项目是一个基于 **ASP.NET Core (Blazor Server) + EF Core + PostgreSQL** 打造的赛博猫咪挂机与钓鱼网页游戏。

为了让你能够最快地熟悉整个项目，看完后能亲自动手写代码、加功能，我们为你量身定制了这套**「由浅入深」的学习导航**。请对照以下阶段顺序逐步学习：

---

## 🧭 学习路线图：六大通关阶段

```
┌────────────────────────────────────────────────────────┐
│  第一阶段：Blazor 与 C# 编程基石 (第 0、1、2 章)          │ 🎯 零基础起步，掌握基本语法
└───────────┬────────────────────────────────────────────┘
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  第二阶段：数据持久化与服务注入 (第 3、4 章)             │ 💾 学会用 EF Core 读写数据库
└───────────┬────────────────────────────────────────────┘
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  第三阶段：多线程并发与心跳循环 (第 5、6、7 章)          │ 🔒 挂机养成的心脏与安全防线
└───────────┬────────────────────────────────────────────┘
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  第四阶段：现代 C# 特性与 UI 雪碧图 (第 8、9 章)        │ 🎨 像素风游戏渲染与现代语法糖
└───────────┬────────────────────────────────────────────┘
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  第五阶段：游戏业务深度剖析 (第 10 ~ 17 章)             │ 🎣 核心玩法（钓鱼、交易、联机）
└───────────┬────────────────────────────────────────────┘
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  第六阶段：实操演练，动手写功能 (实操教学书)              │ 🛠️ 从零开发你的第一个新功能
└────────────────────────────────────────────────────────┘
```

---

## 📅 阶段细节与推荐阅读顺序

### 第一阶段：Blazor 与 C# 编程基石 🧱
> **学习目标**：读懂代码的每一行基本语法，明白网页在浏览器上是怎么显示和互动的。

1. **[第 0 章：C# 与 Razor 基础语法超详细入门](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_00_csharp_razor_basics.md)**
   - *学习重点*：了解什么是类、什么是变量。搞懂 Razor 的 `@if`、`@foreach` 怎么控制 HTML 标签的显示，以及 `@bind` 双向数据绑定的妙用。
2. **[第 1 章：Blazor Server 基础与生命周期](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_01_blazor_lifecycle.md)**
   - *学习重点*：明白为什么 Blazor Server 响应这么快（SignalR 管道技术），了解一个网页组件从出生（`OnInitializedAsync`）到销毁（`DisposeAsync`）的过程。
3. **[第 2 章：Razor 组件的数据流与组件拆分](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_02_components_dataflow.md)**
   - *学习重点*：父子组件怎么通信？记住八字真言：**“属性下行（Parameter），事件上行（EventCallback）”**。

---

### 第二阶段：数据持久化与服务注入 💉
> **学习目标**：打通代码与数据库的连接，学会如何长期保存玩家的存档数据。

4. **[第 3 章：依赖注入 (Dependency Injection) 原理](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_03_dependency_injection.md)**
   - *学习重点*：为什么我们不需要手动 `new` 一个类？依赖注入容器是如何在后台帮我们管理和传递这些服务的。
5. **[第 4 章：数据持久化与 EF Core 核心设计](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_04_efcore_persistence.md)**
   - *学习重点*：认识“翻译官” EF Core，了解如何把 C# 类映射成数据库表。重点理解异步读写 `async` / `await` 的非阻塞设计。

---

### 第三阶段：多线程并发与心跳循环 🔒
> **学习目标**：攻克 Blazor 最硬核的难点，搞懂多线程挂机和安全登录认证的底层秘密。

6. **[第 5 章：并发控制与多线程安全 (核心难点)](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_05_concurrency_control.md)**
   - *学习重点*：挂机游戏后台线程非常多，如何用 `SemaphoreSlim` 异步锁和 `lock` 锁保护玩家的数据，避免“多线程同时读写同一个数据库连接”报错。
7. **[第 6 章：定时器与后台心跳循环](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_06_timers_game_loop.md)**
   - *学习重点*：游戏里的时间怎么流逝？每 2 秒一次的心跳如何运作？玩家下线后，离线补偿机制是怎么精准把钱补给玩家的。
8. **[第 7 章：Cookie 认证与 Minimal API 混合架构](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_07_auth_cookie_endpoints.md)**
   - *学习重点*：如何实现安全的玩家登录？为什么 Blazor 需要借助 Minimal API 写入 Cookie？

---

### 第四阶段：现代 C# 特性与 UI 雪碧图 🎨
> **学习目标**：提升代码逼格，减少冗余代码，并优化像素风游戏界面的渲染效率。

9. **[第 8 章：游戏 UI 呈现与像素风雪碧图](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_08_graphics_sprites.md)**
   - *学习重点*：为什么像素风游戏图标不会糊？理解雪碧图（Sprite Sheets）怎么实现仅请求一张大图就能切出上百个鱼获图标。
10. **[第 9 章：现代 C# 高级特性实战](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_09_csharp_advanced.md)**
    - *学习重点*：学会使用只读记录 `record`、精简的 `switch` 模式匹配，以及将数据和逻辑解耦的 Catalog（静态配置目录）模式。

---

### 第五阶段：游戏业务深度剖析 🎮
> **学习目标**：吃透 CyberPetApp 的核心游戏机制，深入研究真正的挂机挂网游是怎么写出来的。

* **如果想看家具与数值加成** 👉 学习 **[第 10 章：房屋与家具被动养成系统](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_10_housing_furniture.md)**
* **如果想看高难度数学概率公式（泊松分布、遛鱼等）** 👉 学习 **[第 11 章：钓鱼系统底层数学与概率演算](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_11_fishing_deepdive.md)**
* **如果想看装备成长、爆率与乐观 UI 设计** 👉 学习 **[第 12 章：装备成长与商店乐观 UI](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_12_equipment_progression.md)**
* **如果想看物品合成与药剂 Buff 设计** 👉 学习 **[第 13 章：炼金与烹饪双加工系统](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_13_alchemy_cooking.md)**
* **如果想看动态物价与 NPC 讨价还价算法** 👉 学习 **[第 14 章：鱼市交易与砍价博弈](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_14_market_trading.md)**
* **如果想看多人在线船钓、跨页面广播与高并发 DB 处理** 👉 学习 **[第 15 章：联机船钓与 DB 厂模式](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_15_boat_multiplayer.md)**
* **如果想看每日任务与图鉴成就** 👉 学习 **[第 16 章：里程碑成就与每日求购板](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_16_daily_bounties_achievements.md)**
* **如果想看猫咪自动喂养系统** 👉 学习 **[第 17 章：喂食器与猫咪状态自然衰减](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/tutorial-book/chapter_17_feeder_waterer_autocare.md)**

---

### 第六阶段：实操演练，动手写功能 🛠️
> **学习目标**：纸上谈兵终觉浅，通过实现一个真正的新功能，打通你的任督二脉。

* **核心必读**：**[实操教学书：如何从零开发一个游戏新功能](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/docs/practical-feature-guide.md)**
  - 这本书为你梳理了从 0 到 1 写一个“猫咪玩具与亲密度系统”的完整构思。当你阅读完这本实操书后，**不要仅仅只满足于看，请尝试参照它的步骤，在你的电脑上将这些代码真正地写进项目里运行一下！**

---

## 💡 给学习者的三条终极建议

1. **先看主干再看细节**
   不要一开始就纠结于复杂的钓鱼遛鱼数学公式。优先搞懂第一到第三阶段的 Blazor 和 EF Core 机制，因为这是所有挂机业务能跑起来的“大动脉”。
2. **多读代码里我为你留下的 Layman-Friendly 注释**
   我已经为核心逻辑文件（如 `Home.GameLoop.cs`、`Home.Gear.cs`、`FeederService.cs` 等）写上了甚至连外行人都能看懂的“大白话”注释。在阅读这些 C# 文件时，它们会像老师一样随时为你指明每一行代码的作用。
3. **自己动手做个小改动**
   比如，试着修改一下 `chapter_11_fishing_deepdive.md` 里的钓鱼概率，或者尝试在 `Home.razor` 页面改个背景颜色。每一次微小的反馈都会让你对代码的掌控感翻倍！

祝你学习愉快，早日成为 Blazor 游戏开发大师！🚀
