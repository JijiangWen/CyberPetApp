# 第 7 章：Cookie 认证与 Minimal API 混合架构 🔑

### 7.1 为什么 Blazor Server 写入 Cookie 需要 Minimal API？

在传统的 Web 安全认证设计中，服务器通过在 HTTP 响应头（Response Headers）中写入 `Set-Cookie` 指令，从而将加密后的用户会话凭证（如 Session ID 或 JWT）保存在用户的浏览器 Cookie 中。

但是在 **Blazor Server** 交互模式下，我们面临一个重大底层技术限制：
* **WebSocket 通道限制**：一旦 Blazor 初始化握手完成，浏览器与服务端的所有后续通信全部基于唯一的 **WebSocket (SignalR)** 长连接运行。
* **没有 HTTP 响应流**：在 `.razor` 组件中，玩家点击“登录”按钮触发的 C# 方法在服务端被调用，其结果是以二进制 DOM 差异包的形式通过 WebSocket 送回浏览器的。在整个执行流中，根本没有标准的 HTTP Response 对象，因此**无法在 Blazor 按钮事件中直接写入 Cookie**。

#### 💡 混合架构解决方案：
为了解决此底层限制，CyberPetApp 采用了“Blazor 渲染 + Minimal API 端点提交”的**混合安全架构**：
1. 登录与注册页面作为 Blazor 组件渲染在前端，但里面的 `<form>` 表单提交（Post）动作指向一个标准的 **Minimal API** 路由。
2. Minimal API 是标准的 HTTP 控制器，它负责处理表单、校验密码、在标准的 HTTP Response 中签发 Cookie。
3. 签发完成后，Minimal API 将用户重定向（Redirect）回 Blazor 游戏大厅页面。此时 WebSocket 重新握手建立，安全凭证已被浏览器妥善保存。

```
 ┌─────────────────┐        1. Submit Post Form       ┌──────────────────────┐
 │  浏览器 (Client) ├────────────────────────────────>│ Minimal API Endpoint │
 │                 │                                  │ (AuthEndpoints.cs)   │
 │                 │        2. Set-Cookie Header      ├──────────────────────┤
 │                 │<─────────────────────────────────┤   [Sign-In Cookie]   │
 │  3. Reconnect   │                                  └──────────────────────┘
 │   (WebSocket)   │ 4. Read Claims (Authorize OK)    ┌──────────────────────┐
 │                 ├─────────────────────────────────>│    Blazor Server     │
 └─────────────────┘                                  └──────────────────────┘
```

---

### 7.2 Cookie 认证配置与 ClaimsPrincipal

#### 1. 中间件服务注册
在 [Program.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Program.cs) 中启用 Cookie 认证和状态授权中间件：
```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // 未授权时自动重定向到登录页
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // 记住登录30天
    });

builder.Services.AddAuthorization();
```

#### 2. 在 Minimal API 中签发 Cookie
分析 [AuthEndpoints.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/AuthEndpoints.cs)：

```csharp
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // 映射登录提交路由，接收 FromForm 表单提交数据
        app.MapPost("/api/auth/login", async (
            [FromForm] string username, 
            [FromForm] string password, 
            AuthService auth) =>
        {
            var result = await auth.LoginAsync(username, password);
            if (!result.Success)
            {
                // 登录失败，携带错误参数跳回登录视图
                return Results.Redirect("/login?error=" + Uri.EscapeDataString(result.Error!));
            }

            // 登录成功，跳转到游戏大厅，此时 Cookie 已经通过 AuthService 写入
            return Results.Redirect("/");
        }).DisableAntiforgery(); // 根据实际配置决定是否关闭防伪标记
    }
}
```

在 [AuthService.cs](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Services/AuthService.cs) 中，我们利用安全声明（Claims）来签发凭证：
```csharp
private async Task SignInAsync(Guid playerId, string username)
{
    var httpContext = _httpContextAccessor.HttpContext;

    // 1. 声明用户的主键 ID 和名字 (Claims)
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, playerId.ToString()),
        new(ClaimTypes.Name, username)
    };

    // 2. 绑定 Cookie 认证方案
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    // 3. 异步写入加密 Cookie 并推送到浏览器
    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal,
        new AuthenticationProperties 
        { 
            IsPersistent = true, 
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) 
        });
}
```

---

### 7.3 权限防护与路由拦截

一旦浏览器中保存了凭证 Cookie，每次连接 Blazor 页面时，安全组件就会自动提取 Cookie 中的 Claims。

在客户端，[Routes.razor](file:///c:/Users/wen.jijiang/Desktop/blazor_test/CyberPetApp/Components/Routes.razor) 起到了“安全安检大门”的作用：

```razor
@* 级联认证状态组件：将当前登录的用户身份 (AuthenticationState) 级联传递给其下的所有子组件 *@
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly">
        <Found Context="routeData">
            @* 授权路由视图：若页面被打上了 [Authorize] 标记，该组件会强制进行身份校验 *@
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
                <NotAuthorized>
                    @* 用户如果未登录，在此处拦截，渲染自定义组件重定向回 /login *@
                    <RedirectToLogin />
                </NotAuthorized>
            </AuthorizeRouteView>
        </Found>
    </Router>
</CascadingAuthenticationState>
```

#### 在具体页面上开启权限防护：
在任何需要玩家登录后才能访问的 Razor 页面（如 `Home.razor`）的顶部，我们只要打上该特性即可：
```razor
@page "/"
@attribute [Authorize]
```
如果没有登录的用户试图强行敲入地址访问，`AuthorizeRouteView` 就会识别出未授权状态，并渲染 `<RedirectToLogin />` 将其无缝转走，保障了系统数据的安全。
