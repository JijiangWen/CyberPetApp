# 第 9 章：现代 C# 高级特性实战 🛠️

在开发 **CyberPetApp** 的过程中，我们充分利用了现代 C#（C# 9.0、10.0 及 11.0）引入的一些极其强大的高级语法糖和设计模式。这不仅让我们的代码变得极其简练、美白，还从编译器层面避免了各种逻辑漏洞。

---

### 9.1 `record` 与 `init` 属性

在传统开发中，如果我们要传递一些纯粹的“只读数据”或“数据传输对象（DTO）”，我们需要声明一个类，并手写繁琐的 `Equals`、`GetHashCode` 和 `ToString` 重写方法，非常累赘。

C# 引入了 **`record`（记录）** 类型，用于快速声明一个不可变的数据对象。它有以下几大天然特性：
1. **天然的不可变性（Immutability）**：属性一旦实例化，外部不能再次修改。
2. **值相等性（Value Equality）**：如果两个不同的 record 实例，它们的所有字段值都相同，那么用 `==` 判断时，它们的值就是相等的（而普通的 Class 比较的是内存地址指针）。
3. **格式化 ToString**：会自动输出清晰可读的属性键值 JSON 样格式文本。

```csharp
// 一行代码声明一个炼金结果 record
public record AlchemyResult(bool Success, string Message, int CatLevelUps = 0);
```

#### `init` 关键字：
`init` 允许我们将属性声明为“仅在初始化时可写”。属性一旦在 `new` 对象的大括号中被赋初值，后续任何人就无法再修改它，直接从编译器层面防止了数据被篡改：

```csharp
public class FishingLogEntry
{
    // 初始化后就不可更改的时间戳和文本
    public DateTime Time { get; init; } = DateTime.Now;
    public string Text { get; init; } = "";
}

// 正常使用方式：
var log = new FishingLogEntry { Text = "钓到大青鱼！" };

// ❌ 错误示范（编译报错）：
log.Text = "改改文字"; // 编译器会报错：Init-only property can only be assigned in an object initializer.
```

---

### 9.2 模式匹配 (Pattern Matching) 与 Switch 表达式

在游戏开发中，我们有大量的分支映射计算（比如不同的打工状态，不同的消耗品类型计算对应的变化数值）。如果全写 `if-else` 或传统的 `switch-case`，代码会非常臃肿。

C# 的 **Switch 表达式** 能够以类似函数式编程的形式进行声明式的**模式匹配**：

```csharp
using CyberPetApp.Models;

public static class CatActivityCost
{
    // 根据猫咪正在进行的活动，返回对应的状态扣减 Delta 包
    public static Delta Get(CatActivityType type) => type switch
    {
        CatActivityType.FishingCycle => new Delta(-7, -10, +3, -5, 0),
        CatActivityType.WorkTick     => new Delta(0, -2, -1, -1, 0),
        CatActivityType.ExpeditionGo => new Delta(-15, 0, -5, -10, 0),
        CatActivityType.RestBed      => new Delta(0, +35, +15, 0, 0),
        _                            => default // 相当于 default:
    };
}
```

---

### 9.3 静态数据配置与解耦设计 (Catalog Pattern)

在游戏开发中，数值是频繁需要微调和优化的（例如提升某只鱼的卖出价格、调整钓鱼竿升级所需材料、或是修改钓点的等级限制）。如果把这些数值直接写死在核心业务代码里，每次改数值都需要去修改逻辑，非常危险且效率低下。

**CyberPetApp** 采用了**“数据-逻辑彻底分离”的 Catalog（静态目录配置）设计模式**。

我们将所有静态的游戏数据定义在 `Models` 层对应的 Catalog 类中，例如 [GearProgressionCatalog.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/GearProgressionCatalog.cs)：

```csharp
public static class GearProgressionCatalog
{
    // 声明所有可升级鱼竿的静态材料配方和金币需求，只读
    public static readonly Dictionary<string, GearCraftRecipe> GearCraftRecipes = new()
    {
        ["CarbonRod"] = new GearCraftRecipe(
            "碳素路亚竿", 
            GearCraftSlot.Rod,
            [new FishRequirement("镇外溪流", "*", FishRarity.Rare, 3)], 
            [new MaterialRequirement("碳素纤维", 5)],
            450, 
            3, 1),
        // ...
    };

    // 独立的数据查询方法
    public static GearCraftRecipe? FindCraftRecipe(string name) =>
        GearCraftRecipes.TryGetValue(name, out var recipe) ? recipe : null;
}
```

当 `AlchemyService` 的 `CraftGearAsync` 方法需要判定是否可以锻造时，它不需要存有任何数值，直接调用 `GearProgressionCatalog.FindCraftRecipe` 查表即可。

这种设计使得程序员可以非常清晰地修改游戏参数，甚至可以通过读取 JSON 文件来动态生成 Catalog，而核心逻辑代码不需要动一根手指头！
