namespace CyberPetApp.Models;

/// <summary>装备词条：同阶不同 build，数值在目录定义时按系数预烘焙。</summary>
public enum GearAffix
{
    Balanced,
    CastRange,
    Sensitivity,
    Durability,
    MythicRate,
    RarityBoost
}

public static class GearAffixHelper
{
    public static string Label(GearAffix affix) => affix switch
    {
        GearAffix.CastRange => "抛距型",
        GearAffix.Sensitivity => "灵敏型",
        GearAffix.Durability => "耐久型",
        GearAffix.MythicRate => "神话率型",
        GearAffix.RarityBoost => "稀有加成型",
        _ => "均衡型"
    };

    /// <summary>耐久型装备磨损减免系数。</summary>
    public static double WearMultiplier(GearAffix affix) =>
        affix == GearAffix.Durability ? 0.72 : 1.0;
}
