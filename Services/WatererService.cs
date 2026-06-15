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
}
