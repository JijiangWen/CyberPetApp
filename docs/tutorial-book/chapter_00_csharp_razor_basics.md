# 第 0 章：C# 与 Razor 基础语法超详细入门 🚀

为了帮助编程零基础或从其他开发语言（如 JavaScript、Python 或 Go）转过来的开发者能够无缝理解 **CyberPetApp** 的源码，本章将从最基础的 C# 语法和 Blazor 专属的 Razor 模板引擎语法讲起，配合易懂的实际游戏开发例子，带你快速通关！

---

## 0.1 零基础认识 C# 核心语法

C#（读作 C-Sharp）是由微软开发的一种面向对象、强类型的现代编程语言。在 **CyberPetApp** 中，我们使用 C# 来编写游戏逻辑、管理玩家属性、进行多线程控制以及通过 Entity Framework Core 读写数据库。

### 1. 变量与常用数据类型
变量就像是一个贴了标签的“收纳盒”，用来存放游戏运行中的数据。C# 规定每个收纳盒在声明时必须指定特定的数据类型。强类型语言的好处是，编译器能在你写错类型时立即报错，杜绝运行期隐患。

| 数据类型 | 关键字 | 说明 | 游戏中的实际使用例子 |
| :--- | :--- | :--- | :--- |
| **整型** | `int` | 存放整数，如金币、饥饿值、等级。 | `int gold = 100;` |
| **双精度浮点型** | `double` | 存放高精度小数，如经验倍率、百分比加成。 | `double xpMultiplier = 1.25;` |
| **字符串** | `string` | 存放文本，必须用双引号 `""` 包裹。 | `string catName = "赛博猫咪";` |
| **布尔值** | `bool` | 只有真 `true` 或假 `false` 两个状态。 | `bool isUnlocked = false;` |
| **时间日期**| `DateTime`| 记录世界协调时（UTC）的时间。 | `DateTime lastActive = DateTime.UtcNow;` |
| **全局唯一标识**| `Guid` | 自动生成 128 位的不重复超长 ID，作为玩家唯一主键。 | `Guid playerId = Guid.NewGuid();` |

#### 变量声明示例：
```csharp
// 显式声明类型
int playerGold = 250;
string playerName = "路人甲";

// 隐式声明类型（编译器通过右侧的初始值自动推断类型）
var currentEnergy = 800; // 自动推断为 int
var speedRatio = 1.15;   // 自动推断为 double
```

---

### 2. 条件分支：让游戏产生“逻辑选择”
游戏必须能够根据玩家当前的状态做出判断。例如，玩家的钱够不够买特定猫粮？猫咪的精力是否已归零不能出门？
C# 中最常用的条件分支是 `if-else`：

```csharp
int playerMoney = 150;
int foodPrice = 200;

if (playerMoney >= foodPrice)
{
    // 如果括号内的布尔表达式为 true，执行这里的代码
    playerMoney -= foodPrice;
    Console.WriteLine("购买成功！剩余金币：" + playerMoney);
}
else
{
    // 如果为 false，执行这里的代码
    Console.WriteLine("金币不足，无法购买猫粮！");
}
```

---

### 3. 循环控制：重复执行的艺术
我们需要在游戏里遍历玩家背包里的所有鱼获，或者批量处理数据：
* **`foreach` 循环**（最常用）：用来依次取出集合（如列表 `List` 或字典 `Dictionary`）中的每一个元素。

```csharp
// 声明一个字符串列表，存放背包里的鱼
List<string> backpack = new List<string> { "小丑鱼", "蓝唐王鱼", "金枪鱼" };

// 现代 C# 简写形式：List<string> backpack = ["小丑鱼", "蓝唐王鱼", "金枪鱼"];

foreach (string fish in backpack)
{
    // 每次循环，变量 fish 会自动代表列表中的下一条鱼
    Console.WriteLine("背包里有一条：" + fish);
}
```

---

### 4. 类与对象（面向对象编程基础）
C# 是面向对象的语言。我们可以把“类（Class）”理解为**图纸/模板**，而“对象（Object）”是根据图纸生产出来的**实体**。

例如，我们设计一个简化的猫咪类：
```csharp
// 1. 定义猫咪模板
public class CyberCat
{
    // 属性 (Property)：猫咪的特征
    public string Name { get; set; }
    public int Hunger { get; set; } = 1000; // 默认满值 1000

    // 构造函数：创建实体时初始化属性
    public CyberCat(string name)
    {
        Name = name;
    }

    // 方法 (Method)：猫咪的行为
    public void Eat(int foodAmount)
    {
        Hunger += foodAmount;
        if (Hunger > 1000) Hunger = 1000; // 上限控制
        Console.WriteLine(Name + " 吃了猫粮，饥饿度恢复到：" + Hunger);
    }
}

// 2. 根据模板创造出真实的猫咪实体并操作它
CyberCat myCat = new CyberCat("小芝麻"); // 实例化一个对象
myCat.Eat(200); // 调用对象的方法，让小芝麻恢复 200 点饥饿度
```

---

## 0.2 Razor 模板引擎与 C# 混合编写

在 Blazor 框架中，前端页面和交互组件使用的是 **`.razor`** 后缀的文件。这种文件允许我们将 **HTML（网页结构）** 和 **C#（页面逻辑）** 写在同一个文件里。实现这个神奇魔法的桥梁，就是 **`@`** 符号。

### 1. 属性绑定与动态 CSS（将数据渲染到网页）
我们可以使用 `@` 符号把 C# 变量的值渲染到 HTML 标签中，甚至动态修改标签的 CSS 样式：
```razor
@* Razor 中的注释格式 *@
@{
    // 在 @{ ... } 代码块中可以直接写普通的 C# 声明与逻辑
    string statusColor = "color: green;";
    int currentHunger = 450;
}

<!-- 使用 @ 符号将变量值动态塞入 HTML 属性或标签正文 -->
<p style="@statusColor">当前猫咪饱食度为：@currentHunger</p>
```

---

### 2. 条件渲染 `@if`（动态展示网页元素）
有些网页元素需要满足特定条件才显示。例如，当房间被锁定时显示“购买解锁”按钮，解锁后直接显示房间内容：
```razor
@if (isRoomLocked)
{
    <div class="locked-tip">
        <p>此房间未解锁，解锁需要花费 200g！</p>
        <button @onclick="Unlock">花费金币解锁</button>
    </div>
}
else
{
    <div class="room-content">
        <p>欢迎来到厨房！这里有老旧冰箱和烤箱。</p>
    </div>
}

@code {
    // @code 块用来写页面交互的 C# 属性、字段和方法
    private bool isRoomLocked = true;

    private void Unlock()
    {
        isRoomLocked = false; // 点击按钮后改变变量值，UI 会自动重新刷新！
    }
}
```

---

### 3. 循环渲染 `@foreach`（批量生成网页卡片）
在游戏中展示背包道具或商店列表时，我们不需要重复手写 HTML，直接用循环渲染：
```razor
<div class="backpack-grid">
    @foreach (var item in backpackItems)
    {
        <div class="item-card">
            <h4>@item.Name</h4>
            <span>数量：@item.Count</span>
        </div>
    }
</div>

@code {
    // 模拟背包数据
    private class Item { public string Name { get; set; } = ""; public int Count { get; set; } }
    private List<Item> backpackItems = new()
    {
        new Item { Name = "普通猫粮", Count = 5 },
        new Item { Name = "稀有钓饵", Count = 2 }
    };
}
```

---

### 4. 事件绑定与双向绑定
* **事件绑定 `@onclick`**：当用户点击按钮时，执行指定的 C# 方法。
* **双向数据绑定 `@bind`**：常用于输入框。当玩家修改输入框的文本时，绑定的 C# 变量**同步更新**；反之，若 C# 变量在后台改变，输入框内容也会**自动随之改变**。

```razor
<!-- 双向绑定：将输入框和 catName 变量绑定 -->
<input @bind="catName" placeholder="请输入猫咪新名字" />

<!-- 事件绑定：点击按钮触发 ChangeName 方法 -->
<button @onclick="ChangeName">确定修改</button>

<p>修改后的名字是：@displayName</p>

@code {
    private string catName = "";
    private string displayName = "未命名";

    private void ChangeName()
    {
        if (!string.IsNullOrWhiteSpace(catName))
        {
            displayName = catName;
        }
    }
}
```

掌握了本章的 C# 和 Razor 基础语法后，你就可以轻松地继续向下阅读，理解 CyberPetApp 中关于生命周期、数据库持久化和多线程并发控制等高阶章节的代码了！
