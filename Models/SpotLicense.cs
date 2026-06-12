namespace CyberPetApp.Models;

/// <summary>高级钓点许可证（永久或每日租约）。</summary>
public class SpotLicense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string SpotName { get; set; } = "";
    public bool HasPermanent { get; set; }
    /// <summary>当日租约已付费的 UTC 日期（Date 部分）。</summary>
    public DateTime? RentalPaidDate { get; set; }
}

public static class SpotLicenseCatalog
{
    /// <summary>去 IT 化重命名前的钓点名 → 现行名（老存档 SpotLicenses 迁移用）。</summary>
    public static readonly IReadOnlyDictionary<string, string> LegacySpotNameMap =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["小溪"] = "镇外溪流",
            ["神秘深海"] = "近海礁石",
            ["霓虹运河"] = "地下暗河",
            ["霓虹引渠"] = "地下暗河",
            ["彩光水渠"] = "地下暗河",
            ["冰川子网"] = "极光冰湾",
            ["数据港外海"] = "远礁外海",
            ["旧港浅湾"] = "远礁外海",
        };

    /// <summary>需要许可证的钓点（Lv8+ 高级钓点；虚空钓域需全神话）。</summary>
    public static bool RequiresLicense(string spotName) => spotName switch
    {
        "近海礁石" or "芦苇湿地" or "地下暗河" or "深水海湾" or "极光冰湾"
            or "沉船墓场" or "珊瑚暗流" or "远礁外海" or "深渊回廊"
            or "星潮海沟" or "虚空钓域" => true,
        _ => false
    };

    public static bool RequiresAllMythic(string spotName) =>
        spotName == "虚空钓域";

    public static string LicenseHint(string spotName) => spotName switch
    {
        "近海礁石" => $"永久 {EconomySinks.DeepSeaPermanentLicense}g · 日租 {EconomySinks.DeepSeaDailyRental}g/天",
        "芦苇湿地" => $"永久 {EconomySinks.ReedBayPermanentLicense}g · 日租 {EconomySinks.ReedBayDailyRental}g/天",
        "地下暗河" => $"永久 {EconomySinks.NeonCanalPermanentLicense}g · 日租 {EconomySinks.NeonCanalDailyRental}g/天",
        "深水海湾" => $"永久 {EconomySinks.RiftValleyPermanentLicense}g · 日租 {EconomySinks.RiftValleyDailyRental}g/天",
        "极光冰湾" => $"永久 {EconomySinks.GlacierNetPermanentLicense}g · 日租 {EconomySinks.GlacierNetDailyRental}g/天",
        "沉船墓场" => $"永久 {EconomySinks.WreckGravePermanentLicense}g · 日租 {EconomySinks.WreckGraveDailyRental}g/天",
        "珊瑚暗流" => $"永久 {EconomySinks.CoralReefPermanentLicense}g · 日租 {EconomySinks.CoralReefDailyRental}g/天",
        "远礁外海" => $"永久 {EconomySinks.DataPortPermanentLicense}g · 日租 {EconomySinks.DataPortDailyRental}g/天",
        "深渊回廊" => $"永久 {EconomySinks.AbyssCorridorPermanentLicense}g · 日租 {EconomySinks.AbyssCorridorDailyRental}g/天",
        "星潮海沟" => $"永久 {EconomySinks.StarTidePermanentLicense}g · 日租 {EconomySinks.StarTideDailyRental}g/天",
        "虚空钓域" => $"永久 {EconomySinks.VoidDomainPermanentLicense}g · 日租 {EconomySinks.VoidDomainDailyRental}g/天 · 需全神话",
        _ => ""
    };
}
