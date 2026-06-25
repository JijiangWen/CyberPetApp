using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>自动饮水器持久化：WatererWater 表 ↔ 运行时 AutoWaterer。</summary>
public class WatererService
{
    private readonly AppDbContext _context;

    public WatererService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AutoWaterer> LoadOrCreateAsync(Guid playerId)
    {
        var waterer = await _context.AutoWaterers.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (waterer is null)
        {
            waterer = new AutoWaterer { PlayerId = playerId };
            _context.AutoWaterers.Add(waterer);
            await _context.SaveChangesAsync();
        }

        var rows = await _context.WatererWaters
            .Where(w => w.AutoWatererId == waterer.Id)
            .OrderBy(w => w.SlotIndex)
            .ToListAsync();

        waterer.Waters = rows.Select(r => new WaterItem(r.Name, r.ThirstRestore)).ToList();
        return waterer;
    }

    public async Task<(bool Ok, string Message)> AddWaterFromBackpackAsync(Player player, AutoWaterer waterer, WaterItem water)
    {
        if (waterer.WaterCount >= waterer.MaxWaterCount)
            return (false, "饮水器已满");

        int fee = EconomySinks.WatererProcessingFee;
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");
        if (dbPlayer.Money < fee)
            return (false, $"金币不足，装水加工费需 {fee}g/份");

        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == water.Name);
        if (item is null || item.Quantity <= 0)
            return (false, $"背包里没有 {water.Name}");

        dbPlayer.Money -= fee;
        player.Money = dbPlayer.Money;

        item.Quantity--;
        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);

        int slot = 0;
        var existingSlots = await _context.WatererWaters
            .Where(w => w.AutoWatererId == waterer.Id)
            .Select(w => w.SlotIndex)
            .ToListAsync();
        while (existingSlots.Contains(slot))
        {
            slot++;
        }

        _context.WatererWaters.Add(new WatererWater
        {
            AutoWatererId = waterer.Id,
            Name = water.Name,
            ThirstRestore = water.ThirstRestore,
            SlotIndex = slot
        });

        await _context.SaveChangesAsync();

        waterer.AddWater(water);
        int left = player.Backpack.GetValueOrDefault(water.Name) - 1;
        if (left <= 0) player.Backpack.Remove(water.Name);
        else player.Backpack[water.Name] = left;
        return (true, $"装入 [{water.Name}]，扣加工费 {fee}g");
    }

    public async Task<WaterItem?> RemoveLastWaterAsync(AutoWaterer waterer)
    {
        var last = await _context.WatererWaters
            .Where(w => w.AutoWatererId == waterer.Id)
            .OrderByDescending(w => w.SlotIndex)
            .FirstOrDefaultAsync();
        if (last is null) return null;

        _context.WatererWaters.Remove(last);
        await _context.SaveChangesAsync();
        return waterer.RemoveLastWater();
    }

    public async Task SyncSlotsAfterWaterAsync(AutoWaterer waterer)
    {
        var rows = await _context.WatererWaters
            .Where(w => w.AutoWatererId == waterer.Id)
            .OrderBy(w => w.SlotIndex)
            .ToListAsync();

        var waters = waterer.GetWatersSnapshot();
        if (rows.Count != waters.Count)
        {
            _context.WatererWaters.RemoveRange(rows);
            for (int i = 0; i < waters.Count; i++)
            {
                var w = waters[i];
                _context.WatererWaters.Add(new WatererWater
                {
                    AutoWatererId = waterer.Id,
                    Name = w.Name,
                    ThirstRestore = w.ThirstRestore,
                    SlotIndex = i
                });
            }
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>一键将背包中的凉白开水批量装填到自动饮水器中（扣加工费 1g/份）。</summary>
    public async Task<(int LoadedCount, string Message)> BatchAddWaterFromBackpackAsync(Player player, AutoWaterer waterer)
    {
        int limit = waterer.MaxWaterCount - waterer.WaterCount;
        if (limit <= 0)
            return (0, "饮水器已满");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (0, "玩家不存在");

        int feePerUnit = EconomySinks.WatererProcessingFee;
        if (dbPlayer.Money < feePerUnit)
            return (0, $"金币不足，装水加工费需 {feePerUnit}g/份");

        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == WaterCatalog.Purified.Name);
        if (item is null || item.Quantity <= 0)
            return (0, $"背包里没有 {WaterCatalog.Purified.Name}");

        int toLoad = Math.Min(item.Quantity, limit);
        int loaded = 0;

        var existingSlots = await _context.WatererWaters
            .Where(w => w.AutoWatererId == waterer.Id)
            .Select(w => w.SlotIndex)
            .ToListAsync();

        var water = WaterCatalog.Purified;

        for (int i = 0; i < toLoad; i++)
        {
            if (dbPlayer.Money < feePerUnit)
                break;

            dbPlayer.Money -= feePerUnit;
            item.Quantity--;

            int slot = 0;
            while (existingSlots.Contains(slot))
            {
                slot++;
            }
            existingSlots.Add(slot);

            _context.WatererWaters.Add(new WatererWater
            {
                AutoWatererId = waterer.Id,
                Name = water.Name,
                ThirstRestore = water.ThirstRestore,
                SlotIndex = slot
            });

            waterer.AddWater(water);
            loaded++;
        }

        if (item.Quantity <= 0)
        {
            _context.BackpackItems.Remove(item);
        }

        await _context.SaveChangesAsync();

        // 同步内存状态
        player.Money = dbPlayer.Money;
        int left = player.Backpack.GetValueOrDefault(water.Name) - loaded;
        if (left <= 0) player.Backpack.Remove(water.Name);
        else player.Backpack[water.Name] = left;

        return (loaded, $"一键装填成功：装入 {loaded} 份凉白开水，共扣除加工费 {loaded * feePerUnit}g");
    }
}
