# 第 8 章：游戏 UI 呈现与像素风雪碧图 (Sprites) 🎨

### 8.1 什么是 CSS 雪碧图（Sprite Sheets）

在 2D 像素风网页游戏中，界面往往需要渲染成百上千的小图标（如各种鱼获、拟饵、装备、素材等）。如果每个小图标都对应一个独立的 `.png` 图片文件，浏览器渲染网页时就需要向服务器发起成百上千次独立的 HTTP 请求。这不仅会严重霸占浏览器的并发请求信道，还会造成网络拥堵和严重的页面加载白屏、抖动。

**CSS 雪碧图（Sprite Sheet，也称精灵图）** 是一项经典的性能优化方案。我们将游戏里所有可能用到的几十、上百张小像素图标拼接、整合到一张巨大的整图（如 `fish-set.png`）中，浏览器在加载页面时只需要发起一次 HTTP 请求下载这张大图。

接着，我们利用 CSS 中的 `background-image` 和 `background-position`，把需要的部分裁剪展示出来，其他区域则被隐藏：

```
                ┌───────────────────────────────────┐
                │             fish-set.png          │
                │  ┌─────────┐  ┌─────────┐         │
                │  │ 🐠 (0,0) │  │ 🐟(20,0)│ ...     │
                │  │  Item 1 │  │  Item 2 │         │
                │  └─────────┘  └─────────┘         │
                └───────────────────────────────────┘
```

---

### 8.2 CSS 实现像素图剪切

我们在 [cyberpet-theme.css](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/wwwroot/cyberpet-theme.css) 中定义了雪碧图的复用规则：

```css
/* 声明公共精灵图基础样式 */
.fish-sprite {
    display: inline-block;
    width: 64px;
    height: 64px;
    background-image: url('/assets/fish-set.png?v=4');
    background-repeat: no-repeat;
    
    /* 关键属性：image-rendering: pixelated */
    /* 当小图在网页上被缩放拉伸时，浏览器默认会采用双线性过滤进行平滑处理，导致像素画变得模糊肮脏。 */
    /* 设为 pixelated 可以保持像素点对齐的锯齿硬度感，保证在任何缩放比例下都完美还原复古像素画的锐度。 */
    image-rendering: pixelated;
    image-rendering: crisp-edges;
}

/* 根据大图的宽高百分比定位到具体那只鱼的位置 */
.fish-01 { background-position: 0%    0%; }   /* 对应小丑鱼 */
.fish-02 { background-position: 20%   0%; }   /* 对应蓝唐王鱼 */
.fish-03 { background-position: 40%   0%; }   /* 对应大西洋鲑 */
.fish-04 { background-position: 60%   0%; }
.fish-05 { background-position: 80%   0%; }
```

---

### 8.3 SpriteCatalog 静态转换器

为了能让后端的 C# 数据模型方便地决定前端渲染哪一个 CSS 雪碧图类，我们设计了 [SpriteCatalog.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Models/SpriteCatalog.cs) 作为静态目录转换器。它充当了**后端实体名**与**前端 CSS 类名**之间的翻译桥梁：

```csharp
using CyberPetApp.Models;

namespace CyberPetApp.Models;

public static class SpriteCatalog
{
    // 将鱼种的名称和稀有度静态映射到具体的 CSS 样式类上
    public static string Fish(string fishName, FishRarity rarity) => fishName switch
    {
        "溪边小白条" => "fish-01",
        "土麦穗鱼"   => "fish-02",
        "野花翅子(虹鳟)" => "fish-03",
        "大鳍红马口(溪哥)" => "fish-04",
        "金背鲤仙"   => "fish-05",
        "雾海鲈鱼"   => "fish-06",
        _           => "fish-01" // 找不到时的备用默认类
    };

    // 同样映射装备
    public static string Rod(string rodName) => rodName switch
    {
        "新手竹竿" => "rod-bamboo",
        "碳素路亚竿" => "rod-carbon",
        "黄金龙纹竿" => "rod-golden",
        _ => "rod-default"
    };
}
```

---

### 8.4 Razor 视图层动态渲染

在 Razor 文件中，我们再也不需要在 HTML 结构里写死图片路径，直接通过表达式动态拼接 CSS Class，即可瞬间展示正确的图片：

```razor
@using CyberPetApp.Models

<div class="fish-card">
    <!-- 动态拼接 fish-sprite 以及从 SpriteCatalog 算出来的定位样式 class -->
    <i class="fish-sprite @SpriteCatalog.Fish(item.Name, item.Rarity)"></i>
    <h4>@item.Name</h4>
    <p>重量: @item.ActualWeight kg</p>
</div>
```
通过这种“CSS雪碧图定位 + C# 静态目录路由”的设计，我们不仅为服务器极大地减轻了 HTTP 并发负荷，还在前端展现了纯正、干净、高性能的复古像素画面！
