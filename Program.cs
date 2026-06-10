using CyberPetApp.Components;

using CyberPetApp.Data;

using CyberPetApp.Services;

using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.Components.Authorization;

using Microsoft.AspNetCore.Components.Server.Circuits;

using Microsoft.EntityFrameworkCore;



var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>

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

// P2-2：轻量版已实现 GameTickOrchestrator（见 Services/GameTickOrchestrator.cs）；全站 IHostedService 仍 TODO
// P2-3：InteractiveAuto 需 WASM Client 项目，本轮未迁移；Tab 已用 HomeTabBase.ShouldRender 降刷新
// P2-4 TODO：PostgreSQL jsonb 批量写 / 热数据缓存

builder.Services.AddRazorComponents()

    .AddInteractiveServerComponents();



var app = builder.Build();



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


