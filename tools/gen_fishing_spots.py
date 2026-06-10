#!/usr/bin/env python3
"""Generate Models/FishingSpotCatalog.Generated.cs — 13 spots, ~130 fish."""
from __future__ import annotations

# (name, rarity, min_w, max_w, depth, spawn, lure_id or None)
# rarity: C R E L M (myth)
C, R, E, L, M = "C", "R", "E", "L", "M"
S, Mm, D = "Shallow", "Middle", "Deep"

SPOTS = [
    {
        "key": "静溪", "lv": 1, "mul": 1.0, "dep": S, "ft": 2,
        "desc": "终端新手流域 · 浅层缓流 · 竹片与水草主产",
        "rarity": (70, 20, 8, 2),
        "fish": [
            ("野生鲫鱼", C, 0.5, 3.0, S, 50), ("大口鲈鱼", C, 1.0, 5.0, S, 45),
            ("草虾", C, 0.3, 1.5, S, 55), ("溪蟹", C, 0.2, 1.0, S, 48),
            ("柳根鱼", C, 0.4, 2.0, S, 52), ("浮萍鱼", C, 0.3, 1.2, S, 46),
            ("弹跳泥鳅", R, 0.8, 4.0, S, 80), ("彩虹鳟鱼", R, 1.5, 6.0, Mm, 100),
            ("溪石鳜", R, 1.2, 5.5, S, 85), ("翠鳞银鳟", E, 1.8, 7.5, Mm, 95),
            ("溪涧银龙", E, 2.0, 8.0, Mm, 100), ("镜湖灵鲤", E, 2.2, 9.0, S, 90),
            ("黄金锦鲤", L, 2.0, 10.0, S, 100),
            ("神话·镜湖神鲤", M, 4.0, 18.0, S, 120, "lure_mirror_koi"),
            ("神话·翠影鳗王", M, 3.5, 16.0, Mm, 110, "lure_creek_eel"),
        ],
        "preview": ["野生鲫鱼", "彩虹鳟鱼", "黄金锦鲤"],
    },
    {
        "key": "浅塘", "lv": 1, "mul": 1.05, "dep": S, "ft": 2,
        "desc": "城郊人工浅塘 · 莲叶遮蔽 · 入门练竿",
        "rarity": (68, 22, 8, 2),
        "fish": [
            ("浅塘鲫", C, 0.4, 2.5, S, 52), ("塘畔虾", C, 0.25, 1.2, S, 58),
            ("水莲鱼", C, 0.35, 1.8, S, 50), ("泥鲫", C, 0.45, 2.2, S, 48),
            ("浮萍鲤", C, 0.5, 2.8, S, 46), ("荷影小鳅", C, 0.3, 1.5, S, 44),
            ("浅塘鲤", R, 0.9, 4.5, S, 82), ("荷塘蟹", R, 0.7, 3.5, S, 78),
            ("琉璃鳞鱼", E, 1.6, 7.0, Mm, 92), ("浅塘银鳟", E, 1.9, 8.0, S, 88),
            ("塘主金鲤", L, 2.2, 11.0, S, 100),
            ("神话·浅塘幻鳞", M, 3.8, 15.0, S, 115, "lure_shallow_pond"),
        ],
        "preview": ["浅塘鲫", "浅塘鲤", "塘主金鲤"],
    },
    {
        "key": "雾海深渊", "lv": 8, "mul": 1.25, "dep": D, "ft": 2,
        "desc": "离岸雾墙 · 磷光深海 · 碳纤维主产",
        "rarity": (55, 25, 14, 6),
        "fish": [
            ("雾海鲈鱼", C, 1.0, 5.0, D, 40), ("雾海鲫鱼", C, 0.5, 3.0, D, 60),
            ("沙蚕幼体", C, 0.4, 2.0, S, 50), ("磷光小鱿", C, 0.5, 2.5, Mm, 55),
            ("雾海浮游群", C, 0.3, 1.5, D, 48), ("幽光幼鳗", C, 0.45, 2.2, D, 46),
            ("夜光乌贼", R, 1.0, 7.0, Mm, 100), ("深渊电鳗", R, 1.5, 9.0, D, 90),
            ("银鳍枪鱼", R, 1.3, 8.0, D, 85), ("珊瑚焰鱼", R, 1.1, 6.5, Mm, 80),
            ("幽蓝安康", E, 3.0, 12.0, D, 100), ("深渊鳐", E, 2.5, 15.0, D, 85),
            ("雾海巨鱿", E, 2.8, 13.0, D, 92), ("雾海锦鳞", L, 2.0, 10.0, D, 100),
            ("神话·雾海古神鱿", M, 5.0, 22.0, D, 120, "lure_abyss_core"),
        ],
        "preview": ["夜光乌贼", "幽蓝安康", "雾海锦鳞"],
    },
    {
        "key": "芦苇湾", "lv": 12, "mul": 1.3, "dep": S, "ft": 2,
        "desc": "芦苇荡浅湾 · 风噪掩蔽 · 芦苇纤维副产",
        "rarity": (52, 28, 14, 6),
        "fish": [
            ("芦苇鲫", C, 0.5, 2.8, S, 50), ("湾口虾", C, 0.3, 1.6, S, 54),
            ("苇影小鳅", C, 0.4, 2.0, S, 48), ("风纹银鲫", C, 0.55, 3.0, S, 46),
            ("湾畔蟹", C, 0.35, 1.8, S, 52), ("芦花浮鱼", C, 0.28, 1.4, Mm, 44),
            ("芦苇鲈", R, 1.1, 5.5, Mm, 88), ("湾影鳜", R, 1.3, 6.5, S, 85),
            ("风啸银鲑", R, 1.4, 7.0, Mm, 82), ("苇荡幽灵鲶", E, 2.2, 10.0, D, 95),
            ("芦湾金鳍", E, 2.0, 9.5, Mm, 90), ("苇歌传说鲤", L, 2.5, 12.0, S, 100),
            ("神话·芦苇幽歌", M, 4.2, 17.0, Mm, 118, "lure_reed_song"),
        ],
        "preview": ["芦苇鲈", "苇荡幽灵鲶", "苇歌传说鲤"],
    },
    {
        "key": "夜光引渠", "lv": 18, "mul": 1.5, "dep": Mm, "ft": 2,
        "desc": "荧光运河 · 中层夜钓 · 荧光粉主产",
        "rarity": (50, 28, 16, 6),
        "fish": [
            ("夜光鲤", C, 0.8, 4.0, Mm, 50), ("荧光蝌蚪", C, 0.4, 2.0, S, 55),
            ("渠水虾", C, 0.3, 1.8, S, 52), ("渠湾银鲫", C, 0.6, 3.0, S, 48),
            ("引渠泥鳅", R, 1.0, 5.5, S, 90), ("夜光鲈", R, 1.2, 7.0, Mm, 100),
            ("流光虾", R, 0.6, 3.5, S, 80), ("霁光蜓鱼", R, 0.9, 4.5, Mm, 88),
            ("彩光鲑", E, 2.0, 10.0, Mm, 100), ("运河幽灵鲶", E, 2.5, 12.0, D, 95),
            ("彩鳞鳍鱼", E, 1.8, 9.5, Mm, 92), ("引渠金鳍", L, 2.5, 12.0, Mm, 100),
            ("神话·引渠幻龙", M, 4.5, 20.0, Mm, 120, "lure_canal_dragon"),
        ],
        "preview": ["引渠泥鳅", "彩光鲑", "引渠金鳍"],
    },
    {
        "key": "暗涌裂谷", "lv": 18, "mul": 1.55, "dep": D, "ft": 2,
        "desc": "地壳裂谷暗涌 · 高压深层 · 裂谷矿渣",
        "rarity": (48, 30, 16, 6),
        "fish": [
            ("裂谷小鳕", C, 0.5, 2.5, D, 48), ("暗涌虾", C, 0.4, 2.0, D, 52),
            ("矿渣浮游", C, 0.3, 1.5, D, 50), ("裂隙银鲫", C, 0.55, 3.0, Mm, 46),
            ("暗涌鳗苗", C, 0.45, 2.2, D, 44), ("裂谷鲈", R, 1.2, 6.0, D, 86),
            ("暗涌旗鱼", R, 1.5, 8.5, D, 90), ("裂隙幽灵鲶", R, 1.1, 5.5, D, 82),
            ("暗涌巨口鱼", E, 2.4, 11.0, D, 95), ("裂谷银鳗", E, 2.0, 10.0, D, 92),
            ("暗涌金鳞", E, 2.6, 12.5, D, 88), ("裂谷传说鳕", L, 3.0, 14.0, D, 100),
            ("神话·暗涌裂谷兽", M, 4.8, 21.0, D, 120, "lure_rift_beast"),
        ],
        "preview": ["暗涌旗鱼", "暗涌巨口鱼", "裂谷传说鳕"],
    },
    {
        "key": "极光冰湾", "lv": 32, "mul": 1.8, "dep": D, "ft": 2,
        "desc": "极光环冰湾 · 寒渊深层 · 极光冰晶",
        "rarity": (42, 30, 20, 8),
        "fish": [
            ("雪线鲫", C, 0.6, 3.5, S, 50), ("寒冰虾", C, 0.5, 2.5, S, 55),
            ("冰吻小鳕", C, 0.4, 2.2, S, 52), ("霜纹浮鱼", C, 0.35, 1.8, Mm, 48),
            ("冰晶鱼", R, 1.2, 6.0, D, 100), ("极寒银鲑", R, 1.5, 8.0, Mm, 95),
            ("极光鳟", R, 1.3, 7.0, Mm, 88), ("霜纹银鲂", R, 1.0, 5.5, S, 82),
            ("冰纹银鳗", E, 1.8, 10.0, D, 100), ("冰川巨鲈", E, 2.5, 14.0, D, 100),
            ("冰湾幽灵鳕", E, 2.0, 11.0, D, 90), ("寒渊巨口鱼", E, 2.2, 12.0, D, 93),
            ("霜龙传说鱼", L, 3.0, 15.0, D, 100),
            ("神话·极光霜龙", M, 6.0, 25.0, D, 120, "lure_aurora_crystal"),
        ],
        "preview": ["冰晶鱼", "冰纹银鳗", "霜龙传说鱼"],
    },
    {
        "key": "沉船墓场", "lv": 32, "mul": 2.0, "dep": D, "ft": 1,
        "desc": "锈蚀沉船群 · 亡魂巡游 · 沉船铁锈",
        "rarity": (40, 32, 20, 8),
        "fish": [
            ("墓场沙丁", C, 0.3, 1.5, D, 46), ("铁锈虾", C, 0.4, 2.0, D, 50),
            ("亡骸浮游", C, 0.35, 1.8, D, 48), ("墓湾小鳕", C, 0.45, 2.5, D, 44),
            ("锈迹银鲫", R, 0.9, 4.5, Mm, 84), ("墓场电鳗", R, 1.4, 8.0, D, 88),
            ("亡魂小鱿", R, 1.0, 6.0, D, 80), ("暗流旗鱼", R, 2.3, 14.0, D, 95),
            ("墓场幽灵鲨", E, 2.8, 15.0, D, 100), ("亡魂旗鱼", E, 2.5, 13.0, D, 92),
            ("锈壳巨龟", E, 3.0, 18.0, S, 88), ("墓主传说鳕", L, 3.2, 16.0, D, 100),
            ("神话·沉船亡魂", M, 5.5, 23.0, D, 120, "lure_wreck_soul"),
        ],
        "preview": ["暗流旗鱼", "墓场幽灵鲨", "墓主传说鳕"],
    },
    {
        "key": "珊瑚暗流", "lv": 32, "mul": 1.95, "dep": D, "ft": 2,
        "desc": "珊瑚礁暗流带 · 隐蔽深层 · 珊瑚碎片",
        "rarity": (40, 32, 20, 8),
        "fish": [
            ("珊瑚小丑鱼", C, 0.5, 2.5, S, 55), ("暗流小虾", C, 0.3, 1.5, Mm, 50),
            ("礁影浮鱼", C, 0.4, 2.0, Mm, 48), ("珊瑚幼鱿", C, 0.45, 2.2, D, 46),
            ("礁湾沙丁", C, 0.28, 1.4, S, 44), ("礁影石斑", R, 1.1, 6.0, Mm, 88),
            ("珊瑚焰鱼", R, 1.1, 6.5, Mm, 82), ("暗流银鳗", R, 1.5, 9.0, D, 90),
            ("珊瑚暗流鳗", E, 2.0, 11.0, D, 95), ("礁壳巨龟", E, 3.0, 18.0, S, 88),
            ("珊瑚巨鱿", E, 2.6, 13.0, D, 92), ("珊瑚心金鳞", L, 2.8, 14.0, D, 100),
            ("神话·珊瑚心海", M, 5.2, 22.0, D, 118, "lure_coral_heart"),
        ],
        "preview": ["礁影石斑", "珊瑚暗流鳗", "珊瑚心金鳞"],
    },
    {
        "key": "远礁外海", "lv": 45, "mul": 2.2, "dep": D, "ft": 1,
        "desc": "远礁外海开放水域 · 高周转 · 外海星核",
        "rarity": (35, 32, 22, 11),
        "fish": [
            ("礁湾鲈鱼", C, 1.0, 5.0, Mm, 45), ("远海沙丁", C, 0.3, 1.5, S, 50),
            ("外海浮游群", C, 0.35, 1.8, D, 48), ("礁影小鳕", C, 0.4, 2.2, D, 44),
            ("深流银鳗", R, 1.5, 9.0, D, 100), ("深湾金枪", R, 2.0, 12.0, D, 95),
            ("巨口马鲛", R, 1.4, 8.0, D, 90), ("远礁蝠鲼", E, 2.5, 16.0, D, 100),
            ("远礁旗鱼", E, 2.3, 14.0, D, 92), ("外海巨鱿", E, 2.8, 15.0, D, 88),
            ("金鳞锦鲤", L, 3.0, 18.0, D, 100), ("深湾巨口鲨", L, 4.0, 25.0, D, 100),
            ("神话·远海沧龙", M, 6.5, 28.0, D, 120, "lure_reef_leviathan"),
            ("神话·金鳞海皇", M, 5.5, 24.0, D, 115, "lure_sea_emperor"),
        ],
        "preview": ["深流银鳗", "金鳞锦鲤", "深湾巨口鲨"],
    },
    {
        "key": "深渊回廊", "lv": 45, "mul": 2.5, "dep": D, "ft": 1,
        "desc": "海底裂隙回廊 · 凝胶矿脉 · 深渊凝胶",
        "rarity": (32, 34, 24, 10),
        "fish": [
            ("回廊小鳕", C, 0.4, 2.2, D, 44), ("深渊浮游", C, 0.35, 1.8, D, 50),
            ("凝胶幼体", C, 0.3, 1.5, D, 48), ("回廊银鲫", C, 0.5, 2.8, Mm, 46),
            ("深渊鲈", R, 1.3, 7.0, D, 86), ("回廊电鳗", R, 1.6, 9.5, D, 90),
            ("凝胶鱿", R, 1.2, 7.5, D, 84), ("回廊巨口鱼", E, 2.6, 13.0, D, 95),
            ("深渊安康", E, 3.0, 14.0, D, 92), ("回廊蝠鲼", E, 2.8, 15.0, D, 88),
            ("回廊传说鲨", L, 3.5, 20.0, D, 100),
            ("神话·深渊巡礼者", M, 6.0, 26.0, D, 120, "lure_abyss_walker"),
        ],
        "preview": ["回廊电鳗", "深渊安康", "回廊传说鲨"],
    },
    {
        "key": "星潮海沟", "lv": 45, "mul": 2.6, "dep": D, "ft": 1,
        "desc": "星潮引力海沟 · 潮汐异种 · 星潮碎片",
        "rarity": (30, 34, 26, 10),
        "fish": [
            ("星潮沙丁", C, 0.3, 1.5, D, 46), ("潮汐小虾", C, 0.35, 1.8, D, 50),
            ("星尘浮游", C, 0.28, 1.4, D, 48), ("海沟银鲫", C, 0.45, 2.5, Mm, 44),
            ("星潮鲈", R, 1.4, 7.5, D, 88), ("潮汐旗鱼", R, 1.8, 10.0, D, 92),
            ("星尘鳗", R, 1.5, 9.0, D, 86), ("海沟巨口鱼", E, 2.8, 14.0, D, 95),
            ("星潮蝠鲼", E, 3.0, 16.0, D, 90), ("潮汐幽灵鲨", E, 2.6, 13.5, D, 88),
            ("星潮传说鳕", L, 3.8, 22.0, D, 100),
            ("神话·星潮巨兽", M, 6.2, 27.0, D, 120, "lure_star_tide"),
        ],
        "preview": ["潮汐旗鱼", "星潮蝠鲼", "星潮传说鳕"],
    },
    {
        "key": "虚空钓域", "lv": 60, "mul": 3.0, "dep": D, "ft": 1,
        "desc": "终局虚空水域 · 全神话解锁 · 虚空纤维终极产",
        "rarity": (28, 30, 28, 14),
        "fish": [
            ("虚空浮游", C, 0.4, 2.0, D, 42), ("裂隙小鳕", C, 0.45, 2.5, D, 44),
            ("虚影虾", C, 0.35, 1.8, D, 48), ("虚空银鲫", C, 0.5, 3.0, Mm, 46),
            ("虚空鲈", R, 1.5, 8.0, D, 85), ("裂隙电鳗", R, 1.8, 10.5, D, 90),
            ("虚影鱿", R, 1.3, 8.0, D, 82), ("虚空巨口鱼", E, 3.0, 15.0, D, 95),
            ("裂隙蝠鲼", E, 3.2, 17.0, D, 92), ("虚空幽灵鲨", E, 2.8, 14.5, D, 88),
            ("终焉传说鱼", L, 4.5, 28.0, D, 100), ("虚空鲸影", L, 5.0, 32.0, D, 95),
            ("神话·虚空钓主", M, 7.0, 30.0, D, 125, "lure_void_master"),
            ("神话·终焉鲸歌", M, 6.8, 29.0, D, 122, "lure_void_whale"),
        ],
        "preview": ["虚空巨口鱼", "终焉传说鱼", "神话·虚空钓主"],
    },
]

# migratory — add to late spots with low weight
MIGRATORY = [
    ("跨域银鳗", R, 1.6, 9.5, D, 35, ["远礁外海", "深渊回廊", "星潮海沟"]),
    ("归潮鳐", E, 2.4, 12.0, D, 30, ["极光冰湾", "远礁外海", "星潮海沟"]),
    ("星尘巡游鱼", R, 1.2, 7.0, D, 28, ["深渊回廊", "星潮海沟", "虚空钓域"]),
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
