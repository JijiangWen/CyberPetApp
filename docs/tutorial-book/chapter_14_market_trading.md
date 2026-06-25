# 第 14 章：鱼市交易、NPC 偏好与砍价博弈 ⚖️

如果玩家不想将鱼做成饲料或炼金材料，最好的去处是把它们拿到**自由鱼市**挂单上架，等待路过的各式各样 NPC 前来报价收购。这套系统由上架额度、NPC 报价偏好公式以及有趣的“砍价博弈”机制共同构成。

---

### 14.1 摊位额度与上架费（Stall Listings）

为了防止玩家一次性在鱼市倾倒几百条小鱼把数据库撑爆，系统设置了上架摊位上限限制：
* **基础摊位（BaseListingLimit）**：默认 3 个格子。
* **摊位券扩容**：玩家在猫咪打工（如“鱼市搬运工”）中可以极低概率获取“摊位券（StallTicket）”。每个摊位券能永久增加 1 个上架摊位，最高扩容 2 个格子：
  $$\text{ListingLimit} = 3 + \text{Min}(\text{StallTickets}, \; 2)$$
* **摊位手续费（MarketListingFee）**：
  上架会产生不可退还的摊位费（Gold Sink）：
  $$\text{StallFee} = \text{Max}(5\text{g}, \; \lceil\text{SellPrice} \times 2\%\rceil)$$
  *无论最终交易是成功、拒绝还是中途下架，摊位费均不退还，这有效地控制了货币通胀。*

---

### 14.2 五大 NPC 买家偏好与动态报价

当鱼上架后，后台定时器在每次心跳中都会以一定概率驱动不同的 NPC 前来浏览并出价：
* **流浪猫（StrayCat）**：偏好 **Common（普通）** 的小杂鱼，出价极低（$\approx 0.75 \times \text{BasePrice}$），但出价最快、最不挑剔。
* **厨师猫（ChefCat）**：偏好 **Rare（稀有）** 到 **Epic（史诗）** 的优质食材做原料（偏好系数 $1.1 \sim 1.25$）。
* **饕客（Gourmet）**：偏好高档的 **Epic** 或 **Legendary** 珍稀海鲜（偏好系数高达 $1.3 \sim 1.45$）。
* **收藏家（Collector）**：狂热追求 **Legendary** 传说鱼，或者体重突破 $100\%$ 的**“超规格（Oversized）”**巨兽。如果是超规格巨物，其报价偏好直接翻倍到 **$2.2$ 倍**！
* **心情 NPC（MoodNpc）**：其出价完全随玩家当前猫咪的“快乐度”浮动：
  $$\text{Pref} = 0.70 + \frac{\text{CatHappiness}}{1000.0} \times 0.50$$

#### 最终报价公式：
在确定买家偏好系数 $\text{Pref}$ 后，系统会施加一个 $\pm 10\%$ 的波动抖动系数（Jitter），并且报价绝对不会低于该鱼在商店直售的保底回收价：
$$\text{Price}_{\text{Offer}} = \text{Max}(\text{FloorPrice}, \; \lceil\text{BaseSellPrice} \times \text{Pref} \times \text{Random}(0.9, 1.1)\rceil)$$

---

### 14.3 砍价（Counter Offer）博弈算法

当 NPC 给出报价后，玩家可以选择直接接受（成交）、拒绝（移除），或者进行**“砍价博弈”**。选择砍价时，我们要求 NPC 将出价提升 $10\%$，但有可能会直接惹恼对方导致砍价失败。

#### 砍价成功率计算公式：
砍价是否成功由猫咪当前的快乐度（Happiness）和魅力（Charisma, CHM）决定：
$$\text{rate} = 0.50 + \frac{\text{CatHappiness}}{1000.0} \times 0.20 + \frac{\text{CatCharisma}}{1000.0} \times 0.15 - \text{CounterPercent} \times 0.30 + \text{MilestoneBonus}$$
* **砍价幅度惩罚（CounterPercent）**：默认要价上抬 $10\%$，扣减 $3\%$ 成功率。
* **概率区间封顶**：无论猫咪多有魅力，成功率最终被强行 Clamped 在 $[10\%, \; 90\%]$ 之间，保留基本的戏剧性。

#### 砍价结果：
* **成功**：NPC 同意涨价，报价被直接乘以 $1.1$ 并更新在页面上。玩家可以继续砍第二轮，但难度会累加。
* **失败（24小时黑名单机制）**：
  NPC 觉得受到了侮辱，直接拂袖而去并撤回当前报价。更严重的是，该买家（如“饕客”）会在 **24 小时内进入对该商品上架单的黑名单（NpcListingBan）**，这段时间内他绝不会再对此鱼进行任何出价！

---

### 14.4 鱼市乐观 UI 与两步数据库 Commit 模式

由于鱼市挂单包含“扣费、将鱼移出背包、插入上架表、在页面上显示新卡片”这一连串复杂动作，如果全部同步等待，高延迟下页面体验极差。

我们在这里使用了**乐观 UI 与两步 Commit 模式**：

1. **准备与校验**：在内存中调用 `TryPrepareList` 校验金币和背包。
2. **乐观假上架**：直接扣除玩家内存金币，将鱼从 `player.FishBackpack` 移除，塞进列表视图前端。
3. **安全提交数据库**：
   异步调用 `CommitListFishAsync`。该方法在独占锁 `dbLock` 内部执行：
   * 删掉 Fishes 表中对应的记录。
   * 往 MarketListings 表中写入一条新的活跃记录。
   * 成功则静默结束；若失败，执行 `RollbackListFish`，金币退回，鱼塞回背包。

```
 [上架按钮] ──> ApplyListOptimistic ──> 刷新前端 UI (0延迟)
                  └──> CommitListFishAsync 
                         ├──> 写入 DB (SaveChangesAsync) ──> 成功
                         └──> [异常] ──> Rollback ──> 退钱/退鱼 ──> 刷新 UI
```

通过这套巧妙的异步两步提交机制，玩家在摊位上架、撤单时，页面卡片都会瞬间发生漂移响应，毫无黏滞感，而后台的 PostgreSQL 数据库依然在事务锁的保护下井然有序地运行！
