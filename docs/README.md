# CyberPetApp 项目文档

**CyberPetApp** 是一款基于 **Blazor Server** 的挂机养猫 + 钓鱼模拟游戏。玩家注册登录后，在单页主界面中管理猫咪四维状态、家园家具、四槽钓鱼装备、炼金锻造、鱼市交易与离线补偿等系统。后端使用 **ASP.NET Core + EF Core + PostgreSQL** 持久化进度。

## 文档索引

| 文档 | 说明 |
|------|------|
| [Blazor & C# 技术总结](./tech-blazor-csharp.md) | 渲染模式、组件拆分、DI、EF Core、定时器、认证、雪碧图等，每项技术附本项目代码示例 |
| [游戏设计与算法](./game-design.md) | 猫消耗、钓鱼状态机、抽鱼权重、装备成长、炼金 loop、经济 sink、离线补偿等公式与常量 |
| [性能分析报告](./performance-analysis.md) | Blazor Server 热点、EF 写库频率、锁与 SignalR、优化优先级与测量方法 |
| [钓鱼玩法流程与资源闭环](./fishing-gameplay-loop.md) | 钓鱼核心循环、状态流转、产出消耗与资源回流 |

## 技术栈速览

- **前端**：Blazor Server（`InteractiveServer`）、Razor 组件、CSS 雪碧图（`wwwroot/cyberpet-theme.css`）
- **后端**：Minimal API 认证端点、Scoped 服务层、Cookie 认证
- **数据**：EF Core + PostgreSQL（`AppDbContext` + Migrations）
- **资产管线**：`tools/build_assets.py` → 雪碧图 PNG → CSS `background-position`

## 入口文件

- 应用启动：`Program.cs`
- 主游戏页：`Components/Pages/Home.razor`（`@page "/"`，需登录）
- 登录/注册：`Components/Pages/Login.razor`、`Register.razor`
