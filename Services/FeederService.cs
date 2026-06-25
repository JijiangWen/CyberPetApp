using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>
/// 自动喂食器服务类。
/// 负责管理自动喂食器的后台状态与数据库同步，比如玩家往喂食器装填粮食、猫咪自动吃粮食等动作。
/// </summary>
public class FeederService
{
    // 数据库连接对象，用于读写数据库
    private readonly AppDbContext _context;

    // 构造函数，由系统依赖注入（DI）容器自动传入数据库上下文实例
    public FeederService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 加载或者创建玩家的自动喂食器。
    /// 如果玩家是第一次拥有喂食器，数据库没有记录，就会在数据库新建一个；如果有记录，就从数据库里读取并组装成内存中的状态。
    /// </summary>
    public async Task<AutoFeeder> LoadOrCreateAsync(Guid playerId)
    {
        // 1. 在数据库中查询该玩家的自动喂食器主表记录
        var feeder = await _context.AutoFeeders.FirstOrDefaultAsync(a => a.PlayerId == playerId);
        if (feeder is null)
        {
            // 如果查不到，说明需要为新玩家新建一台自动喂食器
            feeder = new AutoFeeder { PlayerId = playerId };
            _context.AutoFeeders.Add(feeder);
            await _context.SaveChangesAsync(); // 保存新建记录
        }

        // 2. 从数据库子表中查询这台喂食器目前装填了哪些食物，并按照槽位顺序（SlotIndex）从小到大排列
        var rows = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderBy(f => f.SlotIndex)
            .ToListAsync();

        // 3. 将数据库里的扁平数据转换成 C# 内存中的 Food 对象列表，赋给喂食器实例
        feeder.Foods = rows.Select(r => new Food(r.Name, r.HungerRestore, r.EnergyRestore, r.HappinessRestore)).ToList();
        feeder.FoodCount = feeder.Foods.Count; // 记录目前装了多少份食物
        return feeder;
    }

    /// <summary>
    /// 从玩家的背包（Backpack）中消耗一份食物，装填进自动喂食器的空槽位中。
    /// 期间会收取 2g 的“装粮手工费”，并且扣减玩家背包中该食物的数量。
    /// </summary>
    public async Task<(bool Ok, string Message)> AddFoodFromBackpackAsync(Player player, AutoFeeder feeder, Food food)
    {
        // 1. 检查喂食器是不是已经塞满了粮食
        if (feeder.FoodCount >= feeder.MaxFoodCount)
            return (false, "喂食器已满");

        // 2. 检查食物是否带有特殊的增益 Buff（如强力药剂或特殊海鲜刺身）
        // 带有持续增益 Buff 的特制食物不能塞入自动喂食器，必须由玩家手动喂食才能生效
        if (CookBook.IsCookedFood(food.Name))
        {
            var recipe = CookBook.RecipeByName(food.Name);
            if (recipe is not null && !recipe.AllowAutoFeeder)
                return (false, $"[{food.Name}] 含有持续 Buff，请手动喂食");
        }

        // 3. 检查玩家身上的钱够不够付装粮的手工加工费（每装一份扣 2g）
        int fee = EconomySinks.FeederProcessingFee;
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");
        if (dbPlayer.Money < fee)
            return (false, $"金币不足，装粮加工费需 {fee}g/份");

        // 4. 检查玩家背包里是不是真的有这份食物
        var item = await _context.BackpackItems
            .FirstOrDefaultAsync(b => b.PlayerId == player.Id && b.ItemName == food.Name);
        if (item is null || item.Quantity <= 0)
            return (false, $"背包里没有 {food.Name}");

        // 5. 扣钱并更新玩家金币
        dbPlayer.Money -= fee;
        player.Money = dbPlayer.Money;

        // 6. 背包中的食物数量减 1，如果减到 0，直接将这行记录从背包表中删掉，节约数据库空间
        item.Quantity--;
        if (item.Quantity <= 0) _context.BackpackItems.Remove(item);

        // 7. 寻找一个空闲的槽位序号（SlotIndex），从 0 开始递增往后找
        int slot = 0;
        var existingSlots = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .Select(f => f.SlotIndex)
            .ToListAsync();
        while (existingSlots.Contains(slot))
        {
            slot++; // 序号冲突就继续往后挪
        }

        // 8. 往数据库子表里塞入一条食物记录
        _context.FeederFoods.Add(new FeederFood
        {
            AutoFeederId = feeder.Id,
            Name = food.Name,
            HungerRestore = food.HungerRestore,
            EnergyRestore = food.EnergyRestore,
            HappinessRestore = food.HappinessRestore,
            SlotIndex = slot
        });

        // 9. 提交事务，保存到 PostgreSQL 中
        await _context.SaveChangesAsync();

        // 10. 同步更新内存中喂食器的口粮列表，并同步扣减玩家身上的内存背包变量
        feeder.AddFood(food);
        int left = player.Backpack.GetValueOrDefault(food.Name) - 1;
        if (left <= 0) player.Backpack.Remove(food.Name);
        else player.Backpack[food.Name] = left;
        
        return (true, $"装入 [{food.Name}]，扣加工费 {fee}g");
    }

    /// <summary>
    /// 取出喂食器槽位中的最后一份食物（即最迟装入的粮食），同步更新数据库。
    /// </summary>
    public async Task<Food?> RemoveLastFoodAsync(AutoFeeder feeder)
    {
        // 查询当前喂食器中槽位号最大的那份食物
        var last = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderByDescending(f => f.SlotIndex)
            .FirstOrDefaultAsync();
        if (last is null) return null;

        // 从数据库中移除这行记录
        _context.FeederFoods.Remove(last);
        await _context.SaveChangesAsync();
        
        // 返回内存中被移除的 Food 对象
        return feeder.RemoveLastFood();
    }

    /// <summary>
    /// 同步并整理喂食器的槽位序号。
    /// 比如猫咪吃了几口，中间的某些槽位空出来了，如果不整理就会有空缝。本方法会把剩余的食物紧凑地重新排列（如 0, 1, 2...）。
    /// </summary>
    public async Task SyncSlotsAfterFeedAsync(AutoFeeder feeder)
    {
        // 1. 查询数据库中该喂食器残留的所有食物
        var rows = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .OrderBy(f => f.SlotIndex)
            .ToListAsync();

        // 2. 获取内存中最新的食物快照
        var foods = feeder.GetFoodsSnapshot();
        
        // 3. 如果发现数据库数量和内存快照数量不一致，说明猫刚刚吃掉了食物，需要用重排后的数据覆盖数据库记录
        if (rows.Count != foods.Count)
        {
            // 清空旧的行
            _context.FeederFoods.RemoveRange(rows);
            // 重新按 0, 1, 2... 序号连续插入
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

    /// <summary>
    /// 辅助方法：检查背包里的某个道具是否适合塞进自动喂食器中。
    /// 规则：必须是烹饪出来的合法日常食物（不含持续Buff），或者是商店卖的常规食物、饮料或小零食。
    /// </summary>
    public static bool TryGetFeederFood(string name, Shop shop, out Food? food)
    {
        food = null;
        
        // A. 如果是烹饪食品
        if (CookBook.IsCookedFood(name))
        {
            var recipe = CookBook.RecipeByName(name);
            if (recipe is not null && recipe.AllowAutoFeeder)
            {
                // 创建一个对应的基础 Food 属性包返回
                food = new Food(recipe.FoodName, recipe.Hunger, recipe.Energy, recipe.Happiness);
                return true;
            }
            return false;
        }

        // B. 如果是商店在售的基础食品/饮料/零食
        var shopItem = shop.ShopItems.FirstOrDefault(s => s.Food.Name == name);
        if (shopItem is not null && (shopItem.Category == "food" || shopItem.Category == "drink" || shopItem.Category == "treat"))
        {
            food = shopItem.Food;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 【一键填满】核心功能：
    /// 扫描玩家背包，自动挑出所有符合条件的常规食物，一口气塞满自动喂食器，省去了玩家一条条点击装填的麻烦。
    /// 收取的金币依然是每份食物 2g。
    /// </summary>
    public async Task<(int LoadedCount, string Message)> BatchAddFoodFromBackpackAsync(Player player, AutoFeeder feeder)
    {
        // 1. 确定本次装填能塞多少个（最大容量减去已有数量）
        int limit = feeder.MaxFoodCount - feeder.FoodCount;
        if (limit <= 0)
            return (0, "喂食器已满");

        // 2. 检查玩家账号
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (0, "玩家不存在");

        // 3. 金币检查：至少有付 1 份加工费的钱
        int feePerUnit = EconomySinks.FeederProcessingFee;
        if (dbPlayer.Money < feePerUnit)
            return (0, $"金币不足，装粮加工费需 {feePerUnit}g/份");

        // 4. 读取玩家背包里所有的物品列表
        var bpItems = await _context.BackpackItems
            .Where(b => b.PlayerId == player.Id)
            .ToListAsync();

        var shop = new Shop();
        int loaded = 0; // 累计成功装填的个数

        // 读取当前喂食器已占用的槽位序号
        var existingSlots = await _context.FeederFoods
            .Where(f => f.AutoFeederId == feeder.Id)
            .Select(f => f.SlotIndex)
            .ToListAsync();

        var foodsToAdd = new List<Food>();
        var itemsToRemove = new List<BackpackItem>();

        // 5. 循环遍历背包物品，只要是食物就往喂食器里倒
        foreach (var item in bpItems)
        {
            if (loaded >= limit) break; // 装满了，打断循环
            if (item.Quantity <= 0) continue;

            // 检查是不是合法猫粮，若是，提取其属性
            if (TryGetFeederFood(item.ItemName, shop, out var food) && food is not null)
            {
                // 计算当前物品最多装入多少份（不能超出玩家背包拥有量，也不能超出喂食器剩余限额）
                int toLoad = Math.Min(item.Quantity, limit - loaded);
                for (int i = 0; i < toLoad; i++)
                {
                    // 扣钱前做一次金币校验
                    if (dbPlayer.Money < feePerUnit)
                        break;

                    dbPlayer.Money -= feePerUnit; // 扣减加工费
                    item.Quantity--; // 背包里减 1 个

                    // 生成空槽位号
                    int slot = 0;
                    while (existingSlots.Contains(slot))
                    {
                        slot++;
                    }
                    existingSlots.Add(slot);

                    // 写入数据库
                    _context.FeederFoods.Add(new FeederFood
                    {
                        AutoFeederId = feeder.Id,
                        Name = food.Name,
                        HungerRestore = food.HungerRestore,
                        EnergyRestore = food.EnergyRestore,
                        HappinessRestore = food.HappinessRestore,
                        SlotIndex = slot
                    });

                    foodsToAdd.Add(food);
                    loaded++;
                }

                // 如果背包里的这摞道具吃光了，放入待清除表
                if (item.Quantity <= 0)
                {
                    itemsToRemove.Add(item);
                }
            }
        }

        // 6. 如果扫描了半天背包里一个能吃的都没有，或者没有金币，直接退出
        if (loaded == 0)
        {
            return (0, "背包里没有可用的食物或饮料，或者金币不足");
        }

        // 7. 在数据库中剔除所有已经消耗干净的背包物品行，优化表体积
        foreach (var item in itemsToRemove)
        {
            _context.BackpackItems.Remove(item);
        }

        // 8. 提交数据库保存事务
        await _context.SaveChangesAsync();

        // 9. 同步更新玩家内存金币和背包缓存，并将刚加入的食物全部塞入内存中的喂食器实例
        player.Money = dbPlayer.Money;
        foreach (var food in foodsToAdd)
        {
            feeder.AddFood(food);
            int left = player.Backpack.GetValueOrDefault(food.Name) - 1;
            if (left <= 0) player.Backpack.Remove(food.Name);
            else player.Backpack[food.Name] = left;
        }

        return (loaded, $"一键装填成功：装入 {loaded} 份食物/饮料，共扣除加工费 {loaded * feePerUnit}g");
    }
}
