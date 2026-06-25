# 第 10 章：房屋、房间与家具被动养成系统 🏠

### 10.1 房屋与房间系统结构

为了给赛博猫咪提供更好的生活环境，并让玩家能够通过消耗游戏币（Gold Sink）来换取长期的挂机与钓鱼被动加成，CyberPetApp 引入了**房屋与房间养成系统**。

系统主要由三个核心实体模型构成（定义在 [PlayerHouse.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/PlayerHouse.cs) 与 [Room.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/Room.cs) 中）：
* **`PlayerHouse`（房屋主类）**：对应玩家所拥有的房子，包含房屋等级和房间列表。通过 `RoomKeys` 定义了 2x2 的平面图网格布局：
  ```
  ┌─────────────┬─────────────┐
  │   客厅      │   厨房      │
  │ (Living)    │  (Kitchen)  │
  ├─────────────┼─────────────┤
  │   卧室      │   卫生间    │
  │  (Bedroom)  │ (Bathroom)  │
  └─────────────┴─────────────┘
  │         花园 (Garden)       │
  └───────────────────────────┘
  ```
* **`Room`（房间类）**：代表房子内的物理空间。包含解锁状态 `IsUnlocked`、解锁价格 `UnlockPrice` 以及当前放置在其中的家具列表 `Furniture`。其中“客厅”默认自动解锁，其余房间需金币手动解锁。
* **`Furniture`（家具类）**：代表房间内的摆件。包含购买价格、解锁状态及升级等级（每次升级加成会提高，但维护费会随之上升）。

---

### 10.2 家具系统与 Configuration-State 分离设计

为了避免在关系数据库中冗余地存储大量的静态加成定义（如加成的百分比、描述说明文本），系统采用了**“配置与状态分离”**的设计模式。

* **数据库仅存储运行状态**：[Furniture.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/PlayerHouse.cs) 实体在数据库表中只包含 `IsUnlocked`、`UpgradeLevel`、`RoomKey` 和 `FurnitureKey` 字段。
* **内存中只读配置类 `FurnitureCatalog`**：我们使用 Catalog 静态类维护每一个家具的加成系数、价格和解锁说明：
  ```csharp
  // Sofa 只存在于 Living 房间，提供 CatEnergyDecay（精力消耗衰减）减免
  ["Sofa"] = new FurnitureSpec(
      "赛博懒人沙发", 
      FurnitureBonusType.EnergyDecayReduction, 
      FurnitureBonusCategory.CatEnergyDecay, 
      0.20, // 基础加成 20%
      "精力活动消耗 -20%", 
      "解锁客厅后可购买")
  ```

#### 💡 核心家具加成一览：
1. **客厅 (LivingRoom)**：
   * `Sofa`（赛博懒人沙发）：精力消耗衰减，每次钓鱼/打工精力消耗扣得更少。
   * `CatToy`（电动逗猫棒）：猫咪快乐度被动自然恢复速率提升。
   * `TV`（老旧大头电视）：猫咪出门打工所得金币收益 +10%。
2. **厨房 (Kitchen)**：
   * `Stove`（老旧烤箱）：进行烹饪时获得的经验（Cooking XP）+25%。
   * `AutoFeederUnit`（智能喂食器）：开启自动喂食，猫咪饿了会自动跑去厨房吃饲料。
3. **卧室 (Bedroom)**：
   * `Bed`（老旧双人床）：睡觉时精力恢复速率 +50%。
   * `CozyBed`（恒温猫窝）：猫咪专用，精力极低时猫咪自动去睡，且提供缓慢的生命回复。

---

### 10.3 被动加成聚合算法 (`HouseBuffAggregator`)

玩家可以购买和升级不同房间的多个家具，系统在心跳 Tick 中每次计算猫咪状态或打工所得时，会通过 `HouseBuffAggregator` 将所有属性加成合并，算出一个当前玩家的只读属性快照 `HouseBuffs`。

为了防止玩家堆叠过多同类家具破坏数值平衡，聚合算法遵循以下严谨步骤：
1. **同品类去重（Category Flattening）**：
   如果属于同一个 `FurnitureBonusCategory`（例如，客厅的“沙发”和卧室的“吊椅”都提供“精力活动消耗减免”），**仅保留加成数值最高的一项**，不叠加生效。
2. **硬上限控制（Hard Cap）**：
   在汇总后，任何被动加成值不得超过设定的硬上限（例如所有的消耗减免最多不得超过 35%）。
3. **里程碑放大系数（Milestone Scale）**：
   如果激活了特定里程碑（如“金杯展架”），会通过 `WithMilestoneScale` 对所有被动加成进行等比缩放放大，实现不同系统之间的协同加持。

---

### 10.4 房屋自动照顾系统 (`HouseAutoCare`)

当玩家离线或者在海里疯狂钓鱼打工时，猫咪的饱食度、水分会逐渐衰减。解锁并放置了“智能喂食器”或“宠物饮水泉”后，系统将在游戏循环 Tick 中开启**自动照顾**功能。

前端 UI 组件 [HouseRoomPanel.razor](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/HouseRoomPanel.razor) 显示设备的水位/食槽状态。在后台：
* **自动补水**：当猫咪口渴值（Thirst）低于 600 点且饮水泉中仍有蓄水时，系统自动扣减蓄水量，并给猫咪回复口渴值。
* **自动喂食**：当猫咪饥饿值（Hunger）低于 600 点且智能喂食器内装有烹饪好的饲料时，系统自动消耗 1 份食物并恢复猫咪的饱食度。

---

### 10.5 数据库持久化升级交互 (`HouseService`)

玩家升级、解锁家具需要调用 [HouseService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/HouseService.cs) 方法，以保障多线程并发下的事务安全：

```csharp
public async Task<(bool Ok, string Message)> UpgradeFurnitureAsync(Player player, Furniture furniture)
{
    // 1. 采用 conditional lock 进行加锁
    var dbPlayer = await _context.Players.FindAsync(player.Id);
    if (dbPlayer is null) return (false, "玩家不存在");

    // 2. 根据公式计算升级所需金币
    int cost = EconomySinks.FurnitureUpgradeCost(furniture);
    if (dbPlayer.Money < cost) return (false, $"金币不足，升级需要 {cost}g");

    var dbFurn = await _context.Furnitures
        .FirstOrDefaultAsync(f => f.PlayerId == player.Id && f.FurnitureKey == furniture.FurnitureKey);
    if (dbFurn is null) return (false, "家具不存在");

    // 3. 扣钱并提升等级
    dbPlayer.Money -= cost;
    dbFurn.UpgradeLevel++;

    // 4. 保存数据库，由于是 Scoped 的 DbContext，保存会直接提交更改
    await _context.SaveChangesAsync();
    
    // 5. 同步内存中的 player 实体的金币
    player.Money = dbPlayer.Money;
    furniture.UpgradeLevel = dbFurn.UpgradeLevel;

    return (true, $"升级成功！扣除 {cost}g，当前等级 Lv.{furniture.UpgradeLevel}");
}
```
通过本服务，家具升级后加成得到提升，同时为了保持经济循环平衡，家具升级后在每日凌晨结算时会根据等级稍微增加几枚金币作为“日常房屋维护费”，形成完整的收支玩法环闭。
