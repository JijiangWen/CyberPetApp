using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Home;

internal static class HomeUiHelpers
{
    public static string ABar(int value, int max = CyberCat.StatMax, int w = 10)
    {
        var f = Math.Clamp((int)Math.Round((double)value / max * w), 0, w);
        return new string('█', f) + new string('░', w - f);
    }

    public static string FishSpriteClass(string name, FishRarity rarity) =>
        $"{SpriteCatalog.FishSheet(name)} {SpriteCatalog.Fish(name, rarity)}";

    public static string FishSpriteStyle(string name)
    {
        var normName = name.StartsWith("超规格·", System.StringComparison.Ordinal) ? name["超规格·".Length..] : name;
        var safeName = normName
            .Replace("\"", "")
            .Replace("“", "")
            .Replace("”", "")
            .Replace(":", "")
            .Replace("*", "")
            .Replace("?", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace("|", "");
        return $"background-image: url('/assets/fish/{safeName}.png?v=4'); background-size: contain; background-position: center; background-repeat: no-repeat;";
    }

    public static string GearSpriteUrl(string prefix, string name)
    {
        if (string.IsNullOrEmpty(name)) return "";
        var safeName = name
            .Replace("\"", "").Replace("“", "").Replace("”", "")
            .Replace(":", "").Replace("*", "").Replace("?", "")
            .Replace("<", "").Replace(">", "").Replace("|", "");
        return $"/assets/gear/{prefix}_{safeName}.png?v=1";
    }

    public static string GearTierClass(string? gearName)
    {
        if (string.IsNullOrEmpty(gearName)) return "gear-tier-t1";
        int tier = GearProgressionCatalog.GetGearTier(gearName);
        return $"gear-tier-t{tier}";
    }

    public static string RarityClass(FishRarity r) => r switch
    {
        FishRarity.Rare => "rarity-rare",
        FishRarity.Epic => "rarity-epic",
        FishRarity.Legendary => "rarity-legendary",
        _ => "rarity-common"
    };

    public static string RarityLabel(FishRarity r) => r switch
    {
        FishRarity.Rare => "稀有",
        FishRarity.Epic => "史诗",
        FishRarity.Legendary => "传说",
        _ => "普通"
    };

    public static string FishingStateBadge(FishingManager manager) => manager.State switch
    {
        FishingState.Waiting => "WAITING 等口",
        FishingState.Biting => "BITING 咬钩!",
        FishingState.Reeling => "REELING 遛鱼!",
        _ => "IDLE"
    };

    public static string PhaseLabel(FishingManager manager) => manager.State switch
    {
        FishingState.Waiting => "wait",
        FishingState.Biting => "hook",
        FishingState.Reeling => "reel",
        _ => "-"
    };

    public static string PhaseBar(FishingManager manager)
    {
        double total = Math.Max(0.001, manager.PhaseTotalSeconds);
        double done = total - manager.PhaseRemainingSeconds;
        return ABar((int)(done * 100), (int)(total * 100));
    }

    public static string OfferExpiryText(NpcOffer offer)
    {
        var remaining = offer.ExpiresAt - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero) return "已过期";
        if (remaining.TotalMinutes >= 1)
            return $"{(int)remaining.TotalMinutes}分{remaining.Seconds:D2}秒";
        return $"{Math.Max(0, (int)remaining.TotalSeconds)}秒";
    }

    public static string AlchemyMaterialIcon(AlchemyMaterialKind kind) => kind switch
    {
        AlchemyMaterialKind.Fish => "🐟",
        AlchemyMaterialKind.Backpack => "📦",
        AlchemyMaterialKind.Gold => "💰",
        _ => "·"
    };
}
