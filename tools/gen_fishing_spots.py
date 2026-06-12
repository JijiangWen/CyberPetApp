#!/usr/bin/env python3
"""Generate Models/FishingSpotCatalog.Generated.cs — 13 spots, ~130 fish."""
from __future__ import annotations

# (name, rarity, min_w, max_w, depth, spawn, lure_id or None)
# rarity: C R E L M (myth)
C, R, E, L, M = "C", "R", "E", "L", "M"
S, Mm, D = "Shallow", "Middle", "Deep"

SPOTS = [
    {
        "key": "镇外溪流", "lv": 1, "mul": 1.0, "dep": S, "ft": 2,
        "desc": "终端新手流域 · 浅层缓流 · 竹片与水草主产",
        "rarity": (70, 20, 8, 2),
        "fish": [
            ("溪边小白条", C, 0.5, 3.0, S, 50), ("土麦穗鱼", C, 1.0, 5.0, S, 45),
            ("河湾青壳虾", C, 0.3, 1.5, S, 55), ("溪底石蟹", C, 0.2, 1.0, S, 48),
            ("野柳根子", C, 0.4, 2.0, S, 52), ("宽鳍马口鱼", C, 0.3, 1.2, S, 46),
            ("滑溜大泥鳅", R, 0.8, 4.0, S, 80), ("野花翅子(虹鳟)", R, 1.5, 6.0, Mm, 100),
            ("溪边黄石爬子", R, 1.2, 5.5, S, 85), ("精明老花鳟", E, 1.8, 7.5, Mm, 95),
            ("大鳍红马口(溪哥)", E, 2.0, 8.0, Mm, 100), ("野池红鳞锦鲤", E, 2.2, 9.0, S, 90),
            ("金背鲤仙", L, 2.0, 10.0, S, 100),
            ("异变·“镜湖水虎兽”", M, 4.0, 18.0, S, 120, "lure_mutant_piranha"),
            ("异变·“铁骨溪鳗”", M, 3.5, 16.0, Mm, 110, "lure_mutant_eel"),
        ],
        "preview": ["溪边小白条", "野花翅子(虹鳟)", "金背鲤仙"],
    },
    {
        "key": "废弃鱼塘", "lv": 1, "mul": 1.05, "dep": S, "ft": 2,
        "desc": "城郊人工浅塘 · 莲叶遮蔽 · 入门练竿",
        "rarity": (68, 22, 8, 2),
        "fish": [
            ("烂泥塘鲫鱼", C, 0.4, 2.5, S, 52), ("腐草泥河虾", C, 0.25, 1.2, S, 58),
            ("塘湾青皮黑鱼", C, 0.35, 1.8, S, 50), ("小昂刺鱼(黄骨鱼)", C, 0.45, 2.2, S, 48),
            ("浮萍野草鱼", C, 0.5, 2.8, S, 46), ("麦穗杂鱼仔", C, 0.3, 1.5, S, 44),
            ("烂泥塘大青鱼", R, 0.9, 4.5, S, 82), ("烂草塘毛蟹", R, 0.7, 3.5, S, 78),
            ("红肚皮罗非鱼", E, 1.6, 7.0, Mm, 92), ("胖头鳙鱼(剁椒鱼头)", E, 1.9, 8.0, S, 88),
            ("塘主·“独眼老草鱼”", L, 2.2, 11.0, S, 100),
            ("异变·“淤泥吞噬者”", M, 3.8, 15.0, S, 115, "lure_swamp_devourer"),
        ],
        "preview": ["烂泥塘鲫鱼", "烂泥塘大青鱼", "塘主·“独眼老草鱼”"],
    },
    {
        "key": "近海礁石", "lv": 8, "mul": 1.25, "dep": D, "ft": 2,
        "desc": "离岸雾墙 · 磷光深海 · 碳纤维主产",
        "rarity": (55, 25, 14, 6),
        "fish": [
            ("浪击海鲈鱼", C, 1.0, 5.0, D, 40), ("礁影红加吉鱼", C, 0.5, 3.0, D, 60),
            ("沙蚕爬虫", C, 0.4, 2.0, S, 50), ("小透明鱿鱼仔", C, 0.5, 2.5, Mm, 55),
            ("礁石小红虾", C, 0.3, 1.5, D, 48), ("小海鳗苗", C, 0.45, 2.2, D, 46),
            ("荧光墨鱼", R, 1.0, 7.0, Mm, 100), ("乱石堆电鳗", R, 1.5, 9.0, D, 90),
            ("小银剪子(银鳍枪鱼)", R, 1.3, 8.0, D, 85), ("烈焰红仙子", R, 1.1, 6.5, Mm, 80),
            ("大灯笼安康鱼", E, 3.0, 12.0, D, 100), ("深水魔鬼鳐", E, 2.5, 15.0, D, 85),
            ("巨型软丝鱿", E, 2.8, 13.0, D, 92), ("斑斓大石斑", L, 2.0, 10.0, D, 100),
            ("异变·“雾海古神鱿”", M, 5.0, 22.0, D, 120, "lure_giant_squid"),
        ],
        "preview": ["荧光墨鱼", "大灯笼安康鱼", "斑斓大石斑"],
    },
    {
        "key": "芦苇湿地", "lv": 12, "mul": 1.3, "dep": S, "ft": 2,
        "desc": "芦苇荡浅湾 · 风噪掩蔽 · 老韧芦苇丝副产",
        "rarity": (52, 28, 14, 6),
        "fish": [
            ("老芦苇青鲫", C, 0.5, 2.8, S, 50), ("浅滩青虾", C, 0.3, 1.6, S, 54),
            ("芦苇根小泥鳅", C, 0.4, 2.0, S, 48), ("风纹白鲫", C, 0.55, 3.0, S, 46),
            ("湿地圆螃蟹", C, 0.35, 1.8, S, 52), ("芦花游鲦", C, 0.28, 1.4, Mm, 44),
            ("野性湿地鲈", R, 1.1, 5.5, Mm, 88), ("苇荡鳜鱼", R, 1.3, 6.5, S, 85),
            ("逆流银大鲑", R, 1.4, 7.0, Mm, 82), ("泥沼黄斑大鲶", E, 2.2, 10.0, D, 95),
            ("湿地黄姑子", E, 2.0, 9.5, Mm, 90), ("湿地老青鲩", L, 2.5, 12.0, S, 100),
            ("异变·“毒沼鳄王”", M, 4.2, 17.0, Mm, 118, "lure_poison_croc"),
        ],
        "preview": ["野性湿地鲈", "泥沼黄斑大鲶", "湿地老青鲩"],
    },
    {
        "key": "地下暗河", "lv": 18, "mul": 1.5, "dep": Mm, "ft": 2,
        "desc": "荧光运河 · 中层夜钓 · 荧光粉主产",
        "rarity": (50, 28, 16, 6),
        "fish": [
            ("盲眼发光鲤", C, 0.8, 4.0, Mm, 50), ("暗河玻璃蝌蚪", C, 0.4, 2.0, S, 55),
            ("暗河透明虾", C, 0.3, 1.8, S, 52), ("暗河长须鲫", C, 0.6, 3.0, S, 48),
            ("暗河黄腊丁", R, 1.0, 5.5, S, 90), ("夜行黑棘鲈", R, 1.2, 7.0, Mm, 100),
            ("五彩荧光大虾", R, 0.6, 3.5, S, 80), ("霁光玻璃鱼", R, 0.9, 4.5, Mm, 88),
            ("引水五彩鲑", E, 2.0, 10.0, Mm, 100), ("地底粉红鲵", E, 2.5, 12.0, D, 95),
            ("七彩霓虹鲷", E, 1.8, 9.5, Mm, 92), ("暗河金丝鲃", L, 2.5, 12.0, Mm, 100),
            ("异变·“白化巨螈”", M, 4.5, 20.0, Mm, 120, "lure_albino_salamander"),
        ],
        "preview": ["暗河黄腊丁", "引水五彩鲑", "暗河金丝鲃"],
    },
    {
        "key": "深水海湾", "lv": 18, "mul": 1.55, "dep": D, "ft": 2,
        "desc": "地壳裂谷暗涌 · 高压深层 · 热液喷口矿渣",
        "rarity": (48, 30, 16, 6),
        "fish": [
            ("岩缝爬岩鱼", C, 0.5, 2.5, D, 48), ("深渊火山虾", C, 0.4, 2.0, D, 52),
            ("热液口磷虾", C, 0.3, 1.5, D, 50), ("裂隙盲鲫", C, 0.55, 3.0, Mm, 46),
            ("深水海鳗苗", C, 0.45, 2.2, D, 44), ("深海石九公", R, 1.2, 6.0, D, 86),
            ("蓝枪鱼", R, 1.5, 8.5, D, 90), ("裂谷无眼鲶", R, 1.1, 5.5, D, 82),
            ("热液大口黑鱼", E, 2.4, 11.0, D, 95), ("白玉长寿鳗", E, 2.0, 10.0, D, 92),
            ("金目鲷", E, 2.6, 12.5, D, 88), ("深湾老船长鳕鱼", L, 3.0, 14.0, D, 100),
            ("异变·“裂谷飞蝠”", M, 4.8, 21.0, D, 120, "lure_mutant_ray"),
        ],
        "preview": ["蓝枪鱼", "热液大口黑鱼", "深湾老船长鳕鱼"],
    },
    {
        "key": "极光冰湾", "lv": 32, "mul": 1.8, "dep": D, "ft": 2,
        "desc": "极光环冰湾 · 寒渊深层 · 不融极地冰晶",
        "rarity": (42, 30, 20, 8),
        "fish": [
            ("破冰雪鳞鲫", C, 0.6, 3.5, S, 50), ("极地白虾", C, 0.5, 2.5, S, 55),
            ("冰吻沙丁鱼", C, 0.4, 2.2, S, 52), ("霜斑鲱鱼", C, 0.35, 1.8, Mm, 48),
            ("透明冰晶鱼", R, 1.2, 6.0, D, 100), ("北极王鲑", R, 1.5, 8.0, Mm, 95),
            ("五彩极光鳟", R, 1.3, 7.0, Mm, 88), ("霜纹白鲂", R, 1.0, 5.5, S, 82),
            ("冰川蛇鳕", E, 1.8, 10.0, D, 100), ("北极重水鲈", E, 2.5, 14.0, D, 100),
            ("白化冰鳕", E, 2.0, 11.0, D, 90), ("深渊巨口宽咽鱼", E, 2.2, 12.0, D, 93),
            ("极光冰川巨鳎", L, 3.0, 15.0, D, 100),
            ("神话·“极光霜龙”", M, 6.0, 25.0, D, 120, "lure_aurora_crystal"),
        ],
        "preview": ["透明冰晶鱼", "冰川蛇鳕", "极光冰川巨鳎"],
    },
    {
        "key": "沉船墓场", "lv": 32, "mul": 2.0, "dep": D, "ft": 1,
        "desc": "锈蚀沉船群 · 亡魂巡游 · 百年沉船铁皮",
        "rarity": (40, 32, 20, 8),
        "fish": [
            ("沉船缝沙丁", C, 0.3, 1.5, D, 46), ("铁皮锈斑虾", C, 0.4, 2.0, D, 50),
            ("幽灵发光浮游", C, 0.35, 1.8, D, 48), ("沉船黑斑鳕", C, 0.45, 2.5, D, 44),
            ("锈斑扁鲫", R, 0.9, 4.5, Mm, 84), ("沉船黑电鳗", R, 1.4, 8.0, D, 88),
            ("黑火透光乌贼", R, 1.0, 6.0, D, 80), ("暗流刺盖鱼", R, 2.3, 14.0, D, 95),
            ("深海皱鳃鲨", E, 2.8, 15.0, D, 100), ("墓场带鱼王", E, 2.5, 13.0, D, 92),
            ("百年老船壳龟", E, 3.0, 18.0, S, 88), ("沉船幽灵鳕", L, 3.2, 16.0, D, 100),
            ("神话·“沉船亡魂”", M, 5.5, 23.0, D, 120, "lure_wreck_soul"),
        ],
        "preview": ["暗流刺盖鱼", "深海皱鳃鲨", "沉船幽灵鳕"],
    },
    {
        "key": "珊瑚暗流", "lv": 32, "mul": 1.95, "dep": D, "ft": 2,
        "desc": "珊瑚礁暗流带 · 隐蔽深层 · 红珊瑚碎屑",
        "rarity": (40, 32, 20, 8),
        "fish": [
            ("红海葵小丑鱼", C, 0.5, 2.5, S, 55), ("珊瑚缝雀尾虾", C, 0.3, 1.5, Mm, 50),
            ("五彩玻璃雀鲷", C, 0.4, 2.0, Mm, 48), ("红点玻璃墨鱼", C, 0.45, 2.2, D, 46),
            ("珊瑚沙丁", C, 0.28, 1.4, S, 44), ("红眉斑石斑", R, 1.1, 6.0, Mm, 88),
            ("烈焰红仙子", R, 1.1, 6.5, Mm, 82), ("珊瑚白星裸胸鳝", R, 1.5, 9.0, D, 90),
            ("红星狼鲈", E, 2.0, 11.0, D, 95), ("玳瑁巨海龟", E, 3.0, 18.0, S, 88),
            ("红花金枪鱿", E, 2.6, 13.0, D, 92), ("红珊瑚金眼鲷", L, 2.8, 14.0, D, 100),
            ("神话·“珊瑚心海”", M, 5.2, 22.0, D, 118, "lure_coral_heart"),
        ],
        "preview": ["红眉斑石斑", "红星狼鲈", "红珊瑚金眼鲷"],
    },
    {
        "key": "远礁外海", "lv": 45, "mul": 2.2, "dep": D, "ft": 1,
        "desc": "远礁外海开放水域 · 高周转 · 落星陨铁晶核",
        "rarity": (35, 32, 22, 11),
        "fish": [
            ("礁盘黄鸡鱼", C, 1.0, 5.0, Mm, 45), ("蓝背沙丁鱼", C, 0.3, 1.5, S, 50),
            ("外海浮游磷虾", C, 0.35, 1.8, D, 48), ("礁影石九公", C, 0.4, 2.2, D, 44),
            ("深水海鳝", R, 1.5, 9.0, D, 100), ("蓝鳍金枪鱼", R, 2.0, 12.0, D, 95),
            ("大马鲛鱼", R, 1.4, 8.0, D, 90), ("飞翼蝠鲼", E, 2.5, 16.0, D, 100),
            ("黑皮旗鱼", E, 2.3, 14.0, D, 92), ("巨型红鱿鱼", E, 2.8, 15.0, D, 88),
            ("大洋金鳞鲷", L, 3.0, 18.0, D, 100), ("大白鲨", L, 4.0, 25.0, D, 100),
            ("神话·“远海沧龙”", M, 6.5, 28.0, D, 120, "lure_reef_leviathan"),
            ("神话·“金鳞海皇”", M, 5.5, 24.0, D, 115, "lure_sea_emperor"),
        ],
        "preview": ["深水海鳝", "大洋金鳞鲷", "大白鲨"],
    },
    {
        "key": "深渊回廊", "lv": 45, "mul": 2.5, "dep": D, "ft": 1,
        "desc": "海底裂隙回廊 · 凝胶矿脉 · 深渊巨兽粘液",
        "rarity": (32, 34, 24, 10),
        "fish": [
            ("裂隙小鳚", C, 0.4, 2.2, D, 44), ("深渊透明小介虫", C, 0.35, 1.8, D, 50),
            ("透明凝胶水母", C, 0.3, 1.5, D, 48), ("深渊琵琶鱼", C, 0.5, 2.8, Mm, 46),
            ("黑首阿氏鲈", R, 1.3, 7.0, D, 86), ("回廊电箭鳗", R, 1.6, 9.5, D, 90),
            ("凝胶玻璃鱿", R, 1.2, 7.5, D, 84), ("深海巨齿鱼", E, 2.6, 13.0, D, 95),
            ("黑鬼安康鱼", E, 3.0, 14.0, D, 92), ("回廊蝠鳐", E, 2.8, 15.0, D, 88),
            ("格陵兰睡鲨", L, 3.5, 20.0, D, 100),
            ("神话·“深渊巡礼者”", M, 6.0, 26.0, D, 120, "lure_abyss_walker"),
        ],
        "preview": ["回廊电箭鳗", "黑鬼安康鱼", "格陵兰睡鲨"],
    },
    {
        "key": "星潮海沟", "lv": 45, "mul": 2.6, "dep": D, "ft": 1,
        "desc": "星潮引力海沟 · 潮汐异种 · 月汐重力碎屑",
        "rarity": (30, 34, 26, 10),
        "fish": [
            ("潮汐玻璃鲱", C, 0.3, 1.5, D, 46), ("潮汐玻璃虾", C, 0.35, 1.8, D, 50),
            ("星光浮游", C, 0.28, 1.4, D, 48), ("海沟扁头鱼", C, 0.45, 2.5, Mm, 44),
            ("星点刺盖鲈", R, 1.4, 7.5, D, 88), ("星海大带鱼", R, 1.8, 10.0, D, 92),
            ("星斑裸胸鳝", R, 1.5, 9.0, D, 86), ("海沟深邃巨口鱼", E, 2.8, 14.0, D, 95),
            ("潮汐蝠鳐", E, 3.0, 16.0, D, 90), ("星潮皱鳃鲨", E, 2.6, 13.5, D, 88),
            ("星海巨鳞鳕", L, 3.8, 22.0, D, 100),
            ("神话·“星潮巨兽”", M, 6.2, 27.0, D, 120, "lure_star_tide"),
        ],
        "preview": ["星海大带鱼", "潮汐蝠鳐", "星海巨鳞鳕"],
    },
    {
        "key": "虚空钓域", "lv": 60, "mul": 3.0, "dep": D, "ft": 1,
        "desc": "终局虚空水域 · 全神话解锁 · 裂隙虚空丝线终极产",
        "rarity": (28, 30, 28, 14),
        "fish": [
            ("虚影介虫", C, 0.4, 2.0, D, 42), ("裂隙小鳕", C, 0.45, 2.5, D, 44),
            ("虚无玻璃虾", C, 0.35, 1.8, D, 48), ("虚空发光鲫", C, 0.5, 3.0, Mm, 46),
            ("虚空棘鲷", R, 1.5, 8.0, D, 85), ("虚空电箭鳗", R, 1.8, 10.5, D, 90),
            ("虚幻透明鱿", R, 1.3, 8.0, D, 82), ("虚空巨口鱼", E, 3.0, 15.0, D, 95),
            ("虚空飞蝠", E, 3.2, 17.0, D, 92), ("虚空幽灵鲨", E, 2.8, 14.5, D, 88),
            ("终焉红棘鲷", L, 4.5, 28.0, D, 100), ("虚空巨鲸影", L, 5.0, 32.0, D, 95),
            ("神话·“虚空钓主”", M, 7.0, 30.0, D, 125, "lure_void_master"),
            ("神话·“终焉鲸歌”", M, 6.8, 29.0, D, 122, "lure_void_whale"),
        ],
        "preview": ["虚空巨口鱼", "终焉红棘鲷", "神话·“虚空钓主”"],
    },
]

# migratory — add to late spots with low weight
MIGRATORY = [
    ("跨洋银裸胸鳝", R, 1.6, 9.5, D, 35, ["远礁外海", "深渊回廊", "星潮海沟"]),
    ("归潮蝠鲼", E, 2.4, 12.0, D, 30, ["极光冰湾", "远礁外海", "星潮海沟"]),
    ("星尘巡浪鱼", R, 1.2, 7.0, D, 28, ["深渊回廊", "星潮海沟", "虚空钓域"]),
]

RARITY_MAP = {"C": "FishRarity.Common", "R": "FishRarity.Rare", "E": "FishRarity.Epic", "L": "FishRarity.Legendary", "M": "FishRarity.Legendary"}
DEPTH_MAP = {"Shallow": "WaterDepth.Shallow", "Middle": "WaterDepth.Middle", "Deep": "WaterDepth.Deep"}

def fish_line(name, r, mn, mx, dep, sp, lure=None):
    rr = RARITY_MAP[r]
    dd = DEPTH_MAP[dep]
    hun = {"C": 14, "R": 22, "E": 35, "L": 50, "M": 72}[r]
    war = {"C": 0.08, "R": 0.30, "E": 0.55, "L": 0.75, "M": 0.88}[r]
    pow = {"C": 0.06, "R": 0.28, "E": 0.52, "L": 0.82, "M": 0.94}[r]
    lure_part = f', TargetLureRecipeId = "{lure}"' if lure else ""
    myth = r == "M"
    display = f"神话·{name[3:]}" if myth and not name.startswith("神话") else name
    if myth and not name.startswith("神话"):
        display = name
    return (
        f'            new FishTemplate("{name}", {hun}, {hun//2+2}, {hun//3+1}, {rr}, {mn}, {mx}, '
        f'spawnWeight: {sp}, wariness: {war}, power: {pow}) {{ PreferredDepth = {dd}{lure_part} }},'
    )

lines = [
    "// <auto-generated by tools/gen_fishing_spots.py — do not edit by hand>",
    "namespace CyberPetApp.Models;",
    "",
    "public static partial class FishingSpotCatalog",
    "{",
    "    private static void BuildAllSpots(Dictionary<string, FishingSpot> spots)",
    "    {",
]

for spot in SPOTS:
    c, r, e, l = spot["rarity"]
    lines.append(f'        // ── {spot["key"]} Lv{spot["lv"]} · ×{spot["mul"]} · {spot["dep"]} ──')
    lines.append(f'        var _{spot["key"]} = new FishingSpot("{spot["key"]}")')
    lines.append("        {")
    lines.append(f'            FishingTime = {spot["ft"]},')
    lines.append(f'            RequiredLevel = {spot["lv"]},')
    lines.append(f'            PriceMultiplier = {spot["mul"]},')
    lines.append(f'            PrimaryDepth = {DEPTH_MAP[spot["dep"]]},')
    lines.append(f'            Description = "{spot["desc"]}",')
    if spot["lv"] > 1:
        lines.append("            FishRarityTable = new()")
        lines.append("            {")
        lines.append(f"                {{ FishRarity.Common, {c} }},")
        lines.append(f"                {{ FishRarity.Rare, {r} }},")
        lines.append(f"                {{ FishRarity.Epic, {e} }},")
        lines.append(f"                {{ FishRarity.Legendary, {l} }}")
        lines.append("            }")
    lines.append("        };")
    lines.append(f'        _{spot["key"]}.FishTable.AddRange([')
    fish_names = {f[0] for f in spot["fish"]}
    for f in spot["fish"]:
        lines.append(fish_line(*f))
    for mig in MIGRATORY:
        if spot["key"] in mig[6]:
            if mig[0] not in fish_names:
                lines.append(fish_line(mig[0], mig[1], mig[2], mig[3], mig[4], mig[5]))
    lines.append("        ]);")
    lines.append(f'        spots["{spot["key"]}"] = _{spot["key"]};')
    lines.append("")

lines.append("    }")
lines.append("")
lines.append("    private static IReadOnlyDictionary<string, string[]> PreviewFishMap => new Dictionary<string, string[]>")
lines.append("    {")
for spot in SPOTS:
    prev = ", ".join(f'"{p}"' for p in spot["preview"])
    lines.append(f'        ["{spot["key"]}"] = [{prev}],')
lines.append("    };")
lines.append("}")

out = "\n".join(lines) + "\n"
path = __file__.replace("tools\\gen_fishing_spots.py", "Models/FishingSpotCatalog.Generated.cs").replace("tools/gen_fishing_spots.py", "Models/FishingSpotCatalog.Generated.cs")
import os
root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
out_path = os.path.join(root, "Models", "FishingSpotCatalog.Generated.cs")
with open(out_path, "w", encoding="utf-8") as f:
    f.write(out)

total = sum(len(s["fish"]) for s in SPOTS)
myth = sum(1 for s in SPOTS for f in s["fish"] if len(f) > 6 and f[6])
print(f"Wrote {out_path}: {len(SPOTS)} spots, ~{total} base fish, {myth} mythics")
