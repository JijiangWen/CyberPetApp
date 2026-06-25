# 第 12 章：装备成长、磨损公式与商店乐观 UI 🛠️

在 CyberPetApp 中，玩家想前往更高等级的钓点钓起凶猛的深海鱼，就必须不断更换、锻造和升级他们的钓鱼装备。这套系统涉及四件套装备属性、磨损计算、阶梯式维修算法以及页面渲染必不可少的 **“乐观 UI”设计模式**。

---

### 12.1 四件套装备与核心属性设计

玩家的装备槽（Loadout）由以下四种核心部件组成：
1. **鱼竿（Fishing Rod）**：决定**感应度（Sensitivity）**（影响抓口成功率）和**施法范围（Cast Range）**（缩短等口时间）。
2. **渔轮（Fishing Reel）**：决定**卸力（Drag Power）**（降低遛鱼拉力惩罚）和**齿轮比（Gear Ratio）**（缩短遛鱼遛鱼时间）。
3. **鱼线（Fishing Line）**：决定**拉力强度（Line Strength）**（决定起鱼体重上限）、**隐蔽性（Line Stealth）**（削减鱼儿警惕性）和**耐磨性（Abrasion Resistance）**（减免断线耐久扣减）。
4. **拟饵（Fishing Lure）**：提供**吸引力（Attraction）**（极大缩短等口时长）和**稀有度加成（Rarity Bonus）**（提高高等级鱼爆率）。

每一件装备都拥有**当前耐久度（Durability）**，满值为 100。

> [!WARNING]
> **低耐久性能腰斩惩罚**：
> 当任何一件装备的耐久度跌破 **30（DurabilityLowThreshold）** 时，系统会判定装备进入损毁边缘。该装备在钓鱼计算中的核心数值（如鱼竿感应度、渔轮卸力）会**瞬间被乘以 0.5（DurabilityLowMultiplier）直接腰斩**！这会导致起鱼概率暴跌。玩家必须及时回到城镇铁匠铺进行修理。

---

### 12.2 装备磨损与损耗数学公式

每一次钓鱼完成（无论是钓起鱼、脱钩溜走还是断线爆轮），系统都会调用 `WearEquippedGearAsync` 计算并扣减当前装备的耐久值：

#### 1. 基础磨损量生成：
每一次抛竿的基础磨损随机为 $1 \sim 3$ 点，在此基础上叠加上**钓点额外磨损（SpotExtraWear）**：
$$\text{wear} = \text{Random}(1, 3) + \text{SpotExtraWear}$$
*例如在“深渊回廊”等超深水钓点，水流湍急，每次起竿会额外增加 $2 \sim 4$ 点高额磨损。*

#### 2. 装备词缀加成（Gear Affix）：
不同鱼竿和卷线器具备不同的词缀，会影响耐久扣减倍率。通过 `GearAffixHelper.WearMultiplier` 计算：
* **耐用词缀（Durable）**：磨损乘数 $\times 0.65$。
* **脆弱词缀（Fragile）**：磨损乘数 $\times 1.50$。
* **最终磨损计算**：
  $$\text{wear}_{\text{final}} = \lceil\text{wear} \times \text{AffixMultiplier}\rceil$$

#### 3. 断线惩罚（Line Break）：
如果大物力量过强导致断线跑鱼，鱼线耐久度会受到重创：
$$\text{wear}_{\text{Line}} = \text{Max}(1, \; \text{wear} \times (1 - \text{AbrasionResistance}))$$
同时，鱼钩上的拟饵耐久直接 $-1$（归零时消耗背包内库存拟饵数量，若无备用拟饵则彻底失去拟饵加成）。

#### 4. 脱钩优待：
如果是普通的鱼咬钩后因为扬竿抓口失败而“脱钩”，系统有 $50\%$ 的概率触发**完全免除鱼竿和渔轮磨损**的优待。

---

### 12.3 阶梯式金币修理费算法

在 [EconomySinks.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/EconomySinks.cs) 中，修理费并非一成不变，而是根据**装备的档位（Tier）**和**装备强度上限**进行双重阶梯式计算：

#### 1. 单次微量修理（Partial Repair）：
每次只修理 20 点耐久，花费固定金币（随 Tier 递增）：
* Tier 1~3：30g
* Tier 6：85g
* Tier 9+：180g

#### 2. 全额一键修理（Full Repair）：
一键将耐久拉回 100。费用取决于最大强度和阶位倍率，并实行封顶保障：
$$\text{baseCost} = \text{Max}(50, \; \text{MaxStrengthOrDrag} \times 10)$$
$$\text{fullCost} = \lceil\text{baseCost} \times \text{TierMultiplier}\rceil$$
* **阶位乘数（TierMultiplier）**：T1 为 1.0，T7 为 1.9，T9+ 封顶 2.8。
* **限幅保护**：$\text{fullCost} = \text{Min}(800, \; \text{fullCost})$（全修封顶 800g，防止后期极品神兵的修理费高到玩家破产）。
* **实际按损耗折算**：
  $$\text{cost}_{\text{pay}} = \lceil\text{fullCost} \times \frac{100.0 - \text{CurrentDurability}}{100.0}\rceil$$

---

### 12.4 商店“乐观 UI”（Optimistic UI）设计模式

在传统的网页登录或购买中，用户点击“购买”按钮后，网页会显示菊花加载图，等待网络请求发往后端、后端写入数据库、数据库返回成功、前端才更新金币和背包。这种模式对于频繁交互的游戏来说，体感极差（有延迟卡顿感）。

**乐观 UI 模式（Optimistic UI）** 颠覆了这一点：
当玩家点击购买时，前端**直接假设服务器必然会成功**，立即在内存中扣除金币、塞入装备并刷新页面。然后静默地在后台发起异步数据库写库请求。如果由于网络崩溃等极端情况导致后台报错，前端再默默执行 **“回滚（Rollback）”**。

在 [EquipmentService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/EquipmentService.cs) 中，我们手写了完整的乐观 UI 交互链：

```csharp
// 1. 准备校验 (无状态纯计算，在 UI 线程瞬间执行完成)
if (EquipmentService.TryPrepareBuyRod(player, myRods, spec, catLevel, dex, licenseCheck, out var newRod, out var error))
{
    // 2. 乐观执行 (立即扣钱，添加新鱼竿，重新渲染页面，玩家体感 0 延迟)
    EquipmentService.ApplyBuyRodOptimistic(player, myRods, spec, newRod);
    StateHasChanged();

    // 3. 后台异步 Commit 数据库
    _ = Task.Run(async () =>
    {
        try
        {
            // 通过 dbLock 独占锁将更改保存到 Postgres 数据库
            var commitResult = await WithDbLock(async () =>
            {
                return await _equipmentService.CommitBuyRodAsync(player, spec, newRod);
            });

            if (!commitResult.Ok) throw new Exception(commitResult.Message);
        }
        catch (Exception ex)
        {
            // 4. 出现错误，默默回滚，退回金币，将装备从内存背包剔除，重新刷新 UI
            EquipmentService.RollbackBuyRod(player, myRods, spec, newRod);
            Logger.LogError(ex, "后台保存数据库失败，已回滚乐观 UI");
            await InvokeAsync(StateHasChanged);
        }
    });
}
```

通过这套“准备-乐观更新-后台异步 Commit-异常回滚”的优雅闭环，游戏既获得了媲美本地单机游戏的零延迟响应体感，又保证了最终数据在 PostgreSQL 数据库中的绝对准确性！
