using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class SpotLicenseService
{
    private readonly AppDbContext _context;

    public SpotLicenseService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SpotLicense>> GetLicensesAsync(Guid playerId)
    {
        await MigrateLegacySpotNamesAsync(playerId);
        return await _context.SpotLicenses.Where(l => l.PlayerId == playerId).ToListAsync();
    }

    /// <summary>将老存档钓点许可证名迁移到现行名称，合并重复记录。</summary>
    public async Task MigrateLegacySpotNamesAsync(Guid playerId)
    {
        var licenses = await _context.SpotLicenses
            .Where(l => l.PlayerId == playerId)
            .ToListAsync();

        bool changed = false;
        foreach (var lic in licenses.ToList())
        {
            if (!SpotLicenseCatalog.LegacySpotNameMap.TryGetValue(lic.SpotName, out var newName))
                continue;

            var existing = licenses.FirstOrDefault(l => l.SpotName == newName && l.Id != lic.Id);
            if (existing is not null)
            {
                existing.HasPermanent |= lic.HasPermanent;
                if (lic.RentalPaidDate.HasValue
                    && (!existing.RentalPaidDate.HasValue || lic.RentalPaidDate > existing.RentalPaidDate))
                    existing.RentalPaidDate = lic.RentalPaidDate;
                _context.SpotLicenses.Remove(lic);
                licenses.Remove(lic);
            }
            else
            {
                lic.SpotName = newName;
            }
            changed = true;
        }

        if (changed)
            await _context.SaveChangesAsync();
    }

    public async Task<bool> HasAccessAsync(Guid playerId, string spotName)
    {
        if (!SpotLicenseCatalog.RequiresLicense(spotName))
            return true;

        var lic = await _context.SpotLicenses
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.SpotName == spotName);
        if (lic is null) return false;
        if (lic.HasPermanent) return true;

        var today = DateTime.UtcNow.Date;
        return lic.RentalPaidDate.HasValue && lic.RentalPaidDate.Value.Date == today;
    }

    public async Task<(bool Ok, string Message)> TryBuyPermanentAsync(Player player, string spotName)
    {
        if (!SpotLicenseCatalog.RequiresLicense(spotName))
            return (false, "该钓点无需许可证");

        int cost = EconomySinks.SpotPermanentLicenseCost(spotName);
        if (cost <= 0) return (false, "无效钓点");

        var lic = await GetOrCreateLicenseAsync(player.Id, spotName);
        if (lic.HasPermanent)
            return (false, "已持有永久许可证");

        if (!await TrySpendAsync(player, cost))
            return (false, $"金币不足，永久许可证需 {cost}g");

        lic.HasPermanent = true;
        await _context.SaveChangesAsync();
        return (true, $"已购买 [{spotName}] 永久许可证，扣 {cost}g");
    }

    public async Task<(bool Ok, string Message)> TryPayDailyRentalAsync(Player player, string spotName)
    {
        if (!SpotLicenseCatalog.RequiresLicense(spotName))
            return (false, "该钓点无需租约");

        int cost = EconomySinks.SpotDailyRentalCost(spotName);
        var lic = await GetOrCreateLicenseAsync(player.Id, spotName);
        if (lic.HasPermanent)
            return (false, "已持有永久许可证，无需日租");

        var today = DateTime.UtcNow.Date;
        if (lic.RentalPaidDate.HasValue && lic.RentalPaidDate.Value.Date == today)
            return (false, "今日租约已生效");

        if (!await TrySpendAsync(player, cost))
            return (false, $"金币不足，日租需 {cost}g");

        lic.RentalPaidDate = today;
        await _context.SaveChangesAsync();
        return (true, $"已支付 [{spotName}] 今日租约 {cost}g，可钓鱼至 UTC 日末");
    }

    public static string AccessStatus(SpotLicense? lic, string spotName)
    {
        if (!SpotLicenseCatalog.RequiresLicense(spotName))
            return "免费开放";

        if (lic is null) return "未授权 · 需许可证";
        if (lic.HasPermanent) return "永久许可证 ✓";
        var today = DateTime.UtcNow.Date;
        if (lic.RentalPaidDate.HasValue && lic.RentalPaidDate.Value.Date == today)
            return $"日租有效 · 至今日 UTC 24:00";
        return "租约过期 · 请续费或购永久证";
    }

    private async Task<SpotLicense> GetOrCreateLicenseAsync(Guid playerId, string spotName)
    {
        var lic = await _context.SpotLicenses
            .FirstOrDefaultAsync(l => l.PlayerId == playerId && l.SpotName == spotName);
        if (lic is not null) return lic;

        lic = new SpotLicense { PlayerId = playerId, SpotName = spotName };
        _context.SpotLicenses.Add(lic);
        await _context.SaveChangesAsync();
        return lic;
    }

    private async Task<bool> TrySpendAsync(Player player, int amount)
    {
        var db = await _context.Players.FindAsync(player.Id);
        if (db is null || db.Money < amount) return false;
        db.Money -= amount;
        player.Money = db.Money;
        await _context.SaveChangesAsync();
        return true;
    }
}
