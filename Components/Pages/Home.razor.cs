using System.Security.Claims;
using CyberPetApp.Models;
using CyberPetApp.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace CyberPetApp.Components.Pages;

public partial class Home : IAsyncDisposable
{
    // 注入字段使用 _ 前缀，避免 partial 中与类型名同名导致 CS0120
    [Inject] private FishingService _fishingService { get; set; } = default!;
    [Inject] private CyberCatService _cyberCatService { get; set; } = default!;
    [Inject] private AuthService _authService { get; set; } = default!;
    [Inject] private PlayerService _playerService { get; set; } = default!;
    [Inject] private EquipmentService _equipmentService { get; set; } = default!;
    [Inject] private CookingService _cookingService { get; set; } = default!;
    [Inject] private CatBuffService _catBuffService { get; set; } = default!;
    [Inject] private AlchemyService _alchemyService { get; set; } = default!;
    [Inject] private GearMaterialService _gearMaterialService { get; set; } = default!;
    [Inject] private HouseService _houseService { get; set; } = default!;
    [Inject] private MarketService _marketService { get; set; } = default!;
    [Inject] private FeederService _feederService { get; set; } = default!;
    [Inject] private WatererService _watererService { get; set; } = default!;
    [Inject] private MaintenanceService _maintenanceService { get; set; } = default!;
    [Inject] private OfflineCompensationService _offlineCompensationService { get; set; } = default!;
    [Inject] private GamePersistenceService _gamePersistenceService { get; set; } = default!;
    [Inject] private FishRecordService _fishRecordService { get; set; } = default!;
    [Inject] private DailyBountyService _dailyBountyService { get; set; } = default!;
    [Inject] private SpotLicenseService _spotLicenseService { get; set; } = default!;
    [Inject] private AchievementService _achievementService { get; set; } = default!;
    [Inject] private CatProgressionService _catProgressionService { get; set; } = default!;
    [Inject] private LeaderboardService _leaderboardService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private AuthenticationStateProvider _authStateProvider { get; set; } = default!;
    [Inject] private ILogger<Home> Logger { get; set; } = default!;
    [Inject] private CircuitSessionContext _circuitSession { get; set; } = default!;
    [Inject] private GameTickOrchestrator _tickOrchestrator { get; set; } = default!;
    [Inject] private OnlineTracker _onlineTracker { get; set; } = default!;

    private CyberCat cat = new CyberCat();
    private AutoFeeder feeder = new AutoFeeder();
    private AutoWaterer waterer = new AutoWaterer();
    private readonly GameTickCounters _tickCounters = new();
    private Shop shop = new Shop();
    private Player? player;
    private string username = "";
    private bool isLoading = true;
    private WorkingPlace workingPlace = new WorkingPlace();
    private PlayerHouse playerHouse = new PlayerHouse();
    private FishingManager fishingManager = new FishingManager();
    private readonly GameSessionState gameSession = new();
    private int tickGeneration;
    private bool _titlebarFishingOn;
    private string activeSection = "fishing";
    private string currentSelectedRoomKey = "";
    private string CurrentSelectedRoomKey
    {
        get => currentSelectedRoomKey;
        set
        {
            Console.WriteLine($"[HOUSE-DEBUG][Home] CurrentSelectedRoomKey set old='{currentSelectedRoomKey}' new='{value}' activeSection='{activeSection}'");
            currentSelectedRoomKey = value;
        }
    }

    private Food normalFood = new Food("“猫乐滋”混合肉干粮", 20, 5, 5);
    private Food luxuryFood = new Food("“猫主子”吞拿鱼大肉罐", 35, 10, 10);

    // ???? ??O??? ????
    private List<FishingRod> myRods = [];
    private List<FishingReel> myReels = [];
    private List<FishingLine> myLines = [];
    private List<FishingLure> myLures = [];
    private FishingLoadout loadout = new();
    private string? gearMessage;
    private string? cookMessage;
    private string? alchemyMessage;
    private List<PlayerGem> playerGems = [];
    private List<PlayerTargetLure> playerTargetLures = [];
    private string? feedMessage;
    private string? lifeShopMessage;
    private string? fishingBlockMessage;
    private string? marketMessage;
    private List<MarketListingView> marketListings = [];
    private readonly string _fallbackCircuitId = Guid.NewGuid().ToString();
    private string CircuitId => _circuitSession.CircuitId ?? _fallbackCircuitId;
    private string? offlineMessage;
    private string? catchBroadcast;
    private bool catchBroadcastLegendary;
    private bool catchBroadcastMyth;
    private List<FishCatchRecord> fishRecords = [];
    private List<SpotLicense> spotLicenses = [];
    private List<AchievementView> achievementViews = [];
    private HashSet<string> milestoneUnlockIds = [];
    private MilestoneBuffs milestoneBuffs = MilestoneBuffs.Empty;
    private string? spotLicenseMessage;
    private string? milestoneMessage;
    private List<FishLeaderboardEntry> topFishLeaderboard = [];
    private List<LevelLeaderboardEntry> topLevelLeaderboard = [];
    private ServerFishingStats? serverFishingStats;
    private List<ActiveCatBuff> activeFoodBuffs = [];
    private CatVitalsSnapshot catVitalsSnapshot;
    private IReadOnlyList<string> _sidebarDebuffLines = Array.Empty<string>();
    private IReadOnlyList<FishDexEntry> _fishDexCache = [];
    private CancellationTokenSource? _leaderboardDebounceCts;
    private bool leaderboardRefreshing;
    private bool isSessionKicked;
    private bool isSettingsOpen;
    private bool showScanlines = true;
    private string currentTheme = "blue";
    private bool enableSfx = true;
    private bool isMobileTabExpanded;

    private void ToggleSettings()
    {
        isSettingsOpen = !isSettingsOpen;
    }

    private void ToggleMobileTabs()
    {
        isMobileTabExpanded = !isMobileTabExpanded;
    }

    // ???s?? scoped DbContext ??
    private readonly SemaphoreSlim dbLock = new(1, 1);
    // ?L???????F???????????^ 2s ????????C?? cat ????s??
    private readonly object catStateLock = new();

    private async Task WithDbLock(Func<Task> action)
    {
        await dbLock.WaitAsync();
        try { await action(); }
        finally { dbLock.Release(); }
    }

    // ASCII progress bar: [????????????]?i?L?????? 1000?j
    private static string ABar(int value, int max = CyberCat.StatMax, int w = 10)
    {
        var f = Math.Clamp((int)Math.Round((double)value / max * w), 0, w);
        return new string('█', f) + new string('░', w - f);
    }

    private string CatState()
    {
        if (fishingManager.IsFishing) return "fishing";
        if (cat.Energy < 250)         return "sleep";
        if (cat.Hunger < 300)         return "hungry";
        if (cat.Happiness >= 800)     return "happy";
        return "content";
    }

    private IReadOnlyList<string> FishingCatBuffLines =>
        CatBuffHelper.Compute(cat, player!.MaintenanceOverdue, GetHouseBuffs()).StatusLines;

    private void BindGameSession()
    {
        gameSession.Player = player;
        gameSession.Cat = cat;
        gameSession.Feeder = feeder;
        gameSession.Waterer = waterer;
        gameSession.PlayerHouse = playerHouse;
        gameSession.FishingManager = fishingManager;
        gameSession.MilestoneBuffs = milestoneBuffs;
        gameSession.Loadout = loadout;
        gameSession.FishRecords = fishRecords;
        gameSession.SpotLicenses = spotLicenses;
        gameSession.TickRenderSnapshot = _tickCounters.LastTickRenderSnapshot;
        gameSession.TickGeneration = tickGeneration;
        gameSession.CatVitalsSnapshot = catVitalsSnapshot;
    }

    private void OnFishingChangedForChrome()
    {
        var on = fishingManager.IsFishing;
        if (on == _titlebarFishingOn) return;
        _titlebarFishingOn = on;
        _ = InvokeAsync(StateHasChanged);
    }

    private string CatMoodText() => CatState() switch
    {
        "fishing" => "专注钓鱼 🎣",
        "sleep"   => "困意满满 zzz",
        "hungry"  => "饥肠辘辘",
        "happy"   => "超级开心 ♥",
        _         => "状态尚可"
    };

    /// <summary>??????????X?V???????C???????????d?Z?B</summary>
    private void TryRefreshSidebarVitals()
    {
        var sidebarCatBuff = CatBuffHelper.Compute(cat, player!.MaintenanceOverdue, GetHouseBuffs());
        UpdateSidebarDebuffLines(sidebarCatBuff.StatusLines);
        bool canTreat = cat.Health < EconomySinks.CatTreatHealthThreshold
                        || cat.Happiness < EconomySinks.CatTreatHappinessThreshold;
        var next = new CatVitalsSnapshot(
            cat.Name,
            cat.CatLevel,
            cat.CatXp,
            CatFishingStatsHelper.XpToNext(cat.CatLevel),
            cat.Hunger,
            cat.Thirst,
            cat.Energy,
            cat.Happiness,
            CatState(),
            CatMoodText(),
            canTreat,
            feedMessage);
        if (next == catVitalsSnapshot) return;
        catVitalsSnapshot = next;
        gameSession.CatVitalsSnapshot = next;
    }

    private void UpdateSidebarDebuffLines(IEnumerable<string> statusLines)
    {
        var newLines = statusLines
            .Where(l => l.Contains("饥饿") || l.Contains("疲劳") || l.Contains("拖欠") || l.Contains("debuff"))
            .ToList();
        if (_sidebarDebuffLines.Count == newLines.Count
            && _sidebarDebuffLines.SequenceEqual(newLines))
            return;
        _sidebarDebuffLines = newLines;
    }

    private void RefreshFishDexCache() =>
        _fishDexCache = FishDexCatalog.BuildAll(fishRecords);

    private void ToggleWork() => workingPlace.ToggleWork(player!);

    private async Task SelectWorkJob(WorkJobType job)
    {
        if (!WorkingPlace.CanSelectJob(job, player!, cat)) return;
        workingPlace.Job = job;
        player!.SelectedWorkJob = job;
        workingPlace.WorkTickCount = 0;
        player.WorkTickCount = 0;
        await WithDbLock(() => _playerService.SaveProgressAsync(player));
    }

    private string WorkJobHint() => workingPlace.Job switch
    {
        WorkJobType.Construction => "工地 +1g/tick，摊位券 150 tick/张",
        WorkJobType.CatCafe => "猫咖 +0g，幸福 +3/tick，摊位券 180 tick/张",
        WorkJobType.FishMarketPorter => "鱼市搬运 +0g，摊位券 120 tick/张，NPC 报价更频",
        _ => ""
    };

    private IReadOnlyList<string> PassiveCareSummaryLines() =>
        PassiveCatCare.SummarizeRates(UnlockedFurnitureIds());

    private IEnumerable<string> UnlockedFurnitureIds() =>
        playerHouse.Rooms.Values.SelectMany(r => r.Furniture)
            .Where(f => f.IsUnlocked)
            .Select(f => f.FurnitureId);

    private void ManualDrinkWater()
    {
        cat.DrinkWater(20);
        TryRefreshSidebarVitals();
        tickGeneration++;
        _ = InvokeAsync(StateHasChanged);
    }

    private void ManualStroke()
    {
        cat.Stroke();
        TryRefreshSidebarVitals();
        tickGeneration++;
        _ = InvokeAsync(StateHasChanged);
    }

    private void ManualSleep()
    {
        cat.Sleep(GetHouseBuffs());
        TryRefreshSidebarVitals();
        tickGeneration++;
        _ = InvokeAsync(StateHasChanged);
    }

    private static string SectionTabLabel(string section) => section switch
    {
        "cat" => "猫咪",
        "house" => "家园",
        "fishing" => "钓鱼",
        "work" => "打工",
        "market" => "鱼市",
        "gear" => "装备",
        "cooking" => "烹饪",
        "alchemy" => "炼金",
        "lifeshop" => "生活商店",
        "milestones" => "里程碑",
        "backpack" => "背包",
        _ => section
    };

    private async Task SelectSectionAsync(string s)
    {
        activeSection = s;
        isMobileTabExpanded = false;
        if (s == "market" && player is not null)
            await WithDbLock(ReloadMarketAsync);
        if (s == "milestones" && player is not null)
            await ReloadMilestonesAsync();
        if (s == "alchemy" && player is not null)
            await ReloadAlchemyAsync();
        if (s == "gear" && player is not null)
            await ReloadGearPanelAsync();
    }

    private int PendingOfferCount() => marketListings.Sum(v => v.PendingOffers.Count);

    private Task GoToMarketFromBackpack() => SelectSectionAsync("market");
    // Reload gear panel
    private async Task ReloadGearPanelAsync(bool acquireLock = true)
    {
        if (player is null) return;
        Func<Task> body = async () =>
        {
            playerGems = await _alchemyService.GetGemsAsync(player.Id);
            playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
            await ReloadGearAsync(acquireLock: false);
        };

        if (acquireLock)
        {
            await WithDbLock(body);
        }
        else
        {
            await body();
        }
    }

    private async Task HandleLoadoutSocketGem((Guid GemId, GearGemSlot Slot) args)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.SocketGemAsync(player!, args.GemId, args.Slot);
            await ReloadGearPanelAsync(acquireLock: false);
        });
        gearMessage = result!.Message;
    }

    private async Task GoAlchemyFromGear() => await SelectSectionAsync("alchemy");

    private async Task EquipTargetLureForGear(Guid lureId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.EquipTargetLureAsync(player!.Id, lureId);
            await ReloadGearPanelAsync(acquireLock: false);
        });
        gearMessage = result!.Message;
    }

    private async Task UnequipTargetLureForGear()
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.UnequipTargetLureAsync(player!.Id);
            await ReloadGearPanelAsync(acquireLock: false);
        });
        gearMessage = result!.Message;
    }

    private async Task ReloadGearAsync(bool acquireLock = true)
    {
        Func<Task> body = async () =>
        {
            myRods = await _equipmentService.GetRodsAsync(player!.Id);
            myReels = await _equipmentService.GetReelsAsync(player.Id);
            myLines = await _equipmentService.GetLinesAsync(player.Id);
            myLures = await _equipmentService.GetLuresAsync(player.Id);

            var fresh = await _equipmentService.BuildLoadoutAsync(player.Id, player.FishingLevel, milestoneBuffs.RarityBonus);
            loadout.RodName = fresh.RodName;
            loadout.Sensitivity = fresh.Sensitivity;
            loadout.CastRange = fresh.CastRange;
            loadout.MaxStrength = fresh.MaxStrength;
            loadout.RodRequiredLevel = fresh.RodRequiredLevel;
            loadout.ReelName = fresh.ReelName;
            loadout.DragPower = fresh.DragPower;
            loadout.GearRatio = fresh.GearRatio;
            loadout.LineCapacity = fresh.LineCapacity;
            loadout.Smoothness = fresh.Smoothness;
            loadout.ReelRequiredLevel = fresh.ReelRequiredLevel;
            loadout.LineName = fresh.LineName;
            loadout.LineStrength = fresh.LineStrength;
            loadout.LineSensitivity = fresh.LineSensitivity;
            loadout.LineStealth = fresh.LineStealth;
            loadout.AbrasionResistance = fresh.AbrasionResistance;
            loadout.LineDepth = fresh.LineDepth;
            loadout.LineRequiredLevel = fresh.LineRequiredLevel;
            loadout.LineDurability = fresh.LineDurability;
            loadout.LureName = fresh.LureName;
            loadout.Attraction = fresh.Attraction;
            loadout.RarityBonus = fresh.RarityBonus;
            loadout.LureDepth = fresh.LureDepth;
            loadout.LureDurabilityRemaining = fresh.LureDurabilityRemaining;
            loadout.LureRequiredLevel = fresh.LureRequiredLevel;
            loadout.LureQuantity = fresh.LureQuantity;
            loadout.RodDurability = fresh.RodDurability;
            loadout.ReelDurability = fresh.ReelDurability;
            loadout.MilestoneRarityBonus = fresh.MilestoneRarityBonus;
            loadout.FishingLevel = fresh.FishingLevel;
            loadout.GemBonuses = fresh.GemBonuses;
            loadout.ActiveTargetLureRecipeId = fresh.ActiveTargetLureRecipeId;
            loadout.ActiveTargetLureUses = fresh.ActiveTargetLureUses;
            loadout.ActiveTargetLureId = fresh.ActiveTargetLureId;
            loadout.RodGearTier = fresh.RodGearTier;
            loadout.SpotGearEffectiveness = fresh.SpotGearEffectiveness;
        };

        if (acquireLock)
        {
            await WithDbLock(body);
        }
        else
        {
            await body();
        }
    }

    private IReadOnlyList<FishDexEntry> GetFishDex() => _fishDexCache;

    private double GetGraduationPercent()
    {
        int rodTier = myRods.Count > 0 ? myRods.Max(r => GearProgressionCatalog.GetGearTier(r.Name)) : 1;
        int reelTier = myReels.Count > 0 ? myReels.Max(r => GearProgressionCatalog.GetGearTier(r.Name)) : 1;
        int lineTier = myLines.Count > 0 ? myLines.Max(l => GearProgressionCatalog.GetGearTier(l.Name)) : 1;
        int lureTier = myLures.Count > 0 ? myLures.Max(l => GearProgressionCatalog.GetGearTier(l.Name)) : 1;
        double dex = GearProgressionCatalog.OverallDexPercent(GetFishDex());
        int mythCount = fishRecords.Count(r => r.FishName.StartsWith("神话·", StringComparison.Ordinal));
        return GearProgressionCatalog.ComputeGraduationPercent(rodTier, reelTier, lineTier, lureTier, dex, mythCount);
    }

    private bool HasSpotAccess(string spotName)
    {
        if (SpotLicenseCatalog.RequiresAllMythic(spotName)
            && GearProgressionCatalog.MythicSpeciesCaught(GetFishDex())
                < GearProgressionCatalog.TotalMythFishSpecies - 2)
            return false;
        if (!SpotLicenseCatalog.RequiresLicense(spotName)) return true;
        var lic = spotLicenses.FirstOrDefault(l => l.SpotName == spotName);
        if (lic?.HasPermanent == true) return true;
        var today = DateTime.UtcNow.Date;
        return lic?.RentalPaidDate?.Date == today;
    }

    private bool HasDeepSeaPermanent() =>
        spotLicenses.Any(l => l.SpotName == "近海礁石" && l.HasPermanent);

    private async Task ReloadSpotLicensesAsync(bool acquireLock = true)
    {
        Func<Task> body = async () =>
        {
            spotLicenses = await _spotLicenseService.GetLicensesAsync(player!.Id);
        };

        if (acquireLock)
        {
            await WithDbLock(body);
        }
        else
        {
            await body();
        }
    }

    private async Task PaySpotRental(string spotName)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = await _spotLicenseService.TryPayDailyRentalAsync(player!, spotName);
            if (result.ok)
            {
                await ReloadSpotLicensesAsync(acquireLock: false);
            }
        });
        spotLicenseMessage = result.msg;
    }

    private async Task BuySpotPermanent(string spotName)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = await _spotLicenseService.TryBuyPermanentAsync(player!, spotName);
            if (result.ok)
            {
                await ReloadSpotLicensesAsync(acquireLock: false);
                await _achievementService.SyncProgressAsync(player!, fishRecords, true);
            }
        });
        spotLicenseMessage = result.msg;
    }

    private async Task ReloadMilestonesAsync(bool acquireLock = true)
    {
        Func<Task> body = async () =>
        {
            milestoneBuffs = await _achievementService.GetBuffsAsync(player!.Id);
            milestoneUnlockIds = (await _achievementService.GetUnlockedItemIdsAsync(player.Id)).ToHashSet();
            achievementViews = await _achievementService.GetViewsAsync(player, fishRecords, HasDeepSeaPermanent());
            InvalidateHouseBuffs();
        };

        if (acquireLock)
        {
            await WithDbLock(body);
        }
        else
        {
            await body();
        }
    }

    private async Task RefreshAfterExpeditionAsync()
    {
        await PersistGameStateAsync();
        StateHasChanged();
    }

    private async Task ClaimAchievement(string id)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = await _achievementService.TryClaimRewardAsync(player!, id);
            if (result.ok)
            {
                await ReloadMilestonesAsync(acquireLock: false);
            }
        });
        milestoneMessage = result.msg;
    }

    private async Task BuyMilestoneItem(string id)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = await _achievementService.TryBuyShopItemAsync(player!, id);
            if (result.ok)
            {
                await ReloadMilestonesAsync(acquireLock: false);
                await ReloadGearAsync(acquireLock: false);
            }
        });
        milestoneMessage = result.msg;
    }

    private async Task DisassembleFish(Fish fish)
    {
        if (player is null || !player.FishBackpack.Any(f => f.Id == fish.Id))
            return;

        var (ok, msg, drops) = _gearMaterialService.ApplyDisassembleInMemory(player, fish);
        feedMessage = msg;
        if (!ok)
        {
            await InvokeAsync(StateHasChanged);
            return;
        }

        tickGeneration++;
        BindGameSession();
        await InvokeAsync(StateHasChanged);

        _ = PersistDisassembleFishAsync(fish, drops);
    }

    private async Task PersistDisassembleFishAsync(Fish fish, IReadOnlyList<(string Name, int Qty)> drops)
    {
        try
        {
            await WithDbLock(() => _gearMaterialService.PersistDisassembleAsync(player!, fish, drops));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistDisassembleFish failed for {FishId}", fish.Id);
        }
    }

    // 烹饪 / 喂食 → Home.Cooking.cs（乐观 UI）

    // Reload alchemy panel
    private async Task ReloadAlchemyAsync(bool acquireLock = true)
    {
        if (player is null) return;
        Func<Task> body = async () =>
        {
            playerGems = await _alchemyService.GetGemsAsync(player.Id);
            playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
            await ReloadGearAsync(acquireLock: false);
        };

        if (acquireLock)
        {
            await WithDbLock(body);
        }
        else
        {
            await body();
        }
    }

    private async Task CraftGem(string recipeId)
    {
        try
        {
            AlchemyResult? result = null;
            await WithDbLock(async () =>
            {
                result = await _alchemyService.CraftGemAsync(player!, cat, recipeId);
                _catProgressionService.AddXp(cat, 15);
                await _cyberCatService.SaveAsync(cat);
                await ReloadAlchemyAsync(acquireLock: false);
            });
            alchemyMessage = result!.Message;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CraftGem 失败 {RecipeId}", recipeId);
            alchemyMessage = "合成失败，请稍后重试";
        }
    }

    private async Task CraftTargetLure(string recipeId)
    {
        try
        {
            AlchemyResult? result = null;
            await WithDbLock(async () =>
            {
                result = await _alchemyService.CraftTargetLureAsync(player!, cat, recipeId);
                _catProgressionService.AddXp(cat, 25);
                await _cyberCatService.SaveAsync(cat);
                await ReloadAlchemyAsync(acquireLock: false);
            });
            alchemyMessage = result!.Message;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CraftTargetLure 失败 {RecipeId}", recipeId);
            alchemyMessage = "合成失败，请稍后重试";
        }
    }

    private async Task CraftLine(string recipeId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.CraftLineAsync(player!, cat, recipeId, _equipmentService);
            _catProgressionService.AddXp(cat, 20);
            await _cyberCatService.SaveAsync(cat);
            await ReloadAlchemyAsync(acquireLock: false);
        });
        alchemyMessage = result!.Message;
    }

    private async Task CraftGear(string recipeId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.CraftGearAsync(player!, cat, recipeId, _equipmentService, GetFishDex(), HasSpotAccess);
            _catProgressionService.AddXp(cat, 25);
            await _cyberCatService.SaveAsync(cat);
            await ReloadAlchemyAsync(acquireLock: false);
        });
        alchemyMessage = result!.Message;
    }

    private async Task SocketGem(Guid gemId, GearGemSlot slot)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.SocketGemAsync(player!, gemId, slot);
            await ReloadAlchemyAsync(acquireLock: false);
        });
        alchemyMessage = result!.Message;
    }

    private Task SocketGemFromTab((Guid GemId, GearGemSlot Slot) args) =>
        SocketGem(args.GemId, args.Slot);

    private async Task EquipTargetLure(Guid lureId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.EquipTargetLureAsync(player!.Id, lureId);
            await ReloadAlchemyAsync(acquireLock: false);
        });
        alchemyMessage = result!.Message;
    }

    private async Task UnequipTargetLure()
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.UnequipTargetLureAsync(player!.Id);
            await ReloadAlchemyAsync(acquireLock: false);
        });
        alchemyMessage = result!.Message;
    }

    private void OnTargetLureConsumed() => _ = HandleTargetLureConsumedAsync();

    private async Task HandleTargetLureConsumedAsync()
    {
        var lureId = fishingManager.Loadout.ActiveTargetLureId ?? loadout.ActiveTargetLureId;
        if (player is null || lureId is null) return;
        await WithDbLock(async () =>
        {
            await _alchemyService.ConsumeTargetLureUseAsync(player.Id, lureId.Value);
            await ReloadGearAsync(acquireLock: false);
            playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
        });
        await InvokeAsync(StateHasChanged);
    }

    // FeedCookedFood / FeedShopFood → Home.Cooking.cs

    private bool HasBackpackFood(string itemName) =>
        player!.Backpack.TryGetValue(itemName, out int qty) && qty > 0;

    private async Task ReloadLeaderboardAsync()
    {
        topFishLeaderboard = await _leaderboardService.GetTopFishAsync(10);
        topLevelLeaderboard = await _leaderboardService.GetTopLevelsAsync(10);
        serverFishingStats = await _leaderboardService.GetServerStatsAsync();
    }

    private void ScheduleLeaderboardRefresh()
    {
        _leaderboardDebounceCts?.Cancel();
        _leaderboardDebounceCts?.Dispose();
        _leaderboardDebounceCts = new CancellationTokenSource();
        var token = _leaderboardDebounceCts.Token;
        _ = DebouncedLeaderboardRefreshAsync(token);
    }

    private async Task DebouncedLeaderboardRefreshAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(30), token);
            await ReloadLeaderboardAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException) { }
    }

    private async Task RefreshLeaderboardManualAsync()
    {
        _leaderboardDebounceCts?.Cancel();
        leaderboardRefreshing = true;
        try
        {
            await ReloadLeaderboardAsync();
        }
        finally
        {
            leaderboardRefreshing = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task AddFoodToFeeder(Food food)
    {
        await WithDbLock(async () =>
        {
            var (_, msg) = await _feederService.AddFoodFromBackpackAsync(player!, feeder, food);
            feedMessage = msg;
        });
    }

    private async Task AddCookedFoodToFeeder(string foodName)
    {
        var food = CookBook.FoodByName(foodName);
        if (food is null) return;
        await WithDbLock(async () =>
        {
            var (_, msg) = await _feederService.AddFoodFromBackpackAsync(player!, feeder, food);
            feedMessage = msg;
        });
    }

    private async Task RemoveLastFromFeeder()
    {
        await WithDbLock(async () => await _feederService.RemoveLastFoodAsync(feeder));
    }

    private bool HasAutoFeederUnit() =>
        playerHouse.Rooms.Values.SelectMany(r => r.Furniture)
            .Any(f => f.IsUnlocked && f.FurnitureId == "AutoFeederUnit");

    private bool HasWaterDispenser() =>
        playerHouse.Rooms.Values.SelectMany(r => r.Furniture)
            .Any(f => f.IsUnlocked && PassiveCatCare.UnlocksAutoWaterer(f.FurnitureId));

    private async Task AddPurifiedWaterToWaterer()
    {
        await WithDbLock(async () =>
        {
            var (_, msg) = await _watererService.AddWaterFromBackpackAsync(
                player!, waterer, WaterCatalog.Purified);
            feedMessage = msg;
        });
    }

    private async Task RemoveLastFromWaterer()
    {
        await WithDbLock(async () => await _watererService.RemoveLastWaterAsync(waterer));
    }

    private async Task SyncWatererAfterWaterAsync()
    {
        try
        {
            await WithDbLock(async () => await _watererService.SyncSlotsAfterWaterAsync(waterer));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "SyncWatererAfterWaterAsync ???");
        }
    }

    private Task FinalizeBuyShopItem(ShopItem item)
    {
        lifeShopMessage = $"已购买 {item.Food.Name} ×1";
        tickGeneration++;
        BindGameSession();
        _ = PersistBuyShopItemAsync(item);
        return Task.CompletedTask;
    }

    private async Task PersistBuyShopItemAsync(ShopItem item)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg) = await _playerService.CommitBuyShopItemAsync(player!, item);
                if (!ok)
                {
                    PlayerService.RollbackBuyShopItem(player!, item);
                    lifeShopMessage = msg;
                }
            });
            tickGeneration++;
            BindGameSession();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistBuyShopItem failed for {ItemName}", item.Food.Name);
        }
    }

    private static string FishSpriteClass(string name, FishRarity rarity) =>
        SpriteCatalog.Fish(name, rarity);

    private static string RarityClass(FishRarity r) => r switch
    {
        FishRarity.Rare      => "rarity-rare",
        FishRarity.Epic      => "rarity-epic",
        FishRarity.Legendary => "rarity-legendary",
        _                    => "rarity-common"
    };

    private static string RarityLabel(FishRarity r) => r switch
    {
        FishRarity.Rare      => "稀有",
        FishRarity.Epic      => "史诗",
        FishRarity.Legendary => "传说",
        _                    => "普通"
    };

    private FoodBuffSnapshot GetFoodBuffSnapshot() =>
        CatBuffService.BuildSnapshot(activeFoodBuffs);

    private static string FormatBuffRemaining(DateTime expiresAt)
    {
        var remaining = expiresAt - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero) return "已过期";
        if (remaining.TotalHours >= 1)
            return $"{(int)remaining.TotalHours}h{remaining.Minutes}m";
        return $"{Math.Max(1, (int)remaining.TotalMinutes)}m";
    }

    private const int BaseFeederCapacity = 10;

    // P0-4?F???/????????X?O?????????C??????????d? LINQ
    private HouseBuffs? _cachedHouseBuffs;

    private HouseBuffs GetHouseBuffs() =>
        _cachedHouseBuffs ??= HouseBuffAggregator.Aggregate(playerHouse)
            .WithMilestoneScale(milestoneBuffs.HouseBuffMultiplier);

    private void InvalidateHouseBuffs() => _cachedHouseBuffs = null;

    private void ApplyFeederCapacity()
    {
        feeder.MaxFoodCount = BaseFeederCapacity + GetHouseBuffs().FeederExtraSlots;
    }

    private int EffectiveWorkGoldPerTick()
    {
        var buffs = GetHouseBuffs();
        return Math.Max(0, (int)Math.Round(workingPlace.GoldPerTick * buffs.WorkGoldMultiplier));
    }

    private int EffectiveStallTicketTicks() =>
        workingPlace.EffectiveTicksPerStallTicketForDisplay(GetHouseBuffs());

    private int BackpackTotalCount() => player!.Backpack.Values.Sum() + player.FishBackpack.Count;

    private Task HandleUpgradeBackpack()
    {
        if (player is null) return Task.CompletedTask;
        if (!PlayerService.TryPrepareFishBackpackUpgrade(player, out var inc, out var cost, out var error))
        {
            feedMessage = error;
            return InvokeAsync(StateHasChanged);
        }

        PlayerService.ApplyFishBackpackUpgradeOptimistic(player, inc, cost);
        feedMessage = $"背包升级 +{inc}，扣除 {cost}g";
        tickGeneration++;
        BindGameSession();
        _ = PersistUpgradeBackpackAsync(inc, cost);
        return InvokeAsync(StateHasChanged);
    }

    private async Task PersistUpgradeBackpackAsync(int inc, int cost)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var ok = await _playerService.CommitFishBackpackUpgradeAsync(player!);
                if (!ok)
                {
                    PlayerService.RollbackFishBackpackUpgrade(player!, inc, cost);
                    feedMessage = "升级失败，已回滚";
                }
            });
            tickGeneration++;
            BindGameSession();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistUpgradeBackpack failed");
        }
    }
    private int UnlockedRoomCount() => playerHouse.Rooms.Values.Count(r => r.IsUnlocked);
    private int TotalFurnitureCount() => playerHouse.Rooms.Values.Sum(r => r.Furniture.Count);
    private int UnlockedFurnitureCount() => playerHouse.Rooms.Values.SelectMany(r => r.Furniture).Count(f => f.IsUnlocked);

    private async Task SaveSkinAndRefreshAsync()
    {
        if (player is not null)
            await _playerService.SaveProgressAsync(player);
        if (cat is not null)
            await _cyberCatService.SaveAsync(cat);
        await InvokeAsync(StateHasChanged);
    }
}
