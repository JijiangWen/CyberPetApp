# 第 13 章：炼金与烹饪双加工系统 🧪🍳

为了给多余的鱼获（尤其是低稀有度的普通鱼和高难度钓到的传说鱼）提供高价值的消耗途径（Sink），游戏设计了**炼金**与**烹饪**两个深度加工系统。这两个系统不仅为猫咪提供食物，还产出强力的装备强化配件。

---

## 13.1 炼金系统（Alchemy System）

[AlchemyService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/AlchemyService.cs) 负责打通材料与装备成长。它支持四类合成及镶嵌：

### 1. 炼成镶嵌宝石（CraftGem）：
* **宝石类型（GemType）**：
  * **抓口宝石（Hook）**：提升抓口率，只能镶嵌在【鱼竿】。
  * **卸力宝石（Drag）**：提升起鱼率，只能镶嵌在【渔轮】。
  * **幸运宝石（Luck）**：倾斜稀有度权重，只能镶嵌在【拟饵】。
  * **钓重宝石（Weight）**：增加装备的最大承重上限，只能镶嵌在【鱼竿】。
  * **丝导宝石（Line）**：提高鱼线敏感度，只能镶嵌在【鱼线】。
* **属性随机波动（RollGemBonus）**：
  新合成的宝石数值不是固定的，系统会随机 Roll 出一个 $3\% \sim 8\%$ 之间的浮动加成：
  $$\text{BonusValue} = 0.03 + \text{Random} \times 0.05$$
* **镶嵌上限封顶（CapGemBonuses）**：
  为避免多颗宝石叠加导致概率直接溢出破产，系统在加载 Loadout 时通过 `FromGems` 会对每项加成求和，并实行 **$15\%$ 的全局硬上限限制**（多出部分直接舍弃）。

### 2. 装备与高级鱼线锻造（CraftGear/CraftLine）：
* **跨服务协作**：炼金服务通过读取 `GearProgressionCatalog` 的锻造配方，扣除原材料后，跨服务调用 `EquipmentService.AddCraftedRodAsync` 等方法直接向玩家名下塞入专属的高阶锻造装备。

### 3. 特殊锁定拟饵（Target Lure）：
* 允许玩家使用特定传说鱼配合打工素材，炼制出**“神话鱼锁定路亚饵”**（如“沉船亡魂特制饵”、“极光霜骨不融冰饵”）。
* **锁定机制**：将其放入特殊饵槽位（Target Lure Slot）并装备后，在目标钓点钓鱼时，每轮判定有 **$22\%$ 的极高直接命中率（TargetFishDirectRollChance）** 命中该神话鱼模板；其余的 $78\%$ 仍走普通概率。捕获成功后，扣减其 1 次剩余使用次数（RemainingUses）。

### 4. 宝石镶嵌事务（SocketGem）：
* 镶嵌动作会产生 200g 的手工费。
* **旧宝石爆毁**：如果目标装备槽上已经镶嵌了宝石，旧宝石会被**直接爆毁移除**，然后将新宝石持久化关联到已配备装备的主键 ID 上。

---

## 13.2 烹饪系统（Cooking System）

[CookingService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/CookingService.cs) 将各种稀有度的鱼加工成为能够用于喂食赛博猫咪的宠物饲料，并为玩家提供烹饪等级成长。

### 1. 经验加成与食谱路由
* 烹饪单条鱼时，系统会先检查玩家当前的 `CookingLevel` 是否达到了该稀有度食谱的解锁要求（如 Common 鱼做成“野溪杂鱼刺身”需要 Lv.1，Legendary 鱼加工成“神话皇家海皇罐”需要极高等级）。
* **房屋加成缩放（ScaleCookingXp）**：
  基础烹饪经验值会受到厨房中已经升级的家具（如“老旧烤箱”）加成聚合后得到的只读系数 `houseBuffs.CookingXpMultiplier` 进行等比乘算放大：
  $$\text{xp}_{\text{final}} = \lceil\text{baseXp} \times \text{CookingXpMultiplier}\rceil$$

### 2. 批量烹饪算法（CookAllAsync）
为了避免玩家背包里积压几十条普通小鱼需要疯狂点击，批量烹饪会一次性筛选背包中所有符合解锁要求的普通鱼，自动计算总金币加工费，通过 conditional dbLock 串行保存数据库：
* 剔除所有鱼实体。
* 根据食谱结果，往 `BackpackItems` 表中插入或增加对应数量的食物。
* 汇总增加烹饪经验，处理可能发生的烹饪等级提升（Level Ups）。

### 3. 烹饪口粮的 Buff 施加机制
与普通商店买的速食猫粮不同，经过玩家烹饪的鱼肉料理在被猫咪食用时，除了立刻恢复饱食度，还会向猫咪施加一个持续的增益状态（Buff）：
* 喂食成功后，烹饪服务调用 `_catBuffService.ApplyRecipeBuffsAsync(playerId, recipe)`。
* 写入 `CatFoodBuffs` 数据库表，记录该 Buff 的类型（如“幸运暴击”、“等口缩短”）、加成数值以及过期的时间戳。
* 游戏心跳 Timer 在计算后续的 Tick 时，会读取这些 Buff 并应用在钓鱼和打工公式里。

```
 [点击喂食食物] ──> ConsumeCookedFood (扣背包) 
                    └──> ApplyRecipeBuffsAsync (写 Buff 表) ──> 在 2s 心跳中生效
```
