namespace CyberPetApp.Models;

/// <summary>雪碧图 CSS 类名映射（furniture-set / fish-set）。</summary>
public static class SpriteCatalog
{
    public static string Furniture(string furnitureId) => furnitureId switch
    {
        "Sofa" => "furn-sofa",
        "TV" => "furn-tv",
        "CatToy" => "furn-cattoy",
        "JoyPad" => "furn-joypad",
        "WaterDispenser" or "WaterFountain" => "furn-water",
        "FishTank" => "furn-fishtank",
        "SunLamp" => "furn-sunlamp",
        "AromaDiffuser" => "furn-aroma",
        "LuxuryTower" => "furn-tower",
        "Fridge" => "furn-fridge",
        "Stove" => "furn-stove",
        "AutoFeederUnit" => "furn-feeder",
        "Bed" => "furn-bed",
        "CozyBed" => "furn-cozybed",
        "Toilet" => "furn-toilet",
        "Sink" => "furn-sink",
        "Garden" => "furn-garden",
        _ => "furn-sofa"
    };

    /// <summary>
    /// 8×4=32格鱼雪碧图映射。
    /// Row0(01-08):淡水普通  Row1(09-16):海洋中级  Row2(17-24):荧光奇幻  Row3(25-32):神话宇宙
    /// </summary>
    public static string Fish(string name, FishRarity rarity)
    {
        // ── 神话层 (Row3: fish-25~32) ──────────────────────────────
        if (name.Contains("神话"))
        {
            if (name.Contains("镜湖神鲤") || name.Contains("翠影鳗王") || name.Contains("废弃鱼塘幻鳞"))  return "fish-25";
            if (name.Contains("雾海古神") || name.Contains("芦苇幽歌"))                                return "fish-26";
            if (name.Contains("引渠幻龙") || name.Contains("深水海湾兽"))                              return "fish-26";
            if (name.Contains("极光霜龙"))                                                             return "fish-28";
            if (name.Contains("沉船亡魂"))                                                             return "fish-27";
            if (name.Contains("珊瑚心海"))                                                             return "fish-29";
            if (name.Contains("深渊巡礼"))                                                             return "fish-30";
            if (name.Contains("星潮巨兽"))                                                             return "fish-31";
            if (name.Contains("远海沧龙") || name.Contains("沧龙"))                                    return "fish-30";
            if (name.Contains("金鳞海皇") || name.Contains("海皇"))                                    return "fish-29";
            if (name.Contains("虚空钓主"))                                                             return "fish-30";
            if (name.Contains("终焉鲸歌"))                                                             return "fish-31";
            return "fish-32";
        }

        // ── 荧光/奇幻层 (Row2: fish-17~24) ────────────────────────
        // 星潮/潮汐/星尘 — 星形发光生物
        if (name.Contains("星潮") || name.Contains("星尘巡游"))                                        return "fish-24";
        if (name.Contains("潮汐"))                        return rarity >= FishRarity.Epic ? "fish-24" : "fish-23";
        // 虚空/终焉/裂隙 — 暗紫虚空
        if (name.Contains("虚空") || name.Contains("终焉") || name.Contains("裂隙"))
            return rarity >= FishRarity.Legendary ? "fish-32" : rarity >= FishRarity.Epic ? "fish-21" : "fish-20";
        // 深渊回廊/凝胶 — 深渊安康/巨口
        if (name.Contains("回廊") || name.Contains("凝胶") || name.Contains("安康"))
            return rarity >= FishRarity.Legendary ? "fish-30" : rarity >= FishRarity.Epic ? "fish-20" : "fish-19";
        // 墓场/亡魂/锈/幽灵 — 苍白幽灵鱼
        if (name.Contains("墓场") || name.Contains("亡魂") || name.Contains("幽灵") || name.Contains("锈"))
            return rarity >= FishRarity.Epic ? "fish-19" : "fish-18";
        // 冰/霜/寒/雪/晶/极光 — 冰晶鱼
        if (name.Contains("冰") || name.Contains("霜") || name.Contains("寒") || name.Contains("雪") ||
            name.Contains("晶") || name.Contains("冻") || name.Contains("极光") || name.Contains("归潮"))
            return rarity >= FishRarity.Legendary ? "fish-28" : rarity >= FishRarity.Epic ? "fish-18" : "fish-17";
        // 夜光/荧光/彩光/流光/霁光/磷光 — 荧光鱼
        if (name.Contains("夜光") || name.Contains("荧光") || name.Contains("彩光") ||
            name.Contains("流光") || name.Contains("霁光") || name.Contains("磷光"))
            return rarity >= FishRarity.Epic ? "fish-22" : "fish-17";
        // 珊瑚/礁/小丑 — 珊瑚系
        if (name.Contains("珊瑚") || name.Contains("礁") || name.Contains("小丑"))
            return rarity >= FishRarity.Legendary ? "fish-29" : rarity >= FishRarity.Epic ? "fish-23" : "fish-22";
        // 深水海湾/矿渣 — 裂谷暗鱼
        if (name.Contains("裂谷") || name.Contains("暗涌") || name.Contains("矿渣"))
            return rarity >= FishRarity.Epic ? "fish-21" : "fish-20";
        // 幽蓝/深渊/暗流 — 深海巨口
        if (name.Contains("幽蓝") || name.Contains("深渊") || name.Contains("暗流") || name.Contains("深湾"))
            return rarity >= FishRarity.Legendary ? "fish-30" : rarity >= FishRarity.Epic ? "fish-20" : "fish-19";
        // 雾海 — 深海荧光
        if (name.Contains("雾海"))
            return rarity >= FishRarity.Legendary ? "fish-24" : rarity >= FishRarity.Epic ? "fish-21" : "fish-17";

        // ── 海洋中级层 (Row1: fish-09~16) ─────────────────────────
        // 鳗类
        if (name.Contains("鳗"))
        {
            if (name.Contains("电") || name.Contains("电鳗"))  return "fish-21"; // 电鳗→奇幻
            if (name.Contains("银") || name.Contains("跨域"))  return "fish-06"; // 银鳗→淡水
            return "fish-09";
        }
        // 鱿/乌贼
        if (name.Contains("鱿") || name.Contains("乌贼"))
            return rarity >= FishRarity.Epic ? "fish-22" : "fish-10";
        // 蝠鲼/鳐
        if (name.Contains("鳐") || name.Contains("蝠鲼"))
            return rarity >= FishRarity.Epic ? "fish-24" : "fish-11";
        // 旗鱼/马鲛/金枪
        if (name.Contains("旗鱼") || name.Contains("马鲛") || name.Contains("金枪"))
            return rarity >= FishRarity.Epic ? "fish-23" : "fish-12";
        // 石斑
        if (name.Contains("石斑"))  return "fish-13";
        // 小丑鱼 (非礁)
        if (name.Contains("小丑鱼")) return "fish-14";
        // 鲨
        if (name.Contains("鲨"))
            return rarity >= FishRarity.Legendary ? "fish-29" : "fish-15";
        // 锦鲤/金鳍/锦鳞
        if (name.Contains("锦鲤") || name.Contains("金鳍") || name.Contains("锦鳞"))
            return rarity >= FishRarity.Legendary ? "fish-25" : "fish-16";
        // 远礁外海普通鲈/沙丁
        if (name.Contains("礁湾") || name.Contains("外海") || name.Contains("远礁")) return "fish-13";

        // ── 淡水普通层 (Row0: fish-01~08) ─────────────────────────
        // 鲫/鲤 (鲫先检测，防止匹配到"锦鲤")
        if (name.Contains("鲫"))  return "fish-01";
        if (name.Contains("鲤") && !name.Contains("锦鲤") && !name.Contains("传说"))  return "fish-01";
        // 鲈 (已排除礁/深系)
        if (name.Contains("鲈"))  return "fish-02";
        // 虾/蟹/浮游/沙蚕/蝌蚪
        if (name.Contains("虾") || name.Contains("蟹") || name.Contains("浮游") || name.Contains("沙蚕") || name.Contains("蝌蚪"))
            return "fish-03";
        // 泥鳅/鱥
        if (name.Contains("泥鳅") || name.Contains("鱥"))  return "fish-04";
        // 鳟/鲑
        if (name.Contains("鳟") || name.Contains("鲑"))    return "fish-05";
        // 鳕/鲢/沙丁/银
        if (name.Contains("鳕") || name.Contains("鲢") || name.Contains("沙丁") || (name.Contains("银") && rarity == FishRarity.Common))
            return "fish-06";
        // 鲶/龟
        if (name.Contains("鲶") || name.Contains("龟"))    return "fish-07";
        // 金鱼/金鳞金鲤
        if (name.Contains("金"))                           return "fish-08";

        // fallback by rarity
        return rarity switch
        {
            FishRarity.Common    => "fish-01",
            FishRarity.Rare      => "fish-05",
            FishRarity.Epic      => "fish-17",
            FishRarity.Legendary => "fish-24",
            _                   => "fish-01"
        };
    }
}
