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

    private Food normalFood = new Food("普通猫粮", 15, 2, 2);
    private Food luxuryFood = new Food("金枪鱼罐头", 35, 15, 5);

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
        return new string('?', f) + new string('?', w - f);
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
        "fishing" => "???? ?`",
        "sleep"   => "?????? zzz",
        "hungry"  => "??q????c",
        "happy"   => "????S ?",
        _         => "?S??s?"
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
            .Where(l => l.Contains("??") || l.Contains("???") || l.Contains("???"))
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
        WorkJobType.Construction => "?H?n +1g/tick?C???? 150 tick/?",
        WorkJobType.CatCafe => "?L? +0g?C?L?K?? +3/tick?C???? 180 tick/?",
        WorkJobType.FishMarketPorter => "??s??? +0g?C???? 120 tick/??CNPC ????X???",
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
        _ = InvokeAsync(StateHasChanged);
    }

    private void ManualStroke()
    {
        cat.Stroke();
        TryRefreshSidebarVitals();
        _ = InvokeAsync(StateHasChanged);
    }

    private void ManualSleep()
    {
        cat.Sleep(GetHouseBuffs());
        TryRefreshSidebarVitals();
        _ = InvokeAsync(StateHasChanged);
    }

    private static string SectionTabLabel(string section) => section switch
    {
        "house" => "???",
        "fishing" => "??",
        "work" => "??H",
        "market" => "??s",
        "gear" => "???",
        "cooking" => "?B?",
        "alchemy" => "???",
        "lifeshop" => "???????X",
        "milestones" => "??????",
        "backpack" => "?w??",
        _ => section
    };

    private async Task SelectSectionAsync(string s)
    {
        activeSection = s;
        if (s == "market" && player is not null)
            await ReloadMarketAsync();
        if (s == "milestones" && player is not null)
            await ReloadMilestonesAsync();
        if (s == "alchemy" && player is not null)
            await ReloadAlchemyAsync();
        if (s == "gear" && player is not null)
            await ReloadGearPanelAsync();
    }

    private int PendingOfferCount() => marketListings.Sum(v => v.PendingOffers.Count);

    private Task GoToMarketFromBackpack() => SelectSectionAsync("market");
    // ?? ????F??? / ?? / ??? ??
    private async Task ReloadGearPanelAsync()
    {
        if (player is null) return;
        playerGems = await _alchemyService.GetGemsAsync(player.Id);
        playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
        await ReloadGearAsync();
    }

    private async Task HandleLoadoutSocketGem((Guid GemId, GearGemSlot Slot) args)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.SocketGemAsync(player!, args.GemId, args.Slot));
        gearMessage = result!.Message;
        await ReloadGearPanelAsync();
    }

    private async Task GoAlchemyFromGear() => await SelectSectionAsync("alchemy");

    private async Task EquipTargetLureForGear(Guid lureId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.EquipTargetLureAsync(player!.Id, lureId));
        gearMessage = result!.Message;
        await ReloadGearPanelAsync();
    }

    private async Task UnequipTargetLureForGear()
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.UnequipTargetLureAsync(player!.Id));
        gearMessage = result!.Message;
        await ReloadGearPanelAsync();
    }

    private Task RepairRodPartial(FishingRod rod) => RepairRod(rod, false);
    private Task RepairReelPartial(FishingReel reel) => RepairReel(reel, false);
    private Task RepairLinePartial(FishingLine line) => RepairLine(line, false);

    private async Task ReloadGearAsync()
    {
        myRods = await _equipmentService.GetRodsAsync(player!.Id);
        myReels = await _equipmentService.GetReelsAsync(player.Id);
        myLines = await _equipmentService.GetLinesAsync(player.Id);
        myLures = await _equipmentService.GetLuresAsync(player.Id);

        // ??? loadout ???p?s??i????????L???????j?C????????i
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
    }

    private IReadOnlyList<FishDexEntry> GetFishDex() => _fishDexCache;

    private double GetGraduationPercent()
    {
        int rodTier = myRods.Count > 0 ? myRods.Max(r => GearProgressionCatalog.GetGearTier(r.Name)) : 1;
        int reelTier = myReels.Count > 0 ? myReels.Max(r => GearProgressionCatalog.GetGearTier(r.Name)) : 1;
        int lineTier = myLines.Count > 0 ? myLines.Max(l => GearProgressionCatalog.GetGearTier(l.Name)) : 1;
        int lureTier = myLures.Count > 0 ? myLures.Max(l => GearProgressionCatalog.GetGearTier(l.Name)) : 1;
        double dex = GearProgressionCatalog.OverallDexPercent(GetFishDex());
        int mythCount = fishRecords.Count(r => r.FishName.StartsWith("?_??E", StringComparison.Ordinal));
        return GearProgressionCatalog.ComputeGraduationPercent(rodTier, reelTier, lineTier, lureTier, dex, mythCount);
    }

    private async Task RepairRod(FishingRod rod, bool full)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _equipmentService.RepairRodAsync(player!, rod.Id, full));
        gearMessage = result.msg;
        await WithDbLock(ReloadGearAsync);
    }

    private async Task RepairReel(FishingReel reel, bool full)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _equipmentService.RepairReelAsync(player!, reel.Id, full));
        gearMessage = result.msg;
        await WithDbLock(ReloadGearAsync);
    }

    private async Task RepairLine(FishingLine line, bool full)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _equipmentService.RepairLineAsync(player!, line.Id, full));
        gearMessage = result.msg;
        await WithDbLock(ReloadGearAsync);
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
        spotLicenses.Any(l => l.SpotName == "??C?[??" && l.HasPermanent);

    private async Task ReloadSpotLicensesAsync()
    {
        spotLicenses = await _spotLicenseService.GetLicensesAsync(player!.Id);
    }

    private async Task PaySpotRental(string spotName)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _spotLicenseService.TryPayDailyRentalAsync(player!, spotName));
        spotLicenseMessage = result.msg;
        if (result.ok) await WithDbLock(ReloadSpotLicensesAsync);
    }

    private async Task BuySpotPermanent(string spotName)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () =>
        {
            result = await _spotLicenseService.TryBuyPermanentAsync(player!, spotName);
            if (result.ok)
            {
                await ReloadSpotLicensesAsync();
                await _achievementService.SyncProgressAsync(player!, fishRecords, true);
            }
        });
        spotLicenseMessage = result.msg;
    }

    private async Task ReloadMilestonesAsync()
    {
        milestoneBuffs = await _achievementService.GetBuffsAsync(player!.Id);
        milestoneUnlockIds = (await _achievementService.GetUnlockedItemIdsAsync(player.Id)).ToHashSet();
        achievementViews = await _achievementService.GetViewsAsync(player, fishRecords, HasDeepSeaPermanent());
        InvalidateHouseBuffs();
    }

    private async Task RefreshAfterExpeditionAsync()
    {
        await PersistGameStateAsync();
        StateHasChanged();
    }

    private async Task ClaimAchievement(string id)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _achievementService.TryClaimRewardAsync(player!, id));
        milestoneMessage = result.msg;
        if (result.ok) await ReloadMilestonesAsync();
    }

    private async Task BuyMilestoneItem(string id)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _achievementService.TryBuyShopItemAsync(player!, id));
        milestoneMessage = result.msg;
        if (result.ok)
        {
            await ReloadMilestonesAsync();
            await ReloadGearAsync();
        }
    }

    private async Task BuyRod(RodSpec spec)
    {
        bool ok = false;
        await WithDbLock(async () => ok = await _equipmentService.BuyRodAsync(player!, spec, cat.CatLevel, GetFishDex(), HasSpotAccess));
        gearMessage = ok ? $"??? {spec.Name}" : GearBuyFailMessage(spec);
        await WithDbLock(ReloadGearAsync);
    }

    private async Task BuyReel(ReelSpec spec)
    {
        bool ok = false;
        await WithDbLock(async () => ok = await _equipmentService.BuyReelAsync(player!, spec, cat.CatLevel, GetFishDex(), HasSpotAccess));
        gearMessage = ok ? $"??? {spec.Name}" : GearBuyFailMessage(spec);
        await WithDbLock(ReloadGearAsync);
    }

    private async Task BuyLine(LineSpec spec)
    {
        bool ok = false;
        await WithDbLock(async () => ok = await _equipmentService.BuyLineAsync(player!, spec, cat.CatLevel, GetFishDex(), HasSpotAccess));
        gearMessage = ok ? $"??? {spec.Name}" : GearBuyFailMessage(spec);
        await WithDbLock(ReloadGearAsync);
    }

    private async Task BuyLure(LureSpec spec)
    {
        bool ok = false;
        await WithDbLock(async () => ok = await _equipmentService.BuyLureAsync(player!, spec, cat.CatLevel, GetFishDex(), HasSpotAccess));
        gearMessage = ok ? $"??? {spec.Name} ?~{spec.PackSize}" : GearBuyFailMessage(spec);
        await WithDbLock(ReloadGearAsync);
    }

    private string GearBuyFailMessage(RodSpec spec)
    {
        if (spec.CraftOnly) return "????????";
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player!.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess))
            return $"??????F{GearProgressionCatalog.UnlockShortfall(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent, spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent, player.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess)}";
        return "????s????????L";
    }

    private string GearBuyFailMessage(ReelSpec spec)
    {
        if (spec.CraftOnly) return "????????";
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player!.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess))
            return $"??????F{GearProgressionCatalog.UnlockShortfall(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent, spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent, player.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess)}";
        return "????s????????L";
    }

    private string GearBuyFailMessage(LineSpec spec)
    {
        if (spec.CraftOnly) return "????????";
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player!.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess))
            return $"??????F{GearProgressionCatalog.UnlockShortfall(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent, spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent, player.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess)}";
        return "????s????????L";
    }

    private string GearBuyFailMessage(LureSpec spec)
    {
        if (spec.CraftOnly) return "????????";
        if (!GearProgressionCatalog.MeetsGearUnlock(spec, player!.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess))
            return $"??????F{GearProgressionCatalog.UnlockShortfall(spec.RequiredLevel, spec.RequiredCatLevel, spec.RequiredDexSpot, spec.RequiredDexPercent, spec.RequiredLicenseSpot, spec.RequiredOverallDexPercent, player.FishingLevel, cat.CatLevel, GetFishDex(), HasSpotAccess)}";
        return "????s??";
    }

    private async Task DisassembleFish(Fish fish)
    {
        (bool ok, string msg) result = default;
        await WithDbLock(async () => result = await _gearMaterialService.DisassembleFishAsync(player!, fish));
        feedMessage = result.msg;
        if (result.ok) await _playerService.SaveProgressAsync(player!);
    }

    private async Task EquipRod(FishingRod rod)
    {
        await WithDbLock(() => _equipmentService.EquipRodAsync(player!.Id, rod.Id));
        await WithDbLock(ReloadGearAsync);
        gearMessage = $"???? {rod.Name}";
    }

    private async Task EquipReel(FishingReel reel)
    {
        await WithDbLock(() => _equipmentService.EquipReelAsync(player!.Id, reel.Id));
        await WithDbLock(ReloadGearAsync);
        gearMessage = $"???? {reel.Name}";
    }

    private async Task EquipLine(FishingLine line)
    {
        await WithDbLock(() => _equipmentService.EquipLineAsync(player!.Id, line.Id));
        await WithDbLock(ReloadGearAsync);
        gearMessage = $"???? {line.Name}";
    }

    private async Task EquipLure(FishingLure lure)
    {
        await WithDbLock(() => _equipmentService.EquipLureAsync(player!.Id, lure.Id));
        await WithDbLock(ReloadGearAsync);
        gearMessage = $"???? {lure.Name}";
    }

    // ?? ?B? ??
    private async Task CookFish(Fish fish, string? recipeId = null)
    {
        try
        {
            CookResult? result = null;
            await WithDbLock(async () =>
            {
                result = await _cookingService.CookFishAsync(player!, cat, fish, GetHouseBuffs(), recipeId);
                await _cyberCatService.SaveAsync(cat);
                await _playerService.SaveProgressAsync(player!);
            });
            cookMessage = result!.Message + (result.LevelUps > 0 ? $" ? ?B????? Lv.{player!.CookingLevel}!" : "");
            _ = WithDbLock(async () => await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent()));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CookFish ????F{FishName}", fish.Name);
            cookMessage = "?B?????C??c?@?d?";
        }
    }

    private async Task CookAll(bool commonOnlyOnly)
    {
        CookResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _cookingService.CookAllAsync(player!, cat, GetHouseBuffs(), commonOnlyOnly);
            await _cyberCatService.SaveAsync(cat);
            await _playerService.SaveProgressAsync(player!);
        });
        cookMessage = result!.Message + (result.LevelUps > 0 ? $" ? ?B????? Lv.{player!.CookingLevel}!" : "");
        await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());
    }

    private Task CookFishFromTab((Fish fish, string recipeId) args) =>
        CookFish(args.fish, args.recipeId);

    // ?? ??? ??
    private async Task ReloadAlchemyAsync()
    {
        if (player is null) return;
        playerGems = await _alchemyService.GetGemsAsync(player.Id);
        playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
        await ReloadGearAsync();
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
            });
            alchemyMessage = result!.Message;
            await ReloadAlchemyAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CraftGem ????F{RecipeId}", recipeId);
            alchemyMessage = "?????????C??c?@?d?";
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
            });
            alchemyMessage = result!.Message;
            await ReloadAlchemyAsync();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "CraftTargetLure ????F{RecipeId}", recipeId);
            alchemyMessage = "????????????C??c?@?d?";
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
        });
        alchemyMessage = result!.Message;
        await ReloadAlchemyAsync();
    }

    private async Task CraftGear(string recipeId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () =>
        {
            result = await _alchemyService.CraftGearAsync(player!, cat, recipeId, _equipmentService, GetFishDex(), HasSpotAccess);
            _catProgressionService.AddXp(cat, 25);
            await _cyberCatService.SaveAsync(cat);
        });
        alchemyMessage = result!.Message;
        await ReloadAlchemyAsync();
    }

    private async Task SocketGem(Guid gemId, GearGemSlot slot)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.SocketGemAsync(player!, gemId, slot));
        alchemyMessage = result!.Message;
        await ReloadAlchemyAsync();
    }

    private Task SocketGemFromTab((Guid GemId, GearGemSlot Slot) args) =>
        SocketGem(args.GemId, args.Slot);

    private async Task EquipTargetLure(Guid lureId)
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.EquipTargetLureAsync(player!.Id, lureId));
        alchemyMessage = result!.Message;
        await ReloadAlchemyAsync();
    }

    private async Task UnequipTargetLure()
    {
        AlchemyResult? result = null;
        await WithDbLock(async () => result = await _alchemyService.UnequipTargetLureAsync(player!.Id));
        alchemyMessage = result!.Message;
        await ReloadAlchemyAsync();
    }

    private void OnTargetLureConsumed() => _ = HandleTargetLureConsumedAsync();

    private async Task HandleTargetLureConsumedAsync()
    {
        var lureId = fishingManager.Loadout.ActiveTargetLureId ?? loadout.ActiveTargetLureId;
        if (player is null || lureId is null) return;
        await WithDbLock(async () =>
        {
            await _alchemyService.ConsumeTargetLureUseAsync(player.Id, lureId.Value);
            await ReloadGearAsync();
            playerTargetLures = await _alchemyService.GetTargetLuresAsync(player.Id);
        });
        await InvokeAsync(StateHasChanged);
    }

    private async Task FeedCookedFood(string foodName)
    {
        string? msg = null;
        Food? food = null;
        string? catLevelMsg = null;
        await WithDbLock(async () =>
        {
            (food, msg) = await _cookingService.FeedCookedFoodAsync(player!, foodName);
        });
        if (food is not null)
        {
            lock (catStateLock)
            {
                cat.FeedFood(food);
                int catXp = CatFishingStatsHelper.XpFromFood(foodName);
                (_, catLevelMsg) = _catProgressionService.AddXp(cat, catXp);
            }
            player!.LifetimeFeedCount++;
            await WithDbLock(async () =>
            {
                activeFoodBuffs = await _catBuffService.LoadActiveAsync(player.Id);
                await _catProgressionService.SaveAsync(cat);
                await _playerService.SaveProgressAsync(player);
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
            });
        }
        feedMessage = food is not null
            ? (string.IsNullOrEmpty(catLevelMsg) ? msg : $"{msg} ?E {catLevelMsg}")
            : msg ?? $"?w???v?L {foodName}";
        TryRefreshSidebarVitals();
        await InvokeAsync(StateHasChanged);
    }

    private bool HasBackpackFood(string itemName) =>
        player!.Backpack.TryGetValue(itemName, out int qty) && qty > 0;

    private async Task FeedShopFood(Food food)
    {
        bool consumed = false;
        await WithDbLock(async () => consumed = await _playerService.ConsumeBackpackItemAsync(player!, food.Name));
        if (consumed)
        {
            string? catLevelMsg;
            lock (catStateLock)
            {
                cat.FeedFood(food);
                int catXp = CatFishingStatsHelper.XpFromFood(food.Name);
                (_, catLevelMsg) = _catProgressionService.AddXp(cat, catXp);
            }
            player!.LifetimeFeedCount++;
            feedMessage = catLevelMsg;
            _ = WithDbLock(async () =>
            {
                await _playerService.SaveProgressAsync(player);
                await _catProgressionService.SaveAsync(cat);
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
            });
        }
        else feedMessage = $"?w???v?L {food.Name}";
        TryRefreshSidebarVitals();
        await InvokeAsync(StateHasChanged);
    }

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

    private async Task BuyShopItem(ShopItem item)
    {
        bool ok = false;
        await WithDbLock(async () => ok = await _playerService.BuyShopItemAsync(player!, item));
        if (!ok) lifeShopMessage = "????s??";
        else lifeShopMessage = $"????? {item.Food.Name} ?~1";
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
        FishRarity.Rare      => "?H?L",
        FishRarity.Epic      => "?j?",
        FishRarity.Legendary => "??",
        _                    => "????"
    };

    private FoodBuffSnapshot GetFoodBuffSnapshot() =>
        CatBuffService.BuildSnapshot(activeFoodBuffs);

    private static string FormatBuffRemaining(DateTime expiresAt)
    {
        var remaining = expiresAt - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero) return "?????";
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
    private int UnlockedRoomCount() => playerHouse.Rooms.Values.Count(r => r.IsUnlocked);
    private int TotalFurnitureCount() => playerHouse.Rooms.Values.Sum(r => r.Furniture.Count);
    private int UnlockedFurnitureCount() => playerHouse.Rooms.Values.SelectMany(r => r.Furniture).Count(f => f.IsUnlocked);
}
