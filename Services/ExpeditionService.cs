using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>
/// 猫咪派遣：消耗 Energy，计时结束后领取随机材料/金币。
/// 与钓鱼并行——猫外出时仍可在后台 tick，归来需喂食恢复。
/// </summary>
public class ExpeditionService
{
    private readonly AppDbContext _context;
    private readonly PlayerService _playerService;
    private readonly CyberCatService _catService;
    private readonly Random _random = new();

    public ExpeditionService(AppDbContext context, PlayerService playerService, CyberCatService catService)
    {
        _context = context;
        _playerService = playerService;
        _catService = catService;
    }

    public ExpeditionStatus GetStatus(Player player)
    {
        var zone = ExpeditionCatalog.Find(player.ExpeditionZoneId);
        if (zone is null || player.ExpeditionEndsAt is null)
            return new ExpeditionStatus(false, null, null, TimeSpan.Zero, false);

        var remaining = player.ExpeditionEndsAt.Value - DateTime.UtcNow;
        if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;

        return new ExpeditionStatus(true, zone, player.ExpeditionEndsAt, remaining, remaining <= TimeSpan.Zero);
    }

    public async Task<(bool Ok, string Message)> StartAsync(
        Player player, CyberCat cat, string zoneId, HouseBuffs houseBuffs = default)
    {
        if (GetStatus(player).IsActive)
            return (false, "已有进行中的派遣，请先领取奖励");

        var zone = ExpeditionCatalog.Find(zoneId);
        if (zone is null)
            return (false, "未知派遣区域");

        if (cat.Energy < zone.EnergyCost)
            return (false, $"Energy 不足，需 {zone.EnergyCost}（当前 {cat.Energy}）");

        if (cat.Happiness < zone.MinCatHappiness)
            return (false, $"猫幸福需 ≥{zone.MinCatHappiness} 才愿出门（当前 {cat.Happiness}）");

        if (cat.Hunger < 200)
            return (false, "猫太饿了，先喂食再派遣");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        cat.ApplyActivityCost(CatActivityType.ExpeditionStart, houseBuffs);
        cat.Energy = Math.Max(0, cat.Energy - zone.EnergyCost);
        await _catService.SaveAsync(cat);

        var endsAt = DateTime.UtcNow.AddMinutes(zone.DurationMinutes);
        dbPlayer.ExpeditionZoneId = zone.Id;
        dbPlayer.ExpeditionEndsAt = endsAt;
        await _context.SaveChangesAsync();

        player.ExpeditionZoneId = zone.Id;
        player.ExpeditionEndsAt = endsAt;

        return (true, $"{cat.Name} 前往【{zone.Name}】，预计 {zone.DurationMinutes}min 后归来");
    }

    public async Task<ExpeditionResult> TryClaimAsync(
        Player player, CyberCat cat, HouseBuffs houseBuffs = default)
    {
        var status = GetStatus(player);
        if (!status.IsActive)
            return new ExpeditionResult(false, "当前没有派遣任务");

        if (!status.CanClaim)
            return new ExpeditionResult(false, $"派遣进行中，剩余 {FormatRemaining(status.Remaining)}");

        var zone = status.Zone!;
        int gold = _random.Next(zone.GoldMin, zone.GoldMax + 1);
        int lootQty = _random.Next(zone.LootMin, zone.LootMax + 1);

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return new ExpeditionResult(false, "玩家不存在");

        dbPlayer.Money += gold;
        dbPlayer.ExpeditionZoneId = null;
        dbPlayer.ExpeditionEndsAt = null;
        player.Money = dbPlayer.Money;
        player.ExpeditionZoneId = null;
        player.ExpeditionEndsAt = null;

        await _playerService.GrantBackpackItemAsync(player, zone.PrimaryLoot, lootQty);
        string lootNote = $"{zone.PrimaryLoot}×{lootQty}";
        if (zone.Id is "ruins" or "datalab" or "black_alley" && _random.NextDouble() < 0.55)
        {
            int gearQty = _random.Next(1, 3);
            await _playerService.GrantBackpackItemAsync(player, AlchemyMaterials.GearSet, gearQty);
            lootNote += $" · {AlchemyMaterials.GearSet}×{gearQty}";
        }
        if (zone.Id == "coral_outpost" && _random.NextDouble() < 0.45)
        {
            int glowQty = _random.Next(1, 3);
            await _playerService.GrantBackpackItemAsync(player, AlchemyMaterials.CanalGlowPowder, glowQty);
            lootNote += $" · {AlchemyMaterials.CanalGlowPowder}×{glowQty}";
        }
        if (zone.Id == "wreck_salvage" && _random.NextDouble() < 0.40)
        {
            int gelQty = _random.Next(1, 2);
            await _playerService.GrantBackpackItemAsync(player, AlchemyMaterials.AbyssGel, gelQty);
            lootNote += $" · {AlchemyMaterials.AbyssGel}×{gelQty}";
        }
        await _context.SaveChangesAsync();

        cat.ApplyActivityCost(CatActivityType.ExpeditionReturn, houseBuffs);
        await _catService.SaveAsync(cat);

        return new ExpeditionResult(
            true,
            $"【{zone.Name}】归来！+{gold}g · {lootNote}（猫饥饿 -80，记得喂食）",
            gold,
            zone.PrimaryLoot,
            lootQty);
    }

    /// <summary>登录/刷新时自动完成已到期派遣（仅标记可领，不自动发奖）。</summary>
    public Task<ExpeditionResult?> PeekExpiredAsync(Player player) =>
        Task.FromResult<ExpeditionResult?>(GetStatus(player).CanClaim ? new ExpeditionResult(true, "派遣已完成，可领取") : null);

    public static string FormatRemaining(TimeSpan t)
    {
        if (t.TotalHours >= 1)
            return $"{(int)t.TotalHours}h {t.Minutes}m";
        if (t.TotalMinutes >= 1)
            return $"{(int)t.TotalMinutes}m {t.Seconds}s";
        return $"{(int)t.TotalSeconds}s";
    }
}
