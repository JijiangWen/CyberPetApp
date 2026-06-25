# 第 16 章：里程碑成就与每日高价求购板 🏆📆

为了引导玩家逐步体验游戏的全部内容，并为游戏的中后期提供明确长远的成长路线，CyberPetApp 引入了**里程碑成就系统**与**每日高价求购任务板**。

---

## 16.1 里程碑成就与专属解锁

成就系统由 [AchievementService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/AchievementService.cs) 提供支持，它负责在数据库中初始化、同步进度、发放点数奖励并兑换被动道具。

### 1. 成就监控指标（AchievementTrackType）：
系统在内存中通过调用 `GetTrackValue` 提取并比对以下十余项玩家的核心游玩状态：
* **首捕传说（FirstLegendary）**：玩家是否成功钓起过传说鱼。
* **累计渔获金币（TotalFishGold）**：通过钓鱼和出售赚取的数据总量。
* **图鉴收集度（DexSpecies）**：[FishCatchRecord] 表中不重复的鱼种数量。
* **终身喂养次数（FeedCount）**：照顾猫咪的次数。
* **鱼市成交次数（MarketSales）**：在鱼市挂单并与 NPC 达成买卖的次数。
* **烹饪次数（CookCount）**：烹饪鱼的累积次数。
* **神话鱼猎手（TargetFishSpecies）**：捕获的目标神话鱼种数。

### 2. 成就初始化与增量同步（SyncProgressAsync）：
每次玩家进入大厅页面，系统会自动检查 `PlayerAchievements` 表中是否存在遗漏的成就。若有（如游戏更新后新增了成就定义），则执行增量插入：
* 系统自动根据当前玩家状态重算最新 Progress，如果发现 Progress 已经满足了 `MilestoneCatalog` 中的要求，成就状态即标为“待领奖（Completed）”。

### 3. 里程碑点数与兑换：
* 玩家手动点击领奖，将获得成就点数 `MilestonePoints`（作为账户全局属性）。
* 玩家可花费里程碑点数和一定金币，在兑换商城中解锁稀有被动纪念品（如“金杯展架”、“极客徽章”）。这些解锁状态记录在 `PlayerMilestoneUnlocks` 表中。
* **被动放大**：在加载房屋加成时，`MilestoneBuffs` 会分析这些解锁项，并为玩家提供稀有度倾斜加算，甚至等比放大房屋中家具的加成倍率。

---

## 16.2 每日高价求购任务板（Daily Bounty Board）

为了给玩家提供每日的快速起步金币并丰富日常目标，[DailyBountyService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/DailyBountyService.cs) 提供了一个高价求购机制。

### 1. 每日任务生成与等级分区：
在每次进入心跳或刷新页面时，系统会对比 `player.DailyBountyDate?.Date` 与 `DateTime.UtcNow.Date`（UTC 时间）。如果不一致，则说明跨天，强制刷新今日目标：
* **求购池构建（BuildFishPool）**：
  为保证新手玩家不会刷出无法钓起的高阶鱼，系统根据玩家的当前钓鱼等级构建候选池：
  * **起步期（Lv.1+）**：仅在“镇外溪流”和“废弃鱼塘”的候选池中抽取（如溪边小白条、大鳍红马口）。
  * **进阶期（Lv.3+）**：开放高等级地图目标鱼（如荧光墨鱼、斑斓大石斑）。
* **求购定价**：
  任务求购的成交价远超常规：
  * 普通（Common）鱼：45g
  * 稀有（Rare）鱼：120g
  * 史诗（Epic）鱼：280g
  * 传说（Legendary）鱼：500g

### 2. 手动刷新与金币惩罚（RefreshBountyAsync）
如果玩家对今天的目标鱼不满意（比如自己还没买高阶许可证），可以手动选择“刷新求购目标”：
* **首刷免费**：每天的第一次手动刷新完全免费（`DailyBountyManualRefreshCount == 0`）。
* **后续收费**：之后的每一次刷新都需要扣除 50g 刷新手续费。

### 3. 兑换领取与高阶炼金素材掉落：
当玩家将目标鱼钓起后，即可在面板上进行“高价交纳”。除了获得极高额的金币回扣，系统还会根据玩家当前的钓鱼等级额外给玩家赠送**高阶炼金锻造材料**：
* **钓鱼等级 $\ge 40$**：高概率掉落 `OpenSeaStarCore`（远海星核）或 `AuroraIceCrystal`（极光冰晶）。
* **钓鱼等级 $\ge 32$**：掉落 `CanalGlowPowder`（引渠荧光粉）或 `AbyssGel`（深渊粘液）。
* **初学者级**：掉落 `ScalePowder`（鱼鳞碎屑）或 `GearSet`（生锈齿轮组）。

```
 检查跨天 (Date != Today UTC) ──> PickTargetFish (构建可用池) ──> 标记待交纳
                                                                    │
   交纳目标鱼 (TryClaimAsync) <──────────────────────────────────────┘
     ├──> 扣减背包中对应的鱼获
     ├──> 奖励求购高额金币
     └──> 根据钓鱼等级发放炼金素材 (ScalePowder / StarCore 等)
```
通过这种每日循环求购，玩家在推进日常小目标的同时获取了锻造高阶装备所需的关键耗材，打通了游戏内的经济链条！
