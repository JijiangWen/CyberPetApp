using CyberPetApp.Data;
using CyberPetApp.Models;

namespace CyberPetApp.Services;

public record OfflineResult(
    int TicksApplied,
    int RawTicks,
    bool WasCapped,
    int WorkGoldEarned,
    int StallTicketsEarned,
    int MarketOfferCycles,
    int MaintenanceCharges,
    string Summary);

/// <summary>
/// 登录时根据 LastActiveAt 补偿离线 tick：打工金币/摊位券、市场 NPC 报价轮次、维护费。
/// 猫属性：背景极慢衰减 + 打工 tick 活动消耗（不再每 2s 四维 -1）。
/// 上限 30 分钟（900 tick），防止 500h 曲线被离线跳过。
/// </summary>
public class OfflineCompensationService
{
    public const int TickIntervalMs = 2000;
    public const int MaxOfflineTicks = 900;

    private readonly AppDbContext _context;
    private readonly MarketService _market;
    private readonly MaintenanceService _maintenance;
    private readonly PlayerService _playerService;
    private readonly CyberCatService _catService;

    public OfflineCompensationService(
        AppDbContext context,
        MarketService market,
        MaintenanceService maintenance,
        PlayerService playerService,
        CyberCatService catService)
    {
        _context = context;
        _market = market;
        _maintenance = maintenance;
        _playerService = playerService;
        _catService = catService;
    }

    public async Task<OfflineResult> ApplyAsync(
        Player player,
        CyberCat cat,
        WorkingPlace workingPlace,
        PlayerHouse house,
        HouseBuffs buffs)
    {
        var now = DateTime.UtcNow;
        if (player.LastActiveAt is null)
        {
            player.LastActiveAt = now;
            await TouchActiveAsync(player);
            return new OfflineResult(0, 0, false, 0, 0, 0, 0, "");
        }

        var elapsed = now - player.LastActiveAt.Value;
        int rawTicks = (int)(elapsed.TotalMilliseconds / TickIntervalMs);
        int ticks = Math.Min(rawTicks, MaxOfflineTicks);
        bool wasCapped = rawTicks > MaxOfflineTicks;
        if (ticks <= 0)
        {
            player.LastActiveAt = now;
            await TouchActiveAsync(player);
            return new OfflineResult(0, rawTicks, false, 0, 0, 0, 0, "");
        }

        workingPlace.Job = player.SelectedWorkJob;
        workingPlace.WorkTickCount = player.WorkTickCount;

        int goldBefore = player.Money;
        int ticketsBefore = player.Backpack.GetValueOrDefault(MarketService.StallTicketItemName);
        int offerInterval = MarketService.NpcOfferIntervalForJob(workingPlace.Job);
        int offerCycles = 0;
        int maintenanceCharges = 0;
        int maintenanceTick = 0;

        for (int i = 0; i < ticks; i++)
        {
            cat.Tick(buffs);

            if (player.IsWorking)
            {
                workingPlace.Tick(player, cat, out bool earnedTicket, buffs);
                if (earnedTicket)
                    await _playerService.GrantBackpackItemAsync(player, MarketService.StallTicketItemName);
            }

            maintenanceTick++;
            if (maintenanceTick >= MaintenanceService.TicksPerGameDay)
            {
                maintenanceTick = 0;
                await _maintenance.TryChargeAsync(player, house, cat);
                maintenanceCharges++;
            }

            if ((i + 1) % offerInterval == 0)
            {
                await _market.TryGenerateNpcOffersAsync(
                    player.Id, cat.Happiness, buffs.NpcOfferChanceMultiplier);
                offerCycles++;
            }
        }

        player.WorkTickCount = workingPlace.WorkTickCount;
        int goldEarned = player.Money - goldBefore;
        int ticketsEarned = player.Backpack.GetValueOrDefault(MarketService.StallTicketItemName) - ticketsBefore;

        await _catService.SaveAsync(cat);
        await _playerService.SaveProgressAsync(player);
        player.LastActiveAt = now;
        await TouchActiveAsync(player);

        string summary = BuildSummary(ticks, rawTicks, wasCapped, goldEarned, ticketsEarned, offerCycles, maintenanceCharges, player.IsWorking);
        return new OfflineResult(ticks, rawTicks, wasCapped, goldEarned, ticketsEarned, offerCycles, maintenanceCharges, summary);
    }

    public async Task TouchActiveAsync(Player player)
    {
        var db = await _context.Players.FindAsync(player.Id);
        if (db is not null)
        {
            db.LastActiveAt = player.LastActiveAt ?? DateTime.UtcNow;
            player.LastActiveAt = db.LastActiveAt;
            await _context.SaveChangesAsync();
        }
    }

    private static string BuildSummary(int ticks, int rawTicks, bool wasCapped, int gold, int tickets, int offers, int maint, bool wasWorking)
    {
        if (ticks <= 0) return "";
        var parts = new List<string> { $"离线补偿 {ticks} tick（≈{ticks * 2 / 60} 分钟）" };
        if (wasCapped)
            parts.Add($"已达 30min 上限（原始 {rawTicks} tick，实际 {ticks} tick，不含钓鱼）");
        if (wasWorking && gold > 0) parts.Add($"打工 +{gold}g");
        if (tickets > 0) parts.Add($"摊位券 +{tickets}");
        if (offers > 0) parts.Add($"市场报价 {offers} 轮");
        if (maint > 0) parts.Add($"维护费结算 {maint} 次");
        return string.Join(" · ", parts);
    }
}
