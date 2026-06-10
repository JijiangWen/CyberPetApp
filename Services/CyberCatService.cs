using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class CyberCatService
{
    private readonly AppDbContext _context;

    public CyberCatService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>P0-3：返回 EF 跟踪实体，后续 Tick 直接改属性无需重复查询。</summary>
    public async Task<CyberCat> GetOrCreateAsync(Guid playerId)
    {
        var cat = _context.CyberCats.Local.FirstOrDefault(c => c.PlayerId == playerId)
            ?? await _context.CyberCats.FirstOrDefaultAsync(c => c.PlayerId == playerId);

        if (cat is not null)
        {
            if (cat.MigrateLegacyStats())
                await _context.SaveChangesAsync();
            return cat;
        }

        cat = new CyberCat { PlayerId = playerId };
        _context.CyberCats.Add(cat);
        await _context.SaveChangesAsync();
        return cat;
    }

    /// <summary>P0-3：已跟踪实体跳过 SELECT；saveChanges=false 供 GamePersistenceService 合并提交。</summary>
    public async Task SaveAsync(CyberCat cat, bool saveChanges = true)
    {
        var entry = _context.Entry(cat);
        if (entry.State == EntityState.Detached)
            SyncDetachedToTracked(cat);

        if (saveChanges)
            await _context.SaveChangesAsync();
    }

    /// <summary>P0-3 进阶：tick 合并提交前写猫；已跟踪则交给 SaveChanges，未跟踪则 ExecuteUpdate。</summary>
    public async Task PersistCatForTickAsync(CyberCat cat)
    {
        if (cat.Id == Guid.Empty)
        {
            SyncDetachedToTracked(cat);
            return;
        }

        if (_context.Entry(cat).State != EntityState.Detached)
            return;

        await _context.CyberCats
            .Where(c => c.Id == cat.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Name, cat.Name)
                .SetProperty(c => c.Health, cat.Health)
                .SetProperty(c => c.Happiness, cat.Happiness)
                .SetProperty(c => c.Energy, cat.Energy)
                .SetProperty(c => c.Hunger, cat.Hunger)
                .SetProperty(c => c.Thirst, cat.Thirst)
                .SetProperty(c => c.BackgroundTickCount, cat.BackgroundTickCount)
                .SetProperty(c => c.CatLevel, cat.CatLevel)
                .SetProperty(c => c.CatXp, cat.CatXp)
                .SetProperty(c => c.Str, cat.Str)
                .SetProperty(c => c.Agi, cat.Agi)
                .SetProperty(c => c.Sen, cat.Sen)
                .SetProperty(c => c.Sta, cat.Sta)
                .SetProperty(c => c.Chm, cat.Chm)
                .SetProperty(c => c.Luk, cat.Luk));
    }

    private void SyncDetachedToTracked(CyberCat cat)
    {
        var tracked = _context.CyberCats.Local.FirstOrDefault(c => c.PlayerId == cat.PlayerId);
        if (tracked is null)
        {
            if (cat.Id == Guid.Empty) cat.Id = Guid.NewGuid();
            _context.CyberCats.Add(cat);
            return;
        }

        tracked.Name = cat.Name;
        tracked.Health = cat.Health;
        tracked.Happiness = cat.Happiness;
        tracked.Energy = cat.Energy;
        tracked.Hunger = cat.Hunger;
        tracked.Thirst = cat.Thirst;
        tracked.BackgroundTickCount = cat.BackgroundTickCount;
        tracked.CatLevel = cat.CatLevel;
        tracked.CatXp = cat.CatXp;
        tracked.Str = cat.Str;
        tracked.Agi = cat.Agi;
        tracked.Sen = cat.Sen;
        tracked.Sta = cat.Sta;
        tracked.Chm = cat.Chm;
        tracked.Luk = cat.Luk;
    }

    /// <summary>猫医疗/美容：Health&lt;500 或 Happiness&lt;200 时可治疗，30g 恢复 +200。</summary>
    public async Task<(bool Ok, string Message)> TreatAsync(Player player, CyberCat cat)
    {
        bool needsTreat = cat.Health < EconomySinks.CatTreatHealthThreshold
                          || cat.Happiness < EconomySinks.CatTreatHappinessThreshold;
        if (!needsTreat)
            return (false, "猫咪状态良好，无需治疗");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");
        if (dbPlayer.Money < EconomySinks.CatTreatFee)
            return (false, $"金币不足，治疗需 {EconomySinks.CatTreatFee}g");

        dbPlayer.Money -= EconomySinks.CatTreatFee;
        player.Money = dbPlayer.Money;

        int restore = EconomySinks.CatTreatRestore;
        cat.Health = Math.Min(CyberCat.StatMax, cat.Health + restore);
        cat.Happiness = Math.Min(CyberCat.StatMax, cat.Happiness + restore);
        await SaveAsync(cat);

        return (true, $"治疗完成，扣 {EconomySinks.CatTreatFee}g，健康/幸福各 +{restore}");
    }
}

