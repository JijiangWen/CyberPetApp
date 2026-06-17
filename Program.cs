using CyberPetApp.Components;

using CyberPetApp.Data;

using CyberPetApp.Services;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Components.Authorization;

using Microsoft.AspNetCore.Components.Server.Circuits;

using Microsoft.EntityFrameworkCore;



#if DEBUG
var rootDir = AppDomain.CurrentDomain.BaseDirectory;
int idx = rootDir.IndexOf(Path.Combine("bin", "Debug"));
if (idx > 0)
{
    rootDir = rootDir.Substring(0, idx);
}
else
{
    rootDir = Directory.GetCurrentDirectory();
}

// 🌟 DEBUG 模式下：最高优先级注入绝对路径和开发环境
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = "CyberPetApp",
    EnvironmentName = Environments.Development,
    ContentRootPath = rootDir,
    WebRootPath = Path.Combine(rootDir, "wwwroot")
});
#else
// 🌟 RELEASE 模式下：保持正常的默认初始化，注意这里依然有 var，两个分支绝对互斥，不会重复定义
var builder = WebApplication.CreateBuilder(args);
#endif



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<AppDbContext>(options =>

    options.UseNpgsql(connectionString));

#if DEBUG
builder.AddDevelopmentProfiling();
#endif

builder.Services.AddHttpContextAccessor();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)

    .AddCookie(options =>

    {

        options.LoginPath = "/login";

        options.Cookie.Name = "CyberPet.Auth";

        options.ExpireTimeSpan = TimeSpan.FromDays(30);

        options.SlidingExpiration = true;

    });

builder.Services.AddAuthorization();



builder.Services.AddScoped<FishingService>();

builder.Services.AddScoped<CyberCatService>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<PlayerService>();

builder.Services.AddScoped<EquipmentService>();

builder.Services.AddScoped<CookingService>();

builder.Services.AddScoped<CatBuffService>();

builder.Services.AddScoped<AlchemyService>();

builder.Services.AddScoped<GearMaterialService>();

builder.Services.AddScoped<HouseService>();

builder.Services.AddScoped<MarketService>();

builder.Services.AddScoped<FeederService>();

builder.Services.AddScoped<WatererService>();

builder.Services.AddScoped<PassiveCatCareService>();

builder.Services.AddScoped<MaintenanceService>();

builder.Services.AddScoped<OfflineCompensationService>();

builder.Services.AddScoped<GamePersistenceService>();

builder.Services.AddScoped<FishRecordService>();

builder.Services.AddScoped<DailyBountyService>();

builder.Services.AddScoped<SpotLicenseService>();

builder.Services.AddScoped<AchievementService>();

builder.Services.AddScoped<ExpeditionService>();

builder.Services.AddScoped<CatProgressionService>();

builder.Services.AddScoped<LeaderboardService>();

builder.Services.AddScoped<GameTickOrchestrator>();

// P2-5：Circuit 断开时清理钓鱼会话注册表
builder.Services.AddScoped<CircuitSessionContext>();
builder.Services.AddScoped<CircuitHandler, FishingCircuitHandler>();

// Multiplayer Boat Fishing Services
builder.Services.AddSingleton<BoatSessionManager>();
builder.Services.AddSingleton<OnlineTracker>();
builder.Services.AddScoped<BoatService>();

// P2-2：轻量版已实现 GameTickOrchestrator（见 Services/GameTickOrchestrator.cs）；全站 IHostedService 仍 TODO
// P2-3：InteractiveAuto 需 WASM Client 项目，本轮未迁移；Tab 已用 HomeTabBase.ShouldRender 降刷新
// P2-4 TODO：PostgreSQL jsonb 批量写 / 热数据缓存

builder.Services.AddRazorComponents()

    .AddInteractiveServerComponents();



var app = builder.Build();

// Run DB Name Migration on Startup
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        CyberPetApp.Services.NameMigrationHelper.Migrate(db);
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Failed to run DB migration on startup: {ex.Message}");
    }
}



if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    app.UseHsts();

}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

#if DEBUG
app.UseDevelopmentProfiling();
#endif

app.UseAuthentication();

app.UseAuthorization();



app.UseAntiforgery();



app.MapStaticAssets();

app.MapAuthEndpoints();

app.MapRazorComponents<App>()

    .AddInteractiveServerRenderMode();



app.Run();


