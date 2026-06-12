using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>
/// 钓鱼相关的数据库服务。
/// 只负责鱼的持久化，不处理定时器或随机逻辑（那些由 FishingManager 负责）。
/// </summary>
public class FishingService
{
    private readonly AppDbContext _context;
    private readonly GearMaterialService _materials;
    private readonly PlayerService _playerService;

    public FishingService(AppDbContext context, GearMaterialService materials, PlayerService playerService)
    {
        _context = context;
        _materials = materials;
        _playerService = playerService;
    }

    /// <summary>
    /// 确保玩家记录存在于数据库（保存鱼之前需要先调用）。
    /// </summary>
    public async Task EnsurePlayerAsync(Player player)
    {
        if (await _context.Players.AnyAsync(p => p.Id == player.Id))
            return;

        _context.Players.Add(new Player
        {
            Id = player.Id,
            Money = player.Money,
            IsWorking = player.IsWorking,
            FishBackpackCapacity = 50
        });
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 把钓到的鱼写入数据库，并关联到指定玩家。
    /// </summary>
    /// <param name="playerGuid">玩家主键（Player.Id，Guid 类型）</param>
    /// <param name="caughtFish">FishingSpot.FishRoll() 生成的鱼实例</param>
    public async Task SaveCaughtFishAsync(Guid playerGuid, Fish caughtFish)
    {
        // 确保鱼有唯一 Id（Fish 构造函数里已 Guid.NewGuid()，这里兜底）
        if (caughtFish.Id == Guid.Empty)
            caughtFish.Id = Guid.NewGuid();

        // 关联玩家外键
        caughtFish.PlayerId = playerGuid;

        // 加入 EF 跟踪队列
        _context.Fishes.Add(caughtFish);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 从数据库读取某玩家的所有鱼（按钓获时间可后续加 CaughtAt 字段再排序）。
    /// </summary>
    public Task<List<Fish>> GetPlayerFishesAsync(Guid playerGuid) =>
        _context.Fishes
            .Where(f => f.PlayerId == playerGuid)
            .ToListAsync();

    /// <summary>
    /// 持久化直售结果（内存状态已由页面乐观更新，此处不再重复加钱）。
    /// </summary>
    public async Task PersistSellFishAsync(Player player, Fish fish)
    {
        await _materials.GrantRecycleBonusAsync(player, fish);

        var dbFish = _context.Fishes.Local.FirstOrDefault(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is not null)
            _context.Fishes.Remove(dbFish);
        else
            _context.Entry(new Fish { Id = fish.Id, PlayerId = player.Id }).State = EntityState.Deleted;

        _playerService.SyncProgressToTracked(player);
        await _context.SaveChangesAsync();
    }
}