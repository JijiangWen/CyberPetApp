using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

/// <summary>猫咪等级与属性成长：喂猫/钓鱼成功获得 XP，升级时随机成长属性。</summary>
public class CatProgressionService
{
    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public CatProgressionService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>增加猫经验，升级时随机 +1~3 某属性；返回升级次数与消息。</summary>
    public (int LevelUps, string? Message) AddXp(CyberCat cat, int xp)
    {
        if (xp <= 0) return (0, null);

        cat.CatXp += xp;
        int ups = 0;
        var grown = new List<string>();

        while (cat.CatXp >= CatFishingStatsHelper.XpToNext(cat.CatLevel))
        {
            cat.CatXp -= CatFishingStatsHelper.XpToNext(cat.CatLevel);
            cat.CatLevel++;
            ups++;
            grown.Add(GrowRandomStat(cat));
        }

        string? msg = ups > 0
            ? $"猫咪升至 Lv.{cat.CatLevel}！{string.Join(" · ", grown)}"
            : null;

        return (ups, msg);
    }

    /// <summary>升级时随机选一个未满属性 +1~3。</summary>
    private string GrowRandomStat(CyberCat cat)
    {
        var candidates = new List<(string Key, int Value, Action<int> Set)>
        {
            ("STR", cat.Str, v => cat.Str = v),
            ("AGI", cat.Agi, v => cat.Agi = v),
            ("SEN", cat.Sen, v => cat.Sen = v),
            ("STA", cat.Sta, v => cat.Sta = v),
            ("CHM", cat.Chm, v => cat.Chm = v),
            ("LUK", cat.Luk, v => cat.Luk = v),
        }.Where(c => c.Value < CatFishingStatsHelper.StatMax).ToList();

        if (candidates.Count == 0)
            return "属性已满";

        var pick = candidates[_random.Next(candidates.Count)];
        int gain = _random.Next(1, 4);
        int next = Math.Min(CatFishingStatsHelper.StatMax, pick.Value + gain);
        pick.Set(next);
        return $"{pick.Key}+{next - pick.Value}";
    }

    public async Task SaveAsync(CyberCat cat)
    {
        var existing = await _context.CyberCats.FirstOrDefaultAsync(c => c.PlayerId == cat.PlayerId);
        if (existing is null) return;

        existing.CatLevel = cat.CatLevel;
        existing.CatXp = cat.CatXp;
        existing.Str = cat.Str;
        existing.Agi = cat.Agi;
        existing.Sen = cat.Sen;
        existing.Sta = cat.Sta;
        existing.Chm = cat.Chm;
        existing.Luk = cat.Luk;
        await _context.SaveChangesAsync();
    }
}
