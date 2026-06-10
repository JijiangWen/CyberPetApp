using CyberPetApp.Data;
using CyberPetApp.Models;

namespace CyberPetApp.Services;

/// <summary>
/// P0-1：合并每 tick 存档为单次 SaveChanges，避免猫/玩家/LastActiveAt/Buff 各自提交。
/// </summary>
public class GamePersistenceService
{
    private readonly AppDbContext _context;
    private readonly CyberCatService _catService;
    private readonly PlayerService _playerService;
    private readonly CatBuffService _catBuffService;

    public GamePersistenceService(
        AppDbContext context,
        CyberCatService catService,
        PlayerService playerService,
        CatBuffService catBuffService)
    {
        _context = context;
        _catService = catService;
        _playerService = playerService;
        _catBuffService = catBuffService;
    }

    /// <summary>游戏心跳（≈2s）：Buff Tick + 猫 + 玩家（含 LastActiveAt），单事务提交。</summary>
    public async Task<List<ActiveCatBuff>> SaveTickAsync(Player player, CyberCat cat, int workTickCount)
    {
        player.WorkTickCount = workTickCount;
        player.LastActiveAt = DateTime.UtcNow;

        var buffs = await _catBuffService.TickBuffsAsync(player.Id, cat, saveChanges: false);
        await _catService.PersistCatForTickAsync(cat);
        _playerService.SyncProgressToTracked(player);
        await _context.SaveChangesAsync();
        return buffs;
    }

    /// <summary>会话结束等场景：猫 + 玩家（含 LastActiveAt），不重复 Tick Buff。</summary>
    public async Task SaveSessionAsync(Player player, CyberCat cat, int workTickCount)
    {
        player.WorkTickCount = workTickCount;
        player.LastActiveAt = DateTime.UtcNow;

        await _catService.SaveAsync(cat, saveChanges: false);
        _playerService.SyncProgressToTracked(player);
        await _context.SaveChangesAsync();
    }
}
