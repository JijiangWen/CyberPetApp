using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class FishRecordService
{
    private readonly AppDbContext _context;

    public FishRecordService(AppDbContext context)
    {
        _context = context;
    }

    public static string NormalizeFishName(string name) => TargetFishCatalog.NormalizeFishName(name);

    public async Task RecordCatchAsync(Guid playerId, Fish fish)
    {
        string key = NormalizeFishName(fish.Name);
        bool isTarget = TargetFishCatalog.IsTargetExclusive(key);
        var record = await _context.FishCatchRecords
            .FirstOrDefaultAsync(r => r.PlayerId == playerId && r.FishName == key);

        if (record is null)
        {
            _context.FishCatchRecords.Add(new FishCatchRecord
            {
                PlayerId = playerId,
                FishName = key,
                CatchCount = 1,
                MaxWeight = fish.ActualWeight,
                BestRarity = fish.Rarity,
                IsTargetExclusive = isTarget
            });
        }
        else
        {
            record.CatchCount++;
            if (fish.ActualWeight > record.MaxWeight)
                record.MaxWeight = fish.ActualWeight;
            if ((int)fish.Rarity > (int)record.BestRarity)
                record.BestRarity = fish.Rarity;
            if (isTarget) record.IsTargetExclusive = true;
        }

        await _context.SaveChangesAsync();
    }

    public Task<List<FishCatchRecord>> GetRecordsAsync(Guid playerId) =>
        _context.FishCatchRecords
            .AsNoTracking()
            .Where(r => r.PlayerId == playerId)
            .OrderByDescending(r => r.CatchCount)
            .ThenByDescending(r => r.MaxWeight)
            .ToListAsync();
}
