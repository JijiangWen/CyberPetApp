using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>自动喂食器持久化：FeederFood 表 ↔ 运行时 AutoFeeder。</summary>
public class FeederService
{
    private readonly AppDbContext _context;

    public FeederService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AutoFeeder> LoadOrCreateAsync(Guid playerId)
    {
        var feeder = await _context.AutoFeeders.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (feeder is null)
        {
            feeder = new AutoFeeder { PlayerId = playerId };
            _context.AutoFeeders.Add(feeder);
            await _context.SaveChangesAsync();
        }

        var rows = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderBy(f => f.SlotIndex)
            .ToListAsync();

        feeder.Foods = rows.Select(r => new Food(r.Name, r.HungerRestore, r.EnergyRestore, r.HappinessRestore)).ToList();
        feeder.FoodCount = feeder.Foods.Count;
        return feeder;
    }

    /// <summary>从背包消耗一份食物装入喂食器并持久化（扣加工费 2g/份）。</summary>
    public async Task<(bool Ok, string Message)> AddFoodFromBackpackAsync(Player player, AutoFeeder feeder, Food food)
    {
        if (feeder.FoodCount >= feeder.MaxFoodCount)
            return (false, "喂食器已满");

        if (CookBook.IsCookedFood(food.Name))
        {
            var recipe = CookBook.RecipeByName(food.Name);
            if (recipe is not null && !recipe.AllowAutoFeeder)
                return (false, $"[{food.Name}] 含持续 Buff，请手动喂食");
        }

        int fee = EconomySinks.FeederProcessingFee;
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");
        if (dbPlayer.Money < fee)
            return (false, $"金币不足，装粮加工费需 {fee}g/份");

        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == food.Name);
        if (item is null || item.Quantity <= 0)
            return (false, $"背包里没有 {food.Name}");

        dbPlayer.Money -= fee;
        player.Money = dbPlayer.Money;

        item.Quantity--;
        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);

        int slot = 0;
        var existingSlots = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .Select(f => f.SlotIndex)
            .ToListAsync();
        while (existingSlots.Contains(slot))
        {
            slot++;
        }

        _context.FeederFoods.Add(new FeederFood
        {
            AutoFeederId = feeder.Id,
            Name = food.Name,
            HungerRestore = food.HungerRestore,
            EnergyRestore = food.EnergyRestore,
            HappinessRestore = food.HappinessRestore,
            SlotIndex = slot
        });

        await _context.SaveChangesAsync();

        feeder.AddFood(food);
        int left = player.Backpack.GetValueOrDefault(food.Name) - 1;
        if (left <= 0) player.Backpack.Remove(food.Name);
        else player.Backpack[food.Name] = left;
        return (true, $"装入 [{food.Name}]，扣加工费 {fee}g");
    }

    /// <summary>移除最后一个槽位并同步 DB。</summary>
    public async Task<Food?> RemoveLastFoodAsync(AutoFeeder feeder)
    {
        var last = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderByDescending(f => f.SlotIndex)
            .FirstOrDefaultAsync();
        if (last is null) return null;

        _context.FeederFoods.Remove(last);
        await _context.SaveChangesAsync();
        return feeder.RemoveLastFood();
    }

    /// <summary>喂食后重排 SlotIndex（高饱腹优先取走后可能乱序，按当前列表重写）。</summary>
    public async Task SyncSlotsAfterFeedAsync(AutoFeeder feeder)
    {
        var rows = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderBy(f => f.SlotIndex)
            .ToListAsync();

        var foods = feeder.GetFoodsSnapshot();
        if (rows.Count != foods.Count)
        {
            _context.FeederFoods.RemoveRange(rows);
            for (int i = 0; i < foods.Count; i++)
            {
                var f = foods[i];
                _context.FeederFoods.Add(new FeederFood
                {
                    AutoFeederId = feeder.Id,
                    Name = f.Name,
                    HungerRestore = f.HungerRestore,
                    EnergyRestore = f.EnergyRestore,
                    HappinessRestore = f.HappinessRestore,
                    SlotIndex = i
                });
            }
            await _context.SaveChangesAsync();
        }
    }
}
