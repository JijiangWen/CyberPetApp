using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public record FishLeaderboardEntry(string Username, string FishName, double Weight, FishRarity Rarity);
public record LevelLeaderboardEntry(string Username, int FishingLevel, int TotalCatches);
public record ServerFishingStats(int TotalCatches, int LegendaryCatches, int ActiveFishers);

/// <summary>全服异步联机 MVP：钓鱼排行榜与协作战报（读 DB 聚合，无实时对战）。</summary>
public class LeaderboardService
{
    private readonly AppDbContext _context;

    public LeaderboardService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>全服单鱼最大体重 Top N。</summary>
    public async Task<List<FishLeaderboardEntry>> GetTopFishAsync(int limit = 10)
    {
        var rows = await _context.FishCatchRecords
            .AsNoTracking()
            .OrderByDescending(r => r.MaxWeight)
            .Take(limit)
            .ToListAsync();

        var playerIds = rows.Select(r => r.PlayerId).Distinct().ToList();
        var names = await _context.GameAccounts
            .AsNoTracking()
            .Where(a => playerIds.Contains(a.PlayerId))
            .ToDictionaryAsync(a => a.PlayerId, a => a.Username);

        return rows.Select(r => new FishLeaderboardEntry(
            names.GetValueOrDefault(r.PlayerId, "???"),
            r.FishName,
            r.MaxWeight,
            r.BestRarity)).ToList();
    }

    /// <summary>钓鱼等级 Top N（附累计钓获数）。</summary>
    public async Task<List<LevelLeaderboardEntry>> GetTopLevelsAsync(int limit = 10)
    {
        var topPlayers = await _context.Players
            .AsNoTracking()
            .OrderByDescending(p => p.FishingLevel)
            .ThenByDescending(p => p.FishingXp)
            .Take(limit)
            .ToListAsync();

        var playerIds = topPlayers.Select(p => p.Id).ToList();
        var names = await _context.GameAccounts
            .AsNoTracking()
            .Where(a => playerIds.Contains(a.PlayerId))
            .ToDictionaryAsync(a => a.PlayerId, a => a.Username);

        var catchCounts = await _context.FishCatchRecords
            .AsNoTracking()
            .Where(r => playerIds.Contains(r.PlayerId))
            .GroupBy(r => r.PlayerId)
            .Select(g => new { PlayerId = g.Key, Total = g.Sum(x => x.CatchCount) })
            .ToDictionaryAsync(x => x.PlayerId, x => x.Total);

        return topPlayers.Select(p => new LevelLeaderboardEntry(
            names.GetValueOrDefault(p.Id, "???"),
            p.FishingLevel,
            catchCounts.GetValueOrDefault(p.Id, 0))).ToList();
    }

    /// <summary>协作战报：全服累计钓获与传说鱼数。</summary>
    public async Task<ServerFishingStats> GetServerStatsAsync()
    {
        int totalCatches = await _context.FishCatchRecords.AsNoTracking().SumAsync(r => (int?)r.CatchCount) ?? 0;
        int legendary = await _context.Players.AsNoTracking().SumAsync(p => (int?)p.LegendaryCatchCount) ?? 0;
        int activeFishers = await _context.Players.AsNoTracking().CountAsync(p => p.FishingLevel > 1 || p.FishingXp > 0);
        return new ServerFishingStats(totalCatches, legendary, activeFishers);
    }
}
