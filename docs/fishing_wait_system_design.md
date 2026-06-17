# 赛步猫咪钓鱼等口与水面状态模拟系统设计文档 (Fishing Wait & Water Simulation Design)

为了提升游戏体验，我们将原先“以倒计时决定咬钩时间”的线性逻辑，重构为符合硬核钓鱼游戏（如《星露谷物语》、《俄罗斯钓鱼4》）感官体验的**指数分布（泊松过程）隐藏等待机制**与**基于马尔可夫状态变化的随机真/假口感知体系**。

本文档详细记录重构后的算法模型、UI 表现机制以及单人/联机小船的实现方式。

---

## 1. 核心算法设计 (Mathematical Model)

在现实物理世界中，鱼儿咬钩是一个独立的随机事件，其等待时间符合统计学中的**指数分布（Exponential Distribution）**。高级钓区与低级钓区的主要区别，在于等待时间偏向与离散度（两极分化度）的不同。

为了在游戏中完美重现这种等待的张力，系统单次抛竿的等待时间 $T$ 的计算流程如下：

### 1.1 期望等待时间 $E(T)$ 统一化
为避免“高级地图等得太久”给玩家带来的憋屈感，所有地图的基础期望等待时间（$T_{base}$）在底层被固定为相同的舒适区间，统一为 **300秒（5分钟）**：

$$T_{base} = 300.0$$

实际期望时间 $E(T)$ 会根据玩家的**装备等级**和**猫咪属性及Buff**进行缩减（最大可缩减 55%）：

1. **装备与饵缩减系数（$Reduction$）**：
   - 玩家的装备品质与拟饵吸引力会缩短等待时间，最大缩减比例上限为 **55%**（`MaxGearReduction = 0.55`）。
   - 计算公式：
     $$Reduction = \text{Attraction} \times 0.5 + \text{FishingLevel} \times 0.003 + \text{CastRange} \times 0.02 + (\text{RodTier} - 1) \times 0.015 + (\text{LureTier} - 1) \times 0.01$$

2. **猫咪与状态乘数（$Factor$）**：
   - 敏捷猫咪的属性会缩短时间：`catStats.WaitMultiplier`（通常在 0.8 ~ 1.2 之间）。
   - 猫咪的饥饿或疲劳等负面状态会延长等待：`catBuff.WaitTimeMultiplier * catBuff.WaitTimePenalty`。

得出这轮抛竿的数学期望时间：
$$E(T) = T_{base} \times (1.0 - Reduction) \times \text{WaitMultiplier} \times \text{BuffMultipliers}$$

### 1.2 地图等级对随机数 $U$ 的“命运扭曲”（两极分化度）
利用均匀分布随机数 $U \sim \text{Uniform}(0, 1]$。在生成指数分布前，我们根据地图的 RequiredLevel 计算命运扭曲系数（`power`）：

1. **地图等级因子（$LevelFactor$）**：
   $$LevelFactor = \frac{\text{spot.RequiredLevel} - 1}{60 - 1}$$
   - 将地图等级 1~60 线性映射为 0.0 ~ 1.0。在联机船钓中，木划桨舢板等同于 Lv.35，双体游艇等同于 Lv.55。

2. **命运扭曲幂次（$Power$）**：
   $$Power = \text{Math.Clamp}(1.5 - LevelFactor, 0.5, 1.5)$$

3. **随机数扭曲**：
   $$U_{distorted} = U^{Power}$$

4. **泊松指数时间生成**：
   $$T = - E(T) \times \ln(U_{distorted})$$

* **数值效果**：
  - **低级地图（Power 接近 1.5）**：随机数向中间收缩，等待时间极其稳定温和（大多落在 3 ~ 8 分钟之间），极少让玩家等超过 10 分钟，适合新手平稳积累资源。
  - **高级地图（Power 接近 0.5）**：两极分化极其严重。具有高概率抽到超短时间（秒咬），同时也伴随一定概率抽到极限长尾（死等），将未知悬念感推到极致。

### 1.3 终极限幅与上下限缩放
不同等级地图的限幅区间也随等级动态拉开：

- **下限（$MinWait$）**：
  $$MinWait = 120.0 - (LevelFactor \times 90.0)$$
  - 新手村（Lv.1）最快需等 2 分钟，而满级钓区（Lv.60）允许最快 30 秒秒咬！
- **上限（$MaxWait$）**：
  $$MaxWait = 900.0 + (LevelFactor \times 600.0)$$
  - 新手村（Lv.1）最长等待 15 分钟，而满级钓区（Lv.60）最长可达 25 分钟。

最终抛竿咬钩等待时间：
$$T_{final} = \text{Math.Clamp}(T, MinWait, MaxWait)$$

---

## 2. 悬念感与随机事件驱动设计 (Game Feel & UX)

为了杜绝“进度条读条”这一机械式的伪随机感，我们引入了以下三个创新设计：

### 2.1 截口机制 (Drop Bite)
- 在抛竿入水的瞬间（前 5 秒内），系统有 **2% 的极小概率** 触发“落水截口”惊喜事件。
- 一旦触发，系统将重置并大幅缩短 $T_{final}$ 至 **2.0 ~ 5.0 秒** 之间，并跳过期间的所有假口阶段，直接触发咬钩。
- 触发时，控制台将输出日志：`[系统] 鱼饵刚刚落水！水底黑影暴起！⚡`（联机时会在公共日志广播此系统级事件）。

### 2.2 假口机制 (Fake Bite / Nibble)
- 在抛竿等待期间（非截口状态下），系统在每 1 秒的 Tick 检查中，根据当前进度动态计算假口概率：
  $$\text{proximity} = \frac{T_{elapsed}}{T_{final}}$$
  $$P(\text{fake\_bite}) = \text{proximity} \times 0.08$$
- 越接近最后的真实咬钩时刻，鱼群聚集度越高，触发假口的概率也就越大（最大可达 8%/秒）。
- 一旦假口触发，系统维持 `IsFakeBiting = true` 持续 **1.5 ~ 3.0 秒**。假口表现为浮漂摇晃，几秒后恢复平静。

### 2.3 水面波浪“呼吸感” (Breathing Wave Animation)
- 我们彻底废除了 0% ~ 100% 的线性进度展示，改为只显示**正向正计时**（如 `已等待: 02:45`）。
- 平常状态下，以 **7 秒为一个自然周期** 进行“呼吸”交替：
  - 前 3 秒水流平缓，显示为 `水波微兴 🍃`，ASCII 波形为 `~ ~ ~ ~ ~ ~`
  - 后 4 秒回归平静，显示为 `水平如镜 🌊`，ASCII 波形为 `░░░░░░░░░░`
- 假口触发时，强制切为 `浮漂轻摇 🐟`（粉红色高亮），波形显示为动荡的 `🐟 ≈≈ ≈≈ ≈≈ 🐟`。
- 最终真口咬下时，切入咬钩阶段，显示为 `水面翻腾 ❗`（青色高亮），波形显示为 `≈!≈ ≈!≈ ⚡ ≈!≈ ≈!≈ ⚡`。

---

## 3. 核心逻辑实现 (C#)

```csharp
// FishingWaitCalculator.cs 等口计算实现
public static double ComputeWaitSeconds(
    FishingLoadout loadout,
    CatFishingStats catStats,
    CatFishingBuff catBuff,
    FishingSpot spot)
{
    // 所有地图的平均期望时间完全一致（统一 5 分钟）
    double baseWait = 300.0;

    // 装备与饵缩减加成
    double reduction = loadout.EffectiveAttraction * 0.5
        + loadout.FishingLevel * 0.003
        + loadout.CastRange * 0.02
        + Math.Max(0, loadout.RodGearTier - 1) * 0.015
        + Math.Max(0, loadout.LureGearTier - 1) * 0.01;
    reduction = Math.Min(MaxGearReduction, reduction);

    double factor = 1.0 - reduction;
    factor *= catStats.WaitMultiplier;
    factor *= catBuff.WaitTimeMultiplier * catBuff.WaitTimePenalty;

    double expectedWait = baseWait * factor;

    // 命运扭曲系数
    double levelFactor = (Math.Clamp(spot.RequiredLevel, 1, 60) - 1) / 59.0;
    double power = Math.Clamp(1.5 - levelFactor, 0.5, 1.5); 

    // 扭曲随机数
    var rand = new Random();
    double u = 1.0 - rand.NextDouble();
    double distortedU = Math.Pow(u, power);

    // 泊松指数公式
    double exponentialRandom = -Math.Log(distortedU);
    double finalWait = expectedWait * exponentialRandom;

    // 终极限幅
    double minWait = 120.0 - (levelFactor * 90.0);
    double maxWait = 900.0 + (levelFactor * 600.0);

    return Math.Clamp(finalWait, minWait, maxWait);
}
```
