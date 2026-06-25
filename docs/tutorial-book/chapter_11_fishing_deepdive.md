# 第 11 章：钓鱼系统底层数学与概率演算 🎣

钓鱼系统是 **CyberPetApp** 中逻辑最丰富、数值公式最严谨的模块。它不仅仅是一个简单的延时任务，而是一个结合了**泊松分布时间流**、**三次概率判定**以及**鱼线拉力动态拉锯**的精密状态机。

---

### 11.1 泊松分布等口时间公式

在现实钓鱼中，鱼咬钩的时间是不可预测且具有随机性的，但在概率论上它符合**泊松过程（Poisson Process）**。在 [FishingWaitCalculator.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/FishingWaitCalculator.cs) 中，我们利用泊松指数分布来生成每一次抛竿等待的时长：

#### 1. 期望等待时间计算：
基础期望时间 `baseWait` 设定为 300 秒（5 分钟）。玩家装备、等级以及拟饵的加成会提供一个“时间缩减比例”：
$$\text{reduction} = \text{Attraction} \times 0.5 + \text{Level} \times 0.003 + \text{Range} \times 0.02 + (\text{RodTier}-1) \times 0.015 + (\text{LureTier}-1) \times 0.01$$
* **缩减上限（MaxGearReduction）**：最高缩减 $55\%$。
* **最终期望时长**：
  $$\text{expectedWait} = 300.0 \times (1.0 - \text{reduction}) \times \text{CatWaitMultiplier} \times \text{CatBuffWaitMultiplier}$$

#### 2. 命运扭曲与随机时间转换：
为了让高级地图的等待时间既有可能非常短（爆发惊喜），又有可能因为遇到“深海巨兽”而需要极度耐心的拉锯，我们引入了**“命运扭曲系数”** $p$：
* $p = \text{Clamp}(1.5 - \text{levelFactor}, 0.5, 1.5)$，其中地图等级越高，功率 $p$ 越小（两极分化越严重）。
* 抽取一个均匀分布的随机数 $u \in (0, 1]$，并通过 $p$ 次幂进行扭曲得到 $u_{\text{distorted}} = u^p$。
* 带入泊松指数公式：
  $$\text{finalWait} = \text{expectedWait} \times (-\ln(u_{\text{distorted}}))$$

最终，通过 Clamps 限制上下限。例如在 60 级的高级地图中，最快只需等待 30 秒，最慢可能需要拉锯 25 分钟！

---

### 11.2 阶段一：等口、截口（Drop Bite）与假口（Fake Bite）波纹

抛竿进入 `Waiting` 阶段后，系统除了计算最终咬钩时间，还为前端提供了逼真的浮漂动态回馈：

#### 1. 截口（Drop Bite）判定：
抛竿时有 $2\%$ 的小概率触发“截口”。此时直接无视正常的泊松时间，鱼饵刚落水黑影瞬间暴起，在 $2 \sim 5$ 秒内直接咬钩！

#### 2. 假口（Fake Bite）波纹判定：
在漫长的等待期中，系统每过 1 秒会进行一次假口计算：
$$\text{fakeBiteChance} = \text{proximity} \times 0.08$$
其中 $\text{proximity}$ 是当前等待已流逝时间占总等待时间的百分比（**越接近咬钩时刻，假口概率越高**）。
* 判定成功后，页面触发“假口试探”，水面出现涟漪波纹，浮漂抖动 $1.5 \sim 3$ 秒，极大地提高了挂机的沉浸感和玩家的手动预判难度。

---

### 11.3 阶段二：扬竿抓口概率（Hook Chance）

一旦等待结束或截口触发，状态机进入 `Biting` 咬钩抓口判定窗口（一般为几秒钟，由猫咪的 Agility 敏捷度延长窗口）。

抓口判定（扬竿上钩率）计算公式如下：
$$\text{hookChance} = \text{Clamp}(0.70 + \text{RodSensitivity} \times 0.1 + \text{CastRange} \times 0.03 + \text{LineSensitivity} \times 0.05 + \text{DepthBonus} + \text{FishingLevel} \times 0.005 + \text{GemHookBonus} + \text{CatHookBonus} - \text{Wariness} \times 0.40 \times (1 - \text{LineStealth}) - \text{CatSuccessPenalty}, \; 0.05, \; 0.98)$$
* **水层深度匹配（DepthBonus）**：如果当前装备的“拟饵深度”与“鱼线深度”均匹配鱼种所属的 PrimaryDepth 水层，各提供 $4\%$ 和 $3\%$ 的额外上钩率加成。
* **精明度（Wariness）**：高等级的精明鱼种会使概率暴跌，此时必须依靠鱼线的“隐蔽性（LineStealth）”去抵消它的戒备心。

如果掷随机数失败，则触发“鱼脱钩溜走”，同时有概率损耗装备耐久。

---

### 11.4 阶段三：遛鱼与起鱼概率（Land Chance）

对于 **Common（普通）** 稀有度的鱼，抓口成功后直接成功捕获。但是如果是 **Rare（稀有）**、**Epic（史诗）** 或 **Legendary（传说）** 级的大物，状态机必须强制进入 `Reeling`（遛鱼与拉锯阶段）：
* **拉锯时间**：取决于渔轮的齿轮比（Gear Ratio），齿轮比越高，卷线越快，遛鱼时长越短：
  $$\text{reelSeconds} = \text{Max}(1.5, \; (3 + \text{Random}) \times \frac{5.0}{\text{GearRatio}})$$

在拉锯过程中，随时有断线爆轮风险。起鱼成功率计算公式：
$$\text{landChance} = \text{Clamp}(0.60 + \text{DragPower} \times 0.05 + \text{Smoothness} \times 0.08 + \text{Level} \times 0.005 + \text{GemDragBonus} + \text{CatLandBonus} - \text{WeightPenalty} - \text{Power} \times 0.20 - \text{CatSuccessPenalty}, \; 0.05, \; 0.95)$$

#### 核心惩罚项：超重系数（WeightPenalty）
如果鱼的实际体重 $\text{ActualWeight}$ 超过了玩家装备的最大拉力上限 $\text{WeightLimit}$（鱼竿、渔轮线杯与鱼线强度的最小值）：
* **直接爆表惩罚**：$\text{WeightPenalty} = 0.45$（扣减 $45\%$ 成功率）。
* **轻度超重系数**：若没有直接超重，则按占比扣减：
  $$\text{WeightPenalty} = \frac{\text{ActualWeight}}{\text{WeightLimit}} \times 0.15$$
* **猫咪力量修正**：猫咪的力量属性（Strength）可以等比削减超重惩罚。

---

### 11.5 鱼获生成与超线性溢价公式

拉锯成功后，系统最终调用 [FishingSpot.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/FishingSpot.cs) 渲染生成一条真实的鱼：

#### 1. 稀有度加权抽取：
基础稀有度概率为 `Common: 70%, Rare: 20%, Epic: 8%, Legendary: 2%`。拟饵品质加成（RarityBonus）会对非普通级进行乘法倾斜：
$$\text{weight}_{\text{Rare+}} = \text{baseWeight} \times (1 + \text{RarityBonus})$$

#### 2. 体型与超规格巨物（Oversized）：
体型数值（0~100%）由 `RollSizePercentage` 确定：
* $1\%$ 极低概率触发“超规格（Oversized）”巨物，体型达到 $100\% \sim 130\%$，体重突破物理极限，直接抬升稀有度至 Legendary！
* 日常体型遵循偏态幂分布：
  $$\text{size} = \text{Random}^{2.2}$$
  *幂指数大于 1 保证了绝大部分鱼体型偏小，满体型的鱼极其罕见。*

#### 3. 大体型超线性溢价公式：
为了让玩家钓到巨物时获得巨大成就感，鱼的价格并非线性增长，而是采用**体型的平方级溢价**：
$$\text{sellPrice} = \text{ActualWeight} \times 10 \times \text{RarityMultiplier} \times (1.0 + \text{sizePercentage}^2) \times \text{SpotPriceMultiplier}$$
*体型百分比越大，后面的平方溢价越夸张，一条满规格的鱼往往可以卖出三到四倍的离谱天价！*
