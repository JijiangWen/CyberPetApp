# 第 2 章：Razor 组件的数据流与组件拆分 🧱

在大型 Blazor 项目中，如果将全部的游戏 UI 结构和业务交互都塞进一个单页面文件（如 `Home.razor`），代码量很快就会破千甚至破万行，导致极难维护与协作开发。因此，我们需要将整个 UI 界面合理拆分为一系列低耦合、高内聚的子组件。

---

### 2.1 父子组件的数据流传递

Blazor 组件之间的通信遵循经典模式：**属性下行（Parameters Down），事件上行（Events Up）**。

* **属性下行（数据流入）**：父组件将状态数据通过 HTML 属性的形式传递给子组件。子组件定义带有 `[Parameter]` 特性的 C# 属性来接收它们。
* **事件上行（行为上报）**：子组件本身不应当有权限直接修改数据库或全局状态。当子组件中发生用户操作（如点击购买、喂食）时，它应当通过 `EventCallback` 触发一个回调事件，“上报”给父组件，交由持有数据源的父组件统一修改状态。

```
                    ┌──────────────────────────┐
                    │        Home.razor        │ (父组件：持有玩家金币、背包和猫咪状态数据)
                    └──────┬────────────▲──────┘
                           │            │ OnFeed="FeedFood"
               Cat="cat"    │            │ (事件上行：上报喂食动作，扣钱并加饱食度)
                           ▼            │
                    ┌──────────────────────────┐
                    │   CatCareSidebar.razor   │ (子组件：展示猫咪当前的维生指标并提供按钮)
                    └──────────────────────────┘
```

---

### 2.2 实战拆解：猫咪属性条 CatVitalsPanel

[CatVitalsPanel.razor](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/CatVitalsPanel.razor) 负责呈现猫咪的五维指标（饱食度、精力、快乐度、水分、健康度）。它不需要自己查询数据库，数据由父页面直接灌入：

```razor
@using CyberPetApp.Models

<div class="vital-bar">
    <span class="vital-label">饥饿度</span>
    <div class="bar-container">
        @* 调用 C# 函数 FillStyle 动态计算宽度与颜色梯度 *@
        <div class="bar-fill" style="@FillStyle(Cat.Hunger)"></div>
    </div>
    <span class="vital-value">@Cat.Hunger / 1000</span>
</div>

@code {
    // 声明接收的父组件参数
    // EditorRequired 特性用于编译期校验，如果父组件在调用时漏传了 Cat，编译会直接报错
    [Parameter, EditorRequired] 
    public CyberCat Cat { get; set; } = default!;

    // 动态计算进度条样式的辅助函数
    private static string FillStyle(int value)
    {
        // 限制百分比在 0-100 之间
        double pct = Math.Clamp(value * 100.0 / CyberCat.StatMax, 0, 100);
        
        // HSL 颜色空间：0度为红色（状态危险），120度为绿色（状态健康）
        int hue = (int)Math.Round(pct * 1.2); 
        
        return FormattableString.Invariant(
            $"width:{pct:0.#}%;background:linear-gradient(90deg,hsl({hue},72%,38%),hsl({hue},82%,52%))");
    }
}
```

---

### 2.3 实战拆解：操作面板 CatCareSidebar

子组件 [CatCareSidebar.razor](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/CatCareSidebar.razor) 不负责保存玩家的背包和金币数据。当玩家在侧边栏点击“喂食”按钮时，它使用 `EventCallback` 将动作上报给父组件处理：

```razor
<button class="action-btn" @onclick="() => OnFeed.InvokeAsync(normalFood)">
    喂食普通猫粮
</button>

@code {
    // 声明接收的普通猫粮实体数据，作为辅助参数
    private Food normalFood = new Food("普通猫粮", 35, 10, 5, 10);

    // 暴露事件回调属性，泛型参数代表事件触发时向父组件传递的数据类型
    [Parameter] 
    public EventCallback<Food> OnFeed { get; set; }
}
```

而在父页面 `Home.razor` 中，它是这样调用子组件并绑定自身的方法的：
```razor
<!-- 声明子组件，并将父页面的 FeedShopFood 方法绑定到子组件的 OnFeed 回调上 -->
<CatCareSidebar OnFeed="FeedShopFood" />
```
当子组件中的按钮被点击时，父组件的 `FeedShopFood` 方法会被调用，从而实现了**状态持有者（父组件）**与**表现层（子组件）**的完美关注点分离。

---

### 2.4 局部渲染优化与 partial 拆分

在 C# 语言中，**`partial`（部分类）** 允许我们将一个大类的定义拆分到多个物理文件中，编译器在编译时会自动把它们合并成一个完整的类。

在 Blazor 中，我们可以将一个 `.razor` 页面或组件拆分为：
1. **HTML 结构部分**：在 `Home.razor` 里只写网页排版标签。
2. **C# 逻辑部分**：创建 `Home.razor.cs` 文件，声明为 `public partial class Home`，用来写变量和方法。

进一步地，为了避免逻辑文件膨胀，我们可以按功能模块继续细化拆分：
* `Home.razor`：主页面的 HTML 骨架。
* `Home.razor.cs`：主类的核心属性、状态及初始化代码。
* `Home.GameLoop.cs`：定时器驱动的游戏心跳循环、数值衰减与离线补偿。
* `Home.Fishing.cs`：钓鱼相关事件监听与收竿结果处理。
* `Home.Gear.cs`：装备更新、升级、磨损与宝石镶嵌。

这种拆分方式大大提高了代码的清晰度，使得开发团队在协作时能各司其职，有效减少了 Git 代码冲突的几率。
